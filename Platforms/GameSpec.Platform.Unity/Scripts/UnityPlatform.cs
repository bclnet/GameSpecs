using GameSpec;
using GameSpec.Platforms;

public class UnityPlatform : UnityEngine.MonoBehaviour
{
    static UnityPlatform() => Platform.Startups.Add(GameSpec.Platforms.UnityPlatform.Startup);
}