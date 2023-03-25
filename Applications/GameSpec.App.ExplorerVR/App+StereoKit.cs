using GameSpec.App.Explorer.Tools;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Color = StereoKit.Color;

namespace GameSpec.App.Explorer
{
    partial class App
    {
        delegate uint XR_xrConvertTimeToWin32PerformanceCounterKHR(ulong instance, long time, out long performanceCounter);
        static XR_xrConvertTimeToWin32PerformanceCounterKHR xrConvertTimeToWin32PerformanceCounterKHR;

        public SKSettings Settings = new()
        {
            appName = "StereoKit C#",
            assetsFolder = "Assets",
            blendPreference = DisplayBlend.AnyTransparent,
            displayPreference = DisplayMode.MixedReality,
            logFilter = LogLevel.Diagnostic
        };

        Model floorMesh;
        Matrix floorTr;
        string startTest = "welcome";

        public async Task PlatformStartup()
        {
            // args
            Tests.IsTesting = Array.IndexOf(args, "-test") != -1;
            Tests.MakeScreenshots = Array.IndexOf(args, "-noscreens") == -1;
            if (Array.IndexOf(args, "-screenfolder") != -1) Tests.ScreenshotRoot = args[Array.IndexOf(args, "-screenfolder") + 1];
            if (Array.IndexOf(args, "-start") != -1) startTest = args[Array.IndexOf(args, "-start") + 1];
            if (Tests.IsTesting)
            {
                Settings.displayPreference = DisplayMode.Flatscreen;
                Settings.disableUnfocusedSleep = true;
            }

            // Preload the StereoKit library for access to Time.Scale before initialization occurs.
            SK.PreLoadLibrary();
            //Time.Scale = 1;
            Log.Subscribe(OnLog);

            // Initialize StereoKit, and the app
            //Backend.OpenXR.RequestExt("XR_KHR_win32_convert_performance_counter_time");
            if (!SK.Initialize(Settings)) Environment.Exit(1);
            //if (Backend.XRType == BackendXRType.OpenXR && Backend.OpenXR.ExtEnabled("XR_KHR_win32_convert_performance_counter_time"))
            //{
            //    xrConvertTimeToWin32PerformanceCounterKHR = Backend.OpenXR.GetFunction<XR_xrConvertTimeToWin32PerformanceCounterKHR>("xrConvertTimeToWin32PerformanceCounterKHR");
            //    if (xrConvertTimeToWin32PerformanceCounterKHR != null)
            //    {
            //        xrConvertTimeToWin32PerformanceCounterKHR(Backend.OpenXR.Instance, Backend.OpenXR.Time, out long counter);
            //        Log.Info($"XrTime: {counter}");
            //    }
            //}

            Initialize(args);
        }

        public void Initialize(string[] args)
        {
            var floorMat = new Material(Shader.FromFile("floor_shader.hlsl"))
            {
                Transparency = Transparency.Blend,
                QueueOffset = -11,
            };
            floorMat.SetVector("radius", new Vec4(5, 10, 0, 0));
            floorMesh = Model.FromMesh(Mesh.GeneratePlane(new Vec2(40, 40), Vec3.Up, Vec3.Forward), floorMat);
            floorTr = Matrix.TR(new Vec3(0, -1.5f, 0), Quat.Identity);
            logPose = new(0, -0.1f, 0.5f, Quat.LookDir(Vec3.Forward));
            //powerButton = Sprite.FromTex(Tex.FromFile("power.png"));
            //demoSelectPose = new Pose(new Vec3(0, 0, -0.6f), Quat.LookDir(-Vec3.Forward));

            //Tests.FindTests();
            //Tests.SetTestActive(startTest);
            //Tests.Initialize();

            if (!Tests.IsTesting) SK.AddStepper(new RenderCamera(new Pose(0.3f, 0, .5f, Quat.FromAngles(0, -90, 0)), 1000, 1000));
        }

        public void Step()
        {
            CheckFocus();

            Tests.Update();

            if (Input.Key(Key.Esc).IsJustActive()) SK.Quit();

            // Toggle the projection mode
            if (SK.ActiveDisplayMode == DisplayMode.Flatscreen && Input.Key(Key.P).IsJustActive()) Renderer.Projection = Renderer.Projection == Projection.Perspective ? Projection.Ortho : Projection.Perspective;
            // If we can't see the world, we'll draw a floor!
            if (SK.System.displayType == Display.Opaque) Renderer.Add(floorMesh, World.HasBounds ? World.BoundsPose.ToMatrix() : floorTr, Color.White);

            // Skip selection window if we're in test mode
            if (Tests.IsTesting) return;

            // Here's some quick and dirty lines for the play boundary rectangle!
            if (World.HasBounds)
            {
                var s = World.BoundsSize / 2;
                var pose = World.BoundsPose.ToMatrix();
                var tl = pose.Transform(new Vec3(s.x, 0, s.y));
                var br = pose.Transform(new Vec3(-s.x, 0, -s.y));
                var tr = pose.Transform(new Vec3(-s.x, 0, s.y));
                var bl = pose.Transform(new Vec3(s.x, 0, -s.y));
                Lines.Add(tl, tr, Color.White, 1.5f * U.cm);
                Lines.Add(bl, br, Color.White, 1.5f * U.cm);
                Lines.Add(tl, bl, Color.White, 1.5f * U.cm);
                Lines.Add(tr, br, Color.White, 1.5f * U.cm);
            }

            //MainPage?.Step();

            //// Make a window for demo selection
            //UI.WindowBegin("Demos", ref demoSelectPose, new Vec2(50 * U.cm, 0));
            ////for (var i = 0; i < demoNames.Count; i++)
            ////{
            ////    if (UI.Button(demoNames[i])) Tests.SetDemoActive(i);
            ////    UI.SameLine();
            ////}
            //UI.NextLine();
            //UI.HSeparator();
            //if (UI.ButtonImg("Exit", powerButton)) SK.Quit();
            //UI.WindowEnd();

            DebugToolWindow.Step();
            LogWindow();
        }

        #region Log

        static Pose logPose;
        static readonly List<string> logList = new();
        static string logText = "";

        static void OnLog(LogLevel level, string text)
        {
            if (logList.Count > 15) logList.RemoveAt(logList.Count - 1);
            logList.Insert(0, text.Length < 100 ? text : text[..100] + "...\n");
            logText = "";
            for (var i = 0; i < logList.Count; i++) logText += logList[i];
        }

        static void LogWindow()
        {
            UI.WindowBegin("Log", ref logPose, new Vec2(40, 0) * U.cm);
            UI.Text(logText);
            UI.WindowEnd();
        }

        #endregion

        /// Checking for changes in application focus
        AppFocus lastFocus = AppFocus.Hidden;
        void CheckFocus()
        {
            if (lastFocus != SK.AppFocus)
            {
                lastFocus = SK.AppFocus;
                Log.Info($"App focus changed to: {lastFocus}");
            }
        }
    }
}