using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem
{
    public struct Particle
    {
        public int ParticleCount { get; set; }

        // Base properties
        public float ConstantAlpha { get; set; }
        public Vector3 ConstantColor { get; set; }
        public float ConstantLifetime { get; set; }
        public float ConstantRadius { get; set; }

        // Variable fields
        public float Alpha { get; set; }
        public float AlphaAlternate { get; set; }

        public Vector3 Color { get; set; }

        public float Lifetime { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 PositionPrevious { get; set; }

        public float Radius { get; set; }

        public float TrailLength { get; set; }

        /// <summary>
        /// Gets or sets (Yaw, Pitch, Roll) Euler angles.
        /// </summary>
        public Vector3 Rotation { get; set; }

        /// <summary>
        /// Gets or sets (Yaw, Pitch, Roll) Euler angles rotation speed.
        /// </summary>
        public Vector3 RotationSpeed { get; set; }

        public int Sequence { get; set; }

        public Vector3 Velocity { get; set; }

        public Particle(IDictionary<string, object> baseProperties)
        {
            ParticleCount = 0;
            Alpha = 1.0f;
            AlphaAlternate = 1.0f;
            Position = Vector3.Zero;
            PositionPrevious = Vector3.Zero;
            Rotation = Vector3.Zero;
            RotationSpeed = Vector3.Zero;
            Velocity = Vector3.Zero;
            ConstantRadius = baseProperties.GetFloat("m_flConstantRadius", 5.0f);
            ConstantAlpha = 1.0f;
            if (baseProperties.ContainsKey("m_ConstantColor"))
            {
                var vectorValues = baseProperties.GetInt64Array("m_ConstantColor");
                ConstantColor = new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]) / 255f;
            }
            else ConstantColor = Vector3.One;
            ConstantLifetime = baseProperties.GetFloat("m_flConstantLifespan", 1);
            TrailLength = 1;
            Sequence = 0;

            Color = ConstantColor;
            Lifetime = ConstantLifetime;
            Radius = ConstantRadius;
        }

        public Matrix4x4 GetTransformationMatrix()
        {
            var scaleMatrix = Matrix4x4.CreateScale(Radius);
            var translationMatrix = Matrix4x4.CreateTranslation(Position.X, Position.Y, Position.Z);
            return Matrix4x4.Multiply(scaleMatrix, translationMatrix);
        }

        public Matrix4x4 GetRotationMatrix()
        {
            var rotationMatrix = Matrix4x4.Multiply(Matrix4x4.CreateRotationZ(Rotation.Z), Matrix4x4.CreateRotationY(Rotation.Y));
            return rotationMatrix;
        }
    }
}
