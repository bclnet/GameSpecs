namespace OpenStack.Graphics.Renderer1
{
    //was:Render/IRenderer
    public interface IRenderer
    {
        AABB BoundingBox { get; }
        void Render(Camera camera, RenderPass renderPass);
        void Update(float frameTime);
    }
}
