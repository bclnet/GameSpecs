using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GameX.Algorithms
{
    public sealed class BerDecodeException : Exception
    {
        readonly int _position;
        public BerDecodeException(string message, int position) : base(message) => _position = position;
        public BerDecodeException(string message, int position, Exception ex) : base(message, ex) => _position = position;
        public override string Message => $"{base.Message} (Position {_position})";
    }

    public class AsnKeyParser
    {
        readonly AsnParser _parser;

        public AsnKeyParser(ICollection<byte> contents) => _parser = new AsnParser(contents);

        public static byte[] TrimLeadingZero(byte[] values)
        {
            byte[] r;
            if (0x00 == values[0] && values.Length > 1) { r = new byte[values.Length - 1]; Array.Copy(values, 1, r, 0, values.Length - 1); }
            else { r = new byte[values.Length]; Array.Copy(values, r, values.Length); }
            return r;
        }

        public static bool EqualOid(byte[] first, byte[] second)
        {
            if (first.Length != second.Length) return false;
            for (var i = 0; i < first.Length; i++) if (first[i] != second[i]) return false;
            return true;
        }

        public RSAParameters ParseRSAPublicKey()
        {
            var parameters = new RSAParameters();
            // Current value, Sanity Check, Checkpoint
            var position = _parser.CurrentPosition();
            // Ignore Sequence - PublicKeyInfo
            var length = _parser.NextSequence();
            if (length != _parser.RemainingBytes()) throw new BerDecodeException($"Incorrect Sequence Size. Specified: {length}, Remaining: {_parser.RemainingBytes()}", position);

            // Checkpoint
            position = _parser.CurrentPosition();

            // Ignore Sequence - AlgorithmIdentifier
            length = _parser.NextSequence();
            if (length > _parser.RemainingBytes()) throw new BerDecodeException($"Incorrect AlgorithmIdentifier Size. Specified: {length}, Remaining: {_parser.RemainingBytes()}", position);

            // Checkpoint
            position = _parser.CurrentPosition();
            // Grab the OID
            var value = _parser.NextOID();
            byte[] oid = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
            if (!EqualOid(value, oid)) throw new BerDecodeException("Expected OID 1.2.840.113549.1.1.1", position);

            // Optional Parameters
            if (_parser.IsNextNull()) _parser.NextNull(); // Also OK: value = _parser.Next();
            else _parser.Next(); // Gracefully skip the optional data

            // Checkpoint
            position = _parser.CurrentPosition();

            // Ignore BitString - PublicKey
            length = _parser.NextBitString();
            if (length > _parser.RemainingBytes()) throw new BerDecodeException($"Incorrect PublicKey Size. Specified: {length}, Remaining: {_parser.RemainingBytes()}", position);

            // Checkpoint
            position = _parser.CurrentPosition();

            // Ignore Sequence - RSAPublicKey
            length = _parser.NextSequence();
            if (length < _parser.RemainingBytes()) throw new BerDecodeException($"Incorrect RSAPublicKey Size. Specified: {length}, Remaining: {_parser.RemainingBytes()}", position);

            parameters.Modulus = TrimLeadingZero(_parser.NextInteger());
            parameters.Exponent = TrimLeadingZero(_parser.NextInteger());

            return parameters;
        }
    }

    public class AsnParser
    {
        readonly int _initialCount;
        readonly List<byte> _octets;

        public AsnParser(ICollection<byte> values)
        {
            _octets = new List<byte>(values.Count);
            _octets.AddRange(values);
            _initialCount = _octets.Count;
        }

        public int CurrentPosition()
            => _initialCount - _octets.Count;

        public int RemainingBytes()
            => _octets.Count;

        int GetLength()
        {
            var length = 0;
            var position = CurrentPosition(); // Checkpoint
            try
            {
                var b = GetNextOctet();
                if (b == (b & 0x7f)) return b;
                var i = b & 0x7f;
                if (i > 4) throw new BerDecodeException($"Invalid Length Encoding. Length uses {i} _octets", position);
                while (0 != i--) { length <<= 8; length |= GetNextOctet(); }  // shift left
            }
            catch (ArgumentOutOfRangeException ex) { throw new BerDecodeException("Error Parsing Key", position, ex); }
            return length;
        }

        public byte[] Next()
        {
            var position = CurrentPosition();
            try
            {
#pragma warning disable 168
#pragma warning disable 219
                var b = GetNextOctet();
#pragma warning restore 219
#pragma warning restore 168
                var length = GetLength();
                if (length > RemainingBytes()) throw new BerDecodeException($"Incorrect Size. Specified: {length}, Remaining: {RemainingBytes()}", position);
                return GetOctets(length);
            }
            catch (ArgumentOutOfRangeException ex) { throw new BerDecodeException("Error Parsing Key", position, ex); }
        }

        byte GetNextOctet()
        {
            var position = CurrentPosition();
            if (RemainingBytes() == 0) throw new BerDecodeException($"Incorrect Size. Specified: {1}, Remaining: {RemainingBytes()}", position);
            var b = GetOctets(1)[0];
            return b;
        }

        byte[] GetOctets(int octetCount)
        {
            var position = CurrentPosition();
            if (octetCount > RemainingBytes()) throw new BerDecodeException($"Incorrect Size. Specified: {octetCount}, Remaining: {RemainingBytes()}", position);
            var values = new byte[octetCount];
            try
            {
                _octets.CopyTo(0, values, 0, octetCount);
                _octets.RemoveRange(0, octetCount);
            }
            catch (ArgumentOutOfRangeException ex) { throw new BerDecodeException("Error Parsing Key", position, ex); }
            return values;
        }

        public bool IsNextNull()
            => _octets[0] == 0x05;

        public int NextNull()
        {
            var position = CurrentPosition();
            try
            {
                var b = GetNextOctet();
                if (b != 0) throw new BerDecodeException($"Expected Null. Specified Identifier: {b}", position);
                b = GetNextOctet(); // Next octet must be 0
                if (b != 0x00) throw new BerDecodeException($"Null has non-zero size. Size: {b}", position);
                return 0;
            }
            catch (ArgumentOutOfRangeException ex) { throw new BerDecodeException("Error Parsing Key", position, ex); }
        }

        public int NextSequence()
        {
            var position = CurrentPosition();
            try
            {
                var b = GetNextOctet();
                if (b != 0x30) throw new BerDecodeException($"Expected Sequence. Specified Identifier: {b}", position);
                var length = GetLength();
                if (length > RemainingBytes()) throw new BerDecodeException($"Incorrect Sequence Size.Specified: {length}, Remaining: {RemainingBytes()}", position);
                return length;
            }
            catch (ArgumentOutOfRangeException ex) { throw new BerDecodeException("Error Parsing Key", position, ex); }
        }

        public int NextBitString()
        {
            var position = CurrentPosition();
            try
            {
                var b = GetNextOctet();
                if (b != 0x03) throw new BerDecodeException($"Expected Bit String. Specified Identifier: {b}", position);
                var length = GetLength();
                b = _octets[0]; _octets.RemoveAt(0); length--; // We need to consume unused bits, which is the first octet of the remaing values
                if (b != 0x00) throw new BerDecodeException("The first octet of BitString must be 0", position);
                return length;
            }
            catch (ArgumentOutOfRangeException ex) { throw new BerDecodeException("Error Parsing Key", position, ex); }
        }

        public byte[] NextInteger()
        {
            var position = CurrentPosition();
            try
            {
                var b = GetNextOctet();
                if (b != 0x02) throw new BerDecodeException($"Expected Integer. Specified Identifier: {b}", position);
                var length = GetLength();
                if (length > RemainingBytes()) throw new BerDecodeException($"Incorrect Integer Size. Specified: {length}, Remaining: {RemainingBytes()}", position);
                return GetOctets(length);
            }
            catch (ArgumentOutOfRangeException ex) { throw new BerDecodeException("Error Parsing Key", position, ex); }
        }

        public byte[] NextOID()
        {
            var position = CurrentPosition();
            try
            {
                var b = GetNextOctet();
                if (b != 0x06) throw new BerDecodeException($"Expected Object Identifier. Specified Identifier: {b}", position);
                var length = GetLength();
                if (length > RemainingBytes()) throw new BerDecodeException($"Incorrect Object Identifier Size. Specified: {length}, Remaining: {RemainingBytes()}", position);
                var values = new byte[length];
                for (var i = 0; i < length; i++) { values[i] = _octets[0]; _octets.RemoveAt(0); }
                return values;
            }
            catch (ArgumentOutOfRangeException ex) { throw new BerDecodeException("Error Parsing Key", position, ex); }
        }
    }
}
