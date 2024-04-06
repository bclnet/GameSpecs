using StereoKit;

namespace GameX.App.Explorer.Views
{
    partial class MainPage
    {
        Pose pose; // = new(new Vec3(0, 0, -0.6f), Quat.LookDir(-Vec3.Forward));
        Sprite powerButton; // = Sprite.FromTex(Tex.FromFile("power.png"));

        public void Step()
        {
            UI.WindowBegin("MainPage", ref pose, new Vec2(50 * U.cm, 0));
            //for (var i = 0; i < demoNames.Count; i++)
            //{
            //    if (UI.Button(demoNames[i])) Tests.SetDemoActive(i);
            //    UI.SameLine();
            //}
            UI.NextLine();
            UI.HSeparator();
            if (UI.ButtonImg("Exit", powerButton)) SK.Quit();
            UI.WindowEnd();
        }
    }
}
