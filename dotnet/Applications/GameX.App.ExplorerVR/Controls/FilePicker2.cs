using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace GameX.App.Explorer.Controls
{
    public class FilePicker2
    {
        public readonly static FilePicker2 Default = new();

        public Task<FileResult> PickAsync(PickOptions pickOptions) => default;
    }
}
