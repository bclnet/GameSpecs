using GameSpec.Algorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace GameSpec.Valve.Formats.Blocks
{
    public partial class DATAEntityLump : DATABinaryKV3OrNTRO
    {
        public static class EntityLumpKeyLookup
        {
            public const uint MURMUR2SEED = 0x31415926;
            static Dictionary<string, uint> Lookup = new Dictionary<string, uint>();
            public static uint Get(string key)
            {
                if (Lookup.ContainsKey(key)) return Lookup[key];
                var hash = MurmurHash2.Hash(key, MURMUR2SEED);
                Lookup[key] = hash;
                return hash;
            }
        }

        public class Entity
        {
            public Dictionary<uint, EntityProperty> Properties { get; } = new Dictionary<uint, EntityProperty>();
            public T Get<T>(string name) => Get<T>(EntityLumpKeyLookup.Get(name));
            public EntityProperty Get(string name) => Get(EntityLumpKeyLookup.Get(name));
            public T Get<T>(uint hash) => Properties.TryGetValue(hash, out var property) ? (T)property.Data : default;
            public EntityProperty Get(uint hash) => Properties.TryGetValue(hash, out var property) ? property : default;
        }

        public class EntityProperty
        {
            public uint Type { get; set; }
            public string Name { get; set; }
            public object Data { get; set; }
        }

        public IEnumerable<string> GetChildEntityNames() => Data.Get<string[]>("m_childLumps");

        public IEnumerable<Entity> GetEntities()
            => Data.GetArray("m_entityKeyValues")
                .Select(entity => ParseEntityProperties(entity.Get<byte[]>("m_keyValuesData")))
                .ToList();

        static Entity ParseEntityProperties(byte[] bytes)
        {
            using (var s = new MemoryStream(bytes))
            using (var r = new BinaryReader(s))
            {
                var a = r.ReadUInt32(); // always 1?
                if (a != 1) throw new NotImplementedException($"First field in entity lump is not 1");
                var hashedFieldsCount = r.ReadUInt32();
                var stringFieldsCount = r.ReadUInt32();
                var entity = new Entity();
                void ReadTypedValue(uint keyHash, string keyName)
                {
                    var type = r.ReadUInt32();
                    var entityProperty = new EntityProperty { Type = type, Name = keyName };
                    switch (type)
                    {
                        case 0x06: entityProperty.Data = r.ReadBoolean(); break; // 1:boolean
                        case 0x01: entityProperty.Data = r.ReadSingle(); break; // 4:float
                        case 0x09: entityProperty.Data = r.ReadBytes(4); break; // 4:color255
                        case 0x05: case 0x25: entityProperty.Data = r.ReadUInt32(); break; // 4:node_id|flags
                        case 0x1a: entityProperty.Data = r.ReadUInt64(); break; // 8:integer
                        case 0x03: case 0x27: entityProperty.Data = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()); break; // 12:vector|angle
                        case 0x1e: entityProperty.Data = r.ReadZUTF8(); break; // var:string
                        default: throw new NotImplementedException($"Unknown type {type}");
                    }
                    entity.Properties.Add(keyHash, entityProperty);
                }
                for (var i = 0; i < hashedFieldsCount; i++) ReadTypedValue(r.ReadUInt32(), null);
                for (var i = 0; i < stringFieldsCount; i++) ReadTypedValue(r.ReadUInt32(), r.ReadZUTF8());
                return entity;
            }
        }

        public override string ToString()
        {
            var knownKeys = KnownKeys;
            var b = new StringBuilder();
            var unknownKeys = new Dictionary<uint, uint>();
            var types = new Dictionary<uint, string>
            {
                { 0x01, "float" },
                { 0x03, "vector" },
                { 0x05, "node_id" },
                { 0x06, "boolean" },
                { 0x09, "color255" },
                { 0x1a, "integer" },
                { 0x1e, "string" },
                { 0x25, "flags" },
                { 0x27, "angle" },
            };
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
                        value = $"Array [{string.Join(", ", tmp.Select(p => p.ToString()).ToArray())}]";
                    }
                    string key;
                    if (knownKeys.ContainsKey(property.Key)) key = knownKeys[property.Key];
                    else if (property.Value.Name != null) key = property.Value.Name;
                    else
                    {
                        key = $"key={property.Key}";
                        if (!unknownKeys.ContainsKey(property.Key)) unknownKeys.Add(property.Key, 1);
                        else unknownKeys[property.Key]++;
                    }
                    b.AppendLine($"{key,-30} {types[property.Value.Type],-10} {value}");
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
