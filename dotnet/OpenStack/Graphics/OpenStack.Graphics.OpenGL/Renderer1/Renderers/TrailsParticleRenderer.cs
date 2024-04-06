using OpenStack.Graphics.ParticleSystem;
using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.OpenGL.Renderer1.Renderers
{
    public class TrailsParticleRenderer : IParticleRenderer
    {
        readonly Shader _shader;
        readonly int _quadVao;
        readonly int _texture;

        readonly TextureSequences _textureSequences;
        readonly float _animationRate;

        readonly bool _additive;
        readonly float _overbrightFactor;
        readonly long _orientationType;

        readonly float _finalTextureScaleU;
        readonly float _finalTextureScaleV;

        readonly float _maxLength;
        readonly float _lengthFadeInTime;

        public TrailsParticleRenderer(IDictionary<string, object> keyValues, IOpenGLGraphic graphic)
        {
            _shader = graphic.LoadShader("vrf.particle.trail", new Dictionary<string, bool>());

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

            _finalTextureScaleU = keyValues.GetFloat("m_flFinalTextureScaleU", 1f);
            _finalTextureScaleV = keyValues.GetFloat("m_flFinalTextureScaleV", 1f);

            _maxLength = keyValues.GetFloat("m_flMaxLength", 2000f);
            _lengthFadeInTime = keyValues.GetFloat("m_flLengthFadeInTime");
        }

        int SetupQuadBuffer()
        {
            GL.UseProgram(_shader.Program);

            // Create and bind VAO
            var vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            var vertices = new[]
            {
                -1.0f, -1.0f, 0.0f,
                -1.0f, 1.0f, 0.0f,
                1.0f, -1.0f, 0.0f,
                1.0f, 1.0f, 0.0f,
            };

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);

            var positionAttributeLocation = GL.GetAttribLocation(_shader.Program, "aVertexPosition");
            GL.VertexAttribPointer(positionAttributeLocation, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindVertexArray(0); // Unbind VAO

            return vao;
        }

        //static (int TextureIndex, Texture TextureData) LoadTexture(string textureName, GuiContext guiContext)
        //{
        //    var textureResource = guiContext.LoadFileByAnyMeansNecessary(textureName);

        //    return textureResource == null
        //        ? (guiContext.MaterialLoader.GetErrorTexture(), null)
        //    : (guiContext.MaterialLoader.LoadTexture(textureName), (Texture)textureResource.DataBlock);
        //}

        public void Render(ParticleBag particleBag, Matrix4x4 viewProjectionMatrix, Matrix4x4 modelViewMatrix)
        {
            var particles = particleBag.LiveParticles;

            GL.Enable(EnableCap.Blend);
            GL.UseProgram(_shader.Program);

            if (_additive) GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            else GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.BindVertexArray(_quadVao);
            GL.EnableVertexAttribArray(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            GL.Uniform1(_shader.GetUniformLocation("uTexture"), 0); // set texture unit 0 as uTexture uniform

            var otkProjection = viewProjectionMatrix.ToOpenTK();
            GL.UniformMatrix4(_shader.GetUniformLocation("uProjectionViewMatrix"), false, ref otkProjection);

            // TODO: This formula is a guess but still seems too bright compared to valve particles
            GL.Uniform1(_shader.GetUniformLocation("uOverbrightFactor"), _overbrightFactor);

            var modelMatrixLocation = _shader.GetUniformLocation("uModelMatrix");
            var colorLocation = _shader.GetUniformLocation("uColor");
            var alphaLocation = _shader.GetUniformLocation("uAlpha");
            var uvOffsetLocation = _shader.GetUniformLocation("uUvOffset");
            var uvScaleLocation = _shader.GetUniformLocation("uUvScale");

            // Create billboarding rotation (always facing camera)
            Matrix4x4.Decompose(modelViewMatrix, out _, out Quaternion modelViewRotation, out _);
            modelViewRotation = Quaternion.Inverse(modelViewRotation);
            var billboardMatrix = Matrix4x4.CreateFromQuaternion(modelViewRotation);

            for (var i = 0; i < particles.Length; ++i)
            {
                var position = new Vector3(particles[i].Position.X, particles[i].Position.Y, particles[i].Position.Z);
                var previousPosition = new Vector3(particles[i].PositionPrevious.X, particles[i].PositionPrevious.Y, particles[i].PositionPrevious.Z);
                var difference = previousPosition - position;
                var direction = Vector3.Normalize(difference);

                var midPoint = position + 0.5f * difference;

                // Trail width = radius
                // Trail length = distance between current and previous times trail length divided by 2 (because the base particle is 2 wide)
                var length = Math.Min(_maxLength, particles[i].TrailLength * difference.Length() / 2f);
                var t = 1 - particles[i].Lifetime / particles[i].ConstantLifetime;
                var animatedLength = t >= _lengthFadeInTime
                    ? length
                    : t * length / _lengthFadeInTime;
                var scaleMatrix = Matrix4x4.CreateScale(particles[i].Radius, animatedLength, 1);

                // Center the particle at the midpoint between the two points
                var translationMatrix = Matrix4x4.CreateTranslation(Vector3.UnitY * animatedLength);

                // Calculate rotation matrix

                var axis = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, direction));
                var angle = (float)Math.Acos(direction.Y);
                var rotationMatrix = Matrix4x4.CreateFromAxisAngle(axis, angle);

                var modelMatrix =
                    _orientationType == 0 ? Matrix4x4.Multiply(scaleMatrix, Matrix4x4.Multiply(translationMatrix, rotationMatrix))
                    : particles[i].GetTransformationMatrix();

                // Position/Radius uniform
                var otkModelMatrix = modelMatrix.ToOpenTK();
                GL.UniformMatrix4(modelMatrixLocation, false, ref otkModelMatrix);

                if (_textureSequences != null && _textureSequences.Count > 0 && _textureSequences[0].Frames.Count > 0)
                {
                    var sequence = _textureSequences[0];

                    var particleTime = particles[i].ConstantLifetime - particles[i].Lifetime;
                    var frame = particleTime * sequence.FramesPerSecond * _animationRate;

                    var currentFrame = sequence.Frames[(int)Math.Floor(frame) % sequence.Frames.Count];
                    var currentImage = currentFrame.Images[0]; // TODO: Support more than one image per frame?

                    // Lerp frame coords and size
                    var subFrameTime = frame % 1.0f;
                    var offset = currentImage.CroppedMin * (1 - subFrameTime) + currentImage.UncroppedMin * subFrameTime;
                    var scale = (currentImage.CroppedMax - currentImage.CroppedMin) * (1 - subFrameTime) +
                        (currentImage.UncroppedMax - currentImage.UncroppedMin) * subFrameTime;

                    GL.Uniform2(uvOffsetLocation, offset.X, offset.Y);
                    GL.Uniform2(uvScaleLocation, scale.X * _finalTextureScaleU, scale.Y * _finalTextureScaleV);
                }
                else
                {
                    GL.Uniform2(uvOffsetLocation, 1f, 1f);
                    GL.Uniform2(uvScaleLocation, _finalTextureScaleU, _finalTextureScaleV);
                }

                // Color uniform
                GL.Uniform3(colorLocation, particles[i].Color.X, particles[i].Color.Y, particles[i].Color.Z);

                GL.Uniform1(alphaLocation, particles[i].Alpha * particles[i].AlphaAlternate);

                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            }

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
            //_shader = graphic.LoadShader(ShaderName, parameters);
        }
    }
}
