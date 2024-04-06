using Android.App;
using Android.Content;
using StereoKit.Maui;

namespace GameX.App.Explorer
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "org.khronos.openxr.intent.category.IMMERSIVE_HMD", "com.oculus.intent.category.VR", Intent.CategoryLauncher })]
    public class MainActivity : MauiSKAppCompatActivity { }
}