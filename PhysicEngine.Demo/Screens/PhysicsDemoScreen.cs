﻿using System;
using PhysicEngine.Dynamics;
using PhysicEngine.Dynamics.Joints;
using PhysicEngine.Extensions.DebugView;
using PhysicEngine.Demo.ScreenSystem;
using PhysicEngine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PhysicEngine.Demo.Demos.Prefabs;

namespace PhysicEngine.Demo.Screens
{
    public class PhysicsDemoScreen : GameScreen, IDisposable
    {
        private static DebugViewFlags _flags;
        private static bool _flagsChanged;

        private float _agentForce;
        private float _agentTorque;
        private FixedMouseJoint _fixedMouseJoint;
        private Body _userAgent;

        protected Camera2D Camera;
        protected DebugView DebugView;
        protected World World;
        private Border _border;
        private readonly bool _hasBorder;

        protected PhysicsDemoScreen(bool hasBorder = true)
        {
            _hasBorder = hasBorder;
            TransitionOnTime = TimeSpan.FromSeconds(0.75);
            TransitionOffTime = TimeSpan.FromSeconds(0.75);
            HasCursor = true;
            EnableCameraControl = true;
        }

        public static DebugViewFlags Flags
        {
            get => _flags;
            set
            {
                _flags = value;
                _flagsChanged = true;
            }
        }

        public bool EnableCameraControl { get; set; }

        protected void SetUserAgent(Body agent, float force, float torque)
        {
            _userAgent = agent;
            _agentForce = force;
            _agentTorque = torque;
        }

        public override void LoadContent()
        {
            if (World == null)
                World = new World(Vector2.Zero);
            else
                World.Clear();

            if (_hasBorder)
                _border = new Border(World, Lines, Framework.GraphicsDevice);

            DebugView = new DebugView(World);
            DebugView.DefaultShapeColor = Color.White;
            DebugView.SleepingShapeColor = Color.LightGray;
            DebugView.LoadContent(Framework.GraphicsDevice, Framework.Content);

            DebugView.Flags = _flags;
            _flagsChanged = false;

            if (Camera == null)
                Camera = new Camera2D(Framework.GraphicsDevice);
            else
                Camera.ResetCamera();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!coveredByOtherScreen && !otherScreenHasFocus)
            {
                // variable time step but never less then 30 Hz
                World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 1f / 30f));
            }

            Camera.Update(gameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsNewButtonPress(Buttons.Start) || input.IsNewKeyPress(Keys.F1))
                ScreenManager.AddScreen(new DescriptionBoxScreen(GetDetails()));

            if (input.IsScreenExit())
                ExitScreen();

            if (HasCursor)
                HandleCursor(input);

            if (_userAgent != null)
                HandleUserAgent(input);

            if (EnableCameraControl)
                HandleCamera(input, gameTime);

            base.HandleInput(input, gameTime);
        }

        private void HandleCursor(InputHelper input)
        {
            Vector2 position = Camera.ConvertScreenToWorld(input.Cursor);

            if ((input.IsNewButtonPress(Buttons.A) || input.IsNewMouseButtonPress(MouseButtons.LeftButton)) && _fixedMouseJoint == null)
            {
                Fixture savedFixture = World.TestPoint(position);
                if (savedFixture != null)
                {
                    Body body = savedFixture.Body;
                    _fixedMouseJoint = new FixedMouseJoint(body, position);
                    _fixedMouseJoint.MaxForce = 1000.0f * body.Mass;

                    JoinHelper.LinearStiffness(5.0f, 0.7f, body, null, out float stiffness, out float damping);
                    _fixedMouseJoint.Stiffness = stiffness;
                    _fixedMouseJoint.Damping = damping;

                    World.AddJoint(_fixedMouseJoint);
                    body.Awake = true;
                }
            }

            if ((input.IsNewButtonRelease(Buttons.A) || input.IsNewMouseButtonRelease(MouseButtons.LeftButton)) && _fixedMouseJoint != null)
            {
                World.RemoveJoint(_fixedMouseJoint);
                _fixedMouseJoint = null;
            }

            if (_fixedMouseJoint != null)
                _fixedMouseJoint.WorldAnchorB = position;
        }

        private void HandleCamera(InputHelper input, GameTime gameTime)
        {
            Vector2 camMove = Vector2.Zero;
            if (input.GamePadState.IsButtonDown(Buttons.RightShoulder))
            {
                camMove = input.GamePadState.ThumbSticks.Right * new Vector2(10f, -10f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (input.GamePadState.IsButtonDown(Buttons.RightTrigger))
                    Camera.Zoom += 5f * (float)gameTime.ElapsedGameTime.TotalSeconds * Camera.Zoom / 20f;

                if (input.GamePadState.IsButtonDown(Buttons.LeftTrigger))
                    Camera.Zoom -= 5f * (float)gameTime.ElapsedGameTime.TotalSeconds * Camera.Zoom / 20f;

                if (input.IsNewButtonPress(Buttons.X))
                    Camera.ResetCamera();
            }
            else
            {
                if (input.KeyboardState.IsKeyDown(Keys.Up))
                    camMove.Y -= 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (input.KeyboardState.IsKeyDown(Keys.Down))
                    camMove.Y += 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (input.KeyboardState.IsKeyDown(Keys.Left))
                    camMove.X -= 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (input.KeyboardState.IsKeyDown(Keys.Right))
                    camMove.X += 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (input.KeyboardState.IsKeyDown(Keys.PageUp))
                    Camera.Zoom += 5f * (float)gameTime.ElapsedGameTime.TotalSeconds * Camera.Zoom / 20f;
                if (input.KeyboardState.IsKeyDown(Keys.PageDown))
                    Camera.Zoom -= 5f * (float)gameTime.ElapsedGameTime.TotalSeconds * Camera.Zoom / 20f;
                if (input.IsNewKeyPress(Keys.Home))
                    Camera.ResetCamera();
            }

            if (camMove != Vector2.Zero)
                Camera.MoveCamera(camMove);
        }

        private void HandleUserAgent(InputHelper input)
        {
            Vector2 force = Vector2.Zero;
            float torque = 0f;

            if (!input.GamePadState.IsButtonDown(Buttons.RightShoulder))
            {
                force = _agentForce * new Vector2(input.GamePadState.ThumbSticks.Right.X, -input.GamePadState.ThumbSticks.Right.Y);
                torque = _agentTorque * (input.GamePadState.Triggers.Right - input.GamePadState.Triggers.Left);
            }

            if (force == Vector2.Zero && torque == 0f)
            {
                float forceAmount = _agentForce * 0.6f;

                if (input.KeyboardState.IsKeyDown(Keys.A))
                    force += new Vector2(-forceAmount, 0);
                if (input.KeyboardState.IsKeyDown(Keys.S))
                    force += new Vector2(0, forceAmount);
                if (input.KeyboardState.IsKeyDown(Keys.D))
                    force += new Vector2(forceAmount, 0);
                if (input.KeyboardState.IsKeyDown(Keys.W))
                    force += new Vector2(0, -forceAmount);
                if (input.KeyboardState.IsKeyDown(Keys.Q))
                    torque -= _agentTorque;
                if (input.KeyboardState.IsKeyDown(Keys.E))
                    torque += _agentTorque;
            }

            _userAgent.ApplyForce(force);
            _userAgent.ApplyTorque(torque);
        }

        public override void Draw()
        {
            if (_hasBorder)
                _border.Draw(ref Camera.SimProjection, ref Camera.SimView);

            if (_flagsChanged)
            {
                DebugView.Flags = _flags;
                _flagsChanged = false;
            }

            DebugView.RenderDebugData(ref Camera.SimProjection, ref Camera.SimView);

            base.Draw();
        }

        public virtual string GetTitle()
        {
            return "GetTitle() not implemented, override it for a proper title.";
        }

        public virtual string GetDetails()
        {
            return "GetDetails() not implemented, override it for a proper demo description.";
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DebugView?.Dispose();
                _border?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}