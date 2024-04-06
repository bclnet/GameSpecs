namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public struct MemInfo
    {
        public string filebase;

        public int total;
        public int assetTotals;

        // memory manager totals
        public int memoryManagerTotal;

        // subsystem totals
        public int gameSubsystemTotal;
        public int renderSubsystemTotal;

        // asset totals
        public int imageAssetsTotal;
        public int modelAssetsTotal;
        public int soundAssetsTotal;
    }
}