namespace GameX.App.Explorer
{
    public static class Config
    {
        public static string DefaultFamily { get; } = FamilyManager.AppDefaultOptions?.Family;
        public static string DefaultGame { get; } = FamilyManager.AppDefaultOptions?.Game;
        public static string DefaultEdition { get; } = FamilyManager.AppDefaultOptions?.Edition;
        public static string ForcePath { get; } = FamilyManager.AppDefaultOptions?.ForcePath;
        public static bool ForceOpen { get; } = FamilyManager.AppDefaultOptions?.ForceOpen ?? false;
        public static bool UseMapBuilder { get; } = false;
    }
}
