namespace StereoKit.Maui
{
    public interface ISKApplication
    {
        SKSettings Settings { get; set; }
        void Initialize();
        void OnStep();
        void OnLog(LogLevel level, string text);
    }
}
