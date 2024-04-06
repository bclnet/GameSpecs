using GameX.WbB.Formats.Entity;
using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x02. 
    /// They are basically 3D model descriptions.
    /// </summary>
    [PakFileType(PakFileType.Setup)]
    public class SetupModel : FileType, IHaveMetaInfo
    {
        public static readonly SetupModel Empty = new SetupModel();
        public readonly SetupFlags Flags;
        public readonly bool AllowFreeHeading;
        public readonly bool HasPhysicsBSP;
        public readonly uint[] Parts;
        public readonly uint[] ParentIndex;
        public readonly Vector3[] DefaultScale;
        public readonly IDictionary<int, LocationType> HoldingLocations;
        public readonly IDictionary<int, LocationType> ConnectionPoints;
        public readonly IDictionary<int, PlacementType> PlacementFrames;
        public readonly CylSphere[] CylSpheres;
        public readonly Sphere[] Spheres;
        public readonly float Height;
        public readonly float Radius;
        public readonly float StepUpHeight;
        public readonly float StepDownHeight;
        public readonly Sphere SortingSphere;
        public readonly Sphere SelectionSphere;
        public readonly IDictionary<int, LightInfo> Lights;
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
            if ((Flags & SetupFlags.HasDefaultScale) != 0) DefaultScale = r.ReadFArray(x => x.ReadVector3(), (int)numParts);
            HoldingLocations = r.ReadL32TMany<int, LocationType>(sizeof(int), x => new LocationType(x));
            ConnectionPoints = r.ReadL32TMany<int, LocationType>(sizeof(int), x => new LocationType(x));
            // there is a frame for each Part
            PlacementFrames = r.ReadL32TMany<int, PlacementType>(sizeof(int), x => new PlacementType(r, (uint)Parts.Length));
            CylSpheres = r.ReadL32FArray(x => new CylSphere(x));
            Spheres = r.ReadL32FArray(x => new Sphere(x));
            Height = r.ReadSingle();
            Radius = r.ReadSingle();
            StepUpHeight = r.ReadSingle();
            StepDownHeight = r.ReadSingle();
            SortingSphere = new Sphere(r);
            SelectionSphere = new Sphere(r);
            Lights = r.ReadL32TMany<int, LightInfo>(sizeof(int), x => new LightInfo(x));
            DefaultAnimation = r.ReadUInt32();
            DefaultScript = r.ReadUInt32();
            DefaultMotionTable = r.ReadUInt32();
            DefaultSoundTable = r.ReadUInt32();
            DefaultScriptTable = r.ReadUInt32();
        }

        //: FileTypes.Setup
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(SetupModel)}: {Id:X8}", items: new List<MetaInfo> {
                    Flags != 0 ? new MetaInfo($"Flags: {Flags}") : null,
                    new MetaInfo("Parts", items: Parts.Select((x, i) => new MetaInfo($"{i} - {x:X8}", clickable: true))),
                    Flags.HasFlag(SetupFlags.HasParent) ? new MetaInfo("Parents", items: ParentIndex.Select(x => new MetaInfo($"{x:X8}"))) : null,
                    Flags.HasFlag(SetupFlags.HasDefaultScale) ? new MetaInfo("Default Scales", items: DefaultScale.Select(x => new MetaInfo($"{x}"))) : null,
                    HoldingLocations.Count > 0 ? new MetaInfo("Holding Locations", items: HoldingLocations.OrderBy(i => i.Key).Select(x => new MetaInfo($"{x.Key} - {(ParentLocation)x.Key} - {x.Value}"))) : null,
                    ConnectionPoints.Count > 0 ? new MetaInfo("Connection Points", items: ConnectionPoints.Select(x => new MetaInfo($"{x.Key}: {x.Value}"))) : null,
                    new MetaInfo("Placement frames", items: PlacementFrames.OrderBy(i => i.Key).Select(x => new MetaInfo($"{x.Key} - {(Placement)x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes()))),
                    CylSpheres.Length > 0 ? new MetaInfo("CylSpheres", items: CylSpheres.Select(x => new MetaInfo($"{x}"))) : null,
                    Spheres.Length > 0 ? new MetaInfo("Spheres", items: Spheres.Select(x => new MetaInfo($"{x}"))) : null,
                    new MetaInfo($"Height: {Height}"),
                    new MetaInfo($"Radius: {Radius}"),
                    new MetaInfo($"Step Up Height: {StepUpHeight}"),
                    new MetaInfo($"Step Down Height: {StepDownHeight}"),
                    new MetaInfo($"Sorting Sphere: {SortingSphere}"),
                    new MetaInfo($"Selection Sphere: {SelectionSphere}"),
                    Lights.Count > 0 ? new MetaInfo($"Lights", items: Lights.Select(x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes(tag: tag)))) : null,
                    DefaultAnimation != 0 ? new MetaInfo($"Default Animation: {DefaultAnimation:X8}", clickable: true) : null,
                    DefaultScript != 0 ? new MetaInfo($"Default Script: {DefaultScript:X8}", clickable: true) : null,
                    DefaultMotionTable != 0 ? new MetaInfo($"Default Motion Table: {DefaultMotionTable:X8}", clickable: true) : null,
                    DefaultSoundTable != 0 ? new MetaInfo($"Default Sound Table: {DefaultSoundTable:X8}", clickable: true) : null,
                    DefaultScriptTable != 0 ? new MetaInfo($"Default Script Table: {DefaultScriptTable:X8}", clickable: true) : null,
                })
            };
            return nodes;
        }
    }
}
