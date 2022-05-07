#region License

/*
 * Doboz Data Compression Library
 * Copyright (C) 2010-2011 Attila T. Afra <attila.afra@gmail.com>
 * 
 * This software is provided 'as-is', without any express or implied warranty. In no event will
 * the authors be held liable for any damages arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose, including commercial
 * applications, and to alter it and redistribute it freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not claim that you wrote the
 *    original software. If you use this software in a product, an acknowledgment in the product
 *    documentation would be appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be misrepresented as
 *    being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

#endregion

using System;
using System.Diagnostics;

namespace Compression.Doboz
{
    /// <summary>
    /// Doboz codec.
    /// </summary>
    public unsafe class DobozCodec : DobozDecoder
    {
        internal const int MAX_MATCH_LENGTH = 255 + MIN_MATCH_LENGTH;
        internal const int MAX_MATCH_CANDIDATE_COUNT = 128;
        internal const int DICTIONARY_SIZE = 1 << 21; // 2 MB, must be a power of 2!
        const int TRAILING_DUMMY_SIZE = WORD_SIZE; // safety trailing bytes which decrease the number of necessary buffer checks

        /// <summary>Encodes the specified input.</summary>
        /// <param name="input">The input.</param>
        /// <param name="inputOffset">The input offset.</param>
        /// <param name="inputLength">Length of the input.</param>
        /// <param name="output">The output.</param>
        /// <param name="outputOffset">The output offset.</param>
        /// <param name="outputLength">Length of the output.</param>
        /// <returns>Number of bytes in compressed buffer. Negative value means thet output buffer was too small.</returns>
        public static int Encode(
            byte[] input, int inputOffset, int inputLength,
            byte[] output, int outputOffset, int outputLength)
        {
            CheckArguments(
                input, inputOffset, ref inputLength,
                output, outputOffset, ref outputLength);

#if DOBOZ_SAFE
            var input_p = input;
            var output_p = output;
#else
            fixed (byte* input_p = input)
            fixed (byte* output_p = output)
#endif
            {
                int length;

                if (Compress(input_p, inputOffset, inputLength, output_p, outputOffset, outputLength, out length) == Result.RESULT_OK)
                    return length;

                // buffer is too small
                return -1;
            }
        }

        /// <summary>Encodes the specified input.</summary>
        /// <param name="input">The input.</param>
        /// <param name="inputOffset">The input offset.</param>
        /// <param name="inputLength">Length of the input.</param>
        /// <returns>Encoded buffer.</returns>
        public static byte[] Encode(
            byte[] input, int inputOffset, int inputLength)
        {
            CheckArguments(input, inputOffset, ref inputLength);

            var maxOutputSize = MaximumOutputLength(inputLength);

            var bufferLength = maxOutputSize;
            var buffer = new byte[bufferLength];

#if DOBOZ_SAFE
            var input_p = input;
            var output_p = buffer;
#else
            fixed (byte* input_p = input)
            fixed (byte* output_p = buffer)
#endif
            {
                int outputLength;

                if (Compress(input_p, inputOffset, inputLength, output_p, 0, bufferLength, out outputLength) != Result.RESULT_OK)
                    throw new InvalidOperationException("Compressed data has been corrupted");

                if (outputLength == bufferLength)
                    return buffer;

                var output = new byte[outputLength];
                var src = output_p;
#if DOBOZ_SAFE
                var dst = output;
                BlockCopy(src, 0, dst, 0, outputLength);
#else
                fixed (byte* dst = output)
                {
                    BlockCopy(src, dst, outputLength);
                }
#endif
                return output;
            }
        }

#if DOBOZ_SAFE
        static void Poke2(byte[] buffer, int offset, ushort value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }

        static void Poke4(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }
#endif

        // Store the source
#if DOBOZ_SAFE
        static Result Store(byte[] source, int sourceOffset, int sourceSize, byte[] destination, int destinationOffset, out int compressedSize)
#else
        static Result Store(byte* source, int sourceOffset, int sourceSize, byte* destination, int destinationOffset, out int compressedSize)
#endif
        {
#if DOBOZ_SAFE
            var src_0 = sourceOffset;
            var dst_0 = destinationOffset;
            var dst_p = dst_0;
#else
            var src_0 = source + sourceOffset;
            var dst_0 = destination + destinationOffset;
            var dst_p = dst_0;
#endif
            // Encode the header
            var maxCompressedSize = MaximumOutputLength(sourceSize);
            var headerSize = GetHeaderSize(maxCompressedSize);

            compressedSize = headerSize + sourceSize;

            var header = new Header
            {
                version = VERSION,
                isStored = true,
                uncompressedSize = sourceSize,
                compressedSize = compressedSize
            };

            EncodeHeader(ref header, maxCompressedSize, destination, destinationOffset);
            dst_p += headerSize;

            // Store the data
#if DOBOZ_SAFE
            BlockCopy(source, src_0, destination, dst_p, sourceSize);
#else
            BlockCopy(src_0, dst_p, sourceSize);
#endif

            return Result.RESULT_OK;
        }

#if DOBOZ_SAFE
        static void EncodeHeader(ref Header header, int maxCompressedSize, byte[] destination, int destinationOffset)
#else
        static void EncodeHeader(ref Header header, int maxCompressedSize, byte* destination, int destinationOffset)
#endif
        {
            Debug.Assert(header.version < 8);
#if DOBOZ_SAFE
            var dst_p = destinationOffset;
#else
            var dst_p = destination + destinationOffset;
#endif

            // Encode the attribute byte
            var attributes = header.version;

            var sizeCodedSize = GetSizeCodedSize(maxCompressedSize);
            attributes |= (sizeCodedSize - 1) << 3;

            if (header.isStored)
            {
                attributes |= 128;
            }

#if DOBOZ_SAFE
            destination[dst_p++] = (byte)attributes;
#else
            *dst_p++ = (byte)attributes;
#endif

            // Encode the uncompressed and compressed sizes
            switch (sizeCodedSize)
            {
#if DOBOZ_SAFE
                case 1:
                    destination[dst_p] = (byte)header.uncompressedSize;
                    destination[dst_p + sizeCodedSize] = (byte)header.compressedSize;
                    break;

                case 2:
                    Poke2(destination, dst_p, (ushort)header.uncompressedSize);
                    Poke2(destination, dst_p + sizeCodedSize, (ushort)header.compressedSize);
                    break;

                case 4:
                    Poke4(destination, dst_p, (uint)header.uncompressedSize);
                    Poke4(destination, dst_p + sizeCodedSize, (uint)header.compressedSize);
                    break;
#else
                case 1:
                    *dst_p = (byte)(header.uncompressedSize);
                    *(dst_p + sizeCodedSize) = (byte)(header.compressedSize);
                    break;

                case 2:
                    *((ushort*)(dst_p)) = (ushort)(header.uncompressedSize);
                    *((ushort*)(dst_p + sizeCodedSize)) = (ushort)(header.compressedSize);
                    break;

                case 4:
                    *((uint*)(dst_p)) = (uint)(header.uncompressedSize);
                    *((uint*)(dst_p + sizeCodedSize)) = (uint)(header.compressedSize);
                    break;
#endif
            }
        }

#if DOBOZ_SAFE
        static Result Compress(
           byte[] source, int sourceOffset, int sourceSize,
           byte[] destination, int destinationOffset, int destinationSize,
           out int compressedSize)
#else
        static Result Compress(
           byte* source, int sourceOffset, int sourceSize, byte* destination, int destinationOffset, int destinationSize, out int compressedSize)
#endif
        {
            Debug.Assert(source != null);
            Debug.Assert(destination != null);

            if (sourceSize == 0)
            {
                compressedSize = 0;
                return Result.RESULT_ERROR_BUFFER_TOO_SMALL;
            }

            var storedSize = MaximumOutputLength(sourceSize);
            var maxCompressedSize = destinationSize;

#if DOBOZ_SAFE
            var src_0 = sourceOffset;
            var dst_0 = destinationOffset;

            Debug.Assert(
                source != destination ||
                (src_0 + sourceSize <= dst_0 || dst_0 + destinationSize <= src_0),
                "The source and destination buffers must not overlap.");
#else
            var src_0 = source + sourceOffset;
            var dst_0 = destination + destinationOffset;

            Debug.Assert(
                src_0 + sourceSize <= dst_0 || dst_0 + destinationSize <= src_0,
                "The source and destination buffers must not overlap.");
#endif
            var dst_end = dst_0 + destinationSize;

            // Compute the maximum output end pointer
            // We use this to determine whether we should store the data instead of compressing it
            var maxOutputEnd = dst_0 + maxCompressedSize;

            // Allocate the header
            var dst_p = dst_0;
            dst_p += GetHeaderSize(maxCompressedSize);

            // Initialize the dictionary
            var dictionary = new Dictionary(source, sourceOffset, sourceSize);

            // Initialize the control word which contains the literal/match bits
            // The highest bit of a control word is a guard bit, which marks the end of the bit list
            // The guard bit simplifies and speeds up the decoding process, and it
            const int controlWordBitCount = WORD_SIZE * 8 - 1;
            const uint controlWordGuardBit = 1u << controlWordBitCount;
            var controlWord = controlWordGuardBit;
            var controlWordBit = 0;

            // Since we do not know the contents of the control words in advance, we allocate space for them and subsequently fill them with data as soon as we can
            // This is necessary because the decoder must encounter a control word *before* the literals and matches it refers to
            // We begin the compressed data with a control word
            var controlWordPointer = dst_p;
            dst_p += WORD_SIZE;

            // The match located at the current inputIterator position

            // The match located at the next inputIterator position
            // Initialize it to 'no match', because we are at the beginning of the inputIterator buffer
            // A match with a length of 0 means that there is no match
            var nextMatch = new Match { length = 0 };

            // The dictionary matching look-ahead is 1 character, so set the dictionary position to 1
            // We don't have to worry about getting matches beyond the inputIterator, because the dictionary ignores such requests
            dictionary.Skip();

            // At each position, we select the best match to encode from a list of match candidates provided by the match finder
            // var matchCandidates = new Match[MAX_MATCH_CANDIDATE_COUNT];

#if DOBOZ_SAFE
            var mc = new Match[MAX_MATCH_CANDIDATE_COUNT];
#else
            fixed (Match* mc = new Match[MAX_MATCH_CANDIDATE_COUNT])
#endif
            {
                // Iterate while there is still data left
                while (dictionary.Position - 1 < sourceSize)
                {
                    // Check whether the output is too large
                    // During each iteration, we may output up to 8 bytes (2 words), and the compressed stream ends with 4 dummy bytes
                    if (dst_p + 2 * WORD_SIZE + TRAILING_DUMMY_SIZE > maxOutputEnd)
                    {
                        compressedSize = 0;
                        return
                            storedSize <= destinationSize
                                ? Store(source, sourceOffset, sourceSize, destination, destinationOffset, out compressedSize)
                                : Result.RESULT_ERROR_BUFFER_TOO_SMALL;
                    }

                    // Check whether the control word must be flushed
                    if (controlWordBit == controlWordBitCount)
                    {
                        // Flush current control word
#if DOBOZ_SAFE
                        Poke4(destination, controlWordPointer, controlWord);
#else
                        *((uint*)(controlWordPointer)) = controlWord;
#endif

                        // New control word
                        controlWord = controlWordGuardBit;
                        controlWordBit = 0;

                        controlWordPointer = dst_p;
                        dst_p += WORD_SIZE;
                    }

                    // The current match is the previous 'next' match
                    var match = nextMatch;

                    // Find the best match at the next position
                    // The dictionary position is automatically incremented
                    var matchCandidateCount = dictionary.FindMatches(mc);
                    nextMatch = GetBestMatch(mc, matchCandidateCount);

                    // If we have a match, do not immediately use it, because we may miss an even better match (lazy evaluation)
                    // If encoding a literal and the next match has a higher compression ratio than encoding the current match, discard the current match
                    if (match.length > 0 && (1 + nextMatch.length) * GetMatchCodedSize(ref match) > match.length * (1 + GetMatchCodedSize(ref nextMatch)))
                    {
                        match.length = 0;
                    }

                    // Check whether we must encode a literal or a match
                    if (match.length == 0)
                    {
                        // Encode a literal (0 control word flag)
                        // In order to efficiently decode literals in runs, the literal bit (0) must differ from the guard bit (1)

                        // The current dictionary position is now two characters ahead of the literal to encode
                        Debug.Assert(dst_p + 1 <= dst_end);
#if DOBOZ_SAFE
                        destination[dst_p] = source[dictionary.Position - 2];
#else
                        *dst_p = source[dictionary.Position - 2];
#endif
                        ++dst_p;
                    }
                    else
                    {
                        // Encode a match (1 control word flag)
                        controlWord |= 1u << controlWordBit;

                        Debug.Assert(dst_p + WORD_SIZE <= dst_end);
#if DOBOZ_SAFE
                        dst_p += EncodeMatch(ref match, destination, dst_p);
#else
                        dst_p += EncodeMatch(ref match, dst_p);
#endif

                        // Skip the matched characters
                        for (var i = 0; i < match.length - 2; ++i)
                        {
                            dictionary.Skip();
                        }

                        matchCandidateCount = dictionary.FindMatches(mc);
                        nextMatch = GetBestMatch(mc, matchCandidateCount);
                    }

                    // Next control word bit
                    ++controlWordBit;
                }

                // Flush the control word
#if DOBOZ_SAFE
                Poke4(destination, controlWordPointer, controlWord);
#else
                *(uint*)controlWordPointer = controlWord;
#endif

                // Output trailing safety dummy bytes
                // This reduces the number of necessary buffer checks during decoding
                Debug.Assert(dst_p + TRAILING_DUMMY_SIZE <= dst_end);
#if DOBOZ_SAFE
                Poke4(destination, dst_p, 0);
#else
                *(uint*)dst_p = 0;
#endif
                dst_p += TRAILING_DUMMY_SIZE;

                // ReSharper disable RedundantCast
                // Done, compute the compressed size
                compressedSize = (int)(dst_p - dst_0);
                // ReSharper restore RedundantCast

                // Encode the header
                var header = new Header
                {
                    version = VERSION,
                    isStored = false,
                    uncompressedSize = sourceSize,
                    compressedSize = compressedSize
                };

                EncodeHeader(ref header, maxCompressedSize, destination, destinationOffset);

                // Return the compressed size
                return Result.RESULT_OK;
            }
        }

        // Selects the best match from a list of match candidates provided by the match finder
#if DOBOZ_SAFE
        static Match GetBestMatch(Match[] matchCandidates, int matchCandidateCount)
#else
        static Match GetBestMatch(Match* matchCandidates, int matchCandidateCount)
#endif
        {
            var bestMatch = new Match { length = 0 };

            // Select the longest match which can be coded efficiently (coded size is less than the length)
            for (var i = matchCandidateCount - 1; i >= 0; --i)
            {
                if (matchCandidates[i].length > GetMatchCodedSize(ref matchCandidates[i]))
                {
                    bestMatch = matchCandidates[i];
                    break;
                }
            }

            return bestMatch;
        }

        static int GetMatchCodedSize(ref Match match)
        {
#if DOBOZ_SAFE
            return EncodeMatch(ref match, null, 0);
#else
            return EncodeMatch(ref match, null);
#endif
        }

#if DOBOZ_SAFE
        static int EncodeMatch(ref Match match, byte[] destination, int destinationOffset)
#else
        static int EncodeMatch(ref Match match, byte* destination)
#endif
        {
            Debug.Assert(match.length <= MAX_MATCH_LENGTH);
            Debug.Assert(match.length == 0 || match.offset < DICTIONARY_SIZE);

            uint word;
            int size;

            var lengthCode = (uint)(match.length - MIN_MATCH_LENGTH);
            var offsetCode = (uint)(match.offset);

            if (lengthCode == 0 && offsetCode < 64)
            {
                word = offsetCode << 2; // 00
                size = 1;
            }
            else if (lengthCode == 0 && offsetCode < 16384)
            {
                word = (offsetCode << 2) | 1; // 01
                size = 2;
            }
            else if (lengthCode < 16 && offsetCode < 1024)
            {
                word = (offsetCode << 6) | (lengthCode << 2) | 2; // 10
                size = 2;
            }
            else if (lengthCode < 32 && offsetCode < 65536)
            {
                word = (offsetCode << 8) | (lengthCode << 3) | 3; // 11
                size = 3;
            }
            else
            {
                word = (offsetCode << 11) | (lengthCode << 3) | 7; // 111
                size = 4;
            }

            if (destination != null)
            {
                {
                    switch (size)
                    {
#if DOBOZ_SAFE
                        case 4:
                        case 3:
                            Poke4(destination, destinationOffset, word);
                            break;
                        case 2:
                            Poke2(destination, destinationOffset, (ushort)word);
                            break;
                        default:
                            destination[destinationOffset] = (byte)word;
                            break;
#else
                        case 4:
                        case 3:
                            *(uint*)(destination) = word;
                            break;
                        case 2:
                            *(ushort*)(destination) = (ushort)word;
                            break;
                        default:
                            *destination = (byte)word;
                            break;
#endif
                    }
                }
            }

            return size;
        }
    }
}
