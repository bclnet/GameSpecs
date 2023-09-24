using System;
using System.IO;
using static GameSpec.Unreal.Formats.Core.Game;
namespace GameSpec.Unreal.Formats.Core
{
    partial class FPackageFileSummary
    {
        // Engine-specific serializers
        void Serialize4(BinaryReader r)
        {
        }
    }

    partial class FObjectExport
    {
        void Serialize4(BinaryReader r, UPackage ar)
        {
        }
    }

    partial class UPackage
    {
        unsafe void LoadNames4(BinaryReader r, UPackage ar)
        {
        }
    }
}