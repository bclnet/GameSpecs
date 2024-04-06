using OpenTK.Input;
using System;
using System.Numerics;

namespace OpenStack.Graphics.OpenGL.Renderer1
{
    public class GLDebugCamera : GLCamera
    {
        public bool MouseOverRenderArea { get; set; } // Set from outside this class by forms code

        bool MouseDragging;

        Vector2 MouseDelta;
        Vector2 MousePreviousPosition;

        KeyboardState KeyboardState;
        MouseState MouseState;
        int ScrollWheelDelta;

        public override void Tick(float deltaTime)
        {
            if (!MouseOverRenderArea) return;

            // Use the keyboard state to update position
            HandleInputTick(deltaTime);

            // Full width of the screen is a 1 PI (180deg)
            Yaw -= (float)Math.PI * MouseDelta.X / WindowSize.X;
            Pitch -= (float)Math.PI / AspectRatio * MouseDelta.Y / WindowSize.Y;

            ClampRotation();

            RecalculateMatrices();
        }

        public void HandleInput(MouseState mouseState, KeyboardState keyboardState)
        {
            ScrollWheelDelta += mouseState.ScrollWheelValue - MouseState.ScrollWheelValue;
            MouseState = mouseState;
            KeyboardState = keyboardState;
            if (!MouseOverRenderArea || mouseState.LeftButton == ButtonState.Released)
            {
                MouseDragging = false;
                MouseDelta = default;
                if (!MouseOverRenderArea) return;
            }

            // drag
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (!MouseDragging)
                {
                    MouseDragging = true;
                    MousePreviousPosition = new Vector2(mouseState.X, mouseState.Y);
                }

                var mouseNewCoords = new Vector2(mouseState.X, mouseState.Y);

                MouseDelta.X = mouseNewCoords.X - MousePreviousPosition.X;
                MouseDelta.Y = mouseNewCoords.Y - MousePreviousPosition.Y;

                MousePreviousPosition = mouseNewCoords;
            }
        }

        void HandleInputTick(float deltaTime)
        {
            var speed = CAMERASPEED * deltaTime;

            // Double speed if shift is pressed
            if (KeyboardState.IsKeyDown(Key.ShiftLeft)) speed *= 2;
            else if (KeyboardState.IsKeyDown(Key.F)) speed *= 10;

            if (KeyboardState.IsKeyDown(Key.W)) Location += GetForwardVector() * speed;
            if (KeyboardState.IsKeyDown(Key.S)) Location -= GetForwardVector() * speed;
            if (KeyboardState.IsKeyDown(Key.D)) Location += GetRightVector() * speed;
            if (KeyboardState.IsKeyDown(Key.A)) Location -= GetRightVector() * speed;
            if (KeyboardState.IsKeyDown(Key.Z)) Location += new Vector3(0, 0, -speed);
            if (KeyboardState.IsKeyDown(Key.Q)) Location += new Vector3(0, 0, speed);

            // scroll
            if (ScrollWheelDelta != 0)
            {
                Location += GetForwardVector() * ScrollWheelDelta * speed;
                ScrollWheelDelta = 0;
            }
        }
    }
}
