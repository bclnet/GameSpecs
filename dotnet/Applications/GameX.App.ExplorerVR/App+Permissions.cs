using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace GameX.App.Explorer
{
    public class ReadWriteStoragePerms : BasePlatformPermission
    {
#if __ANDROID__
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
        {
            (global::Android.Manifest.Permission.ReadExternalStorage, true),
            (global::Android.Manifest.Permission.WriteExternalStorage, true)
        }.ToArray();
#endif
    }

    public partial class App
    {
        async static Task<bool> HasPermissions()
        {
            var status = await CheckAndRequestPermission<ReadWriteStoragePerms>();
            if (status != PermissionStatus.Granted)
            {
                Instance.MainPage.DisplayAlert("Prompt", $"NO ACCESS", "Cancel").Wait();
                return true;
            }
            return false;
        }

        async static Task<PermissionStatus> CheckAndRequestPermission<TPermission>() where TPermission : BasePermission, new()
        {
            var status = await CheckStatusAsync<TPermission>();
            if (status == PermissionStatus.Granted) return status;
            else if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                await Instance.MainPage.DisplayAlert("Prompt", $"turn on in settings", "Cancel");
                return status;
            }
            else if (ShouldShowRationale<TPermission>())
            {
                await Instance.MainPage.DisplayAlert("Prompt", "Why the permission is needed", "Cancel");
            }
            status = await RequestAsync<TPermission>();
            return status;
        }
    }
}