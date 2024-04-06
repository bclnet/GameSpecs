using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.QGL;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        const DrawElementsType GlIndexType = DrawElementsType.UnsignedInt;
        static float RB_overbright = 1;

        static bool RB_DrawElementsWithCounters_Once = true;
        public static void RB_DrawElementsWithCounters(DrawSurf surf)
        {
            backEnd.pc.c_drawElements++;
            //backEnd.pc.c_drawIndexes += tri.numIndexes;
            //backEnd.pc.c_drawVertexes += tri.numVerts;
            //if (tri.ambientSurface != null)
            //{
            //    if (tri.indexes == surf.ambientSurface.indexes) backEnd.pc.c_drawRefIndexes += tri.numIndexes;
            //    if (tri.verts == surf.ambientSurface.verts) backEnd.pc.c_drawRefVertexes += tri.numVerts;
            //}
            if (surf.indexCache != null)
            {
                qglDrawElements(PrimitiveType.Triangles, surf.numIndexes, GlIndexType, vertexCache.Position(surf.indexCache));
                backEnd.pc.c_vboIndexes += surf.numIndexes;
            }
            else if (RB_DrawElementsWithCounters_Once) { common.Warning("Attempting to draw without index caching. This is a bug.\n"); RB_DrawElementsWithCounters_Once = false; }
        }

        // May not use all the indexes in the surface if caps are skipped
        static bool RB_DrawShadowElementsWithCounters_once = true;
        public static void RB_DrawShadowElementsWithCounters(DrawSurf surf, int numIndexes)
        {
            backEnd.pc.c_shadowElements++;
            //backEnd.pc.c_shadowIndexes += numIndexes;
            //backEnd.pc.c_shadowVertexes += tri.numVerts;
            if (surf.indexCache != null)
            {
                qglDrawElements(PrimitiveType.Triangles, numIndexes, GlIndexType, vertexCache.Position(surf.indexCache));
                backEnd.pc.c_vboIndexes += numIndexes;
            }
            else if (RB_DrawShadowElementsWithCounters_once) { common.Warning("Attempting to draw without index caching. This is a bug.\n"); RB_DrawShadowElementsWithCounters_once = false; }
        }

        public static void RB_GetShaderTextureMatrix(float[] shaderRegisters, TextureStage texture, float* matrix)
        {
            matrix[0] = shaderRegisters[texture.matrix[0][0]];
            matrix[4] = shaderRegisters[texture.matrix[0][1]];
            matrix[8] = 0f;
            matrix[12] = shaderRegisters[texture.matrix[0][2]];

            // we attempt to keep scrolls from generating incredibly large texture values, but center rotations and center scales can still generate offsets that need to be > 1
            if (matrix[12] < -40f || matrix[12] > 40f) matrix[12] -= (int)matrix[12];

            matrix[1] = shaderRegisters[texture.matrix[1][0]];
            matrix[5] = shaderRegisters[texture.matrix[1][1]];
            matrix[9] = 0f;
            matrix[13] = shaderRegisters[texture.matrix[1][2]];
            if (matrix[13] < -40f || matrix[13] > 40f) matrix[13] -= (int)matrix[13];

            matrix[2] = 0f;
            matrix[6] = 0f;
            matrix[10] = 1f;
            matrix[14] = 0f;

            matrix[3] = 0f;
            matrix[7] = 0f;
            matrix[11] = 0f;
            matrix[15] = 1f;
        }

        // Handles generating a cinematic frame if needed
        public static void RB_BindVariableStageImage(TextureStage texture, float* shaderRegisters)
        {
            if (texture.cinematic != null)
            {
                if (r_skipDynamicTextures.Bool) { globalImages.defaultImage.Bind(); return; }

                // For multithreading. Images will be 1 fame behind..oh well
                if (texture.image != null)
                {
                    // The first time the image will be invalid so wont bind, so bind black image
                    if (texture.image.Bind() == false) globalImages.blackImage.Bind();
                    // Save time to display
                    texture.image.cinmaticNextTime = (int)(1000 * (backEnd.viewDef.floatTime + backEnd.viewDef.renderView.shaderParms[11]));
                    // Update next time
                    globalImages.AddAllocList(texture.image);
                }

                //// offset time by shaderParm[7] (FIXME: make the time offset a parameter of the shader?)
                //// We make no attempt to optimize for multiple identical cinematics being in view, or for cinematics going at a lower framerate than the renderer.
                //CinData cin = texture.cinematic.ImageForTime((int)(1000 * (backEnd.viewDef.floatTime + backEnd.viewDef.renderView.shaderParms[11])));
                //if (cin.image != null) globalImages.cinematicImage.UploadScratch(cin.image, cin.imageWidth, cin.imageHeight);
                //else globalImages.blackImage.Bind();
            }
            //FIXME: see why image is invalid
            else if (texture.image != null) texture.image.Bind();
        }

        // Any mirrored or portaled views have already been drawn, so prepare to actually render the visible surfaces for this view
        public static void RB_BeginDrawingView()
        {
            // set the window clipping
            qglViewport(tr.viewportOffset[0] + backEnd.viewDef.viewport.x1,
                         tr.viewportOffset[1] + backEnd.viewDef.viewport.y1,
                         backEnd.viewDef.viewport.x2 + 1 - backEnd.viewDef.viewport.x1,
                         backEnd.viewDef.viewport.y2 + 1 - backEnd.viewDef.viewport.y1);

            // the scissor may be smaller than the viewport for subviews
            qglScissor(tr.viewportOffset[0] + backEnd.viewDef.viewport.x1 + backEnd.viewDef.scissor.x1,
                        tr.viewportOffset[1] + backEnd.viewDef.viewport.y1 + backEnd.viewDef.scissor.y1,
                        backEnd.viewDef.scissor.x2 + 1 - backEnd.viewDef.scissor.x1,
                        backEnd.viewDef.scissor.y2 + 1 - backEnd.viewDef.scissor.y1);
            backEnd.currentScissor = backEnd.viewDef.scissor;

            // ensures that depth writes are enabled for the depth clear
            GL_State(GLS_DEFAULT);

            // we don't have to clear the depth / stencil buffer for 2D rendering
            if (backEnd.viewDef.viewEntitys != null)
            {
                qglStencilMask(0xff);
                // some cards may have 7 bit stencil buffers, so don't assume this should be 128
                qglClearStencil(1 << (glConfig.stencilBits - 1));
                qglClear((uint)(AttribMask.DepthBufferBit | AttribMask.StencilBufferBit));
                qglEnable(EnableCap.DepthTest);
            }
            else
            {
                qglDisable(EnableCap.DepthTest);
                qglDisable(EnableCap.StencilTest);
            }

            backEnd.glState.faceCulling = (CT)(-1);       // force face culling to set next time
            GL_Cull(CT.FRONT_SIDED);
        }

        public static void RB_SetDrawInteraction(ShaderStage surfaceStage, float[] surfaceRegs, out Image image, Vector4[] matrix, float* color)
        {
            image = surfaceStage.texture.image;
            if (surfaceStage.texture.hasMatrix)
            {
                matrix[0].x = surfaceRegs[surfaceStage.texture.matrix[0][0]];
                matrix[0].y = surfaceRegs[surfaceStage.texture.matrix[0][1]];
                matrix[0].z = 0f;
                matrix[0].w = surfaceRegs[surfaceStage.texture.matrix[0][2]];

                matrix[1].x = surfaceRegs[surfaceStage.texture.matrix[1][0]];
                matrix[1].y = surfaceRegs[surfaceStage.texture.matrix[1][1]];
                matrix[1].z = 0f;
                matrix[1].w = surfaceRegs[surfaceStage.texture.matrix[1][2]];

                // we attempt to keep scrolls from generating incredibly large texture values, but center rotations and center scales can still generate offsets that need to be > 1
                if (matrix[0].w < -40f || matrix[0].w > 40f) matrix[0].w -= (int)matrix[0].w;
                if (matrix[1].w < -40f || matrix[1].w > 40f) matrix[1].w -= (int)matrix[1].w;
            }
            else
            {
                matrix[0].x = 1f;
                matrix[0].y = 0f;
                matrix[0].z = 0f;
                matrix[0].w = 0f;

                matrix[1].x = 0f;
                matrix[1].y = 1f;
                matrix[1].z = 0f;
                matrix[1].w = 0f;
            }

            if (color != null)
                for (var i = 0; i < 4; i++)
                {
                    color[i] = surfaceRegs[surfaceStage.color.registers[i]];
                    // clamp here, so card with greater range don't look different. we could perform overbrighting like we do for lights, but it doesn't currently look worth it.
                    if (color[i] < 0f) color[i] = 0f;
                    else if (color[i] > 1f) color[i] = 1f;
                }
        }

        public static void RB_SubmitInteraction(DrawInteraction din, Action<DrawInteraction> drawInteraction)
        {
            if (din.bumpImage == null) return;

            if (din.diffuseImage == null || r_skipDiffuse.Bool) din.diffuseImage = globalImages.blackImage;
            if (din.specularImage == null || r_skipSpecular.Bool || din.ambientLight != 0) din.specularImage = globalImages.blackImage;
            if (din.bumpImage == null || r_skipBump.Bool) din.bumpImage = globalImages.flatNormalMap;

            // if we wouldn't draw anything, don't call the Draw function
            if (
                ((din.diffuseColor.x > 0f || din.diffuseColor.y > 0f || din.diffuseColor.z > 0f) && din.diffuseImage != globalImages.blackImage) ||
                ((din.specularColor.x > 0f || din.specularColor.y > 0f || din.specularColor.z > 0f) && din.specularImage != globalImages.blackImage))
            {
                din.diffuseColor.x *= RB_overbright;
                din.diffuseColor.y *= RB_overbright;
                din.diffuseColor.z *= RB_overbright;
                drawInteraction(din);
            }
        }

        public static void RB_DrawView(DrawSurfsCommand data)
        {
            var cmd = data;
            backEnd.viewDef = cmd.viewDef;

            // we will need to do a new copyTexSubImage of the screen when a SS_POST_PROCESS material is used
            backEnd.currentRenderCopied = false;

            // if there aren't any drawsurfs, do nothing
            if (backEnd.viewDef.numDrawSurfs == 0) return;

            // skip render bypasses everything that has models, assuming them to be 3D views, but leaves 2D rendering visible
            if (r_skipRender.Bool && backEnd.viewDef.viewEntitys != null) return;

            backEnd.pc.c_surfaces += backEnd.viewDef.numDrawSurfs;

            // render the scene, jumping to the hardware specific interaction renderers
            RB_RenderView();
        }
    }
}
