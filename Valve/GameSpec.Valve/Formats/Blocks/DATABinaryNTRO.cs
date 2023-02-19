using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameSpec.Valve.Formats.Blocks
{
    public class DATABinaryNTRO : DATA
    {
        protected BinaryPak Parent { get; private set; }

        public IDictionary<string, object> Data { get; private set; }
        public string StructName { get; set; }

        public override void Read(BinaryPak parent, BinaryReader r)
        {
            Parent = parent;
            var refStruct = StructName != null
                ? parent.NTRO.ReferencedStructs.Find(s => s.Name == StructName)
                : parent.NTRO.ReferencedStructs.First();
            Data = ReadStructure(r, refStruct, Offset);
        }

        IDictionary<string, object> ReadStructure(BinaryReader r, NTRO.ResourceDiskStruct refStruct, long startingOffset)
        {
            var data = new Dictionary<string, object> { { "_name", refStruct.Name } };
            foreach (var field in refStruct.FieldIntrospection)
            {
                r.Seek(startingOffset + field.OnDiskOffset);
                ReadFieldIntrospection(r, field, ref data);
            }
            // Some structs are padded, so all the field sizes do not add up to the size on disk
            r.BaseStream.Position = startingOffset + refStruct.DiskSize;
            if (refStruct.BaseStructId != 0)
                r.Peek(z =>
                {
                    var newStruct = Parent.NTRO.ReferencedStructs.First(x => x.Id == refStruct.BaseStructId);
                    // Valve doesn't print this struct's type, so we can't just call ReadStructure *sigh*
                    foreach (var field in newStruct.FieldIntrospection)
                    {
                        z.BaseStream.Position = startingOffset + field.OnDiskOffset;
                        ReadFieldIntrospection(z, field, ref data);
                    }
                });
            return data;
        }

        void ReadFieldIntrospection(BinaryReader r, NTRO.ResourceDiskStruct.Field field, ref Dictionary<string, object> data)
        {
            var count = (uint)field.Count;
            if (count == 0)
                count = 1;
            var pointer = false;

            var prevOffset = 0L;
            if (field.Indirections.Count > 0)
            {
                if (field.Indirections.Count > 1) throw new NotImplementedException("More than one indirection, not yet handled.");
                if (field.Count > 0) throw new NotImplementedException("Indirection.Count > 0 && field.Count > 0");

                var indirection = field.Indirections[0];
                var offset = r.ReadUInt32();
                if (indirection == 0x03)
                {
                    pointer = true;
                    if (offset == 0) { data.Add(field.FieldName, MakeValue<byte?>(field.Type, null, true)); return; } // being byte shouldn't matter 
                    prevOffset = r.BaseStream.Position;
                    r.BaseStream.Position += offset - 4;
                }
                else if (indirection == 0x04)
                {
                    count = r.ReadUInt32();
                    prevOffset = r.BaseStream.Position;
                    if (count > 0) r.BaseStream.Position += offset - 8;
                }
                else throw new ArgumentOutOfRangeException(nameof(indirection), $"Unknown indirection. ({indirection})");
            }
            if (field.Count > 0 || field.Indirections.Count > 0)
            {
                var values = new object[(int)count];
                for (var i = 0; i < count; i++) values[i] = ReadField(r, field, pointer);
                data.Add(field.FieldName, values);
            }
            else for (var i = 0; i < count; i++) data.Add(field.FieldName, ReadField(r, field, pointer));
            if (prevOffset > 0) r.BaseStream.Position = prevOffset;
        }

        object ReadField(BinaryReader r, NTRO.ResourceDiskStruct.Field field, bool pointer)
        {
            switch (field.Type)
            {
                case NTRO.DataType.Struct:
                    {
                        var value = ReadStructure(r, Parent.NTRO.ReferencedStructs.First(x => x.Id == field.TypeData), r.BaseStream.Position);
                        return MakeValue<IDictionary<string, object>>(field.Type, value, pointer);
                    }
                case NTRO.DataType.Enum: return MakeValue<uint>(field.Type, r.ReadUInt32(), pointer);
                case NTRO.DataType.SByte: return MakeValue<sbyte>(field.Type, r.ReadSByte(), pointer);
                case NTRO.DataType.Byte: return MakeValue<byte>(field.Type, r.ReadByte(), pointer);
                case NTRO.DataType.Boolean: return MakeValue<bool>(field.Type, r.ReadByte() == 1 ? true : false, pointer);
                case NTRO.DataType.Int16: return MakeValue<short>(field.Type, r.ReadInt16(), pointer);
                case NTRO.DataType.UInt16: return MakeValue<ushort>(field.Type, r.ReadUInt16(), pointer);
                case NTRO.DataType.Int32: return MakeValue<int>(field.Type, r.ReadInt32(), pointer);
                case NTRO.DataType.UInt32: return MakeValue<uint>(field.Type, r.ReadUInt32(), pointer);
                case NTRO.DataType.Float: return MakeValue<float>(field.Type, r.ReadSingle(), pointer);
                case NTRO.DataType.Int64: return MakeValue<long>(field.Type, r.ReadInt64(), pointer);
                case NTRO.DataType.ExternalReference:
                    {
                        var id = r.ReadUInt64();
                        var value = id > 0 ? Parent.RERL?.RERLInfos.FirstOrDefault(c => c.Id == id)?.Name : null;
                        return MakeValue<string>(field.Type, value, pointer);
                    }
                case NTRO.DataType.UInt64: return MakeValue<ulong>(field.Type, r.ReadUInt64(), pointer);
                case NTRO.DataType.Vector: return MakeValue<Vector3>(field.Type, new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()), pointer);
                case NTRO.DataType.Quaternion: return MakeValue<Quaternion>(field.Type, new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()), pointer);
                case NTRO.DataType.Color:
                case NTRO.DataType.Fltx4:
                case NTRO.DataType.Vector4D:
                case NTRO.DataType.Vector4D_44: return MakeValue<Vector4>(field.Type, new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()), pointer);
                case NTRO.DataType.String4:
                case NTRO.DataType.String: return MakeValue<string>(field.Type, r.ReadO32UTF8(), pointer);
                case NTRO.DataType.Matrix2x4:
                    return MakeValue<Matrix4x4>(field.Type, new Matrix4x4(
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0, 0,
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0, 0,
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0, 0,
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0, 0), pointer);
                case NTRO.DataType.Matrix3x4:
                case NTRO.DataType.Matrix3x4a:
                    return MakeValue<Matrix4x4>(field.Type, new Matrix4x4(
                        r.ReadSingle(),
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0,
                        r.ReadSingle(),
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0,
                        r.ReadSingle(),
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0,
                        r.ReadSingle(),
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0), pointer);
                case NTRO.DataType.CTransform:
                    return MakeValue<Matrix4x4>(field.Type, new Matrix4x4(
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0, 0,
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0, 0,
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0, 0,
                        r.ReadSingle(),
                        r.ReadSingle(),
                        0, 0), pointer);
                default: throw new ArgumentOutOfRangeException(nameof(field.Type), $"Unknown data type: {field.Type} (name: {field.FieldName})");
            }
        }

        static object MakeValue<T>(NTRO.DataType type, object data, bool pointer) => data;

        public override string ToString() => Data?.ToString() ?? "None";
    }
}
