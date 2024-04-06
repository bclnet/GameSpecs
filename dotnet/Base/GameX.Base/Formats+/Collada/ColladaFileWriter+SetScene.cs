using grendgine_collada;

namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {
        /// <summary>
        /// Adds the Scene element to the Collada document.
        /// </summary>
        void SetScene()
            => daeObject.Scene = new Grendgine_Collada_Scene
            {
                Visual_Scene = new Grendgine_Collada_Instance_Visual_Scene { URL = "#Scene", Name = "Scene" }
            };
    }
}