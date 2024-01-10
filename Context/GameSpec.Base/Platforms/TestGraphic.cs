using OpenStack.Graphics;
using System;
using System.Threading.Tasks;

namespace GameSpec.Platforms
{
    public interface ITestGraphic : IOpenGraphic { }

    public class TestGraphic : ITestGraphic
    {
        readonly PakFile _source;

        public TestGraphic(PakFile source) => _source = source;
        public object Source => _source;
        public Task<T> LoadFileObjectAsync<T>(string path) => throw new NotSupportedException();
        public void PreloadTexture(string texturePath) => throw new NotSupportedException();
        public void PreloadObject(string filePath) => throw new NotSupportedException();
    }
}