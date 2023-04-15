namespace StereoKit.Maui
{
    public interface ISKApplication
    {
        SKSettings Settings { get; }
        void Initialize();
        void OnStep();
        void OnLog(LogLevel level, string text);
    }
}
