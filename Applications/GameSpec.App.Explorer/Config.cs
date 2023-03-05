using System.Windows;
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

namespace GameSpec.App.Explorer
{
    public static class Config
    {
        public static string DefaultFamily { get; } = FamilyManager.AppDefaultOptions?.Family;
        public static string DefaultGameId { get; } = FamilyManager.AppDefaultOptions?.GameId;
        public static string ForcePath { get; } = FamilyManager.AppDefaultOptions?.ForcePath;
        public static bool ForceOpen { get; } = FamilyManager.AppDefaultOptions?.ForceOpen ?? false;
        public static bool UseMapBuilder { get; } = false;
    }
}
