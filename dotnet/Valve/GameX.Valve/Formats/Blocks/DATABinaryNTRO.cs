using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/NTRO
    public class DATABinaryNTRO : DATA
    {
        protected Binary_Pak Parent { get; private set; }
        public IDictionary<string, object> Data { get; private set; }
        public string StructName { get; set; }

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            Parent = parent;
            Data = ReadStructure(r, StructName != null
                ? parent.NTRO.ReferencedStructs.Find(s => s.Name == StructName)
                : parent.NTRO.ReferencedStructs.First(), Offset);
        }

        IDictionary<string, object> ReadStructure(BinaryReader r, NTRO.ResourceDiskStruct refStruct, long startingOffset)
        {
            var structEntry = new Dictionary<string, object> {
                { "_name", refStruct.Name }
            };
            foreach (var field in refStruct.FieldIntrospection)
            {
                r.Seek(startingOffset + field.OnDiskOffset);
                ReadFieldIntrospection(r, field, ref structEntry);
            }

            // Some structs are padded, so all the field sizes do not add up to the size on disk
            r.Seek(startingOffset + refStruct.DiskSize);
            if (refStruct.BaseStructId != 0)
                r.Peek(z =>
                {
                    var newStruct = Parent.NTRO.ReferencedStructs.First(x => x.Id == refStruct.BaseStructId);
                    // Valve doesn't print this struct's type, so we can't just call ReadStructure *sigh*
                    foreach (var field in newStruct.FieldIntrospection)
                    {
                        z.Seek(startingOffset + field.OnDiskOffset);
                        ReadFieldIntrospection(z, field, ref structEntry);
                    }
                });
            return structEntry;
        }

        void ReadFieldIntrospection(BinaryReader r, NTRO.ResourceDiskStruct.Field field, ref Dictionary<string, object> structEntry)
        {
            var count = (uint)field.Count;
            if (count == 0) count = 1;
            var pointer = false;
            var prevOffset = 0L;

            if (field.Indirections.Count > 0)
            {
                if (field.Indirections.Count > 1) throw new NotImplementedException("More than one indirection, not yet handled.");
                if (field.Count > 0) throw new NotImplementedException("Indirection.Count > 0 && field.Count > 0");

                var indirection = (NTRO.SchemaIndirectionType)field.Indirections[0];
                var offset = r.ReadUInt32();
                if (indirection == NTRO.SchemaIndirectionType.ResourcePointer)
                {
                    pointer = true;
                    if (offset == 0)
                    {
                        structEntry.Add(field.FieldName, MakeValue<byte?>(field.Type, null, true)); // being byte shouldn't matter 
                        return;
                    }
                    prevOffset = r.Tell();
                    r.Skip(offset - 4);
                }
                else if (indirection == NTRO.SchemaIndirectionType.ResourceArray)
                {
                    count = r.ReadUInt32();
                    prevOffset = r.Tell();
                    if (count > 0) r.Skip(offset - 8);
                }
                else throw new ArgumentOutOfRangeException(nameof(indirection), $"Unsupported indirection {indirection}");
            }
            if (field.Count > 0 || field.Indirections.Count > 0)
            {
                //if (field.Type == NTRO.DataType.Byte) { }
                var values = new object[(int)count];
                for (var i = 0; i < count; i++) values[i] = ReadField(r, field, pointer);
                structEntry.Add(field.FieldName, values);
            }
            else for (var i = 0; i < count; i++) structEntry.Add(field.FieldName, ReadField(r, field, pointer));
            if (prevOffset > 0) r.Seek(prevOffset);
        }

        object ReadField(BinaryReader r, NTRO.ResourceDiskStruct.Field field, bool pointer)
        {
            switch (field.Type)
            {
                case NTRO.SchemaFieldType.Struct:
                    {
                        var newStruct = Parent.NTRO.ReferencedStructs.First(x => x.Id == field.TypeData);
                        return MakeValue<IDictionary<string, object>>(field.Type, ReadStructure(r, newStruct, r.BaseStream.Position), pointer);
                    }
                case NTRO.SchemaFieldType.Enum: return MakeValue<uint>(field.Type, r.ReadUInt32(), pointer);
                case NTRO.SchemaFieldType.SByte: return MakeValue<sbyte>(field.Type, r.ReadSByte(), pointer);
                case NTRO.SchemaFieldType.Byte: return MakeValue<byte>(field.Type, r.ReadByte(), pointer);
                case NTRO.SchemaFieldType.Boolean: return MakeValue<bool>(field.Type, r.ReadByte() == 1 ? true : false, pointer);
                case NTRO.SchemaFieldType.Int16: return MakeValue<short>(field.Type, r.ReadInt16(), pointer);
                case NTRO.SchemaFieldType.UInt16: return MakeValue<ushort>(field.Type, r.ReadUInt16(), pointer);
                case NTRO.SchemaFieldType.Int32: return MakeValue<int>(field.Type, r.ReadInt32(), pointer);
                case NTRO.SchemaFieldType.UInt32: return MakeValue<uint>(field.Type, r.ReadUInt32(), pointer);
                case NTRO.SchemaFieldType.Float: return MakeValue<float>(field.Type, r.ReadSingle(), pointer);
                case NTRO.SchemaFieldType.Int64: return MakeValue<long>(field.Type, r.ReadInt64(), pointer);
                case NTRO.SchemaFieldType.ExternalReference:
                    {
                        var id = r.ReadUInt64();
                        var value = id > 0 ? Parent.RERL?.RERLInfos.FirstOrDefault(c => c.Id == id)?.Name : null;
                        return MakeValue<string>(field.Type, value, pointer);
                    }
                case NTRO.SchemaFieldType.UInt64: return MakeValue<ulong>(field.Type, r.ReadUInt64(), pointer);
                case NTRO.SchemaFieldType.Vector3D: return MakeValue<Vector3>(field.Type, new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()), pointer);
                case NTRO.SchemaFieldType.Quaternion: return MakeValue<Quaternion>(field.Type, new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()), pointer);
                case NTRO.SchemaFieldType.Color: return MakeValue<Vector4<byte>>(field.Type, new Vector4<byte>(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte()), pointer);
                case NTRO.SchemaFieldType.Fltx4:
                case NTRO.SchemaFieldType.Vector4D:
                case NTRO.SchemaFieldType.FourVectors: return MakeValue<Vector4>(field.Type, new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()), pointer);
                case NTRO.SchemaFieldType.Char:
                case NTRO.SchemaFieldType.ResourceString: return MakeValue<string>(field.Type, r.ReadO32UTF8(), pointer);
                case NTRO.SchemaFieldType.Vector2D: return MakeValue<float[]>(field.Type, new[] { r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle() }, pointer);
                case NTRO.SchemaFieldType.Matrix3x4:
                case NTRO.SchemaFieldType.Matrix3x4a:
                    return MakeValue<Matrix4x4>(field.Type, new Matrix4x4(
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0,
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0,
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0,
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0), pointer);
                case NTRO.SchemaFieldType.Transform:
                    return MakeValue<Matrix3x3>(field.Type, new Matrix4x4(
                        r.ReadSingle(), r.ReadSingle(), 0, 0,
                        r.ReadSingle(), r.ReadSingle(), 0, 0,
                        r.ReadSingle(), r.ReadSingle(), 0, 0,
                        r.ReadSingle(), r.ReadSingle(), 0, 0), pointer);
                default: throw new ArgumentOutOfRangeException(nameof(field.Type), $"Unknown data type: {field.Type} (name: {field.FieldName})");
            }
        }

        static object MakeValue<T>(NTRO.SchemaFieldType type, object data, bool pointer) => data;

        public override string ToString() => Data?.ToString() ?? "None";
    }
}
