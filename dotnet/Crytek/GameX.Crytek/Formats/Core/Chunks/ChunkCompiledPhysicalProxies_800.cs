using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkCompiledPhysicalProxies_800 : ChunkCompiledPhysicalProxies
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            NumPhysicalProxies = (int)r.ReadUInt32(); // number of Bones in this chunk.
            PhysicalProxies = new PhysicalProxy[NumPhysicalProxies]; // now have an array of physical proxies
            for (var i = 0; i < NumPhysicalProxies; i++)
            {
                ref PhysicalProxy proxy = ref PhysicalProxies[i];
                // Start populating the physical stream array.  This is the Header.
                proxy.ID = r.ReadUInt32();
                proxy.NumVertices = (int)r.ReadUInt32();
                proxy.NumIndices = (int)r.ReadUInt32();
                proxy.Material = r.ReadUInt32(); // Probably a fill of some sort?
                proxy.Vertices = r.ReadTArray<Vector3>(MathX.SizeOfVector3, proxy.NumVertices);
                proxy.Indices = r.ReadTArray<ushort>(sizeof(ushort), proxy.NumIndices);
                // read the crap at the end so we can move on.
                SkipBytes(r, proxy.Material);
            }

            // Add to SkinningInfo
            var skin = GetSkinningInfo();
            skin.PhysicalBoneMeshes = PhysicalProxies.ToList();
        }
    }
}