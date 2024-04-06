using GameX;
using GameX.Platforms;

public class UnityPlatform : UnityEngine.MonoBehaviour
{
    static UnityPlatform() => Platform.Startups.Add(GameX.Platforms.UnityPlatform.Startup);
}