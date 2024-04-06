using System.Collections.Generic;

namespace OpenStack.Graphics
{
    public interface IObjectManager<Object, Material, Texture>
    {
        Object CreateObject(string path, out IDictionary<string, object> data);
        void PreloadObject(string path);
    }
}