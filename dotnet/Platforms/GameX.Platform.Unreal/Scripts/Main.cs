using GameX;
using GameX.Platforms;
using UnrealEngine.Framework;

namespace GameSpecUnreal
{
    public class Main
    {
        static Main() => Platform.Startups.Add(UnrealPlatform.Startup);

        static UnrealTest Test;

        public static void OnWorldBegin()
        {
            Debug.Log(LogLevel.Display, "World-Begin");
            Test = new UnrealTest();
            Test.Awake();
            Test.Start();
        }

        public static void OnWorldEnd()
        {
            Test?.OnDestroy(); Test = null;
            Debug.Log(LogLevel.Display, "World-End");
        }

        public static void OnWorldPrePhysicsTick(float deltaTime) => Test?.Update(deltaTime);
    }
}