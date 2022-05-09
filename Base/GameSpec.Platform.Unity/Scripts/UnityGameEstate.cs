using GameSpec;

public class UnityGameEstate : UnityEngine.MonoBehaviour
{
    static UnityGameEstate() => FamilyPlatform.Startups.Add(UnityPlatform.Startup);
}