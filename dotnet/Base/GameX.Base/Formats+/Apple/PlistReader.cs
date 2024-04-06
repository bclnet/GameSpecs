using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace GameX.Formats.Apple
{
    /// <summary>
    /// Performs de-serialization of binary plists.
    /// </summary>
    public class PlistReader
    {
        public interface ISerializable
        {
            /// <summary>
            /// Populates this instance from the given plist <see cref="IDictionary"/> representation.
            /// Note that nested <see cref="IPlistSerializable"/> objects found in the graph during
            /// <see cref="ToPlistDictionary()"/> are represented as nested <see cref="IDictionary"/> instances here.
            /// </summary>
            /// <param name="plist">The plist <see cref="IDictionary"/> representation of this instance.</param>
            void FromDictionary(IDictionary plist);

            /// <summary>
            /// Gets a plist friendly <see cref="IDictionary"/> representation of this instance.
            /// The returned dictionary may contain nested implementations of <see cref="IPlistSerializable"/>.
            /// </summary>
            /// <returns>A plist friendly <see cref="IDictionary"/> representation of this instance.</returns>
            IDictionary ToDictionary();
        }

        /// <summary>
        /// Represents an item in a binary plist's object table.
        /// </summary>
        class PlistItem
        {
            /// <summary>
            /// Initializes a new instance of the BinaryPlistItem class.
            /// </summary>
            /// <param name="value">The value of the object the item represents.</param>
            public PlistItem(object value) => Value = value;

            /// <summary>
            /// Gets the item's byte value collection.
            /// </summary>
            public List<byte> ByteValue = new List<byte>();

            /// <summary>
            /// Gets or sets a value indicating whether this item represents an array.
            /// </summary>
            public bool IsArray;

            /// <summary>
            /// Gets or sets a value indicating whether this item represents a dictionary.
            /// </summary>
            public bool IsDictionary;

            /// <summary>
            /// Gets the item's marker value collection.
            /// </summary>
            public List<byte> Marker = new List<byte>();

            /// <summary>
            /// Gets the item's size, which is a sum of the <see cref="Marker"/> and <see cref="ByteValue"/> lengths.
            /// </summary>
            public int Size => Marker.Count + ByteValue.Count;

            /// <summary>
            /// Gets or sets the object value this item represents.
            /// </summary>
            public object Value;

            /// <summary>
            /// Sets the <see cref="ByteValue"/> to the given collection.
            /// </summary>
            /// <param name="buffer">The collection to set.</param>
            public void SetByteValue(IEnumerable<byte> buffer)
            {
                ByteValue.Clear();
                if (buffer != null) ByteValue.AddRange(buffer);
            }

            /// <summary>
            /// Gets the string representation of this instance.
            /// </summary>
            /// <returns>The string representation of this instance.</returns>
            public override string ToString() => Value != null ? Value.ToString() : "null";
        }

        /// <summary>
        /// Represents an array value in a binary plist.
        /// </summary>
        class PlistArray
        {
            /// <summary>
            /// Initializes a new instance of the BinaryPlistArray class.
            /// </summary>
            /// <param name="objectTable">A reference to the binary plist's object table.</param>
            public PlistArray(IList<PlistItem> objectTable) : this(objectTable, 0) { }

            /// <summary>
            /// Initializes a new instance of the BinaryPlistArray class.
            /// </summary>
            /// <param name="objectTable">A reference to the binary plist's object table.</param>
            /// <param name="size">The size of the array.</param>
            public PlistArray(IList<PlistItem> objectTable, int size)
            {
                ObjectReference = new List<int>(size);
                ObjectTable = objectTable;
            }

            /// <summary>
            /// Gets the array's object reference collection.
            /// </summary>
            public IList<int> ObjectReference;

            /// <summary>
            /// Gets a reference to the binary plist's object table.
            /// </summary>
            public IList<PlistItem> ObjectTable;

            /// <summary>
            /// Converts this instance into an <see cref="T:object[]"/> array.
            /// </summary>
            /// <returns>The <see cref="T:object[]"/> array representation of this instance.</returns>
            public object[] ToArray()
            {
                var array = new object[ObjectReference.Count];
                int objectRef;
                object objectValue;
                PlistArray innerArray;
                PlistDictionary innerDict;
                for (var i = 0; i < array.Length; i++)
                {
                    objectRef = ObjectReference[i];
                    if (objectRef >= 0 && objectRef < ObjectTable.Count && (ObjectTable[objectRef] == null || ObjectTable[objectRef].Value != this))
                    {
                        objectValue = ObjectTable[objectRef]?.Value;
                        innerDict = objectValue as PlistDictionary;
                        if (innerDict != null) objectValue = innerDict.ToDictionary();
                        else
                        {
                            innerArray = objectValue as PlistArray;
                            if (innerArray != null)
                                objectValue = innerArray.ToArray();
                        }
                        array[i] = objectValue;
                    }
                }
                return array;
            }

            /// <summary>
            /// Returns the string representation of this instance.
            /// </summary>
            /// <returns>This instance's string representation.</returns>
            public override string ToString()
            {
                var b = new StringBuilder("[");
                int objectRef;
                for (var i = 0; i < ObjectReference.Count; i++)
                {
                    if (i > 0) b.Append(",");
                    objectRef = ObjectReference[i];
                    if (ObjectTable.Count > objectRef && (ObjectTable[objectRef] == null || ObjectTable[objectRef].Value != this)) b.Append(ObjectReference[objectRef]);
                    else b.Append($"*{objectRef}");
                }
                return b.ToString() + "]";
            }
        }

        /// <summary>
        /// Represents a dictionary in a binary plist.
        /// </summary>
        class PlistDictionary
        {
            /// <summary>
            /// Initializes a new instance of the BinaryPlistDictionary class.
            /// </summary>
            /// <param name="objectTable">A reference to the binary plist's object table.</param>
            /// <param name="size">The size of the dictionary.</param>
            public PlistDictionary(IList<PlistItem> objectTable, int size)
            {
                KeyReference = new List<int>(size);
                ObjectReference = new List<int>(size);
                ObjectTable = objectTable;
            }

            /// <summary>
            /// Gets the dictionary's key reference collection.
            /// </summary>
            public IList<int> KeyReference;

            /// <summary>
            /// Gets the dictionary's object reference collection.
            /// </summary>
            public IList<int> ObjectReference;

            /// <summary>
            /// Gets a reference to the binary plist's object table.
            /// </summary>
            public IList<PlistItem> ObjectTable;

            /// <summary>
            /// Converts this instance into a <see cref="Dictionary{Object, Object}"/>.
            /// </summary>
            /// <returns>A <see cref="Dictionary{Object, Object}"/> representation this instance.</returns>
            public Dictionary<object, object> ToDictionary()
            {
                var dictionary = new Dictionary<object, object>();
                int keyRef, objectRef;
                object keyValue, objectValue;
                PlistArray innerArray;
                PlistDictionary innerDict;
                for (var i = 0; i < KeyReference.Count; i++)
                {
                    keyRef = KeyReference[i];
                    objectRef = ObjectReference[i];
                    if (keyRef >= 0 && keyRef < ObjectTable.Count && (ObjectTable[keyRef] == null || ObjectTable[keyRef].Value != this) &&
                        objectRef >= 0 && objectRef < ObjectTable.Count && (ObjectTable[objectRef] == null || ObjectTable[objectRef].Value != this))
                    {
                        keyValue = ObjectTable[keyRef]?.Value;
                        objectValue = ObjectTable[objectRef]?.Value;
                        innerDict = objectValue as PlistDictionary;
                        if (innerDict != null) objectValue = innerDict.ToDictionary();
                        else
                        {
                            innerArray = objectValue as PlistArray;
                            if (innerArray != null) objectValue = innerArray.ToArray();
                        }
                        dictionary[keyValue] = objectValue;
                    }
                }
                return dictionary;
            }

            /// <summary>
            /// Returns the string representation of this instance.
            /// </summary>
            /// <returns>This instance's string representation.</returns>
            public override string ToString()
            {
                var b = new StringBuilder("{");
                int keyRef, objectRef;
                for (var i = 0; i < KeyReference.Count; i++)
                {
                    if (i > 0) b.Append(",");

                    keyRef = KeyReference[i];
                    objectRef = ObjectReference[i];

                    if (keyRef < 0 || keyRef >= ObjectTable.Count) b.Append($"#{keyRef}");
                    else if (ObjectTable[keyRef] != null && ObjectTable[keyRef].Value == this) b.Append($"*{keyRef}");
                    else b.Append(ObjectTable[keyRef]);

                    b.Append(":");
                    if (objectRef < 0 || objectRef >= ObjectTable.Count) b.Append($"#{objectRef}");
                    else if (ObjectTable[objectRef] != null && ObjectTable[objectRef].Value == this) b.Append($"*{objectRef}");
                    else b.Append(ObjectTable[objectRef]);
                }
                return b.ToString() + "}";
            }
        }

        /// <summary>
        /// Gets the magic number value used in a binary plist header.
        /// </summary>
        const uint HeaderMagicNumber = 0x62706c69;

        /// <summary>
        /// Gets the version number value used in a binary plist header.
        /// </summary>
        const uint HeaderVersionNumber = 0x73743030;

        /// <summary>
        /// Gets Apple's reference date value.
        /// </summary>
        static readonly DateTime ReferenceDate = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        List<PlistItem> objectTable;
        List<int> offsetTable;
        int offsetIntSize, objectRefSize, objectCount, topLevelObjectOffset, offsetTableOffset;

        /// <summary>
        /// Reads a binary plist from the given file path into an <see cref="IDictionary"/>.
        /// </summary>
        /// <param name="path">The path of the file to read.</param>
        /// <returns>The result plist <see cref="IDictionary"/>.</returns>
        public IDictionary ReadObject(string path)
        {
            using var s = File.OpenRead(path);
            return ReadObject(s);
        }

        /// <summary>
        /// Reads a binary plist from the given stream into an <see cref="IDictionary"/>.
        /// </summary>
        /// <param name="s">The <see cref="Stream"/> to read.</param>
        /// <returns>The result plist <see cref="IDictionary"/>.</returns>
        public IDictionary ReadObject(Stream s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s), "stream cannot be null.");
            if (!s.CanRead) throw new ArgumentException("The stream must be readable.", nameof(s));

            var concreteStream = s;
            var disposeConcreteStream = false;

            if (!s.CanSeek)
            {
                concreteStream = new MemoryStream();
                var buffer = new byte[4096];
                int count;
                while (0 < (count = s.Read(buffer, 0, buffer.Length)))
                    concreteStream.Write(buffer, 0, count);
                concreteStream.Position = 0;
                disposeConcreteStream = true;
            }

            try
            {
                Dictionary<object, object> dictionary = null;
                Reset();

                // Header + trailer = 40.
                if (s.Length > 40)
                {
                    using var r = new BinaryReader(concreteStream);
                    // Read the header.
                    s.Position = 0;
                    var bpli = r.ReadInt32E();
                    var version = r.ReadInt32E();

                    if (bpli != HeaderMagicNumber || version != HeaderVersionNumber)
                        throw new ArgumentException("The stream data does not start with required 'bplist00' header.", nameof(s));

                    // Read the trailer.
                    // The first six bytes of the first eight-byte block are unused, so offset by 26 instead of 32.
                    s.Position = s.Length - 26;
                    offsetIntSize = (int)r.ReadByte();
                    objectRefSize = (int)r.ReadByte();
                    objectCount = (int)r.ReadInt64E();
                    topLevelObjectOffset = (int)r.ReadInt64E();
                    offsetTableOffset = (int)r.ReadInt64E();
                    var offsetTableSize = offsetIntSize * objectCount;

                    // Ensure our sanity.
                    //if (offsetIntSize < 1
                    //    || offsetIntSize > 8
                    //    || objectRefSize < 1
                    //    || objectRefSize > 8
                    //    || offsetTableOffset < 8
                    //    //|| topLevelObjectOffset >= objectCount
                    //    || offsetTableSize + offsetTableOffset + 32 > s.Length)
                    //    throw new ArgumentException("The stream data contains an invalid trailer.", nameof(s));

                    // Read the offset table and then the object table.
                    ReadOffsetTable(r);
                    ReadObjectTable(r);
                }
                else throw new ArgumentException("The stream is too short to be a valid binary plist.", nameof(s));

                if (objectTable[^1].Value is PlistDictionary root) dictionary = root.ToDictionary();
                else throw new InvalidOperationException($"Unsupported root plist object: {objectTable[^1].GetType()}. A dictionary must be the root plist object.");
                return dictionary ?? new Dictionary<object, object>();
            }
            finally
            {
                if (disposeConcreteStream && concreteStream != null) concreteStream.Dispose();
            }
        }

        /// <summary>
        /// Reads a binary plist from the given file path into a new <see cref="IPlistSerializable"/> object instance.
        /// </summary>
        /// <typeparam name="T">The concrete <see cref="IPlistSerializable"/> type to create.</typeparam>
        /// <param name="path">The path of the file to read.</param>
        /// <returns>The result <see cref="IPlistSerializable"/> object instance.</returns>
        public T ReadObject<T>(string path) where T : ISerializable, new()
        {
            using var s = File.OpenRead(path);
            return ReadObject<T>(path);
        }

        /// <summary>
        /// Reads a binary plist from the given stream into a new <see cref="IPlistSerializable"/> object instance.
        /// </summary>
        /// <typeparam name="T">The concrete <see cref="IPlistSerializable"/> type to create.</typeparam>
        /// <param name="s">The <see cref="Stream"/> to read.</param>
        /// <returns>The result <see cref="IPlistSerializable"/> object instance.</returns>
        public T ReadObject<T>(Stream s) where T : ISerializable, new()
        {
            var obj = new T();
            obj.FromDictionary(ReadObject(s));
            return obj;
        }

        /// <summary>
        /// Reads an ASCII string value from the given reader, starting at the given index and of the given size.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> to read the ASCII string value from.</param>
        /// <param name="index">The index in the stream the string value starts at.</param>
        /// <param name="size">The number of bytes that make up the string value.</param>
        /// <returns>A string value.</returns>
        static string ReadAsciiString(BinaryReader reader, long index, int size)
        {
            var buffer = ReadData(reader, index, size);
            return buffer.Length > 0 ? Encoding.ASCII.GetString(buffer) : string.Empty;
        }

        /// <summary>
        /// Reads a data value from the given reader, starting at the given index and of the given size.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> to read the data value from.</param>
        /// <param name="index">The index in the stream the data value starts at.</param>
        /// <param name="size">The number of bytes that make up the data value.</param>
        /// <returns>A data value.</returns>
        static byte[] ReadData(BinaryReader r, long index, int size)
        {
            r.BaseStream.Position = index;
            var buffer = new byte[size];
            int bufferIndex = 0, count;
            while (0 < (count = r.Read(buffer, bufferIndex, buffer.Length - bufferIndex)))
                bufferIndex += count;
            return buffer;
        }

        /// <summary>
        /// Reads a date value from the given reader, starting at the given index and of the given size.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> to read the date value from.</param>
        /// <param name="index">The index in the stream the date value starts at.</param>
        /// <param name="size">The number of bytes that make up the date value.</param>
        /// <returns>A date value.</returns>
        static DateTime ReadDate(BinaryReader r, long index, int size)
            => ReferenceDate.AddSeconds(ReadReal(r, index, size)).ToLocalTime();

        /// <summary>
        /// Reads an integer value from the given reader, starting at the given index and of the given size.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> to read the integer value from.</param>
        /// <param name="index">The index in the stream the integer value starts at.</param>
        /// <param name="size">The number of bytes that make up the integer value.</param>
        /// <returns>An integer value.</returns>
        static long ReadInteger(BinaryReader r, long index, int size)
        {
            var buffer = ReadData(r, index, size);
            if (buffer.Length > 1 && BitConverter.IsLittleEndian) Array.Reverse(buffer);
            return size switch
            {
                1 => (long)buffer[0],
                2 => (long)BitConverter.ToUInt16(buffer, 0),
                4 => (long)BitConverter.ToUInt32(buffer, 0),
                8 => (long)BitConverter.ToUInt64(buffer, 0),
                _ => throw new InvalidOperationException($"Unsupported variable-length integer size: {size}"),
            };
        }

        /// <summary>
        /// Reads a primitive (true, false or null) value from the given reader, starting at the given index.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> to read the primitive value from.</param>
        /// <param name="index">The index in the stream the value starts at.</param>
        /// <param name="primitive">Contains the read primitive value upon completion.</param>
        /// <returns>True if a value was read, false if the value was a fill byte.</returns>
        static bool ReadPrimitive(BinaryReader r, long index, out bool? primitive)
        {
            r.BaseStream.Position = index;
            var value = r.ReadByte();
            switch (value & 0xf)
            {
                case 0: primitive = null; return true;
                case 8: primitive = false; return true;
                case 9: primitive = true; return true;
                case 15: primitive = null; return false; // This is a fill byte.
                default: throw new InvalidOperationException($"Illegal primitive: {Convert.ToString(value, 2)}");
            }
        }

        /// <summary>
        /// Reads a floating-point value from the given reader, starting at the given index and of the given size.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> to read the floating-point value from.</param>
        /// <param name="index">The index in the stream the floating-point value starts at.</param>
        /// <param name="size">The number of bytes that make up the floating-point value.</param>
        /// <returns>A floating-point value.</returns>
        static double ReadReal(BinaryReader reader, long index, int size)
        {
            var buffer = ReadData(reader, index, size);
            if (BitConverter.IsLittleEndian) Array.Reverse(buffer);
            return size switch
            {
                4 => (double)BitConverter.ToSingle(buffer, 0),
                8 => BitConverter.ToDouble(buffer, 0),
                _ => throw new InvalidOperationException($"Unsupported floating point number size: {size}"),
            };
        }

        /// <summary>
        /// Reads a Unicode string value from the given reader, starting at the given index and of the given size.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> to read the Unicode string value from.</param>
        /// <param name="index">The index in the stream the string value starts at.</param>
        /// <param name="size">The number of characters that make up the string value.</param>
        /// <returns>A string value.</returns>
        static string ReadUnicodeString(BinaryReader r, long index, int size)
        {
            r.BaseStream.Position = index;
            size *= 2;
            var buffer = new byte[size];
            byte one, two;
            for (var i = 0; i < size; i++)
            {
                one = r.ReadByte(); two = r.ReadByte();
                if (BitConverter.IsLittleEndian) { buffer[i++] = two; buffer[i] = one; }
                else { buffer[i++] = one; buffer[i] = two; }
            }
            return Encoding.Unicode.GetString(buffer);
        }

        /// <summary>
        /// Reads a unique ID value from the given reader, starting at the given index and of the given size.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> to read the unique ID value from.</param>
        /// <param name="index">The index in the stream the unique ID value starts at.</param>
        /// <param name="size">The number of bytes that make up the unique ID value.</param>
        /// <returns>A unique ID value.</returns>
        static IDictionary ReadUniqueId(BinaryReader r, long index, int size)
            // Unique IDs in XML plists are <dict><key>CF$UID</key><integer>value</integer></dict>.
            // They're used by Cocoa's key-value coder. 
            => new Dictionary<string, ulong>
            {
                ["CF$UID"] = (ulong)ReadInteger(r, index, size)
            };

        /// <summary>
        /// Reads an array value from the given reader, starting at the given index and of the given size.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> to read the array value from.</param>
        /// <param name="index">The index in the stream the array value starts at.</param>
        /// <param name="size">The number of items in the array.</param>
        /// <returns>An array value.</returns>
        PlistArray ReadArray(BinaryReader r, long index, int size)
        {
            var array = new PlistArray(objectTable, size);
            for (var i = 0; i < size; i++)
                array.ObjectReference.Add((int)ReadInteger(r, index + (i * objectRefSize), objectRefSize));
            return array;
        }

        /// <summary>
        /// Reads a dictionary value from the given reader, starting at the given index and of the given size.
        /// </summary>
        /// <param name="r">The <see cref="BinaryReader"/> to read the dictionary value from.</param>
        /// <param name="index">The index in the stream the dictionary value starts at.</param>
        /// <param name="size">The number of items in the dictionary.</param>
        /// <returns>A dictionary value.</returns>
        PlistDictionary ReadDictionary(BinaryReader r, long index, int size)
        {
            PlistDictionary dictionary = new PlistDictionary(objectTable, size);
            var skip = size * objectRefSize;
            for (var i = 0; i < size; i++)
            {
                dictionary.KeyReference.Add((int)ReadInteger(r, index + (i * objectRefSize), objectRefSize));
                dictionary.ObjectReference.Add((int)ReadInteger(r, skip + index + (i * objectRefSize), objectRefSize));
            }
            return dictionary;
        }

        /// <summary>
        /// Reads the object table from the given reader.
        /// </summary>
        /// <param name="r">The reader to read the object table from.</param>
        void ReadObjectTable(BinaryReader r)
        {
            byte marker;
            int size, intSize;
            long parsedInt;
            for (var i = 0; i < objectCount; i++)
            {
                r.BaseStream.Position = offsetTable[i];
                marker = r.ReadByte();
                // The first half of the byte is the base marker.
                switch ((marker & 0xf0) >> 4)
                {
                    case 0:
                        if (ReadPrimitive(r, r.BaseStream.Position - 1, out var primitive))
                            objectTable.Add(new PlistItem(primitive));
                        break;
                    case 1:
                        size = 1 << (marker & 0xf);
                        parsedInt = ReadInteger(r, r.BaseStream.Position, size);
                        if (size < 4) objectTable.Add(new PlistItem((short)parsedInt));
                        else if (size < 8) objectTable.Add(new PlistItem((int)parsedInt));
                        else objectTable.Add(new PlistItem(parsedInt));
                        break;
                    case 2:
                        size = 1 << (marker & 0xf);
                        objectTable.Add(new PlistItem(ReadReal(r, r.BaseStream.Position, size)));
                        break;
                    case 3:
                        size = marker & 0xf;
                        if (size == 3)
                            objectTable.Add(new PlistItem(ReadDate(r, r.BaseStream.Position, 8)));
                        else throw new InvalidOperationException($"Unsupported date size: {Convert.ToString(size, 2)}");
                        break;
                    case 4:
                        size = marker & 0xf;
                        if (size == 15)
                        {
                            intSize = 1 << (r.ReadByte() & 0xf);
                            size = (int)ReadInteger(r, r.BaseStream.Position, intSize);
                        }
                        objectTable.Add(new PlistItem(ReadData(r, r.BaseStream.Position, size)));
                        break;
                    case 5:
                        size = marker & 0xf;
                        if (size == 15)
                        {
                            intSize = 1 << (r.ReadByte() & 0xf);
                            size = (int)ReadInteger(r, r.BaseStream.Position, intSize);
                        }
                        objectTable.Add(new PlistItem(ReadAsciiString(r, r.BaseStream.Position, size)));
                        break;
                    case 6:
                        size = marker & 0xf;
                        if (size == 15)
                        {
                            intSize = 1 << (r.ReadByte() & 0xf);
                            size = (int)ReadInteger(r, r.BaseStream.Position, intSize);
                        }
                        objectTable.Add(new PlistItem(ReadUnicodeString(r, r.BaseStream.Position, size)));
                        break;
                    case 8:
                        size = (marker & 0xf) + 1;
                        objectTable.Add(new PlistItem(ReadUniqueId(r, r.BaseStream.Position, size)));
                        break;
                    case 10:
                    case 12:
                        size = marker & 0xf;
                        if (size == 15)
                        {
                            intSize = 1 << (r.ReadByte() & 0xf);
                            size = (int)ReadInteger(r, r.BaseStream.Position, intSize);
                        }
                        objectTable.Add(new PlistItem(ReadArray(r, r.BaseStream.Position, size)) { IsArray = true });
                        break;
                    case 13:
                        size = marker & 0xf;
                        if (size == 15)
                        {
                            intSize = 1 << (r.ReadByte() & 0xf);
                            size = (int)ReadInteger(r, r.BaseStream.Position, intSize);
                        }
                        objectTable.Add(new PlistItem(ReadDictionary(r, r.BaseStream.Position, size)) { IsDictionary = true });
                        break;
                    default: throw new InvalidOperationException($"An invalid marker was found while reading the object table: {Convert.ToString(marker, 2)}");
                }
            }
        }

        /// <summary>
        /// Reads the offset table from the given reader.
        /// </summary>
        /// <param name="reader">The reader to read the offset table from.</param>
        void ReadOffsetTable(BinaryReader reader)
        {
            for (var i = 0; i < objectCount; i++)
                offsetTable.Add((int)ReadInteger(reader, offsetTableOffset + (i * offsetIntSize), offsetIntSize));
        }

        /// <summary>
        /// Resets this instance's state.
        /// </summary>
        void Reset()
        {
            objectRefSize = objectCount = offsetIntSize = offsetTableOffset = topLevelObjectOffset = 0;
            objectTable = new List<PlistItem>();
            offsetTable = new List<int>();
        }
    }
}
