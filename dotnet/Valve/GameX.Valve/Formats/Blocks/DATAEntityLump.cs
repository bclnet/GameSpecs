using GameX.Algorithms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/EntityLump
    public partial class DATAEntityLump : DATABinaryKV3OrNTRO
    {
        public enum EntityFieldType : uint //was:Resource/Enums/EntityFieldType
        {
            Void = 0x0,
            Float = 0x1,
            String = 0x2,
            Vector = 0x3,
            Quaternion = 0x4,
            Integer = 0x5,
            Boolean = 0x6,
            Short = 0x7,
            Character = 0x8,
            Color32 = 0x9,
            Embedded = 0xa,
            Custom = 0xb,
            ClassPtr = 0xc,
            EHandle = 0xd,
            PositionVector = 0xe,
            Time = 0xf,
            Tick = 0x10,
            SoundName = 0x11,
            Input = 0x12,
            Function = 0x13,
            VMatrix = 0x14,
            VMatrixWorldspace = 0x15,
            Matrix3x4Worldspace = 0x16,
            Interval = 0x17,
            Unused = 0x18,
            Vector2d = 0x19,
            Integer64 = 0x1a,
            Vector4D = 0x1b,
            Resource = 0x1c,
            TypeUnknown = 0x1d,
            CString = 0x1e,
            HScript = 0x1f,
            Variant = 0x20,
            UInt64 = 0x21,
            Float64 = 0x22,
            PositiveIntegerOrNull = 0x23,
            HScriptNewInstance = 0x24,
            UInt = 0x25,
            UtlStringToken = 0x26,
            QAngle = 0x27,
            NetworkOriginCellQuantizedVector = 0x28,
            HMaterial = 0x29,
            HModel = 0x2a,
            NetworkQuantizedVector = 0x2b,
            NetworkQuantizedFloat = 0x2c,
            DirectionVectorWorldspace = 0x2d,
            QAngleWorldspace = 0x2e,
            QuaternionWorldspace = 0x2f,
            HScriptLightbinding = 0x30,
            V8_value = 0x31,
            V8_object = 0x32,
            V8_array = 0x33,
            V8_callback_info = 0x34,
            UtlString = 0x35,
            NetworkOriginCellQuantizedPositionVector = 0x36,
            HRenderTexture = 0x37,
        }

        public class Entity
        {
            public Dictionary<uint, EntityProperty> Properties { get; } = new Dictionary<uint, EntityProperty>();
            public List<IDictionary<string, object>> Connections { get; internal set; }
            public T Get<T>(string name) => Get<T>(StringToken.Get(name));
            public EntityProperty Get(string name) => Get(StringToken.Get(name));
            public T Get<T>(uint hash) => Properties.TryGetValue(hash, out var property) ? (T)property.Data : default;
            public EntityProperty Get(uint hash) => Properties.TryGetValue(hash, out var property) ? property : default;
        }

        public class EntityProperty
        {
            public EntityFieldType Type { get; set; }
            public string Name { get; set; }
            public object Data { get; set; }
        }

        public IEnumerable<string> GetChildEntityNames() => Data.Get<string[]>("m_childLumps");

        public IEnumerable<Entity> GetEntities() => Data.GetArray("m_entityKeyValues").Select(entity => ParseEntityProperties(entity.Get<byte[]>("m_keyValuesData"), entity.GetArray("m_connections"))).ToList();

        static Entity ParseEntityProperties(byte[] bytes, IDictionary<string, object>[] connections)
        {
            using var s = new MemoryStream(bytes);
            using var r = new BinaryReader(s);
            var a = r.ReadUInt32(); // always 1?
            if (a != 1) throw new NotImplementedException($"First field in entity lump is not 1");
            var hashedFieldsCount = r.ReadUInt32();
            var stringFieldsCount = r.ReadUInt32();
            var entity = new Entity();
            void ReadTypedValue(uint keyHash, string keyName)
            {
                var type = (EntityFieldType)r.ReadUInt32();
                var entityProperty = new EntityProperty
                {
                    Type = type,
                    Name = keyName,
                    Data = type switch
                    {
                        EntityFieldType.Boolean => r.ReadBoolean(),
                        EntityFieldType.Float => r.ReadSingle(),
                        EntityFieldType.Color32 => r.ReadBytes(4),
                        EntityFieldType.Integer => r.ReadInt32(),
                        EntityFieldType.UInt => r.ReadUInt32(),
                        EntityFieldType.Integer64 => r.ReadUInt64(),
                        var x when x == EntityFieldType.Vector || x == EntityFieldType.QAngle => new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                        EntityFieldType.CString => r.ReadZUTF8(), // null term variable
                        _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown type {type}"),
                    }
                };
                entity.Properties.Add(keyHash, entityProperty);
            }
            for (var i = 0; i < hashedFieldsCount; i++) ReadTypedValue(r.ReadUInt32(), null); // murmur2 hashed field name (see EntityLumpKeyLookup)
            for (var i = 0; i < stringFieldsCount; i++) ReadTypedValue(r.ReadUInt32(), r.ReadZUTF8());
            if (connections.Length > 0) entity.Connections = connections.ToList();
            return entity;
        }

        public override string ToString()
        {
            var knownKeys = StringToken.InvertedTable;
            var b = new StringBuilder();
            var unknownKeys = new Dictionary<uint, uint>();

            var index = 0;
            foreach (var entity in GetEntities())
            {
                b.AppendLine($"===={index++}====");
                foreach (var property in entity.Properties)
                {
                    var value = property.Value.Data;
                    if (value.GetType() == typeof(byte[]))
                    {
                        var tmp = value as byte[];
                        value = $"Array [{string.Join(", ", tmp.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToArray())}]";
                    }
                    string key;
                    if (knownKeys.TryGetValue(property.Key, out var knownKey)) key = knownKey;
                    else if (property.Value.Name != null) key = property.Value.Name;
                    else
                    {
                        key = $"key={property.Key}";
                        if (!unknownKeys.ContainsKey(property.Key)) unknownKeys.Add(property.Key, 1);
                        else unknownKeys[property.Key]++;
                    }
                    b.AppendLine($"{key,-30} {property.Value.Type.ToString(),-10} {value}");
                }

                if (entity.Connections != null)
                    foreach (var connection in entity.Connections)
                    {
                        b.Append('@'); b.Append(connection.Get<string>("m_outputName")); b.Append(' ');
                        var delay = connection.GetFloat("m_flDelay");
                        if (delay > 0) b.Append($"Delay={delay} ");
                        var timesToFire = connection.GetInt32("m_nTimesToFire");
                        if (timesToFire == 1) b.Append("OnlyOnce ");
                        else if (timesToFire != -1) throw new ArgumentOutOfRangeException(nameof(timesToFire), $"Unexpected times to fire {timesToFire}");
                        b.Append(connection.Get<string>("m_inputName")); b.Append(' '); b.Append(connection.Get<string>("m_targetName"));
                        var param = connection.Get<string>("m_overrideParam");
                        if (!string.IsNullOrEmpty(param) && param != "(null)") { b.Append(' '); b.Append(param); }
                        b.AppendLine();
                    }

                b.AppendLine();
            }

            if (unknownKeys.Count > 0)
            {
                b.AppendLine($"@@@@@ UNKNOWN KEY LOOKUPS:");
                b.AppendLine($"If you know what these are, add them to EntityLumpKnownKeys.cs");
                foreach (var unknownKey in unknownKeys) b.AppendLine($"key={unknownKey.Key} hits={unknownKey.Value}");
            }
            return b.ToString();
        }
    }
}
