namespace OpenStack.Graphics.Renderer1
{
    /// <summary>
    /// IPickingTexture
    /// </summary>
    public interface IPickingTexture
    {
        bool IsActive { get; }
        bool Debug { get; }
        Shader Shader { get; }
        Shader DebugShader { get; }
        void Render();
        void Resize(int width, int height);
        void Finish();
    }
}