namespace GameX.IW.Zone
{
    public unsafe partial class Asset
    {
        public uint name;
        public UnkAssetType type;
        public object data;
        public int offset;
        public bool written;
#if DEBUG
        public string debugName;
        public bool verified;
#endif
    }
}