using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.OpenGL2
{
    public class TerrainBatch : IDisposable
    {
        public TerrainBatch(TextureAtlasChain overlayAtlasChain, TextureAtlasChain alphaAtlasChain)
        {
            OverlayAtlasChain = overlayAtlasChain;
            AlphaAtlasChain = alphaAtlasChain;
        }

        public void Dispose()
        {
            foreach (var batch in Batches) batch.Dispose();
        }

        public static Effect Effect => Render.Effect_Clamp;
        public TextureAtlasChain OverlayAtlasChain { get; set; }
        public TextureAtlasChain AlphaAtlasChain { get; set; }
        public List<TerrainBatchDraw> Batches { get; set; } = new List<TerrainBatchDraw>();
        public TerrainBatchDraw CurrentBatch { get; set; }

        public void AddTerrain(R_Landblock landblock)
        {
            if (CurrentBatch == null || !CurrentBatch.CanAdd(landblock)) Batches.Add(CurrentBatch = new TerrainBatchDraw(OverlayAtlasChain, AlphaAtlasChain));
            CurrentBatch.AddTerrain(landblock);
        }

        public void OnCompleted()
        {
            foreach (var batch in Batches) batch.OnCompleted();
        }

        public void Draw()
        {
            Effect.CurrentTechnique = Effect.Techniques["LandscapeSinglePass"];

            // assumed to be at index 0
            if (OverlayAtlasChain.TextureAtlases.Count > 0) Effect.Parameters["xOverlays"].SetValue(OverlayAtlasChain.TextureAtlases[0].Texture);
            if (AlphaAtlasChain.TextureAtlases.Count > 0) Effect.Parameters["xAlphas"].SetValue(AlphaAtlasChain.TextureAtlases[0].Texture);
            foreach (var batch in Batches) batch.Draw();
        }
    }
}
