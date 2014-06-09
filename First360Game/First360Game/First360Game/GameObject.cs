using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace First360Game
{
    class GameObject
    {
        public Model model { get; set; }
        public Vector3 position { get; set; }
        public bool IsActive { get; set; }
        public BoundingSphere boundingSphere { get; set; }

        public GameObject()
        {
            model = null;
            position = Vector3.Zero;
            IsActive = false;
            boundingSphere = new BoundingSphere();
        }
    }

    class FuelCell : GameObject
    {
        public bool Retrieved { get; set; }
        public FuelCell() : base()
        {
            Retrieved = false;
        }

        public void LoadContent(ContentManager content, string modelname )
        {
            model = content.Load<Model>(modelname);
            position = Vector3.Down;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix translateMatrix = Matrix.CreateTranslation(position);
            Matrix worldMatrix = translateMatrix;

            if (!Retrieved)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = worldMatrix * transforms[mesh.ParentBone.Index];
                        effect.View = view;
                        effect.Projection = projection;
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;
                    }
                    mesh.Draw();
                }
            }
        }
    }

    class Barrier : GameObject
    {
        public string BarrierType { get; set; }

        public Barrier()
        {
            BarrierType = null;
        }

        public void LoadContent(ContentManager content, string modelName)
        {
            model = content.Load<Model>(modelName);
            BarrierType = modelName;
            position = Vector3.Down; //symbol that this hasn't been initialized
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix translateMatrix = Matrix.CreateTranslation(position);
            Matrix worldMatrix = translateMatrix;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = worldMatrix * transforms[mesh.ParentBone.Index];
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }
    }

    class FuelCarrier : GameObject
    {
        public float ForwardDirection { get; set; }
        public int MaxRange { get; set; }

        public FuelCarrier()
        {
            ForwardDirection = 0.0f;
            MaxRange = GameConstants.MaxRange;
        }

        public void LoadContent(ContentManager content, string modelName)
        {
            model = content.Load<Model>(modelName);
        }

        public void Update(GamePadState gamepadState, KeyboardState keyboardState, Barrier[] barriers)
        {
            Vector3 futurePosition = position;
            float turnAmount = 0;

            if (keyboardState.IsKeyDown(Keys.A))
            {
                turnAmount = 1;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                turnAmount = -1;
            }
            else if (gamepadState.ThumbSticks.Left.X != 0)
            {
                turnAmount = -gamepadState.ThumbSticks.Left.X;
            }
            ForwardDirection += turnAmount * GameConstants.TurnSpeed;
            Matrix orientationMatrix = Matrix.CreateRotationY(ForwardDirection);

            Vector3 movement = Vector3.Zero;
            if (keyboardState.IsKeyDown(Keys.W))
            {
                movement.Z = 1;
            }
            else if (keyboardState.IsKeyDown(Keys.S))
            {
                movement.Z = -1;
            }
            else if (gamepadState.ThumbSticks.Left.Y != 0)
            {
                movement.Z = gamepadState.ThumbSticks.Left.Y;
            }

            Vector3 speed = Vector3.Transform(movement, orientationMatrix);
            speed *= GameConstants.Velocity;
            futurePosition = position + speed;

            if (ValidateMovement(futurePosition, barriers))
            {
                position = futurePosition;
            }
        }

        private bool ValidateMovement(Vector3 futurePosition, Barrier[] barriers)
        {
            //Don't allow off-terrain driving
            if ((Math.Abs(futurePosition.X) > MaxRange) ||
                (Math.Abs(futurePosition.Z) > MaxRange))
                return false;

            return true;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix worldMatrix = Matrix.Identity;
            Matrix rotationYMatrix = Matrix.CreateRotationY(ForwardDirection);
            Matrix translateMatrix = Matrix.CreateTranslation(position);

            worldMatrix = rotationYMatrix * translateMatrix;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = worldMatrix * transforms[mesh.ParentBone.Index];
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }
    }

    class Camera
    {
        public Vector3 AvatarheadOffSet{ get; set; }
        public Vector3 TargetOffSet{ get; set; }
        public Matrix ViewMatrix{ get; set; }
        public Matrix ProjectionMatrix { get; set; }

        public Camera()
        {
            AvatarheadOffSet = new Vector3(0, 7, -15);
            TargetOffSet = new Vector3(0, 5, 0);
            ViewMatrix = Matrix.Identity;
            ProjectionMatrix = Matrix.Identity;
        }

        public void Update(float avatarYaw, Vector3 aposition, float aspectRatio)
        {
            Matrix rotationMatrix = Matrix.CreateRotationY(avatarYaw);

            Vector3 transformedHeadOffSet = Vector3.Transform(AvatarheadOffSet, rotationMatrix);
            Vector3 transformedReference = Vector3.Transform(TargetOffSet, rotationMatrix);

            Vector3 cameraPosition = aposition + transformedHeadOffSet;
            Vector3 cameraTarget = aposition + transformedReference;

            ViewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(GameConstants.ViewAngle), aspectRatio, GameConstants.NearClip, GameConstants.FarClip);
        }
    }
}
