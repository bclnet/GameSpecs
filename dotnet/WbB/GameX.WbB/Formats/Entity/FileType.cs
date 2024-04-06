using System;

namespace GameX.WbB.Formats.Entity
{
    //: Entity.FileType
    public class FileType
    {
        public uint Id;
        public string Name;
        public string Description;
        public Type Type;

        public FileType(uint id, string name, Type t, string description = "")
        {
            Id = id;
            Name = name;
            Type = t;
            Description = description;
        }

        //: Entity.FileType
        public override string ToString() => $"0x{Id:X2} - {Name}";
    }
}
