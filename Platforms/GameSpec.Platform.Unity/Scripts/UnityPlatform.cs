using GameSpec;

public class UnityPlatform : UnityEngine.MonoBehaviour
{
    static UnityPlatform() => FamilyPlatform.Startups.Add(GameSpec.UnityPlatform.Startup);
}