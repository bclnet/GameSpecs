using System;

namespace GameSpec
{
    [Flags]
    public enum FileOption
    {
        Hosting = Raw | Marker,
        None = 0x0,
        Raw = 0x1,
        Marker = 0x2,
        Stream = 0x4,
        Model = 0x8,
        Supress = 0x10,
    }
}