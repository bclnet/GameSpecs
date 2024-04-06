namespace GameX.Bethesda.Formats
{
    public static class ConvertUtils
    {
        const int yardInUnits = 64;
        const float meterInYards = 1.09361f;
        public const float MeterInUnits = meterInYards * yardInUnits;

        const int exteriorCellSideLengthInUnits = 128 * yardInUnits;
        public const float ExteriorCellSideLengthInMeters = exteriorCellSideLengthInUnits / MeterInUnits;
    }
}