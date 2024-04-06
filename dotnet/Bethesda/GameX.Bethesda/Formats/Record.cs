using GameX.Formats;
using System;
using System.IO;
using System.Linq;
using static OpenStack.Debug;

namespace GameX.Bethesda.Formats
{
    public class FieldHeader
    {
        public override string ToString() => Type;
        public string Type; // 4 bytes
        public int DataSize;

        public FieldHeader(BinaryReader r, BethesdaFormat format)
        {
            Type = r.ReadFString(4);
            DataSize = (int)(format == BethesdaFormat.TES3 ? r.ReadUInt32() : r.ReadUInt16());
        }
    }

    public abstract class Record : IRecord
    {
        internal Header Header;
        public uint Id => Header.FormId;

        /// <summary>
        /// Return an uninitialized subrecord to deserialize, or null to skip.
        /// </summary>
        /// <returns>Return an uninitialized subrecord to deserialize, or null to skip.</returns>
        public abstract bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize);

        public void Read(BinaryReader r, string filePath, BethesdaFormat format)
        {
            var startPosition = r.Tell();
            var endPosition = startPosition + Header.DataSize;
            while (r.BaseStream.Position < endPosition)
            {
                var fieldHeader = new FieldHeader(r, format);
                if (fieldHeader.Type == "XXXX")
                {
                    if (fieldHeader.DataSize != 4)
                        throw new InvalidOperationException();
                    fieldHeader.DataSize = (int)r.ReadUInt32();
                    continue;
                }
                else if (fieldHeader.Type == "OFST" && Header.Type == "WRLD")
                {
                    r.Seek(endPosition);
                    continue;
                }
                var position = r.BaseStream.Position;
                if (!CreateField(r, format, fieldHeader.Type, fieldHeader.DataSize))
                {
                    Log($"Unsupported ESM record type: {Header.Type}:{fieldHeader.Type}");
                    r.Skip(fieldHeader.DataSize);
                    continue;
                }
                // check full read
                if (r.BaseStream.Position != position + fieldHeader.DataSize)
                    throw new FormatException($"Failed reading {Header.Type}:{fieldHeader.Type} field data at offset {position} in {filePath} of {r.BaseStream.Position - position - fieldHeader.DataSize}");
            }
            // check full read
            if (r.Tell() != endPosition)
                throw new FormatException($"Failed reading {Header.Type} record data at offset {startPosition} in {filePath}");
        }
    }
}
