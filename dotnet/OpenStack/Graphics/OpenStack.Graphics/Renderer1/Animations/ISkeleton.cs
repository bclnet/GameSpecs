namespace OpenStack.Graphics.Renderer1.Animations
{
    public interface ISkeleton
    {
        Bone[] Roots { get; }
        Bone[] Bones { get; }
    }
}
