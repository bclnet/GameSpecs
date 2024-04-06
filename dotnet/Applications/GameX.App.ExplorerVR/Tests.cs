using StereoKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GameX.App.Explorer
{
    interface ITest
    {
        void Initialize();
        void Update();
        void Shutdown();
    }

    public static class Tests
    {
        static List<Type> allTests;
        static ITest activeScene;
        static ITest nextScene;
        static int testIndex = 0;
        static int runFrames = -1;
        static float runSeconds = 0;
        static int sceneFrame = 0;
        static float sceneTime = 0;
        static readonly HashSet<string> screens = new();

        static Type ActiveTest
        {
            set => nextScene = (ITest)Activator.CreateInstance(value);
        }
        public static bool IsTesting { get; set; }
        public static string ScreenshotRoot { get; set; } = "../../../docs/img/screenshots";
        public static bool MakeScreenshots { get; set; } = true;

        public static void FindTests()
        {
            allTests = Assembly.GetExecutingAssembly().GetTypes().Where(a => a != typeof(ITest) && typeof(ITest).IsAssignableFrom(a)).ToList();
        }

        public static void Initialize()
        {
            if (IsTesting) nextScene = null;
            nextScene ??= (ITest)Activator.CreateInstance(allTests[testIndex]);
        }

        public static void Update()
        {
            if (IsTesting && runSeconds != 0) Time.SetTime(Time.Total + (1 / 90.0), 1 / 90.0);

            if (nextScene != null)
            {
                activeScene?.Shutdown();
                GC.Collect(int.MaxValue, GCCollectionMode.Forced);
                if (IsTesting)
                {
                    Time.SetTime(0);
                    Input.HandVisible(Handed.Max, false);
                    Input.HandClearOverride(Handed.Left);
                    Input.HandClearOverride(Handed.Right);
                }

                nextScene.Initialize();
                if (IsTesting) Assets.BlockForPriority(int.MaxValue);
                sceneTime = Time.Totalf;
                activeScene = nextScene;
                nextScene = null;
            }
            activeScene?.Update();
            sceneFrame++;

            if (IsTesting && FinishedWithTest())
            {
                testIndex += 1;
                if (testIndex >= allTests.Count) SK.Quit();
                else SetTestActive(allTests[testIndex].Name);
            }
        }

        public static void Shutdown()
        {
            activeScene.Shutdown();
            activeScene = null;
            GC.Collect(int.MaxValue, GCCollectionMode.Forced);
        }

        public static void SetTestActive(string name)
        {
            name = name.ToLowerInvariant();
            var result = allTests.OrderBy(a =>
            {
                var str = a.Name.ToLowerInvariant();
                return str == name ? 0
                    : str.Contains(name) ? str.Length - name.Length
                    : 1000 + string.Compare(str, name);
            }).First();
            Log.Write(LogLevel.Info, "Starting Scene: " + result.Name);
            sceneFrame = 0;
            runFrames = -1;
            runSeconds = 0;
            ActiveTest = result;
        }

        public static void Test(Func<bool> testFunction)
        {
            if (!testFunction())
            {
                Log.Err("Test failed for {0}!", testFunction.Method.Name);
                Environment.Exit(-1);
            }
        }

        static bool FinishedWithTest() => runSeconds != 0
            ? Time.Totalf - sceneTime > runSeconds
            : runFrames == -1 || sceneFrame == runFrames;

        public static void RunForFrames(int frames) => runFrames = frames;
        public static void RunForSeconds(float seconds) => runSeconds = seconds;

        public static void Screenshot(string name, int width, int height, float fov, Vec3 from, Vec3 at) => Screenshot(name, 0, width, height, fov, from, at);
        public static void Screenshot(string name, int width, int height, Vec3 from, Vec3 at) => Screenshot(name, 0, width, height, 90, from, at);
        public static void Screenshot(string name, int frame, int width, int height, float fov, Vec3 from, Vec3 at)
        {
            if (!IsTesting || frame != sceneFrame || screens.Contains(name) || !MakeScreenshots) return;
            screens.Add(name);
            var file = Path.Combine(ScreenshotRoot, name);
            var dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            Renderer.Screenshot(file, from, at, width, height, fov);
        }

        //public static void Hand(in HandJoint[] joints) => Hand(Handed.Right, joints);
        //public static void Hand(Handed hand, in HandJoint[] joints)
        //{
        //    if (!IsTesting) return;
        //    Input.HandVisible(hand, true);
        //    Input.HandOverride(hand, joints);
        //}
    }
}