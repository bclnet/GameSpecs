﻿using GameSpec;

public class UnityPlatform : UnityEngine.MonoBehaviour
{
    static UnityPlatform() => Platform.Startups.Add(GameSpec.UnityPlatform.Startup);
}