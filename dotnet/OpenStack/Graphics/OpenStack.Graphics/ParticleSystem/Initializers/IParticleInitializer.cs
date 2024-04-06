namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public interface IParticleInitializer
    {
        Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemState);
    }
}
