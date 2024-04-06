using System;
using System.IO;
using static GameX.Epic.Formats.Core.Game;
namespace GameX.Epic.Formats.Core
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