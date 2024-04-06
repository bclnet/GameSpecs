using System.Collections.Generic;
using System.Linq;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public class RenderModelManagerLocal : IRenderModelManager
    {
        Dictionary<string, IRenderModel> models = new(StringComparer.OrdinalIgnoreCase);
        IRenderModel defaultModel;
        IRenderModel beamModel;
        IRenderModel spriteModel;
        IRenderModel trailModel;
        bool insideLevelLoad;       // don't actually load now

        public RenderModelManagerLocal()
        {
            defaultModel = null;
            beamModel = null;
            spriteModel = null;
            insideLevelLoad = false;
            trailModel = null;
        }

        public void Init()
        {
            cmdSystem.AddCommand("listModels", ListModels_f, CMD_FL.RENDERER, "lists all models");
            cmdSystem.AddCommand("printModel", PrintModel_f, CMD_FL.RENDERER, "prints model info", CmdArgs.ArgCompletion_ModelName);
            cmdSystem.AddCommand("reloadModels", ReloadModels_f, CMD_FL.RENDERER | CMD_FL.CHEAT, "reloads models");
            cmdSystem.AddCommand("touchModel", TouchModel_f, CMD_FL.RENDERER, "touches a model", CmdArgs.ArgCompletion_ModelName);

            insideLevelLoad = false;

            // create a default model
            var model = new RenderModelStatic();
            model.InitEmpty("_DEFAULT");
            model.MakeDefaultModel();
            model.IsLevelLoadReferenced = true;
            defaultModel = model;
            AddModel(model);

            // create the beam model
            var beam = new RenderModelBeam();
            beam.InitEmpty("_BEAM");
            beam.IsLevelLoadReferenced = true;
            beamModel = beam;
            AddModel(beam);

            var sprite = new RenderModelSprite();
            sprite.InitEmpty("_SPRITE");
            sprite.IsLevelLoadReferenced = true;
            spriteModel = sprite;
            AddModel(sprite);
        }

        public void Shutdown()
            => models.Clear();

        IRenderModel GetModel(string modelName, bool createIfNotFound)
        {
            if (string.IsNullOrEmpty(modelName)) return null;

            // see if it is already present
            if (models.TryGetValue(modelName, out var model))
            {
                // reload it if it was purged
                if (!model.IsLoaded) model.LoadModel();
                // we are reusing a model already in memory, but touch all the materials to make sure they stay in memory as well
                else if (insideLevelLoad && !model.IsLevelLoadReferenced) model.TouchData();
                model.SetLevelLoadReferenced(true);
                return model;
            }

            // see if we can load it, determine which subclass of RenderModel to initialize
            var canonical = modelName.ToLowerInvariant();
            var extension = PathX.ExtractFileExtension(canonical).ToLowerInvariant();

            if (extension == "ase" || extension == "lwo" || extension == "flt") { model = new RenderModelStatic(); model.InitFromFile(modelName); }
            else if (extension == "ma") { model = new RenderModelStatic(); model.InitFromFile(modelName); }
            else if (extension == ModelX.MD5_MESH_EXT) { model = new RenderModelMD5(); model.InitFromFile(modelName); }
            else if (extension == "md3") { model = new RenderModelMD3(); model.InitFromFile(modelName); }
            else if (extension == "prt") { model = new RenderModelPrt(); model.InitFromFile(modelName); }
            else if (extension == "liquid") { model = new RenderModelLiquid(); model.InitFromFile(modelName); }
            else
            {
                if (extension.Length != 0) common.Warning($"unknown model type '{canonical}'");
                if (!createIfNotFound) return null;

                var smodel = new RenderModelStatic();
                smodel.InitEmpty(modelName);
                smodel.MakeDefaultModel();
                model = smodel;
            }

            model.SetLevelLoadReferenced(true);

            if (!createIfNotFound && model.IsDefaultModel) { model = null; return null; }
            AddModel(model);

            return model;
        }

        public IRenderModel AllocModel()
            => new RenderModelStatic();

        public void FreeModel(ref IRenderModel model)
        {
            if (model == null) return;
            if (!(model is RenderModelStatic)) { common.Error($"RenderModelManager::FreeModel: model '{model.Name}' is not a static model"); return; }
            if (model == defaultModel) { common.Error("RenderModelManager::FreeModel: can't free the default model"); return; }
            if (model == beamModel) { common.Error("RenderModelManager::FreeModel: can't free the beam model"); return; }
            if (model == spriteModel) { common.Error("RenderModelManager::FreeModel: can't free the sprite model"); return; }

            R_CheckForEntityDefsUsingModel(model);
        }

        public IRenderModel FindModel(string modelName)
            => GetModel(modelName, true);

        public IRenderModel CheckModel(string modelName)
            => GetModel(modelName, false);

        public IRenderModel DefaultModel()
            => defaultModel;

        public void AddModel(IRenderModel model)
            => models.Add(model.Name, model);

        public void RemoveModel(IRenderModel model)
            => models.Remove(model.Name);

        public void ReloadModels(bool forceAll = false)
        {
            common.Printf(forceAll
                ? "Reloading all model files...\n"
                : "Checking for changed model files...\n");
            R_FreeDerivedData();

            // skip the default model at index 0
            foreach (var model in models.Values.Skip(1))
            {
                // we may want to allow world model reloading in the future, but we don't now
                if (!model.IsReloadable) continue;

                // check timestamp
                if (!forceAll)
                {
                    fileSystem.ReadFile(model.Name, out var current);
                    if (current <= model.Timestamp) continue;
                }

                common.DPrintf($"reloading {model.Name}.\n");

                model.LoadModel();
            }

            // we must force the world to regenerate, because models may have changed size, making their references invalid
            R_ReCreateWorldReferences();
        }

        public void FreeModelVertexCaches()
        {
            foreach (var model in models.Values) model.FreeVertexCache();
        }

        public void WritePrecacheCommands(VFile file)
        {
            foreach (var model in models.Values)
            {
                if (model == null || !model.IsReloadable) continue;

                var str = $"touchModel {model.Name}\n";
                common.Printf(str);
                file.Printf(str);
            }
        }

        public void BeginLevelLoad()
        {
            insideLevelLoad = true;

            foreach (var model in models.Values)
            {
                if (C.com_purgeAll.Bool && model.IsReloadable)
                {
                    R_CheckForEntityDefsUsingModel(model);
                    model.PurgeModel();
                }

                model.SetLevelLoadReferenced(false);
            }

            // purge unused triangle surface memory
            R_PurgeTriSurfData(frameData);
        }

        public void EndLevelLoad()
        {
            common.Printf("----- RenderModelManagerLocal::EndLevelLoad -----\n");

            var start = SysW.Milliseconds;

            insideLevelLoad = false;
            int purgeCount = 0, keepCount = 0, loadCount = 0;

            // purge any models not touched
            foreach (var model in models.Values)
                if (!model.IsLevelLoadReferenced && model.IsLoaded && model.IsReloadable)
                {
                    //common.Printf($"purging {model.Name}\n");
                    purgeCount++;
                    R_CheckForEntityDefsUsingModel(model);
                    model.PurgeModel();
                }
                else
                {
                    //common.Printf($"keeping {model.Name}\n");
                    keepCount++;
                }

            // purge unused triangle surface memory
            R_PurgeTriSurfData(frameData);

            // load any new ones
            foreach (var model in models.Values)
            {
                if (model.IsLevelLoadReferenced && !model.IsLoaded && model.IsReloadable)
                {
                    loadCount++;
                    model.LoadModel();

                    if ((loadCount & 15) == 0)
                    {
                        session.PacifierUpdate();
#if __EMSCRIPTEN__
                        emscripten_sleep(1);
#endif
                    }
                }
            }

            // _D3XP added this
            var end = SysW.Milliseconds;
            common.Printf($"{purgeCount,5} models purged from previous level, ");
            common.Printf($"{keepCount,5} models kept.\n");
            if (loadCount != 0) common.Printf($"{loadCount,5} new models loaded in {(end - start) * 0.001:5.1} seconds\n");
        }

        public void PrintMemInfo(MemInfo mi)
        {
            int i, j, totalMem = 0;

            var f = fileSystem.OpenFileWrite($"{mi.filebase}_models.txt");
            if (f == null) return;

            var models = localModelManager.models.Values.ToArray();

            // sort first
            var sortIndex = new int[localModelManager.models.Count];
            for (i = 0; i < localModelManager.models.Count; i++) sortIndex[i] = i;
            for (i = 0; i < localModelManager.models.Count - 1; i++)
                for (j = i + 1; j < localModelManager.models.Count; j++) if (models[sortIndex[i]].Memory < models[sortIndex[j]].Memory) Swap(ref sortIndex[i], ref sortIndex[j]);

            // print next
            for (i = 0; i < models.Length; i++)
            {
                var model = models[sortIndex[i]];
                if (!model.IsLoaded) continue;

                var mem = model.Memory;
                totalMem += mem;
                f.Printf($"{mem:n} {model.Name}\n");
            }

            mi.modelAssetsTotal = totalMem;

            f.Printf($"\nTotal model bytes allocated: {totalMem:n}\n");
            fileSystem.CloseFile(f);
        }

        static void PrintModel_f(CmdArgs args)
        {
            if (args.Count != 2) { common.Printf("usage: printModel <modelName>\n"); return; }

            var model = renderModelManager.CheckModel(args[1]);
            if (model == null) { common.Printf($"model \"{args[1]}\" not found\n"); return; }
            model.Print();
        }

        static void ListModels_f(CmdArgs args)
        {
            int totalMem = 0, inUse = 0;

            common.Printf(" mem   srf verts tris\n");
            common.Printf(" ---   --- ----- ----\n");

            foreach (var model in localModelManager.models.Values)
            {
                if (!model.IsLoaded) continue;
                model.List();
                totalMem += model.Memory;
                inUse++;
            }

            common.Printf(" ---   --- ----- ----\n");
            common.Printf(" mem   srf verts tris\n");

            common.Printf($"{inUse} loaded models\n");
            common.Printf($"total memory: {(float)totalMem / (1024 * 1024):4.1}M\n");
        }

        static void ReloadModels_f(CmdArgs args)
            => localModelManager.ReloadModels(string.Equals(args[1], "all", StringComparison.OrdinalIgnoreCase));

        static void TouchModel_f(CmdArgs args)
        {
            var model = args[1];
            if (string.IsNullOrEmpty(model)) { common.Printf("usage: touchModel <modelName>\n"); return; }

            common.Printf("touchModel {model}\n");
            session.UpdateScreen();
            var m = renderModelManager.CheckModel(model);
            if (m == null) common.Printf("...not found\n");
        }
    }
}
