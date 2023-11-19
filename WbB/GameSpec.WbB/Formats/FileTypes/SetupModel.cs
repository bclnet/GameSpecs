using GameSpec.WbB.Formats.Entity;
using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameSpec.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x02. 
    /// They are basically 3D model descriptions.
    /// </summary>
    [PakFileType(PakFileType.Setup)]
    public class SetupModel : FileType, IGetMetadataInfo
    {
        public static readonly SetupModel Empty = new SetupModel();
        public readonly SetupFlags Flags;
        public readonly bool AllowFreeHeading;
        public readonly bool HasPhysicsBSP;
        public readonly uint[] Parts;
        public readonly uint[] ParentIndex;
        public readonly Vector3[] DefaultScale;
        public readonly Dictionary<int, LocationType> HoldingLocations;
        public readonly Dictionary<int, LocationType> ConnectionPoints;
        public readonly Dictionary<int, PlacementType> PlacementFrames;
        public readonly CylSphere[] CylSpheres;
        public readonly Sphere[] Spheres;
        public readonly float Height;
        public readonly float Radius;
        public readonly float StepUpHeight;
        public readonly float StepDownHeight;
        public readonly Sphere SortingSphere;
        public readonly Sphere SelectionSphere;
        public readonly Dictionary<int, LightInfo> Lights;
        public readonly uint DefaultAnimation;
        public readonly uint DefaultScript;
        public readonly uint DefaultMotionTable;
        public readonly uint DefaultSoundTable;
        public readonly uint DefaultScriptTable;

        public bool HasMissileFlightPlacement => PlacementFrames.ContainsKey((int)Placement.MissileFlight);

        SetupModel()
        {
            SortingSphere = Sphere.Empty;
            SelectionSphere = Sphere.Empty;
            AllowFreeHeading = true;
        }
        public SetupModel(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Flags = (SetupFlags)r.ReadUInt32();
            AllowFreeHeading = (Flags & SetupFlags.AllowFreeHeading) != 0;
            HasPhysicsBSP = (Flags & SetupFlags.HasPhysicsBSP) != 0;
            // Get all the GraphicsObjects in this SetupModel. These are all the 01-types.
            var numParts = r.ReadUInt32();
            Parts = r.ReadTArray<uint>(sizeof(uint), (int)numParts);
            if ((Flags & SetupFlags.HasParent) != 0) ParentIndex = r.ReadTArray<uint>(sizeof(uint), (int)numParts);
            if ((Flags & SetupFlags.HasDefaultScale) != 0) DefaultScale = r.ReadTArray(x => x.ReadVector3(), (int)numParts);
            HoldingLocations = r.ReadL32Many<int, LocationType>(sizeof(int), x => new LocationType(x));
            ConnectionPoints = r.ReadL32Many<int, LocationType>(sizeof(int), x => new LocationType(x));
            // there is a frame for each Part
            PlacementFrames = r.ReadL32Many<int, PlacementType>(sizeof(int), x => new PlacementType(r, (uint)Parts.Length));
            CylSpheres = r.ReadL32Array(x => new CylSphere(x));
            Spheres = r.ReadL32Array(x => new Sphere(x));
            Height = r.ReadSingle();
            Radius = r.ReadSingle();
            StepUpHeight = r.ReadSingle();
            StepDownHeight = r.ReadSingle();
            SortingSphere = new Sphere(r);
            SelectionSphere = new Sphere(r);
            Lights = r.ReadL32Many<int, LightInfo>(sizeof(int), x => new LightInfo(x));
            DefaultAnimation = r.ReadUInt32();
            DefaultScript = r.ReadUInt32();
            DefaultMotionTable = r.ReadUInt32();
            DefaultSoundTable = r.ReadUInt32();
            DefaultScriptTable = r.ReadUInt32();
        }

        //: FileTypes.Setup
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(SetupModel)}: {Id:X8}", items: new List<MetadataInfo> {
                    Flags != 0 ? new MetadataInfo($"Flags: {Flags}") : null,
                    new MetadataInfo("Parts", items: Parts.Select((x, i) => new MetadataInfo($"{i} - {x:X8}", clickable: true))),
                    Flags.HasFlag(SetupFlags.HasParent) ? new MetadataInfo("Parents", items: ParentIndex.Select(x => new MetadataInfo($"{x:X8}"))) : null,
                    Flags.HasFlag(SetupFlags.HasDefaultScale) ? new MetadataInfo("Default Scales", items: DefaultScale.Select(x => new MetadataInfo($"{x}"))) : null,
                    HoldingLocations.Count > 0 ? new MetadataInfo("Holding Locations", items: HoldingLocations.OrderBy(i => i.Key).Select(x => new MetadataInfo($"{x.Key} - {(ParentLocation)x.Key} - {x.Value}"))) : null,
                    ConnectionPoints.Count > 0 ? new MetadataInfo("Connection Points", items: ConnectionPoints.Select(x => new MetadataInfo($"{x.Key}: {x.Value}"))) : null,
                    new MetadataInfo("Placement frames", items: PlacementFrames.OrderBy(i => i.Key).Select(x => new MetadataInfo($"{x.Key} - {(Placement)x.Key}", items: (x.Value as IGetMetadataInfo).GetInfoNodes()))),
                    CylSpheres.Length > 0 ? new MetadataInfo("CylSpheres", items: CylSpheres.Select(x => new MetadataInfo($"{x}"))) : null,
                    Spheres.Length > 0 ? new MetadataInfo("Spheres", items: Spheres.Select(x => new MetadataInfo($"{x}"))) : null,
                    new MetadataInfo($"Height: {Height}"),
                    new MetadataInfo($"Radius: {Radius}"),
                    new MetadataInfo($"Step Up Height: {StepUpHeight}"),
                    new MetadataInfo($"Step Down Height: {StepDownHeight}"),
                    new MetadataInfo($"Sorting Sphere: {SortingSphere}"),
                    new MetadataInfo($"Selection Sphere: {SelectionSphere}"),
                    Lights.Count > 0 ? new MetadataInfo($"Lights", items: Lights.Select(x => new MetadataInfo($"{x.Key}", items: (x.Value as IGetMetadataInfo).GetInfoNodes(tag: tag)))) : null,
                    DefaultAnimation != 0 ? new MetadataInfo($"Default Animation: {DefaultAnimation:X8}", clickable: true) : null,
                    DefaultScript != 0 ? new MetadataInfo($"Default Script: {DefaultScript:X8}", clickable: true) : null,
                    DefaultMotionTable != 0 ? new MetadataInfo($"Default Motion Table: {DefaultMotionTable:X8}", clickable: true) : null,
                    DefaultSoundTable != 0 ? new MetadataInfo($"Default Sound Table: {DefaultSoundTable:X8}", clickable: true) : null,
                    DefaultScriptTable != 0 ? new MetadataInfo($"Default Script Table: {DefaultScriptTable:X8}", clickable: true) : null,
                })
            };
            return nodes;
        }
    }
}
