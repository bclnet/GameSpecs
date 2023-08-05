using GameSpec;
using UnrealEngine.Framework;

public class UnrealPlatform
{
    static UnrealPlatform() => FamilyPlatform.Startups.Add(GameSpec.UnrealPlatform.Startup);

    public static void OnWorldPostBegin() => Debug.Log(LogLevel.Display, "OnWorldPostBegin");

    public static void OnWorldEnd() => Debug.Log(LogLevel.Display, "OnWorldEnd");
}
