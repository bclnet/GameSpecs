using OpenStack.Graphics.ParticleSystem;
using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.OpenGL.Renderer1.Renderers
{
    public class SpritesParticleRenderer : IParticleRenderer
    {
        const int VertexSize = 9;

        readonly Shader _shader;
        readonly int _quadVao;
        readonly int _texture;

        readonly TextureSequences _textureSequences;
        readonly float _animationRate;

        readonly bool _additive;
        readonly float _overbrightFactor;
        readonly long _orientationType;

        float[] _rawVertices;
        QuadIndexBuffer _quadIndices;
        int _vertexBufferHandle;

        public SpritesParticleRenderer(IDictionary<string, object> keyValues, IOpenGLGraphic graphic)
        {
            _shader = graphic.LoadShader("vrf.particle.sprite");
            _quadIndices = graphic.QuadIndices;

            // The same quad is reused for all particles
            _quadVao = SetupQuadBuffer();

            string textureName = null;
            if (keyValues.ContainsKey("m_hTexture")) textureName = keyValues.Get<string>("m_hTexture");
            else if (keyValues.ContainsKey("m_vecTexturesInput"))
            {
                var textures = keyValues.GetArray("m_vecTexturesInput");
                if (textures.Length > 0) textureName = textures[0].Get<string>("m_hTexture");
            }

            if (textureName != null)
            {
                var texture = graphic.LoadTexture(textureName, out var info);
                _texture = texture;
                _textureSequences = info?.Get<TextureSequences>("sequences");
            }
            else _texture = graphic.TextureManager.DefaultTexture;

            _additive = keyValues.Get<bool>("m_bAdditive");
            _overbrightFactor = keyValues.GetFloat("m_flOverbrightFactor", 1f);
            _orientationType = keyValues.GetInt64("m_nOrientationType");
            _animationRate = keyValues.GetFloat("m_flAnimationRate", .1f);
        }

        int SetupQuadBuffer()
        {
            GL.UseProgram(_shader.Program);

            // Create and bind VAO
            var vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            _vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferHandle);

            var stride = sizeof(float) * VertexSize;
            var positionAttributeLocation = GL.GetAttribLocation(_shader.Program, "aVertexPosition");
            GL.VertexAttribPointer(positionAttributeLocation, 3, VertexAttribPointerType.Float, false, stride, 0);
            var colorAttributeLocation = GL.GetAttribLocation(_shader.Program, "aVertexColor");
            GL.VertexAttribPointer(colorAttributeLocation, 4, VertexAttribPointerType.Float, false, stride, sizeof(float) * 3);
            var uvAttributeLocation = GL.GetAttribLocation(_shader.Program, "aTexCoords");
            GL.VertexAttribPointer(uvAttributeLocation, 2, VertexAttribPointerType.Float, false, stride, sizeof(float) * 7);

            GL.EnableVertexAttribArray(positionAttributeLocation);
            GL.EnableVertexAttribArray(colorAttributeLocation);
            GL.EnableVertexAttribArray(uvAttributeLocation);

            GL.BindVertexArray(0);

            return vao;
        }

        //static (int TextureIndex, Texture TextureData) LoadTexture(string textureName, GuiContext guiContext)
        //{
        //    var textureResource = guiContext.LoadFileByAnyMeansNecessary(textureName);
        //    return textureResource == null
        //        ? (guiContext.MaterialLoader.GetErrorTexture(), null)
        //        : (guiContext.MaterialLoader.LoadTexture(textureName), (Texture)textureResource.DataBlock);
        //}

        void EnsureSpaceForVertices(int count)
        {
            var numFloats = count * VertexSize;
            if (_rawVertices == null) _rawVertices = new float[numFloats];
            else if (_rawVertices.Length < numFloats)
            {
                var nextSize = (count / 64 + 1) * 64 * VertexSize;
                Array.Resize(ref _rawVertices, nextSize);
            }
        }

        void UpdateVertices(ParticleBag particleBag, Matrix4x4 modelViewMatrix)
        {
            var particles = particleBag.LiveParticles;

            // Create billboarding rotation (always facing camera)
            Matrix4x4.Decompose(modelViewMatrix, out _, out Quaternion modelViewRotation, out _);
            modelViewRotation = Quaternion.Inverse(modelViewRotation);
            var billboardMatrix = Matrix4x4.CreateFromQuaternion(modelViewRotation);

            // Update vertex buffer
            EnsureSpaceForVertices(particleBag.Count * 4);
            for (var i = 0; i < particleBag.Count; ++i)
            {
                // Positions
                var modelMatrix = _orientationType == 0
                    ? particles[i].GetRotationMatrix() * billboardMatrix * particles[i].GetTransformationMatrix()
                    : particles[i].GetRotationMatrix() * particles[i].GetTransformationMatrix();

                var tl = Vector4.Transform(new Vector4(-1, -1, 0, 1), modelMatrix);
                var bl = Vector4.Transform(new Vector4(-1, 1, 0, 1), modelMatrix);
                var br = Vector4.Transform(new Vector4(1, 1, 0, 1), modelMatrix);
                var tr = Vector4.Transform(new Vector4(1, -1, 0, 1), modelMatrix);

                var quadStart = i * VertexSize * 4;
                _rawVertices[quadStart + 0] = tl.X;
                _rawVertices[quadStart + 1] = tl.Y;
                _rawVertices[quadStart + 2] = tl.Z;
                _rawVertices[quadStart + VertexSize * 1 + 0] = bl.X;
                _rawVertices[quadStart + VertexSize * 1 + 1] = bl.Y;
                _rawVertices[quadStart + VertexSize * 1 + 2] = bl.Z;
                _rawVertices[quadStart + VertexSize * 2 + 0] = br.X;
                _rawVertices[quadStart + VertexSize * 2 + 1] = br.Y;
                _rawVertices[quadStart + VertexSize * 2 + 2] = br.Z;
                _rawVertices[quadStart + VertexSize * 3 + 0] = tr.X;
                _rawVertices[quadStart + VertexSize * 3 + 1] = tr.Y;
                _rawVertices[quadStart + VertexSize * 3 + 2] = tr.Z;

                // Colors
                for (var j = 0; j < 4; ++j)
                {
                    _rawVertices[quadStart + VertexSize * j + 3] = particles[i].Color.X;
                    _rawVertices[quadStart + VertexSize * j + 4] = particles[i].Color.Y;
                    _rawVertices[quadStart + VertexSize * j + 5] = particles[i].Color.Z;
                    _rawVertices[quadStart + VertexSize * j + 6] = particles[i].Alpha;
                }

                // UVs
                if (_textureSequences != null && _textureSequences.Count > 0 && _textureSequences[0].Frames.Count > 0)
                {
                    var sequence = _textureSequences[particles[i].Sequence % _textureSequences.Count];

                    var particleTime = particles[i].ConstantLifetime - particles[i].Lifetime;
                    var frame = particleTime * sequence.FramesPerSecond * _animationRate;

                    var currentFrame = sequence.Frames[(int)Math.Floor(frame) % sequence.Frames.Count];
                    var currentImage = currentFrame.Images[0]; // TODO: Support more than one image per frame?

                    // Lerp frame coords and size
                    var subFrameTime = frame % 1.0f;
                    var offset = currentImage.CroppedMin * (1 - subFrameTime) + currentImage.UncroppedMin * subFrameTime;
                    var scale = (currentImage.CroppedMax - currentImage.CroppedMin) * (1 - subFrameTime) +
                        (currentImage.UncroppedMax - currentImage.UncroppedMin) * subFrameTime;

                    _rawVertices[quadStart + VertexSize * 0 + 7] = offset.X + scale.X * 0;
                    _rawVertices[quadStart + VertexSize * 0 + 8] = offset.Y + scale.Y * 1;
                    _rawVertices[quadStart + VertexSize * 1 + 7] = offset.X + scale.X * 0;
                    _rawVertices[quadStart + VertexSize * 1 + 8] = offset.Y + scale.Y * 0;
                    _rawVertices[quadStart + VertexSize * 2 + 7] = offset.X + scale.X * 1;
                    _rawVertices[quadStart + VertexSize * 2 + 8] = offset.Y + scale.Y * 0;
                    _rawVertices[quadStart + VertexSize * 3 + 7] = offset.X + scale.X * 1;
                    _rawVertices[quadStart + VertexSize * 3 + 8] = offset.Y + scale.Y * 1;
                }
                else
                {
                    _rawVertices[quadStart + VertexSize * 0 + 7] = 0;
                    _rawVertices[quadStart + VertexSize * 0 + 8] = 1;
                    _rawVertices[quadStart + VertexSize * 1 + 7] = 0;
                    _rawVertices[quadStart + VertexSize * 1 + 8] = 0;
                    _rawVertices[quadStart + VertexSize * 2 + 7] = 1;
                    _rawVertices[quadStart + VertexSize * 2 + 8] = 0;
                    _rawVertices[quadStart + VertexSize * 3 + 7] = 1;
                    _rawVertices[quadStart + VertexSize * 3 + 8] = 1;
                }
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, particleBag.Count * VertexSize * 4 * sizeof(float), _rawVertices, BufferUsageHint.DynamicDraw);
        }

        public void Render(ParticleBag particleBag, Matrix4x4 viewProjectionMatrix, Matrix4x4 modelViewMatrix)
        {
            if (particleBag.Count == 0) return;

            // Update vertex buffer
            UpdateVertices(particleBag, modelViewMatrix);

            // Draw it
            GL.Enable(EnableCap.Blend);
            GL.UseProgram(_shader.Program);

            if (_additive) GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            else GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.BindVertexArray(_quadVao);
            GL.EnableVertexAttribArray(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferHandle);

            GL.Uniform1(_shader.GetUniformLocation("uTexture"), 0); // set texture unit 0 as uTexture uniform

            var otkProjection = viewProjectionMatrix.ToOpenTK();
            GL.UniformMatrix4(_shader.GetUniformLocation("uProjectionViewMatrix"), false, ref otkProjection);

            // TODO: This formula is a guess but still seems too bright compared to valve particles
            GL.Uniform1(_shader.GetUniformLocation("uOverbrightFactor"), _overbrightFactor);

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(false);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _quadIndices.GLHandle);
            GL.DrawElements(BeginMode.Triangles, particleBag.Count * 6, DrawElementsType.UnsignedShort, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(true);

            GL.BindVertexArray(0);
            GL.UseProgram(0);

            if (_additive) GL.BlendEquation(BlendEquationMode.FuncAdd);

            GL.Disable(EnableCap.Blend);
        }

        public IEnumerable<string> GetSupportedRenderModes() => _shader.RenderModes;

        public void SetRenderMode(string renderMode)
        {
            var parameters = new Dictionary<string, bool>();
            if (renderMode != null && _shader.RenderModes.Contains(renderMode)) parameters.Add($"renderMode_{renderMode}", true);
            //_shader = guiContext.ShaderLoader.LoadShader(ShaderName, parameters);
        }
    }
}
