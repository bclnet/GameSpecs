using System.NumericsX.OpenStack.Gngine.Framework;
using System.Runtime.CompilerServices;
using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.QGL;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.Gngine.Render.VertexCacheX;
using static System.NumericsX.OpenStack.OpenStack;
using static WaveEngine.Bindings.OpenGLES.GL;
using static System.NumericsX.OpenStack.Gngine.Render.GlslShaders;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe static partial class TR
    {
        #region Common

        public static void RB_BakeTextureMatrixIntoTexgen(in Matrix4x4 lightProject, float* textureMatrix)
        {
            float* genMatrix = stackalloc float[16], final = stackalloc float[16];

            genMatrix[0] = lightProject[0].x;
            genMatrix[4] = lightProject[0].y;
            genMatrix[8] = lightProject[0].z;
            genMatrix[12] = lightProject[0].w;

            genMatrix[1] = lightProject[1].x;
            genMatrix[5] = lightProject[1].y;
            genMatrix[9] = lightProject[1].z;
            genMatrix[13] = lightProject[1].w;

            genMatrix[2] = 0f;
            genMatrix[6] = 0f;
            genMatrix[10] = 0f;
            genMatrix[14] = 0f;

            genMatrix[3] = lightProject[3].x;
            genMatrix[7] = lightProject[3].y;
            genMatrix[11] = lightProject[3].z;
            genMatrix[15] = lightProject[3].w;

            myGlMultMatrix(genMatrix, textureMatrix, final);

            lightProject[0].x = final[0];
            lightProject[0].y = final[4];
            lightProject[0].z = final[8];
            lightProject[0].w = final[12];

            lightProject[1].x = final[1];
            lightProject[1].y = final[5];
            lightProject[1].z = final[9];
            lightProject[1].w = final[13];
        }

        public static void RB_RenderView()
        {
            var drawSurfs = backEnd.viewDef.drawSurfs;
            var numDrawSurfs = backEnd.viewDef.numDrawSurfs;

            // clear the z buffer, set the projection matrix, etc
            RB_BeginDrawingView();

            // Setup GLSL shader state
            RB_GLSL_PrepareShaders();

            // fill the depth buffer and clear color buffer to black except on subviews
            RB_GLSL_FillDepthBuffer(drawSurfs, numDrawSurfs);

#if NO_LIGHT
            if (!r_noLight.Bool)
#endif
            // main light renderer
            RB_GLSL_DrawInteractions();

            // disable stencil shadow test
            qglStencilFunc(StencilFunction.Always, 128, 255);

            // now draw any non-light dependent shading passes
            var processed = RB_GLSL_DrawShaderPasses(drawSurfs, numDrawSurfs);

            // fog and blend lights
            RB_GLSL_FogAllLights();

            // now draw any post-processing effects using _currentRender
            if (processed < numDrawSurfs) RB_GLSL_DrawShaderPasses(drawSurfs.AsSpan(processed), numDrawSurfs - processed);
        }

        #endregion

        static ShaderProgram interactionShader;
        static ShaderProgram interactionPhongShader;
        static ShaderProgram fogShader;
        static ShaderProgram blendLightShader;
        static ShaderProgram zfillShader;
        static ShaderProgram zfillClipShader;
        static ShaderProgram diffuseMapShader;
        static ShaderProgram diffuseCubeShader;
        static ShaderProgram skyboxCubeShader;
        static ShaderProgram reflectionCubeShader;
        static ShaderProgram stencilShadowShader;

        const int ORTHO_PROJECTION = 0;
        const int NORMAL_PROJECTION = 1;
        const int WEAPON_PROJECTION = 2;
        const int DEPTH_HACK_PROJECTION = 3;
        const int NUM_DEPTH_HACK_PROJECTIONS = 50;
        static uint viewMatricesBuffer;

        static bool projectionMatricesSet = false;
        static uint[] projectionMatricesBuffer = new uint[DEPTH_HACK_PROJECTION + NUM_DEPTH_HACK_PROJECTIONS + 1];

        const int ATTR_VERTEX = 0;   // Don't change this, as WebGL require the vertex attrib 0 to be always bound
        const int ATTR_COLOR = 1;
        const int ATTR_TEXCOORD = 2;
        const int ATTR_NORMAL = 3;
        const int ATTR_TANGENT = 4;
        const int ATTR_BITANGENT = 5;

        static float[] global_zero = { 0f };
        static float[] global_one = { 1f };
        static float[] global_oneScaled = { 1 / 255f };
        static float[] global_negOneScaled = { -1f / 255f };
        static Plane[] global_fogPlanes = new Plane[4];

        public static ShaderProgram GL_UseProgram(ShaderProgram program)
        {
            if (backEnd.glState.currentProgram == program) return program;

            qglUseProgram(program != null ? program.program : 0);
            backEnd.glState.currentProgram = program;
            return program;
        }

        public static void GL_Uniform1fv(int location, float* value) => qglUniform1fv(location, 1, value);
        public static void GL_Uniform1fv(int location, float[] value) { fixed (float* _ = value) qglUniform1fv(location, 1, _); }

        public static void GL_Uniform1iv(int location, int[] value) { fixed (int* _ = value) qglUniform1iv(location, 1, _); }

        public static void GL_Uniform4fv(int location, float* value) => qglUniform4fv(location, 1, value);
        public static void GL_Uniform4fv(int location, in Vector4 value) => value.Fixed(_ => qglUniform4fv(location, 1, _));
        public static void GL_Uniform4fv(int location, in Plane value) => value.Fixed(_ => qglUniform4fv(location, 1, _));

        public static void GL_UniformMatrix4fv(int location, float* value) => qglUniformMatrix4fv(location, 1, false, value);
        public static void GL_UniformMatrix4fv(int location, float[] value) { fixed (float* _ = value) qglUniformMatrix4fv(location, 1, false, _); }
        public static void GL_UniformMatrix4fv(int location, in Matrix4x4 value) => value.Fixed(_ => qglUniformMatrix4fv(location, 1, false, _));

        public static void GL_VertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int stride, void* pointer) => qglVertexAttribPointer((uint)index, size, type, normalized, stride, pointer);
        public static void GL_VertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int stride, in Vector2 pointer) => pointer.Fixed(_ => qglVertexAttribPointer((uint)index, size, type, normalized, stride, _));
        public static void GL_VertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int stride, in Vector3 pointer) => pointer.Fixed(_ => qglVertexAttribPointer((uint)index, size, type, normalized, stride, _));
        public static void GL_VertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int stride, in Vector4 pointer) => pointer.Fixed(_ => qglVertexAttribPointer((uint)index, size, type, normalized, stride, _));

        public static void GL_ViewMatricesUniformBuffer(float* value)
        {
            // Update the scene matrices.
            glBindBuffer(BufferTargetARB.UniformBuffer, viewMatricesBuffer);
            var viewMatrices = (float*)glMapBufferRange(BufferTargetARB.UniformBuffer, IntPtr.Zero, 2 * 16 * sizeof(float), GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_BUFFER_BIT);
            if (viewMatrices == null) { common.Error("View Matrices Uniform Buffer is NULL"); return; }

            Unsafe.CopyBlock((byte*)viewMatrices, value, 32 * sizeof(float));

            glUnmapBuffer(BufferTargetARB.UniformBuffer);
            qglBindBuffer(BufferTargetARB.UniformBuffer, 0);
        }

        public static void GL_ProjectionMatricesUniformBuffer(uint projectionMatricesBuffer, float* value)
        {
            // Update the scene matrices.
            glBindBuffer(BufferTargetARB.UniformBuffer, projectionMatricesBuffer);
            var projectionMatrix = (float*)glMapBufferRange(BufferTargetARB.UniformBuffer, IntPtr.Zero, 16 * sizeof(float), GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_BUFFER_BIT);
            if (projectionMatrix == null) { common.Error("Projection Matrices Uniform Buffer is NULL"); return; }

            Unsafe.CopyBlock((byte*)projectionMatrix, value, 16 * sizeof(float));

            glUnmapBuffer(BufferTargetARB.UniformBuffer);
            qglBindBuffer(BufferTargetARB.UniformBuffer, 0);
        }

        public static void GL_EnableVertexAttribArray(uint index)
            => qglEnableVertexAttribArray(index);

        public static void GL_DisableVertexAttribArray(uint index)
            => qglDisableVertexAttribArray(index);

        // loads GLSL vertex or fragment shaders
        public static void R_LoadGLSLShader(string buffer, ShaderProgram shaderProgram, ShaderType type)
        {
            if (!glConfig.isInitialized) return;

            switch (type)
            {
                // create vertex shader
                case ShaderType.VertexShader:
                    shaderProgram.vertexShader = qglCreateShader(ShaderType.VertexShader);
                    qglShaderSource(shaderProgram.vertexShader, 1, buffer);
                    qglCompileShader(shaderProgram.vertexShader);
                    break;
                // create fragment shader
                case ShaderType.FragmentShader:
                    shaderProgram.fragmentShader = qglCreateShader(ShaderType.FragmentShader);
                    qglShaderSource(shaderProgram.fragmentShader, 1, buffer);
                    qglCompileShader(shaderProgram.fragmentShader);
                    break;
                default: common.Printf("R_LoadGLSLShader: no type\n"); return;
            }
        }

        // links the GLSL vertex and fragment shaders together to form a GLSL program
        public static bool R_LinkGLSLShader(ShaderProgram shaderProgram, string name)
        {
            const int BUFSIZ = 512; //: stdio.h
            int len, linked;
            var buf = stackalloc char[BUFSIZ];

            shaderProgram.program = qglCreateProgram();

            qglAttachShader(shaderProgram.program, shaderProgram.vertexShader);
            qglAttachShader(shaderProgram.program, shaderProgram.fragmentShader);

            // Bind attributes locations
            qglBindAttribLocation(shaderProgram.program, ATTR_VERTEX, "attr_Vertex");
            qglBindAttribLocation(shaderProgram.program, ATTR_COLOR, "attr_Color");
            qglBindAttribLocation(shaderProgram.program, ATTR_TEXCOORD, "attr_TexCoord");
            qglBindAttribLocation(shaderProgram.program, ATTR_NORMAL, "attr_Normal");
            qglBindAttribLocation(shaderProgram.program, ATTR_TANGENT, "attr_Tangent");
            qglBindAttribLocation(shaderProgram.program, ATTR_BITANGENT, "attr_Bitangent");

            qglLinkProgram(shaderProgram.program);
            qglGetProgramiv(shaderProgram.program, ProgramPropertyARB.LinkStatus, &linked);
            if (C.com_developer.Bool)
            {
                qglGetShaderInfoLog(shaderProgram.vertexShader, BUFSIZ, &len, buf); common.Printf($"VS:\n{new string(buf, 0, len)}\n");
                qglGetShaderInfoLog(shaderProgram.fragmentShader, BUFSIZ, &len, buf); common.Printf($"FS:\n{new string(buf, 0, len)}\n");
            }
            if (linked == 0) { common.Error($"R_LinkGLSLShader: program failed to link: {name}\n"); return false; }
            return true;
        }

        // makes sure GLSL program is valid
        public static bool R_ValidateGLSLProgram(ShaderProgram shaderProgram)
        {
            int validProgram;

            qglValidateProgram(shaderProgram.program);
            qglGetProgramiv(shaderProgram.program, ProgramPropertyARB.ValidateStatus, &validProgram);
            if (validProgram == 0) { common.Printf("R_ValidateGLSLProgram: program invalid\n"); return false; }
            return true;
        }

        public static void RB_GLSL_GetUniformLocations(ShaderProgram shader)
        {
            int i;

            GL_UseProgram(shader);

            shader.localLightOrigin = qglGetUniformLocation(shader.program, "u_lightOrigin");

            // May need to move this to the shader matrices uniform block?
            shader.localViewOrigin = qglGetUniformLocation(shader.program, "u_viewOrigin");
            shader.lightProjection = qglGetUniformLocation(shader.program, "u_lightProjection");
            shader.bumpMatrixS = qglGetUniformLocation(shader.program, "u_bumpMatrixS");
            shader.bumpMatrixT = qglGetUniformLocation(shader.program, "u_bumpMatrixT");
            shader.diffuseMatrixS = qglGetUniformLocation(shader.program, "u_diffuseMatrixS");
            shader.diffuseMatrixT = qglGetUniformLocation(shader.program, "u_diffuseMatrixT");
            shader.specularMatrixS = qglGetUniformLocation(shader.program, "u_specularMatrixS");
            shader.specularMatrixT = qglGetUniformLocation(shader.program, "u_specularMatrixT");
            shader.colorModulate = qglGetUniformLocation(shader.program, "u_colorModulate");
            shader.colorAdd = qglGetUniformLocation(shader.program, "u_colorAdd");
            shader.fogColor = qglGetUniformLocation(shader.program, "u_fogColor");
            shader.diffuseColor = qglGetUniformLocation(shader.program, "u_diffuseColor");
            shader.specularColor = qglGetUniformLocation(shader.program, "u_specularColor");
            shader.glColor = qglGetUniformLocation(shader.program, "u_glColor");
            shader.alphaTest = qglGetUniformLocation(shader.program, "u_alphaTest");
            shader.specularExponent = qglGetUniformLocation(shader.program, "u_specularExponent");

            shader.modelMatrix = qglGetUniformLocation(shader.program, "u_modelMatrix");

            // Shader Matrices for the View Matrices
            var viewMatricesUniformLocation = qglGetUniformBlockIndex(shader.program, "ViewMatrices");
            var numBufferBindings = 0U;
            shader.viewMatricesBinding = numBufferBindings++;
            glUniformBlockBinding(shader.program, viewMatricesUniformLocation, shader.viewMatricesBinding);

            // Shader Matrices for the Projection Matrix
            var projectionMatrixUniformLocation = qglGetUniformBlockIndex(shader.program, "ProjectionMatrix");
            shader.projectionMatrixBinding = numBufferBindings++;
            glUniformBlockBinding(shader.program, projectionMatrixUniformLocation, shader.projectionMatrixBinding);

            shader.modelViewMatrix = qglGetUniformLocation(shader.program, "u_modelViewMatrix");
            shader.textureMatrix = qglGetUniformLocation(shader.program, "u_textureMatrix");
            shader.clipPlane = qglGetUniformLocation(shader.program, "u_clipPlane");
            shader.fogMatrix = qglGetUniformLocation(shader.program, "u_fogMatrix");

            shader.attr_TexCoord = qglGetAttribLocation(shader.program, "attr_TexCoord");
            shader.attr_Tangent = qglGetAttribLocation(shader.program, "attr_Tangent");
            shader.attr_Bitangent = qglGetAttribLocation(shader.program, "attr_Bitangent");
            shader.attr_Normal = qglGetAttribLocation(shader.program, "attr_Normal");
            shader.attr_Vertex = qglGetAttribLocation(shader.program, "attr_Vertex");
            shader.attr_Color = qglGetAttribLocation(shader.program, "attr_Color");

            // Init default values
            for (i = 0; i < ShaderStage.MAX_FRAGMENT_IMAGES; i++) { shader.u_fragmentMap[i] = qglGetUniformLocation(shader.program, $"u_fragmentMap{i}"); qglUniform1i(shader.u_fragmentMap[i], i); }
            for (i = 0; i < ShaderStage.MAX_FRAGMENT_IMAGES; i++) { shader.u_fragmentCubeMap[i] = qglGetUniformLocation(shader.program, $"u_fragmentCubeMap{i}"); qglUniform1i(shader.u_fragmentCubeMap[i], i); }

            // Load identity matrix for Texture marix
            if (shader.textureMatrix >= 0) Matrix4x4.identity.Fixed(_ => GL_UniformMatrix4fv(shader.textureMatrix, _));

            // Alpha test always pass by default
            if (shader.alphaTest >= 0) GL_Uniform1fv(shader.alphaTest, global_one);
            if (shader.colorModulate >= 0) GL_Uniform1fv(shader.colorModulate, global_zero);
            if (shader.colorAdd >= 0) GL_Uniform1fv(shader.colorAdd, global_one);

            GL_CheckErrors();
            GL_UseProgram(null);
        }

        public static bool RB_GLSL_InitShaders()
        {
            // Generate buffer for 2 * view matrices
            qglGenBuffers(1, out viewMatricesBuffer);
            glBindBuffer(BufferTargetARB.UniformBuffer, viewMatricesBuffer);
            glBufferData(BufferTargetARB.UniformBuffer, 2 * 16 * sizeof(float), null, BufferUsageARB.StaticDraw);
            glBindBuffer(BufferTargetARB.UniformBuffer, 0);

            for (var i = 0; i <= (NUM_DEPTH_HACK_PROJECTIONS + DEPTH_HACK_PROJECTION); ++i)
            {
                qglGenBuffers(1, out projectionMatricesBuffer[i]);
                glBindBuffer(BufferTargetARB.UniformBuffer, projectionMatricesBuffer[i]);
                glBufferData(BufferTargetARB.UniformBuffer, 16 * sizeof(float), null, BufferUsageARB.StaticDraw);
                glBindBuffer(BufferTargetARB.UniformBuffer, 0);
            }

            // main Interaction shader
            common.Printf("Loading main interaction shader\n");
            interactionShader.memset();
            R_LoadGLSLShader(interactionShaderVP.Value, interactionShader, ShaderType.VertexShader);
            R_LoadGLSLShader(interactionShaderFP.Value, interactionShader, ShaderType.FragmentShader);
            if (!R_LinkGLSLShader(interactionShader, "interaction") && !R_ValidateGLSLProgram(interactionShader)) return false;
            else RB_GLSL_GetUniformLocations(interactionShader);

            // main Interaction shader, Phong version
            common.Printf("Loading main interaction shader (Phong) \n");
            interactionPhongShader.memset();
            R_LoadGLSLShader(interactionPhongShaderVP.Value, interactionPhongShader, ShaderType.VertexShader);
            R_LoadGLSLShader(interactionPhongShaderFP.Value, interactionPhongShader, ShaderType.FragmentShader);
            if (!R_LinkGLSLShader(interactionPhongShader, "interactionPhong") && !R_ValidateGLSLProgram(interactionPhongShader)) return false;
            else RB_GLSL_GetUniformLocations(interactionPhongShader);

            // default diffuse shader
            common.Printf("Loading default diffuse shader\n");
            diffuseMapShader.memset();
            R_LoadGLSLShader(diffuseMapShaderVP.Value, diffuseMapShader, ShaderType.VertexShader);
            R_LoadGLSLShader(diffuseMapShaderFP.Value, diffuseMapShader, ShaderType.FragmentShader);
            if (!R_LinkGLSLShader(diffuseMapShader, "diffuseMap") && !R_ValidateGLSLProgram(diffuseMapShader)) return false;
            else RB_GLSL_GetUniformLocations(diffuseMapShader);

            // Skybox cubemap shader
            common.Printf("Loading skybox cubemap shader\n");
            skyboxCubeShader.memset();
            R_LoadGLSLShader(skyboxCubeShaderVP.Value, skyboxCubeShader, ShaderType.VertexShader);
            R_LoadGLSLShader(cubeMapShaderFP.Value, skyboxCubeShader, ShaderType.FragmentShader);   // Use the common "cubeMapShaderFP"
            if (!R_LinkGLSLShader(skyboxCubeShader, "skyboxCube") && !R_ValidateGLSLProgram(skyboxCubeShader)) return false;
            else RB_GLSL_GetUniformLocations(skyboxCubeShader);

            // Reflection cubemap shader
            common.Printf("Loading reflection cubemap shader\n");
            reflectionCubeShader.memset();
            R_LoadGLSLShader(reflectionCubeShaderVP.Value, reflectionCubeShader, ShaderType.VertexShader);
            R_LoadGLSLShader(cubeMapShaderFP.Value, reflectionCubeShader, ShaderType.FragmentShader); // Use the common "cubeMapShaderFP"
            if (!R_LinkGLSLShader(reflectionCubeShader, "reflectionCube") && !R_ValidateGLSLProgram(reflectionCubeShader)) return false;
            else RB_GLSL_GetUniformLocations(reflectionCubeShader);

            // Diffuse cubemap shader
            common.Printf("Loading diffuse cubemap shader\n");
            diffuseCubeShader.memset();
            R_LoadGLSLShader(diffuseCubeShaderVP.Value, diffuseCubeShader, ShaderType.VertexShader);
            R_LoadGLSLShader(cubeMapShaderFP.Value, diffuseCubeShader, ShaderType.FragmentShader); // Use the common "cubeMapShaderFP"
            if (!R_LinkGLSLShader(diffuseCubeShader, "diffuseCube") && !R_ValidateGLSLProgram(diffuseCubeShader)) return false;
            else RB_GLSL_GetUniformLocations(diffuseCubeShader);

            // Z Fill shader
            common.Printf("Loading Zfill shader\n");
            zfillShader.memset();
            R_LoadGLSLShader(zfillShaderVP.Value, zfillShader, ShaderType.VertexShader);
            R_LoadGLSLShader(zfillShaderFP.Value, zfillShader, ShaderType.FragmentShader);
            if (!R_LinkGLSLShader(zfillShader, "zfill") && !R_ValidateGLSLProgram(zfillShader)) return false;
            else RB_GLSL_GetUniformLocations(zfillShader);

            // Z Fill shader, Clip planes version
            common.Printf("Loading Zfill shader (Clip plane version)\n");
            zfillClipShader.memset();
            R_LoadGLSLShader(zfillClipShaderVP.Value, zfillClipShader, ShaderType.VertexShader);
            R_LoadGLSLShader(zfillClipShaderFP.Value, zfillClipShader, ShaderType.FragmentShader);
            if (!R_LinkGLSLShader(zfillClipShader, "zfillClip") && !R_ValidateGLSLProgram(zfillClipShader)) return false;
            else RB_GLSL_GetUniformLocations(zfillClipShader);

            // Fog shader
            common.Printf("Loading Fog shader\n");
            fogShader.memset();
            R_LoadGLSLShader(fogShaderVP.Value, fogShader, ShaderType.VertexShader);
            R_LoadGLSLShader(fogShaderFP.Value, fogShader, ShaderType.FragmentShader);
            if (!R_LinkGLSLShader(fogShader, "fog") && !R_ValidateGLSLProgram(fogShader)) return false;
            else RB_GLSL_GetUniformLocations(fogShader);

            // BlendLight shader
            common.Printf("Loading BlendLight shader\n");
            blendLightShader.memset();
            R_LoadGLSLShader(blendLightShaderVP.Value, blendLightShader, ShaderType.VertexShader);
            R_LoadGLSLShader(fogShaderFP.Value, blendLightShader, ShaderType.FragmentShader);       // Reuse the common "FogShaderFP"
            if (!R_LinkGLSLShader(blendLightShader, "blendLight") && !R_ValidateGLSLProgram(blendLightShader)) return false;
            else RB_GLSL_GetUniformLocations(blendLightShader);

            // Stencil shadow shader
            common.Printf("Loading Stencil shadow shader\n");
            stencilShadowShader.memset();
            R_LoadGLSLShader(stencilShadowShaderVP.Value, stencilShadowShader, ShaderType.VertexShader);
            R_LoadGLSLShader(stencilShadowShaderFP.Value, stencilShadowShader, ShaderType.FragmentShader);
            if (!R_LinkGLSLShader(stencilShadowShader, "stencilShadow") && !R_ValidateGLSLProgram(stencilShadowShader)) return false;
            else RB_GLSL_GetUniformLocations(stencilShadowShader);

            return true;
        }

        public static void R_ReloadGLSLPrograms_f(CmdArgs args)
        {
            common.Printf("----- R_ReloadGLSLPrograms -----\n");
            if (!RB_GLSL_InitShaders()) common.Printf("GLSL shaders failed to init.\n");
            common.Printf("-------------------------------\n");
        }

        // Compute the required projection matrix depth hacks
        public static void RB_ComputeProjection(bool weaponDepthHack, float modelDepthHack, float* projection)
        {
            // Get the projection matrix
            var localProjectionMatrix = stackalloc float[16];
            fixed (float* _ = backEnd.viewDef.projectionMatrix) Unsafe.CopyBlock(localProjectionMatrix, _, sizeof(float) * 16);

            // Quick and dirty hacks on the projection matrix
            if (weaponDepthHack) localProjectionMatrix[14] = backEnd.viewDef.projectionMatrix[14] * 0.25f;
            else if (modelDepthHack != 0.0) localProjectionMatrix[14] = backEnd.viewDef.projectionMatrix[14] - modelDepthHack;

            Unsafe.CopyBlock(projection, localProjectionMatrix, sizeof(float) * 16);
        }

        // Compute which projection matrix buffer should be used
        public static uint RB_CalculateProjection(DrawSurf surf)
        {
            uint result = NORMAL_PROJECTION;
            if (surf.space.weaponDepthHack) result = WEAPON_PROJECTION;
            else if (surf.space.modelDepthHack > 0f && surf.space.modelDepthHack <= 1f) result = DEPTH_HACK_PROJECTION + (uint)(surf.space.modelDepthHack * (float)NUM_DEPTH_HACK_PROJECTIONS);
            //Is this set up as an orthographic projection?
            else if (
                backEnd.viewDef.projectionMatrix[0] == 2f / 640f &&
                backEnd.viewDef.projectionMatrix[5] == -2f / 480f &&
                backEnd.viewDef.projectionMatrix[10] == -2f / 1f &&
                backEnd.viewDef.projectionMatrix[12] == -1f &&
                backEnd.viewDef.projectionMatrix[13] == 1f &&
                backEnd.viewDef.projectionMatrix[14] == -1f &&
                backEnd.viewDef.projectionMatrix[15] == 1f)
                result = ORTHO_PROJECTION;
            return result;
        }

        public static void RB_GLSL_DrawInteraction(DrawInteraction din)
        {
            var program = backEnd.glState.currentProgram;

            // load all the vertex program parameters
            GL_Uniform4fv(program.localLightOrigin, din.localLightOrigin);
            GL_Uniform4fv(program.localViewOrigin, din.localViewOrigin);
            GL_UniformMatrix4fv(program.lightProjection, din.lightProjection);
            GL_Uniform4fv(program.bumpMatrixS, din.bumpMatrix[0]);
            GL_Uniform4fv(program.bumpMatrixT, din.bumpMatrix[1]);
            GL_Uniform4fv(program.diffuseMatrixS, din.diffuseMatrix[0]);
            GL_Uniform4fv(program.diffuseMatrixT, din.diffuseMatrix[1]);
            GL_Uniform4fv(program.specularMatrixS, din.specularMatrix[0]);
            GL_Uniform4fv(program.specularMatrixT, din.specularMatrix[1]);

            switch (din.vertexColor)
            {
                case SVC.MODULATE:
                    GL_Uniform1fv(program.colorModulate, global_oneScaled);
                    GL_Uniform1fv(program.colorAdd, global_zero);
                    break;
                case SVC.INVERSE_MODULATE:
                    GL_Uniform1fv(program.colorModulate, global_negOneScaled);
                    GL_Uniform1fv(program.colorAdd, global_one);
                    break;
                // This is already the default values (zero, one)
                case SVC.IGNORE: default: break;
            }

            // set the constant colors
            GL_Uniform4fv(program.diffuseColor, din.diffuseColor);
            GL_Uniform4fv(program.specularColor, din.specularColor);

            // set the textures

            // texture 0 will be the per-surface bump map
            // NB: Texture 0 is expected to be active at this point
            din.bumpImage.Bind();
            // texture 1 will be the light falloff texture
            GL_SelectTexture(1); din.lightFalloffImage.Bind();
            // texture 2 will be the light projection texture
            GL_SelectTexture(2); din.lightImage.Bind();
            // texture 3 is the per-surface diffuse map
            GL_SelectTexture(3); din.diffuseImage.Bind();
            // texture 4 is the per-surface specular map
            GL_SelectTexture(4); din.specularImage.Bind();
            // Be sure to activate Texture 0 for next interaction, or next pass
            GL_SelectTexture(0);

            // draw it
            RB_DrawElementsWithCounters(din.surf);

            // Restore color modulation state to default values
            if (din.vertexColor != SVC.IGNORE) { GL_Uniform1fv(program.colorModulate, global_zero); GL_Uniform1fv(program.colorAdd, global_one); }
        }

        // This can be used by different draw_* backends to decompose a complex light / surface interaction into primitive interactions
        public static void RB_GLSL_CreateSingleDrawInteractions(in DrawSurf surf, Action<DrawInteraction> drawInteraction, ViewLight vLight)
        {
            var surfaceShader = surf.material;
            var surfaceRegs = surf.shaderRegisters;
            var lightShader = vLight.lightShader;
            var lightRegs = vLight.shaderRegisters;
            DrawInteraction inter = new();

            if (r_skipInteractions.Bool || surf.geoFrontEnd == null || surf.ambientCache == null) return;

            // change the scissor if needed
            if (r_useScissor.Bool && !backEnd.currentScissor.Equals(surf.scissorRect))
            {
                backEnd.currentScissor = surf.scissorRect;
                if ((backEnd.currentScissor.x2 + 1 - backEnd.currentScissor.x1) < 0f || (backEnd.currentScissor.y2 + 1 - backEnd.currentScissor.y1) < 0f) backEnd.currentScissor = backEnd.viewDef.scissor;
                qglScissor(backEnd.viewDef.viewport.x1 + backEnd.currentScissor.x1,
                    backEnd.viewDef.viewport.y1 + backEnd.currentScissor.y1,
                    backEnd.currentScissor.x2 + 1 - backEnd.currentScissor.x1,
                    backEnd.currentScissor.y2 + 1 - backEnd.currentScissor.y1);
            }

            inter.surf = surf;
            inter.lightFalloffImage = vLight.falloffImage;

            R_GlobalPointToLocal(surf.space.modelMatrix, vLight.globalLightOrigin, out inter.localLightOrigin.ToVec3());
            R_GlobalPointToLocal(surf.space.modelMatrix, backEnd.viewDef.renderView.vieworg, out inter.localViewOrigin.ToVec3());
            inter.localLightOrigin.w = 0;
            inter.localViewOrigin.w = 1;
            inter.ambientLight = lightShader.IsAmbientLight ? 1 : 0;

            // the base projections may be modified by texture matrix on light stages
            var lightProject = stackalloc Plane[4];

            for (var i = 0; i < 4; i++) R_GlobalPlaneToLocal(surf.space.modelMatrix, vLight.lightProject[i], out lightProject[i]);

            var lightTextureMatrix = stackalloc float[16];
            var lightColor = stackalloc float[4];
            for (var lightStageNum = 0; lightStageNum < lightShader.NumStages; lightStageNum++)
            {
                var lightStage = lightShader.GetStage(lightStageNum);

                // ignore stages that fail the condition
                if (lightRegs[lightStage.conditionRegister] == 0f) continue;

                inter.lightImage = lightStage.texture.image;

                inter.lightProjection[0] = lightProject[0].ToVec4(); // S
                inter.lightProjection[1] = lightProject[1].ToVec4(); // T
                inter.lightProjection[2] = lightProject[3].ToVec4(); // CAUTION! this is the 4th vector. R = Falloff
                inter.lightProjection[3] = lightProject[2].ToVec4(); // CAUTION! this is the 3rd vector. Q

                // now multiply the texgen by the light texture matrix
                if (lightStage.texture.hasMatrix)
                {
                    RB_GetShaderTextureMatrix(lightRegs, lightStage.texture, lightTextureMatrix);
                    RB_BakeTextureMatrixIntoTexgen(inter.lightProjection, lightTextureMatrix);
                }

                inter.bumpImage = null;
                inter.specularImage = null;
                inter.diffuseImage = null;
                inter.diffuseColor[0] = inter.diffuseColor[1] = inter.diffuseColor[2] = inter.diffuseColor[3] = 0;
                inter.specularColor[0] = inter.specularColor[1] = inter.specularColor[2] = inter.specularColor[3] = 0;

                var lightscale = r_lightScale.Float;
                lightColor[0] = lightscale * lightRegs[lightStage.color.registers[0]];
                lightColor[1] = lightscale * lightRegs[lightStage.color.registers[1]];
                lightColor[2] = lightscale * lightRegs[lightStage.color.registers[2]];
                lightColor[3] = lightRegs[lightStage.color.registers[3]];

                // go through the individual stages
                for (var surfaceStageNum = 0; surfaceStageNum < surfaceShader.NumStages; surfaceStageNum++)
                {
                    var surfaceStage = surfaceShader.GetStage(surfaceStageNum);

                    switch (surfaceStage.lighting)
                    {
                        // ignore ambient stages while drawing interactions
                        case SL.AMBIENT: break;
                        case SL.BUMP:
                            // ignore stage that fails the condition
                            if (surfaceRegs[surfaceStage.conditionRegister] == 0) break;
                            // draw any previous interaction
                            RB_SubmitInteraction(inter, drawInteraction);
                            inter.diffuseImage = null;
                            inter.specularImage = null;
                            RB_SetDrawInteraction(surfaceStage, surfaceRegs, out inter.bumpImage, inter.bumpMatrix, null);
                            break;
                        case SL.DIFFUSE:
                            // ignore stage that fails the condition
                            if (surfaceRegs[surfaceStage.conditionRegister] == 0) break;
                            if (inter.diffuseImage != null) RB_SubmitInteraction(inter, drawInteraction);
                            inter.diffuseColor.Fixed(_ => RB_SetDrawInteraction(surfaceStage, surfaceRegs, out inter.diffuseImage, inter.diffuseMatrix, _));
                            inter.diffuseColor[0] *= lightColor[0];
                            inter.diffuseColor[1] *= lightColor[1];
                            inter.diffuseColor[2] *= lightColor[2];
                            inter.diffuseColor[3] *= lightColor[3];
                            inter.vertexColor = surfaceStage.vertexColor;
                            break;
                        case SL.SPECULAR:
                            // ignore stage that fails the condition
                            if (surfaceRegs[surfaceStage.conditionRegister] == 0) break;
                            if (inter.specularImage != null) RB_SubmitInteraction(inter, drawInteraction);
                            inter.specularColor.Fixed(_ => RB_SetDrawInteraction(surfaceStage, surfaceRegs, out inter.specularImage, inter.specularMatrix, _));
                            inter.specularColor[0] *= lightColor[0];
                            inter.specularColor[1] *= lightColor[1];
                            inter.specularColor[2] *= lightColor[2];
                            inter.specularColor[3] *= lightColor[3];
                            inter.vertexColor = surfaceStage.vertexColor;
                            break;
                    }
                }

                // draw the final interaction
                RB_SubmitInteraction(inter, drawInteraction);
            }
        }

        public static void RB_GLSL_CreateDrawInteractions(DrawSurf surf, ViewLight vLight, int depthFunc = GLS_DEPTHFUNC_EQUAL)
        {
            if (surf == null) return;

            // perform setup here that will be constant for all interactions
            GL_State(GLS_SRCBLEND_ONE | GLS_DSTBLEND_ONE | GLS_DEPTHMASK | depthFunc);

            // bind the vertex and fragment shader
            if (r_usePhong.Bool)
            {
                GL_UseProgram(interactionPhongShader);

                // Set the specular exponent now (NB: it could be cached instead)
                GL_Uniform1fv(interactionPhongShader.specularExponent, new[] { r_specularExponent.Float });
            }
            else GL_UseProgram(interactionShader);

            var program = backEnd.glState.currentProgram;

            glBindBufferBase(BufferTargetARB.UniformBuffer, program.viewMatricesBinding, viewMatricesBuffer);

            // Setup attributes arrays
            // Vertex attribute is always enabled
            // Color attribute is always enabled
            // TexCoord attribute is always enabled
            // Enable the rest
            GL_EnableVertexAttribArray(ATTR_TANGENT);
            GL_EnableVertexAttribArray(ATTR_BITANGENT);
            GL_EnableVertexAttribArray(ATTR_NORMAL);

            backEnd.currentSpace = null;

            for (; surf != null; surf = surf.nextOnLight)
            {
                // perform setup here that will not change over multiple interaction passes

                if (surf.space != backEnd.currentSpace)
                {
                    GL_UniformMatrix4fv(program.modelMatrix, surf.space.modelMatrix);

                    glBindBufferBase(BufferTargetARB.UniformBuffer, program.projectionMatrixBinding, projectionMatricesBuffer[RB_CalculateProjection(surf)]);
                }

                // Hack Depth Range if necessary
                var needRestoreDepthRange = false;
                if (surf.space.weaponDepthHack && surf.space.modelDepthHack == 0f) { qglDepthRangef(0f, 0.5f); needRestoreDepthRange = true; }

                // set the vertex pointers
                var ac = (DrawVert*)vertexCache.Position(surf.ambientCache);

                GL_VertexAttribPointer(program.attr_Normal, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->normal);
                GL_VertexAttribPointer(program.attr_Bitangent, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->tangents1);
                GL_VertexAttribPointer(program.attr_Tangent, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->tangents0);
                GL_VertexAttribPointer(program.attr_TexCoord, 2, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->st);
                GL_VertexAttribPointer(program.attr_Vertex, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->xyz);
                GL_VertexAttribPointer(program.attr_Color, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(DrawVert), &ac->color0);

                // this may cause RB_GLSL_DrawInteraction to be exacuted multiple times with different colors and images if the surface or light have multiple layers
                RB_GLSL_CreateSingleDrawInteractions(surf, RB_GLSL_DrawInteraction, vLight);

                // Restore the Depth Range in case it have been hacked
                if (needRestoreDepthRange) qglDepthRangef(0f, 1f);

                backEnd.currentSpace = surf.space;
            }

            backEnd.currentSpace = null;

            // Restore attributes arrays
            // Vertex attribute is always enabled
            // Color attribute is always enabled
            // TexCoord attribute is always enabled
            GL_DisableVertexAttribArray(ATTR_TANGENT);
            GL_DisableVertexAttribArray(ATTR_BITANGENT);
            GL_DisableVertexAttribArray(ATTR_NORMAL);
        }

        // the shadow volumes face INSIDE
        public static void RB_T_GLSL_Shadow(DrawSurf surf, ViewLight vLight)
        {
            if (surf.shadowCache == null) return;

            var program = backEnd.glState.currentProgram;

            // set the light position for the vertex program to project the rear surfaces
            if (surf.space != backEnd.currentSpace)
            {
                Vector4 localLight = default;

                R_GlobalPointToLocal(surf.space.modelMatrix, vLight.globalLightOrigin, out localLight.ToVec3());
                localLight.w = 0f;
                GL_Uniform4fv(program.localLightOrigin, localLight);
            }

            GL_VertexAttribPointer(program.attr_Vertex, 4, VertexAttribPointerType.Float, false, sizeof(ShadowCache), vertexCache.Position(surf.shadowCache));

            // we always draw the sil planes, but we may not need to draw the front or rear caps
            int numIndexes;
            var external = false;

            if (r_useExternalShadows.Integer == 0) numIndexes = surf.numIndexes;
            // force to no caps for testing
            else if (r_useExternalShadows.Integer == 2) numIndexes = surf.numShadowIndexesNoCaps;
            // if we aren't inside the shadow projection, no caps are ever needed needed
            else if ((surf.dsFlags & DrawSurf.DSF_VIEW_INSIDE_SHADOW) == 0) { numIndexes = surf.numShadowIndexesNoCaps; external = true; }
            else if (!vLight.viewInsideLight && (surf.shadowCapPlaneBits & DrawSurf.SHADOW_CAP_INFINITE) == 0)
            {
                // if we are inside the shadow projection, but outside the light, and drawing a non-infinite shadow, we can skip some caps
                numIndexes = (vLight.viewSeesShadowPlaneBits & surf.shadowCapPlaneBits) != 0
                    // we can see through a rear cap, so we need to draw it, but we can skip the caps on the actual surface
                    ? surf.numShadowIndexesNoFrontCaps
                    // we don't need to draw any caps
                    : surf.numShadowIndexesNoCaps;
                external = true;
            }
            // must draw everything
            else numIndexes = surf.numIndexes;

            // depth-fail stencil shadows
            if (!external)
            {
                qglStencilOpSeparate(backEnd.viewDef.isMirror ? StencilFaceDirection.Front : StencilFaceDirection.Back, StencilOp.Keep, StencilOp.Decr, StencilOp.Keep);
                qglStencilOpSeparate(backEnd.viewDef.isMirror ? StencilFaceDirection.Back : StencilFaceDirection.Front, StencilOp.Keep, StencilOp.Incr, StencilOp.Keep);
            }
            else
            {
                // traditional depth-pass stencil shadows
                qglStencilOpSeparate(backEnd.viewDef.isMirror ? StencilFaceDirection.Front : StencilFaceDirection.Back, StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
                qglStencilOpSeparate(backEnd.viewDef.isMirror ? StencilFaceDirection.Back : StencilFaceDirection.Front, StencilOp.Keep, StencilOp.Keep, StencilOp.Decr);
            }
            RB_DrawShadowElementsWithCounters(surf, numIndexes);

            #region patent-free work around
#if false
            var tri = surf;

            if (!external)
            {
                // "preload" the stencil buffer with the number of volumes that get clipped by the near or far clip plane
                qglStencilOp(StencilOp.Keep, StencilOp.Decr, StencilOp.Decr); GL_Cull(CT.FRONT_SIDED); RB_DrawShadowElementsWithCounters(tri, numIndexes);
                qglStencilOp(StencilOp.Keep, StencilOp.Incr, StencilOp.Incr); GL_Cull(CT.BACK_SIDED); RB_DrawShadowElementsWithCounters(tri, numIndexes);
            }

            // traditional depth-pass stencil shadows
            qglStencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr); GL_Cull(CT.FRONT_SIDED); RB_DrawShadowElementsWithCounters(tri, numIndexes);
            qglStencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Decr); GL_Cull(CT.BACK_SIDED); RB_DrawShadowElementsWithCounters(tri, numIndexes);
#endif
            #endregion
        }

        public static void RB_GLSL_RenderDrawSurfChainWithFunction(DrawSurf drawSurfs, Action<DrawSurf, ViewLight> triFunc, ViewLight vLight)
        {
            DrawSurf drawSurf;
            var program = backEnd.glState.currentProgram;

            backEnd.currentSpace = null;

            for (drawSurf = drawSurfs; drawSurf != null; drawSurf = drawSurf.nextOnLight)
            {
                // Change the MVP matrix if needed
                if (drawSurf.space != backEnd.currentSpace)
                {
                    GL_UniformMatrix4fv(program.modelMatrix, drawSurf.space.modelMatrix);

                    glBindBufferBase(BufferTargetARB.UniformBuffer, program.projectionMatrixBinding, projectionMatricesBuffer[RB_CalculateProjection(drawSurf)]);
                }

                // Hack Depth Range if necessary
                var needRestoreDepthRange = false;
                if (drawSurf.space.weaponDepthHack && drawSurf.space.modelDepthHack == 0f) { qglDepthRangef(0f, 0.5f); needRestoreDepthRange = true; }

                // change the scissor if needed
                if (r_useScissor.Bool && !backEnd.currentScissor.Equals(drawSurf.scissorRect))
                {
                    backEnd.currentScissor = drawSurf.scissorRect;
                    if ((backEnd.currentScissor.x2 + 1 - backEnd.currentScissor.x1) < 0f || (backEnd.currentScissor.y2 + 1 - backEnd.currentScissor.y1) < 0f) backEnd.currentScissor = backEnd.viewDef.scissor;
                    qglScissor(backEnd.viewDef.viewport.x1 + backEnd.currentScissor.x1,
                               backEnd.viewDef.viewport.y1 + backEnd.currentScissor.y1,
                               backEnd.currentScissor.x2 + 1 - backEnd.currentScissor.x1,
                               backEnd.currentScissor.y2 + 1 - backEnd.currentScissor.y1);
                }

                // render it
                triFunc(drawSurf, vLight);

                // Restore the Depth Range in case it have been hacked
                if (needRestoreDepthRange) qglDepthRangef(0f, 1f);

                backEnd.currentSpace = drawSurf.space;
            }

            backEnd.currentSpace = null;
        }

        // Stencil test should already be enabled, and the stencil buffer should have been set to 128 on any surfaces that might receive shadows
        public static void RB_GLSL_StencilShadowPass(DrawSurf drawSurfs, ViewLight vLight)
        {
            if (!r_shadows.Bool || drawSurfs == null) return;

            // Expected client GL state:
            // Vertex attribute enabled
            // Color attribute enabled

            // Use the stencil shadow shader
            var shader = GL_UseProgram(stencilShadowShader);

            glBindBufferBase(BufferTargetARB.UniformBuffer, shader.viewMatricesBinding, viewMatricesBuffer);

            // Setup attributes arrays
            // Vertex attribute is always enabled
            // Disable Color attribute (as it is enabled by default)
            // Disable TexCoord attribute (as it is enabled by default)
            GL_DisableVertexAttribArray(ATTR_COLOR);
            GL_DisableVertexAttribArray(ATTR_TEXCOORD);

            // don't write to the color buffer, just the stencil buffer
            GL_State(GLS_DEPTHMASK | GLS_COLORMASK | GLS_ALPHAMASK | GLS_DEPTHFUNC_LESS);

            if (r_shadowPolygonFactor.Float != 0 || r_shadowPolygonOffset.Float != 0) { qglPolygonOffset(r_shadowPolygonFactor.Float, -r_shadowPolygonOffset.Float); qglEnable(EnableCap.PolygonOffsetFill); }

            qglStencilFunc(StencilFunction.Always, 1, 255);

            // Culling will be done two side for shadows
            GL_Cull(CT.TWO_SIDED);

            RB_GLSL_RenderDrawSurfChainWithFunction(drawSurfs, RB_T_GLSL_Shadow, vLight);

            // Restore culling
            GL_Cull(CT.FRONT_SIDED);

            if (r_shadowPolygonFactor.Float != 0 || r_shadowPolygonOffset.Float != 0) qglDisable(EnableCap.PolygonOffsetFill);

            qglStencilFunc(StencilFunction.Gequal, 128, 255);
            qglStencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

            // Restore attributes arrays
            // Vertex attribute is always enabled
            // Re-enable Color attribute (as it is enabled by default)
            // Re-enable TexCoord attribute (as it is enabled by default)
            GL_EnableVertexAttribArray(ATTR_COLOR);
            GL_EnableVertexAttribArray(ATTR_TEXCOORD);
        }

        public static void RB_GLSL_DrawInteractions()
        {
            // for each light, perform adding and shadowing
            for (var vLight = backEnd.viewDef.viewLights; vLight != null; vLight = vLight.next)
            {
                if (vLight.lightShader.IsFogLight || vLight.lightShader.IsBlendLight) continue;
                if (vLight.localInteractions == null && vLight.globalInteractions == null && vLight.translucentInteractions == null) continue;

                // clear the stencil buffer if needed
                if (vLight.globalShadows != null || vLight.localShadows != null)
                {
                    if (r_useScissor.Bool && !backEnd.currentScissor.Equals(vLight.scissorRect))
                    {
                        backEnd.currentScissor = vLight.scissorRect;
                        if ((backEnd.currentScissor.x2 + 1 - backEnd.currentScissor.x1) < 0f || (backEnd.currentScissor.y2 + 1 - backEnd.currentScissor.y1) < 0f) backEnd.currentScissor = backEnd.viewDef.scissor;
                        qglScissor(backEnd.viewDef.viewport.x1 + backEnd.currentScissor.x1,
                            backEnd.viewDef.viewport.y1 + backEnd.currentScissor.y1,
                            backEnd.currentScissor.x2 + 1 - backEnd.currentScissor.x1,
                            backEnd.currentScissor.y2 + 1 - backEnd.currentScissor.y1);
                    }

                    qglClear((uint)AttribMask.StencilBufferBit);
                }
                // no shadows, so no need to read or write the stencil buffer we might in theory want to use GL_ALWAYS instead of disabling completely, to satisfy the invarience rules
                else qglStencilFunc(StencilFunction.Always, 128, 255);

                RB_GLSL_StencilShadowPass(vLight.globalShadows, vLight);
                RB_GLSL_CreateDrawInteractions(vLight.localInteractions, vLight);
                RB_GLSL_StencilShadowPass(vLight.localShadows, vLight);
                RB_GLSL_CreateDrawInteractions(vLight.globalInteractions, vLight);

                // translucent surfaces never get stencil shadowed
                if (r_skipTranslucent.Bool) continue;

                qglStencilFunc(StencilFunction.Always, 128, 255);
                RB_GLSL_CreateDrawInteractions(vLight.translucentInteractions, vLight, GLS_DEPTHFUNC_LESS);
            }

            // disable stencil shadow test
            qglStencilFunc(StencilFunction.Always, 128, 255);
        }

        public static void RB_T_GLSL_BasicFog(DrawSurf surf, ViewLight vLight)
        {
            var program = backEnd.glState.currentProgram;

            if (backEnd.currentSpace != surf.space)
            {
                var transfoFogPlane = stackalloc Plane[4];

                R_GlobalPlaneToLocal(surf.space.modelMatrix, global_fogPlanes[0], out transfoFogPlane[0]); transfoFogPlane[0].d += 0.5f;
                transfoFogPlane[1].a = transfoFogPlane[1].b = transfoFogPlane[1].c = 0; transfoFogPlane[1].d = 0.5f;
                R_GlobalPlaneToLocal(surf.space.modelMatrix, global_fogPlanes[2], out transfoFogPlane[2]); transfoFogPlane[2].d += IRenderSystem.FOG_ENTER;
                R_GlobalPlaneToLocal(surf.space.modelMatrix, global_fogPlanes[3], out transfoFogPlane[3]);

                Matrix4x4 fogMatrix = default;
                fogMatrix[0] = transfoFogPlane[0].ToVec4();
                fogMatrix[1] = transfoFogPlane[1].ToVec4();
                fogMatrix[2] = transfoFogPlane[2].ToVec4();
                fogMatrix[3] = transfoFogPlane[3].ToVec4();

                GL_UniformMatrix4fv(program.fogMatrix, fogMatrix);
            }

            var ac = (DrawVert*)vertexCache.Position(surf.ambientCache);

            GL_VertexAttribPointer(program.attr_Vertex, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->xyz);

            RB_DrawElementsWithCounters(surf);
        }

        public static void RB_GLSL_FogPass(DrawSurf drawSurfs, DrawSurf drawSurfs2, ViewLight vLight)
        {
            // create a surface for the light frustom triangles, which are oriented drawn side out
            var frustumTris = vLight.frustumTris;

            // if we ran out of vertex cache memory, skip it
            if (frustumTris.ambientCache == null) return;

            // Initial expected GL state:
            // Texture 0 is active, and bound to NULL
            // Vertex attribute array is enabled
            // All other attributes array are disabled
            // No shaders active

            var shader = GL_UseProgram(fogShader);

            glBindBufferBase(BufferTargetARB.UniformBuffer, shader.viewMatricesBinding, viewMatricesBuffer);

            // Setup attributes arrays
            // Vertex attribute is always enabled
            // Disable Color attribute (as it is enabled by default)
            // Disable TexCoord attribute (as it is enabled by default)
            GL_DisableVertexAttribArray(ATTR_COLOR);
            GL_DisableVertexAttribArray(ATTR_TEXCOORD);

            DrawSurf ds = new();
            ds.space = backEnd.viewDef.worldSpace;
            //ds.geo = frustumTris;
            ds.ambientCache = frustumTris.ambientCache;
            ds.indexCache = frustumTris.indexCache;
            ds.shadowCache = frustumTris.shadowCache;
            ds.numIndexes = frustumTris.numIndexes;
            ds.scissorRect = backEnd.viewDef.scissor;

            // find the current color and density of the fog
            var lightShader = vLight.lightShader;
            var regs = vLight.shaderRegisters;
            // assume fog shaders have only a single stage
            var stage = lightShader.GetStage(0);

            var lightColor = stackalloc float[4];
            lightColor[0] = regs[stage.color.registers[0]];
            lightColor[1] = regs[stage.color.registers[1]];
            lightColor[2] = regs[stage.color.registers[2]];
            lightColor[3] = regs[stage.color.registers[3]];

            // FogColor
            GL_Uniform4fv(shader.fogColor, lightColor);

            // calculate the falloff planes
            var a = lightColor[3] <= 1f
                ? -0.5f / IRenderSystem.DEFAULT_FOG_DISTANCE
                : -0.5f / lightColor[3];

            // texture 0 is the falloff image
            // It is expected to be already active
            globalImages.fogImage.Bind();

            global_fogPlanes[0].a = a * backEnd.viewDef.worldSpace.u.eyeViewMatrix2[2];
            global_fogPlanes[0].b = a * backEnd.viewDef.worldSpace.u.eyeViewMatrix2[6];
            global_fogPlanes[0].c = a * backEnd.viewDef.worldSpace.u.eyeViewMatrix2[10];
            global_fogPlanes[0].d = a * backEnd.viewDef.worldSpace.u.eyeViewMatrix2[14];

            global_fogPlanes[1].a = a * backEnd.viewDef.worldSpace.u.eyeViewMatrix2[0];
            global_fogPlanes[1].b = a * backEnd.viewDef.worldSpace.u.eyeViewMatrix2[4];
            global_fogPlanes[1].c = a * backEnd.viewDef.worldSpace.u.eyeViewMatrix2[8];
            global_fogPlanes[1].d = a * backEnd.viewDef.worldSpace.u.eyeViewMatrix2[12];

            // texture 1 is the entering plane fade correction
            GL_SelectTexture(1);
            globalImages.fogEnterImage.Bind();
            // reactive texture 0 for next passes
            GL_SelectTexture(0);

            // T will get a texgen for the fade plane, which is always the "top" plane on unrotated lights
            global_fogPlanes[2].a = 0.001f * vLight.fogPlane.a;
            global_fogPlanes[2].b = 0.001f * vLight.fogPlane.b;
            global_fogPlanes[2].c = 0.001f * vLight.fogPlane.c;
            global_fogPlanes[2].d = 0.001f * vLight.fogPlane.d;

            // S is based on the view origin
            var s = backEnd.viewDef.renderView.vieworg * global_fogPlanes[2].Normal + global_fogPlanes[2].d;
            global_fogPlanes[3].a = 0;
            global_fogPlanes[3].b = 0;
            global_fogPlanes[3].c = 0;
            global_fogPlanes[3].d = IRenderSystem.FOG_ENTER + s;

            // draw it
            GL_State(GLS_DEPTHMASK | GLS_SRCBLEND_SRC_ALPHA | GLS_DSTBLEND_ONE_MINUS_SRC_ALPHA | GLS_DEPTHFUNC_EQUAL);
            RB_GLSL_RenderDrawSurfChainWithFunction(drawSurfs, RB_T_GLSL_BasicFog, vLight);
            RB_GLSL_RenderDrawSurfChainWithFunction(drawSurfs2, RB_T_GLSL_BasicFog, vLight);

            // the light frustum bounding planes aren't in the depth buffer, so use depthfunc_less instead
            // of depthfunc_equal
            GL_State(GLS_DEPTHMASK | GLS_SRCBLEND_SRC_ALPHA | GLS_DSTBLEND_ONE_MINUS_SRC_ALPHA | GLS_DEPTHFUNC_LESS);
            GL_Cull(CT.BACK_SIDED);
            RB_GLSL_RenderDrawSurfChainWithFunction(ds, RB_T_GLSL_BasicFog, vLight);
            // Restore culling
            GL_Cull(CT.FRONT_SIDED);
            GL_State(GLS_DEPTHMASK | GLS_DEPTHFUNC_EQUAL); // Restore DepthFunc

            // Restore attributes arrays
            // Vertex attribute is always enabled
            // Re-enable Color attribute (as it is enabled by default)
            // Re-enable TexCoord attribute (as it is enabled by default)
            GL_EnableVertexAttribArray(ATTR_COLOR);
            GL_EnableVertexAttribArray(ATTR_TEXCOORD);
        }

        public static void RB_T_GLSL_FillDepthBuffer(DrawSurf surf)
        {
            var program = backEnd.glState.currentProgram;

            var shader = surf.material;
            if (
                !shader.IsDrawn
                // some deforms may disable themselves by setting numIndexes = 0
                || surf.numIndexes == 0
                // translucent surfaces don't put anything in the depth buffer and don't test against it, which makes them fail the mirror clip plane operation
                || shader.Coverage == MC.TRANSLUCENT
                || surf.ambientCache == null) return;

            // get the expressions for conditionals / color / texcoords
            var regs = surf.shaderRegisters;

            // if all stages of a material have been conditioned off, don't do anything
            int stage;
            for (stage = 0; stage < shader.NumStages; stage++)
            {
                var pStage = shader.GetStage(stage);

                // check the stage enable condition
                if (regs[pStage.conditionRegister] != 0) break;
            }
            if (stage == shader.NumStages) return;

            ///////////////////////////////////////////
            // GL Shader setup for the current surface
            ///////////////////////////////////////////

            // update the clip plane if needed
            if (backEnd.viewDef.numClipPlanes != 0 && surf.space != backEnd.currentSpace)
            {
                R_GlobalPlaneToLocal(surf.space.modelMatrix, backEnd.viewDef.clipPlanes[0], out var plane);
                plane.d += 0.5f;  // the notch is in the middle

                GL_Uniform4fv(program.clipPlane, plane);
            }

            // set polygon offset if necessary
            // NB: will be restored at the end of the process
            if (shader.TestMaterialFlag(MF.POLYGONOFFSET))
            {
                qglEnable(EnableCap.PolygonOffsetFill);
                qglPolygonOffset(r_offsetFactor.Float, r_offsetUnits.Float * shader.PolygonOffset);
            }

            // Color
            // black by default
            var color = stackalloc float[4] { 0, 0, 0, 1 };
            // subviews will just down-modulate the color buffer by overbright
            // NB: will be restored at end of the process
            if (shader.Sort == (float)SS.SUBVIEW)
            {
                GL_State(GLS_SRCBLEND_DST_COLOR | GLS_DSTBLEND_ZERO | GLS_DEPTHFUNC_LESS);
                color[0] = color[1] = color[2] = 1f; // NB: was 1f / backEnd.overBright
            }
            GL_Uniform4fv(program.glColor, color);

            // Get vertex data
            var ac = (DrawVert*)vertexCache.Position(surf.ambientCache);

            // Setup attribute pointers
            GL_VertexAttribPointer(program.attr_Vertex, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->xyz);
            GL_VertexAttribPointer(program.attr_TexCoord, 2, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->st);

            var drawSolid = shader.Coverage == MC.OPAQUE;

            ////////////////////////////////
            // Perforated surfaces handling
            ////////////////////////////////

            // we may have multiple alpha tested stages
            if (shader.Coverage == MC.PERFORATED)
            {
                // if the only alpha tested stages are condition register omitted, draw a normal opaque surface
                var didDraw = false;

                ///////////////////////
                // For each stage loop
                ///////////////////////

                // perforated surfaces may have multiple alpha tested stages
                var matrix = stackalloc float[16];
                for (stage = 0; stage < shader.NumStages; stage++)
                {
                    var pStage = shader.GetStage(stage);

                    if (!pStage.hasAlphaTest
                        // check the stage enable condition
                        || regs[pStage.conditionRegister] == 0)
                        continue;

                    // if we at least tried to draw an alpha tested stage, we won't draw the opaque surface
                    didDraw = true;

                    // set the alpha modulate
                    color[3] = regs[pStage.color.registers[3]];

                    // skip the entire stage if alpha would be black
                    if (color[3] <= 0) continue;

                    //////////////////////////
                    // GL Setup for the stage
                    //////////////////////////

                    // Color
                    // alpha testing
                    GL_Uniform4fv(program.glColor, color);
                    fixed (float* _ = &regs[pStage.alphaTestRegister]) GL_Uniform1fv(program.alphaTest, _);

                    // bind the texture
                    pStage.texture.image.Bind();

                    // Setup the texture matrix if needed
                    // NB: will be restored to identity
                    if (pStage.texture.hasMatrix)
                    {
                        RB_GetShaderTextureMatrix(surf.shaderRegisters, pStage.texture, matrix);
                        GL_UniformMatrix4fv(program.textureMatrix, matrix);
                    }

                    ///////////
                    // Draw it
                    ///////////
                    RB_DrawElementsWithCounters(surf);

                    ////////////////////////////////////////////////////////////
                    // Restore everything to an acceptable state for next stage
                    ////////////////////////////////////////////////////////////

                    // Restore identity matrix
                    if (pStage.texture.hasMatrix) GL_UniformMatrix4fv(program.textureMatrix, Matrix4x4.identity);
                }

                if (!didDraw) drawSolid = true;

                ///////////////////////////////////////////////////////////
                // Restore everything to an acceptable state for next step
                ///////////////////////////////////////////////////////////

                // Restore color alpha to opaque
                color[3] = 1;
                GL_Uniform4fv(program.glColor, color);

                // Restore alphatest always passing
                GL_Uniform1fv(program.alphaTest, global_one);

                // Restore white image binding to Tex0
                globalImages.whiteImage.Bind();
            }

            ////////////////////////////////////////
            // Normal surfaces case (non perforated)
            ////////////////////////////////////////

            // draw the entire surface solid
            ///////////
            // Draw it
            ///////////
            if (drawSolid) RB_DrawElementsWithCounters(surf);

            /////////////////////////////////////////////
            // Restore everything to an acceptable state
            /////////////////////////////////////////////

            // reset polygon offset
            if (shader.TestMaterialFlag(MF.POLYGONOFFSET)) qglDisable(EnableCap.PolygonOffsetFill);

            // Restore blending
            if (shader.Sort == (float)SS.SUBVIEW) GL_State(GLS_DEPTHFUNC_LESS);
        }

        // If we are rendering a subview with a near clip plane, use a second texture to force the alpha test to fail when behind that clip plane
        static void RB_GLSL_FillDepthBuffer(DrawSurf[] drawSurfs, int numDrawSurfs)
        {
            // if we are just doing 2D rendering, no need to fill the depth buffer
            if (backEnd.viewDef.viewEntitys == null) return;

            ////////////////////////////////////////
            // GL Shader setup for the current pass (ie. common to each surface)
            ////////////////////////////////////////

            // Expected client GL State at this point
            // Tex0 active
            // Vertex attribute enabled
            // Color attribute enabled
            // Shader AlphaTest is one
            // Shader Texture Matrix is Identity

            // If clip planes are enabled in the view, use he "Clip" version of zfill shader and enable the second texture for mirror plane clipping if needed
            if (backEnd.viewDef.numClipPlanes != 0)
            {
                // Use he zfillClip shader
                GL_UseProgram(zfillClipShader);

                // Bind the Texture 1 to alphaNotchImage
                GL_SelectTexture(1);
                globalImages.alphaNotchImage.Bind();

                // Be sure to reactivate Texture 0, as it will be bound right after
                GL_SelectTexture(0);
            }
            // If no clip planes, just use the regular zfill shader
            else GL_UseProgram(zfillShader);

            var program = backEnd.glState.currentProgram;

            glBindBufferBase(BufferTargetARB.UniformBuffer, program.viewMatricesBinding, viewMatricesBuffer);

            // Setup attributes arrays
            // Vertex attribute is always enabled
            // TexCoord attribute is always enabled
            // Disable Color attribute (as it is enabled by default)
            GL_DisableVertexAttribArray(ATTR_COLOR);

            // Texture 0 will be used for alpha tested surfaces. It should be already active.
            // Bind it to white image by default
            globalImages.whiteImage.Bind();

            // Decal surfaces may enable polygon offset
            // GAB Note: Looks like it is not needed, because in case of offsetted surface we will use the offset value of the surface
            // qglPolygonOffset(r_offsetFactor.GetFloat(), r_offsetUnits.GetFloat());

            // Depth func to LESS
            GL_State(GLS_DEPTHFUNC_LESS);

            // Enable stencil test if we are going to be using it for shadows. If we didn't do this, it would be legal behavior to get z fighting from the ambient pass and the light passes.
            qglEnable(EnableCap.StencilTest);
            qglStencilFunc(StencilFunction.Always, 1, 255);

            //////////////////////////
            // For each surfaces loop
            //////////////////////////

            // Optimization to only change MVP matrix when needed
            backEnd.currentSpace = null;

            for (var i = 0; i < numDrawSurfs; i++)
            {
                var drawSurf = drawSurfs[i];

                ///////////////////////////////////////////
                // GL shader setup for the current surface
                ///////////////////////////////////////////

                // Change the MVP matrix if needed
                if (drawSurf.space != backEnd.currentSpace)
                {
                    GL_UniformMatrix4fv(program.modelMatrix, drawSurf.space.modelMatrix);

                    glBindBufferBase(BufferTargetARB.UniformBuffer, program.projectionMatrixBinding, projectionMatricesBuffer[RB_CalculateProjection(drawSurf)]);
                }

                // Hack Depth Range if necessary
                var needRestoreDepthRange = false;
                if (drawSurf.space.weaponDepthHack && drawSurf.space.modelDepthHack == 0f) { qglDepthRangef(0, 0.5f); needRestoreDepthRange = true; }

                // change the scissor if needed
                if (r_useScissor.Bool && !backEnd.currentScissor.Equals(drawSurf.scissorRect))
                {
                    backEnd.currentScissor = drawSurf.scissorRect;
                    if ((backEnd.currentScissor.x2 + 1 - backEnd.currentScissor.x1) < 0f || (backEnd.currentScissor.y2 + 1 - backEnd.currentScissor.y1) < 0f) backEnd.currentScissor = backEnd.viewDef.scissor;
                    qglScissor(backEnd.viewDef.viewport.x1 + backEnd.currentScissor.x1,
                        backEnd.viewDef.viewport.y1 + backEnd.currentScissor.y1,
                        backEnd.currentScissor.x2 + 1 - backEnd.currentScissor.x1,
                        backEnd.currentScissor.y2 + 1 - backEnd.currentScissor.y1);
                }

                ////////////////////
                // Do the real work
                ////////////////////
                RB_T_GLSL_FillDepthBuffer(drawSurf);

                /////////////////////////////////////////////
                // Restore everything to an acceptable state
                /////////////////////////////////////////////
                if (needRestoreDepthRange) qglDepthRangef(0f, 1f);

                // Let's change space for next iteration
                backEnd.currentSpace = drawSurf.space;
            }

            /////////////////////////////////////////////
            // Restore everything to an acceptable state
            /////////////////////////////////////////////
            // Restore current space to NULL
            backEnd.currentSpace = null;

            // Restore attributes arrays
            // Vertex attribute is always enabled
            // TexCoord attribute is always enabled
            // Re-enable Color attribute (as it is enabled by default)
            GL_EnableVertexAttribArray(ATTR_COLOR);
        }

        // This is also called for the generated 2D rendering
        public static void RB_GLSL_T_RenderShaderPasses(DrawSurf surf, uint projection)
        {
            // usefull pointers
            var shader = surf.material;
            //tri = surf;

            //////////////
            // Skip cases
            //////////////

#if NO_LIGHT
            if (!r_noLight.Bool)
#endif
            if (!shader.HasAmbient) return;
            if (
                shader.IsPortalSky
                // some deforms may disable themselves by setting numIndexes = 0
                || surf.numIndexes == 0
                || surf.ambientCache == null) return;


            ///////////////////////////////////
            // GL shader setup for the surface
            // (ie. common to each Stage)
            ///////////////////////////////////

            // change the scissor if needed
            if (r_useScissor.Bool && !backEnd.currentScissor.Equals(surf.scissorRect))
            {
                backEnd.currentScissor = surf.scissorRect;
                if ((backEnd.currentScissor.x2 + 1 - backEnd.currentScissor.x1) < 0f || (backEnd.currentScissor.y2 + 1 - backEnd.currentScissor.y1) < 0f) backEnd.currentScissor = backEnd.viewDef.scissor;
                qglScissor(backEnd.viewDef.viewport.x1 + backEnd.currentScissor.x1,
                    backEnd.viewDef.viewport.y1 + backEnd.currentScissor.y1,
                    backEnd.currentScissor.x2 + 1 - backEnd.currentScissor.x1,
                    backEnd.currentScissor.y2 + 1 - backEnd.currentScissor.y1);
            }

            // set polygon offset if necessary
            // NB: must be restored at end of process
            if (shader.TestMaterialFlag(MF.POLYGONOFFSET))
            {
                qglEnable(EnableCap.PolygonOffsetFill);
                qglPolygonOffset(r_offsetFactor.Float, r_offsetUnits.Float * shader.PolygonOffset);
            }

            // set face culling appropriately
            GL_Cull(shader.CullType);

            // Location of vertex attributes data
            var ac = (DrawVert*)vertexCache.Position(surf.ambientCache);

            // get the expressions for conditionals / color / texcoords
            var regs = surf.shaderRegisters;

            // Caches to set per surface shader GL state only when necessary
            var bMVPSet = stackalloc bool[TG.GLASSWARP - TG.EXPLICIT]; Unsafe.InitBlock(bMVPSet, 0, (TG.GLASSWARP - TG.EXPLICIT) * sizeof(bool));
            var bVASet = stackalloc bool[TG.GLASSWARP - TG.EXPLICIT]; Unsafe.InitBlock(bVASet, 0, (TG.GLASSWARP - TG.EXPLICIT) * sizeof(bool));

            // precompute the local view origin (might be needed for some texgens)
            Vector4 localViewOrigin = default;
            R_GlobalPointToLocal(surf.space.modelMatrix, backEnd.viewDef.renderView.vieworg, out localViewOrigin.ToVec3());
            localViewOrigin.w = 1f;

            ///////////////////////
            // For each stage loop
            ///////////////////////
            float* color = stackalloc float[4],
                matrix = stackalloc float[16],
                texturematrix = stackalloc float[16];
            ShaderProgram program;
            for (var stage = 0; stage < shader.NumStages; stage++)
            {
                var pStage = shader.GetStage(stage);

                ///////////////
                // Skip cases
                ///////////////
                NewShaderStage newStage;
                if (
                    // check the enable condition
                    regs[pStage.conditionRegister] == 0
                    // skip the stages involved in lighting
                    || pStage.lighting != SL.AMBIENT
                    // skip if the stage is ( GL_ZERO, GL_ONE ), which is used for some alpha masks
                    || ((pStage.drawStateBits & (GLS_SRCBLEND_BITS | GLS_DSTBLEND_BITS)) == (GLS_SRCBLEND_ZERO | GLS_DSTBLEND_ONE))
                    // see if we are a new-style stage, new style stages: Not implemented in GLSL yet!
                    || (newStage = pStage.newStage) != null)
                    continue;

                // old style stages

                /////////////////////////
                // Additional skip cases
                /////////////////////////

                // precompute the color
                color[0] = regs[pStage.color.registers[0]];
                color[1] = regs[pStage.color.registers[1]];
                color[2] = regs[pStage.color.registers[2]];
                color[3] = regs[pStage.color.registers[3]];

                if (
                    // skip the entire stage if an add would be black
                    ((pStage.drawStateBits & (GLS_SRCBLEND_BITS | GLS_DSTBLEND_BITS)) == (GLS_SRCBLEND_ONE | GLS_DSTBLEND_ONE) && color[0] <= 0 && color[1] <= 0 && color[2] <= 0)
                    // skip the entire stage if a blend would be completely transparent
                    || ((pStage.drawStateBits & (GLS_SRCBLEND_BITS | GLS_DSTBLEND_BITS)) == (GLS_SRCBLEND_SRC_ALPHA | GLS_DSTBLEND_ONE_MINUS_SRC_ALPHA) && color[3] <= 0))
                    continue;

                /////////////////////////////////
                // GL shader setup for the stage
                /////////////////////////////////
                // The very first thing we need to do before going down into GL is to choose he correct GLSL shader depending on
                // the associated TexGen. Then, setup its specific uniforms/attribs, and then only we can setup the common uniforms/attribs

                if (pStage.texture.texgen == TG.DIFFUSE_CUBE)
                {
                    // Not used in game, but implemented because trivial

                    // This is diffuse cube mapping
                    program = GL_UseProgram(diffuseCubeShader);

                    glBindBufferBase(BufferTargetARB.UniformBuffer, program.viewMatricesBinding, viewMatricesBuffer);

                    // Possible that normals should be transformed by a normal matrix in the shader ? I am not sure...

                    // Setup texcoord array to use the normals
                    GL_VertexAttribPointer(program.attr_TexCoord, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->normal);

                    // Setup the texture matrix
                    if (pStage.texture.hasMatrix)
                    {
                        RB_GetShaderTextureMatrix(surf.shaderRegisters, pStage.texture, matrix);
                        GL_UniformMatrix4fv(program.textureMatrix, matrix);
                    }
                }
                else if (pStage.texture.texgen == TG.SKYBOX_CUBE)
                {
                    // This is skybox cube mapping
                    program = GL_UseProgram(skyboxCubeShader);

                    glBindBufferBase(BufferTargetARB.UniformBuffer, program.viewMatricesBinding, viewMatricesBuffer);

                    // Disable TexCoord attribute
                    GL_DisableVertexAttribArray(ATTR_TEXCOORD);

                    // Setup the local view origin uniform
                    GL_Uniform4fv(program.localViewOrigin, localViewOrigin);

                    // Setup the texture matrix
                    if (pStage.texture.hasMatrix)
                    {
                        RB_GetShaderTextureMatrix(surf.shaderRegisters, pStage.texture, matrix);
                        GL_UniformMatrix4fv(program.textureMatrix, matrix);
                    }
                }
                else if (pStage.texture.texgen == TG.WOBBLESKY_CUBE)
                {
                    // This is skybox cube mapping, with special texture matrix
                    program = GL_UseProgram(skyboxCubeShader);

                    glBindBufferBase(BufferTargetARB.UniformBuffer, program.viewMatricesBinding, viewMatricesBuffer);

                    // Disable TexCoord attribute
                    GL_DisableVertexAttribArray(ATTR_TEXCOORD);

                    // Setup the local view origin uniform
                    GL_Uniform4fv(program.localViewOrigin, localViewOrigin);

                    // Setup the texture matrix
                    // Note: here, we combine the shader texturematrix and precomputed wobblesky matrix
                    if (pStage.texture.hasMatrix)
                    {
                        RB_GetShaderTextureMatrix(surf.shaderRegisters, pStage.texture, texturematrix);
                        myGlMultMatrix(texturematrix, surf.wobbleTransform, matrix);
                        GL_UniformMatrix4fv(program.textureMatrix, matrix);
                    }
                    else GL_UniformMatrix4fv(program.textureMatrix, surf.wobbleTransform);
                }
                // Not used in game, so not implemented
                else if (pStage.texture.texgen == TG.SCREEN) continue;
                // Not used in game, so not implemented
                else if (pStage.texture.texgen == TG.SCREEN2) continue;
                // Not used in game, so not implemented. The shader code is even not present in original D3 data
                else if (pStage.texture.texgen == TG.GLASSWARP) continue;
                else if (pStage.texture.texgen == TG.REFLECT_CUBE)
                {
                    // This is reflection cubemapping
                    program = GL_UseProgram(reflectionCubeShader);

                    glBindBufferBase(BufferTargetARB.UniformBuffer, program.viewMatricesBinding, viewMatricesBuffer);

                    // NB: in original D3, if the surface had a bump map it would lead to the "Bumpy reflection cubemaping" shader being used.
                    // This is not implemented for now, we only do standard reflection cubemaping. Visual difference is really minor.

                    // Setup texcoord array to use the normals
                    GL_VertexAttribPointer(program.attr_TexCoord, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->normal);

                    // Setup the viewMatrix, we will need it to compute the reflection
                    fixed (float* _ = surf.space.u.eyeViewMatrix2) GL_UniformMatrix4fv(program.modelViewMatrix, _);

                    // Setup the texture matrix like original D3 code does: using the transpose viewMatrix of the view
                    // NB: this is curious, not sure why this is done like this....
                    fixed (float* _ = backEnd.viewDef.worldSpace.u.eyeViewMatrix2) R_TransposeGLMatrix(_, matrix);
                    GL_UniformMatrix4fv(program.textureMatrix, matrix);
                }
                else
                { // TG_EXPLICIT
                  // Otherwise, this is just regular surface shader with explicit texcoords
                    program = GL_UseProgram(diffuseMapShader);

                    glBindBufferBase(BufferTargetARB.UniformBuffer, program.viewMatricesBinding, viewMatricesBuffer);

                    // Setup the TexCoord pointer
                    GL_VertexAttribPointer(program.attr_TexCoord, 2, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->st);

                    // Setup the texture matrix
                    if (pStage.texture.hasMatrix)
                    {
                        RB_GetShaderTextureMatrix(surf.shaderRegisters, pStage.texture, matrix);
                        GL_UniformMatrix4fv(program.textureMatrix, matrix);
                    }
                }

                // Now we have a shader, we can setup the uniforms and attribute pointers common to all kind of shaders
                // The specifics have already been done in the shader selection code (see above)

                // Non-stage dependent state (per drawsurf, may be done once per GL shader)
                program = backEnd.glState.currentProgram;
                {
                    // Vertex Attributes
                    if (!bVASet[(int)pStage.texture.texgen])
                    {
                        // Setup the Vertex Attrib pointer
                        GL_VertexAttribPointer(program.attr_Vertex, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->xyz);

                        // Setup the Color pointer
                        GL_VertexAttribPointer(program.attr_Color, 4, VertexAttribPointerType.UnsignedByte, false, sizeof(DrawVert), &ac->color0);

                        bVASet[(int)pStage.texture.texgen] = true;
                    }

                    // MVP
                    if (!bMVPSet[(int)pStage.texture.texgen])
                    {
                        GL_UniformMatrix4fv(program.modelMatrix, surf.space.modelMatrix);

                        glBindBufferBase(BufferTargetARB.UniformBuffer, program.projectionMatrixBinding, projectionMatricesBuffer[projection]);

                        bMVPSet[(int)pStage.texture.texgen] = true;
                    }
                }

                // Stage dependent state

                // Setup the Color uniform
                GL_Uniform4fv(program.glColor, color);

                // Setup the Color modulation
                switch (pStage.vertexColor)
                {
                    case SVC.MODULATE:
                        GL_Uniform1fv(program.colorModulate, global_oneScaled);
                        GL_Uniform1fv(program.colorAdd, global_zero);
                        break;
                    case SVC.INVERSE_MODULATE:
                        GL_Uniform1fv(program.colorModulate, global_negOneScaled);
                        GL_Uniform1fv(program.colorAdd, global_one);
                        break;
                    // This is already the default values (zero, one)
                    case SVC.IGNORE: default: break;
                }

                // bind the texture (this will be either a dynamic texture, or a static one)
                fixed (float* _ = regs) RB_BindVariableStageImage(pStage.texture, _);

                // set the state
                GL_State(pStage.drawStateBits);

                // set privatePolygonOffset if necessary
                if (pStage.privatePolygonOffset != 0f)
                {
                    qglEnable(EnableCap.PolygonOffsetFill);
                    qglPolygonOffset(r_offsetFactor.Float, r_offsetUnits.Float * pStage.privatePolygonOffset);
                }

#if NO_LIGHT
                if (r_noLight.Bool)
                    GL_State(pStage.drawStateBits != 9000 ? pStage.drawStateBits : shader.TestMaterialFlag(MF.POLYGONOFFSET)
                        ? GLS_SRCBLEND_ONE | GLS_DSTBLEND_ONE | GLS_DEPTHFUNC_LESS
                        : GLS_SRCBLEND_ONE | GLS_DSTBLEND_ONE | GLS_DEPTHFUNC_LESS);
#endif

                /////////////////////
                // Draw the surface!
                /////////////////////
                RB_DrawElementsWithCounters(surf);

                /////////////////////////////////////////////
                // Restore everything to an acceptable state
                /////////////////////////////////////////////

                // Disable the other attributes array
                if (pStage.texture.texgen == TG.DIFFUSE_CUBE)
                {
                    // Restore identity to the texture matrix
                    if (pStage.texture.hasMatrix) GL_UniformMatrix4fv(program.textureMatrix, Matrix4x4.identity);
                }
                else if (pStage.texture.texgen == TG.SKYBOX_CUBE)
                {
                    // Reenable TexCoord attribute
                    GL_EnableVertexAttribArray(ATTR_TEXCOORD);

                    // Restore identity to the texture matrix
                    if (pStage.texture.hasMatrix) GL_UniformMatrix4fv(program.textureMatrix, Matrix4x4.identity);
                }
                else if (pStage.texture.texgen == TG.WOBBLESKY_CUBE)
                {
                    // Reenable TexCoord attribute
                    GL_EnableVertexAttribArray(ATTR_TEXCOORD);

                    // Restore identity to the texture matrix (shall be done each time, as there is the wobblesky transform combined inside)
                    GL_UniformMatrix4fv(program.textureMatrix, Matrix4x4.identity);
                }
                else if (pStage.texture.texgen == TG.SCREEN) { }
                else if (pStage.texture.texgen == TG.SCREEN2) { }
                else if (pStage.texture.texgen == TG.GLASSWARP) { }
                else if (pStage.texture.texgen == TG.REFLECT_CUBE)
                {
                    // Restore identity to the texture matrix (shall be done each time)
                    GL_UniformMatrix4fv(program.textureMatrix, Matrix4x4.identity);
                }
                // Restore identity to the texture matrix
                else
                {
                    if (pStage.texture.hasMatrix) GL_UniformMatrix4fv(program.textureMatrix, Matrix4x4.identity);
                }

                // unset privatePolygonOffset if necessary
                if (pStage.privatePolygonOffset != 0 && !surf.material.TestMaterialFlag(MF.POLYGONOFFSET)) qglDisable(EnableCap.PolygonOffsetFill);
                else if (pStage.privatePolygonOffset != 0 && surf.material.TestMaterialFlag(MF.POLYGONOFFSET)) qglPolygonOffset(r_offsetFactor.Float, r_offsetUnits.Float * shader.PolygonOffset);

                // Restore color modulation state to default values
                if (pStage.vertexColor != SVC.IGNORE)
                {
                    GL_Uniform1fv(program.colorModulate, global_zero);
                    GL_Uniform1fv(program.colorAdd, global_one);
                }

                // Don't touch the rest, as this will either reset by the next stage, or handled by end of this method
            }

            /////////////////////////////////////////////
            // Restore everything to an acceptable state
            /////////////////////////////////////////////

            // reset polygon offset
            if (shader.TestMaterialFlag(MF.POLYGONOFFSET)) qglDisable(EnableCap.PolygonOffsetFill);
        }

        // Draw non-light dependent passes
        public static int RB_GLSL_DrawShaderPasses(Span<DrawSurf> drawSurfs, int numDrawSurfs)
        {
            // only obey skipAmbient if we are rendering a view
            if (backEnd.viewDef.viewEntitys != null && r_skipAmbient.Bool) return numDrawSurfs;

            // if we are about to draw the first surface that needs the rendering in a texture, copy it over
            if (drawSurfs[0].material.Sort >= (float)SS.POST_PROCESS)
            {
                if (r_skipPostProcess.Bool) return 0;

                // only dump if in a 3d view
                //if (backEnd.viewDef.viewEntitys != null)
                //    globalImages.currentRenderImage.CopyFramebuffer(
                //        backEnd.viewDef.viewport.x1, backEnd.viewDef.viewport.y1,
                //        backEnd.viewDef.viewport.x2 - backEnd.viewDef.viewport.x1 + 1, backEnd.viewDef.viewport.y2 - backEnd.viewDef.viewport.y1 + 1,
                //        true);

                backEnd.currentRenderCopied = true;
            }

            ////////////////////////////////////////
            // GL shader setup for the current pass
            // (ie. common to each surface)
            ////////////////////////////////////////

            // Texture 0 is expected to be active

            // Setup attributes arrays
            // Vertex attribute is always enabled
            // Color attribute is always enabled
            // Texcoord attribute is always enabled

            /////////////////////////
            // For each surface loop
            /////////////////////////

            var projection = uint.MaxValue;
            backEnd.currentSpace = null;

            int i;
            for (i = 0; i < numDrawSurfs; i++)
            {
                //////////////
                // Skip cases
                //////////////
                if (
                    drawSurfs[i].material.SuppressInSubview()
                    || (backEnd.viewDef.isXraySubview && drawSurfs[i].space.entityDef != null && drawSurfs[i].space.entityDef.parms.xrayIndex != 2))
                    continue;

                // we need to draw the post process shaders after we have drawn the fog lights
                if (drawSurfs[i].material.Sort >= (float)SS.POST_PROCESS && !backEnd.currentRenderCopied) break;

                // Change the MVP matrix if needed
                if (drawSurfs[i].space != backEnd.currentSpace) projection = RB_CalculateProjection(drawSurfs[i]); // We can't set the uniform now, as we still don't know which shader to use

                // Hack Depth Range if necessary
                var needRestoreDepthRange = false;
                if (drawSurfs[i].space.weaponDepthHack && drawSurfs[i].space.modelDepthHack == 0f) { qglDepthRangef(0f, 0.5f); needRestoreDepthRange = true; }

                ////////////////////
                // Do the real work
                ////////////////////
                RB_GLSL_T_RenderShaderPasses(drawSurfs[i], projection);

                if (needRestoreDepthRange) qglDepthRangef(0f, 1f);

                backEnd.currentSpace = drawSurfs[i].space;
            }

            /////////////////////////////////////////////
            // Restore everything to an acceptable state
            /////////////////////////////////////////////

            backEnd.currentSpace = null;

            // Restore culling
            GL_Cull(CT.FRONT_SIDED);

            // Restore attributes arrays
            // Vertex attribute is always enabled
            // Color attribute is always enabled
            // Texcoord attribute is always enabled

            // Trashed state:
            //   Current Program
            //   Tex0 binding

            // Return the counter of drawn surfaces
            return i;
        }

        public static void RB_T_GLSL_BlendLight(DrawSurf surf, ViewLight vLight)
        {
            var program = backEnd.glState.currentProgram;
            //var tri = surf.geo;

            ////////////
            // GL setup
            ////////////

            // Shader uniforms

            // Setup the fogMatrix as being the local Light Projection
            // Only do this once per space
            if (backEnd.currentSpace != surf.space)
            {
                var lightProject = stackalloc Plane[4];
                for (var i = 0; i < 4; i++) R_GlobalPlaneToLocal(surf.space.modelMatrix, vLight.lightProject[i], out lightProject[i]);

                Matrix4x4 fogMatrix = default;
                fogMatrix[0] = lightProject[0].ToVec4();
                fogMatrix[1] = lightProject[1].ToVec4();
                fogMatrix[2] = lightProject[2].ToVec4();
                fogMatrix[3] = lightProject[3].ToVec4();
                GL_UniformMatrix4fv(program.fogMatrix, fogMatrix);
            }

            // Attributes pointers

            // This gets used for both blend lights and shadow draws
            if (surf.ambientCache != null)
            {
                var ac = (DrawVert*)vertexCache.Position(surf.ambientCache);
                GL_VertexAttribPointer(program.attr_Vertex, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), ac->xyz);
            }
            else if (surf.shadowCache != null)
            {
                var sc = (ShadowCache*)vertexCache.Position(surf.shadowCache);
                GL_VertexAttribPointer(program.attr_Vertex, 3, VertexAttribPointerType.Float, false, sizeof(DrawVert), sc->xyz);
            }

            ////////////////////
            // Draw the surface
            ////////////////////
            RB_DrawElementsWithCounters(surf);
        }

        // Dual texture together the falloff and projection texture with a blend mode to the framebuffer, instead of interacting with the surface texture
        public static void RB_GLSL_BlendLight(DrawSurf drawSurfs, DrawSurf drawSurfs2, ViewLight vLight)
        {
            var lightShader = vLight.lightShader;
            var regs = vLight.shaderRegisters;

            //////////////
            // Skip Cases
            //////////////
            if (
                drawSurfs == null
                || r_skipBlendLights.Bool)
                return;

            ////////////////////////////////////
            // GL setup for the current pass
            // (ie. common to all Light Stages)
            ////////////////////////////////////

            // Use blendLight shader
            var program = GL_UseProgram(blendLightShader);

            glBindBufferBase(BufferTargetARB.UniformBuffer, program.viewMatricesBinding, viewMatricesBuffer);

            // Texture 1 will get the falloff texture
            GL_SelectTexture(1); vLight.falloffImage.Bind();

            // Texture 0 will get the projected texture
            GL_SelectTexture(0);

            ////////////////////////
            // For each Light Stage
            ////////////////////////

            int i;
            float* color = stackalloc float[4], matrix = stackalloc float[16];
            for (i = 0; i < lightShader.NumStages; i++)
            {
                var stage = lightShader.GetStage(i);

                if (regs[stage.conditionRegister] == 0) continue;

                ////////////////////////////////////////
                // GL setup for the current Light Stage
                // (ie. common to all surfaces)
                ////////////////////////////////////////

                // Global GL state

                // Setup the drawState
                GL_State(GLS_DEPTHMASK | stage.drawStateBits | GLS_DEPTHFUNC_EQUAL);

                // Bind the projected texture
                stage.texture.image.Bind();

                // Shader Uniforms

                // Setup the texture matrix
                if (stage.texture.hasMatrix)
                {
                    RB_GetShaderTextureMatrix(regs, stage.texture, matrix);
                    GL_UniformMatrix4fv(program.textureMatrix, matrix);
                }

                // Setup the Fog Color
                color[0] = regs[stage.color.registers[0]];
                color[1] = regs[stage.color.registers[1]];
                color[2] = regs[stage.color.registers[2]];
                color[3] = regs[stage.color.registers[3]];
                GL_Uniform4fv(program.fogColor, color);

                ////////////////////
                // Do the Real Work
                ////////////////////

                RB_GLSL_RenderDrawSurfChainWithFunction(drawSurfs, RB_T_GLSL_BlendLight, vLight);
                RB_GLSL_RenderDrawSurfChainWithFunction(drawSurfs2, RB_T_GLSL_BlendLight, vLight);

                ////////////////////
                // GL state restore
                ////////////////////

                // Restore texture matrix to identity
                if (stage.texture.hasMatrix) GL_UniformMatrix4fv(program.textureMatrix, Matrix4x4.identity);
            }
        }

        public static void RB_GLSL_FogAllLights()
        {
            //////////////
            // Skip Cases
            //////////////

            //GB Never do this on Doom3Quest
            return;

            if (r_skipFogLights.Bool || backEnd.viewDef.isXraySubview) return; // dont fog in xray mode

            /////////////////////////////////////////////
            // GL setup for the current pass
            // (ie. common to both fog and blend lights)
            /////////////////////////////////////////////

            // Disable Stencil Test
            qglDisable(EnableCap.StencilTest);

            // Disable TexCoord array
            // Disable Color array
            GL_DisableVertexAttribArray(ATTR_TEXCOORD);
            GL_DisableVertexAttribArray(ATTR_COLOR);

            //////////////////
            // For each Light
            //////////////////

            ViewLight vLight;
            for (vLight = backEnd.viewDef.viewLights; vLight != null; vLight = vLight.next)
            {
                //////////////
                // Skip Cases
                //////////////

                // We are only interested in Fog and Blend lights
                if (!vLight.lightShader.IsFogLight && !vLight.lightShader.IsBlendLight) continue;

                ///////////////////////
                // Do the Light passes
                ///////////////////////
                if (vLight.lightShader.IsFogLight) RB_GLSL_FogPass(vLight.globalInteractions, vLight.localInteractions, vLight);
                else if (vLight.lightShader.IsBlendLight) RB_GLSL_BlendLight(vLight.globalInteractions, vLight.localInteractions, vLight);
            }

            ////////////////////
            // GL state restore
            ////////////////////

            // Re-enable TexCoord array
            // Re-enable Color array
            GL_EnableVertexAttribArray(ATTR_TEXCOORD);
            GL_EnableVertexAttribArray(ATTR_COLOR);

            // Re-enable Stencil Test
            qglEnable(EnableCap.StencilTest);
        }

        static float[] RB_GLSL_PrepareShaders_defaultProjection = new float[16];
        public static void RB_GLSL_PrepareShaders()
        {
            // No shaders set by default
            GL_UseProgram(null);

            //Set up the buffers that won't change this frame
            fixed (float* _ = backEnd.viewDef.worldSpace.u.viewMatrix) GL_ViewMatricesUniformBuffer(_);

            // We only need to do the following if the default projection changes
            fixed (float* defaultProjectionF = RB_GLSL_PrepareShaders_defaultProjection)
            fixed (float* projectionMatrixF = backEnd.viewDef.projectionMatrix)
                if (UnsafeX.CompareBlock(defaultProjectionF, projectionMatrixF, 16 * sizeof(float)) != 0)
                {
                    // Take a copy of the default projection
                    Unsafe.CopyBlock(defaultProjectionF, projectionMatrixF, 16 * sizeof(float));

                    var matrix = stackalloc float[16];
                    Unsafe.InitBlock(matrix, 0, 16 * sizeof(float));
                    matrix[0] = 2f / 640f;
                    matrix[5] = -2f / 480f;
                    matrix[10] = -2f / 1f;
                    matrix[12] = -1f;
                    matrix[13] = 1f;
                    matrix[14] = -1f;
                    matrix[15] = 1f;

                    //0 is ortho projection matrix
                    GL_ProjectionMatricesUniformBuffer(projectionMatricesBuffer[ORTHO_PROJECTION], matrix);

                    // 1 is unadjusted projection matrix
                    GL_ProjectionMatricesUniformBuffer(projectionMatricesBuffer[NORMAL_PROJECTION], projectionMatrixF);

                    // 2 is weapon depth hack projection
                    var projection = stackalloc float[16];
                    RB_ComputeProjection(true, 0f, projection);
                    GL_ProjectionMatricesUniformBuffer(projectionMatricesBuffer[WEAPON_PROJECTION], projection);

                    // 3+ ore model depth hack projections
                    for (var i = 0; i <= NUM_DEPTH_HACK_PROJECTIONS; ++i)
                    {
                        var depthHack = (i + 1) / (NUM_DEPTH_HACK_PROJECTIONS + 1f);
                        RB_ComputeProjection(false, depthHack, projection);
                        GL_ProjectionMatricesUniformBuffer(projectionMatricesBuffer[DEPTH_HACK_PROJECTION + i], projection);
                    }

                    projectionMatricesSet = true;
                }

            // Always enable the vertex, color and texcoord attributes arrays
            GL_EnableVertexAttribArray(ATTR_VERTEX);
            GL_EnableVertexAttribArray(ATTR_COLOR);
            GL_EnableVertexAttribArray(ATTR_TEXCOORD);
            // Disable the other arrays
            GL_DisableVertexAttribArray(ATTR_NORMAL);
            GL_DisableVertexAttribArray(ATTR_TANGENT);
            GL_DisableVertexAttribArray(ATTR_BITANGENT);
        }
    }
}