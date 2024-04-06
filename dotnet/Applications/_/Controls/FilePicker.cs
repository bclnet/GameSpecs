using StereoKit;
using System;
using System.Threading.Tasks;

namespace GameX.App.ExplorerVR.Controls
{
    public class PickOptions
    {
        public string PickerTitle { get; set; }
    }

    public class FileResult
    {
        public string FullPath { get; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }

    public class FilePicker
    {
        public readonly static FilePicker Default = new();

        public Task<FileResult> PickAsync(PickOptions pickOptions) => default;
    }
}
