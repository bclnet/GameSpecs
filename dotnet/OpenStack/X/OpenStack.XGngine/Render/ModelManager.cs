using System.NumericsX.OpenStack.Gngine.Framework;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public interface IRenderModelManager
    {
        // registers console commands and clears the list
        void Init();

        // frees all the models
        void Shutdown();

        // called only by renderer::BeginLevelLoad
        void BeginLevelLoad();

        // called only by renderer::EndLevelLoad
        void EndLevelLoad();

        // allocates a new empty render model.
        IRenderModel AllocModel();

        // frees a render model
        void FreeModel(ref IRenderModel model);

        // returns NULL if modelName is NULL or an empty string, otherwise it will create a default model if not loadable
        IRenderModel FindModel(string modelName);

        // returns NULL if not loadable
        IRenderModel CheckModel(string modelName);

        // returns the default cube model
        IRenderModel DefaultModel();

        // world map parsing will add all the inline models with this call
        void AddModel(IRenderModel model);

        // when a world map unloads, it removes its internal models from the list before freeing them.
        // There may be an issue with multiple renderWorlds that share data...
        void RemoveModel(IRenderModel model);

        // the reloadModels console command calls this, but it can
        // also be explicitly invoked
        void ReloadModels(bool forceAll = false);

        // write "touchModel <model>" commands for each non-world-map model
        void WritePrecacheCommands(VFile f);

        // called during vid_restart
        void FreeModelVertexCaches();

        // print memory info
        void PrintMemInfo(MemInfo mi);
    }
}
