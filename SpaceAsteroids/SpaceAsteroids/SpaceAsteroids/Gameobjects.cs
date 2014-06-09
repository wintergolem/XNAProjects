using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SpaceAsteroids
{
    class Gameobject
    {
        enum RelocateFlip { XtoMin , XtoMax , YtoMin , YtoMax }

        //variables
        Texture2D texture;
        private bool _active;
        public bool active 
        { 
            get { return this._active; } 
            set { this._active = value; } 
        }
        public GameManager gameManager;
        public Vector2 v2Position;
        public float fCircleSize; //every object will have a circle collider
        protected float fMoveSpeed;
        protected Vector2 v2MoveVelocity;
        public Vector2 v2Rotation;
        Viewport gameViewport;
        Rectangle drawRect;

        //functions
        public Gameobject()
        {
            active = false;
            v2Position = Vector2.Zero;
            v2Rotation = Vector2.Zero;
            v2MoveVelocity = Vector2.Zero;
            fCircleSize = 0;
            fMoveSpeed = 0;
        }

        public Gameobject(float a_size, GameManager a_manager, Vector2 a_pos, float a_fMoveSpeed, Vector2 a_v2Rot, Texture2D a_texture)
        {
            fCircleSize = a_size;
            gameManager = a_manager;
            v2Position = a_pos;
            fMoveSpeed = a_fMoveSpeed;
            v2Rotation = a_v2Rot;
            texture = a_texture;

            active = true;
        }
        
        public virtual void Update( float a_deltaTime )
        {
            Move( a_deltaTime );
            CheckOnScreen();
        }

        protected virtual void Move( float a_deltaTime)
        {
            v2Position += v2MoveVelocity ;
        }

        void Relocate( RelocateFlip a_flip)
        {
            if (a_flip == RelocateFlip.XtoMin)
            {
                v2Position.X = 0;
            }
            else if (a_flip == RelocateFlip.XtoMax)
            {
                v2Position.X = gameViewport.Width;
            }
            else if (a_flip == RelocateFlip.YtoMin)
            {
                v2Position.Y = 0;
            }
            else if (a_flip == RelocateFlip.YtoMax)
            {
                v2Position.Y = gameViewport.Height;
            }
        }

        void CheckOnScreen() //returns false is not on screen
        {

            gameViewport = gameManager.viewport;
            if (v2Position.X < 0)
                Relocate(RelocateFlip.XtoMax);
            else if( v2Position.X > gameManager.viewport.Width )
                Relocate(RelocateFlip.XtoMin);
            else if( v2Position.Y < 0 )
                Relocate(RelocateFlip.YtoMax);
            else if (v2Position.Y > gameManager.viewport.Height)
                Relocate(RelocateFlip.YtoMin);
        }

        public virtual void Draw( SpriteBatch a_spriteBatch)
        {
            if (active)
            {
                CalculateDrawRect();
                a_spriteBatch.Begin();
                a_spriteBatch.Draw(texture, drawRect, null , Color.White , (float)Math.Atan2(v2Rotation.X,v2Rotation.Y) , new Vector2( texture.Width/2 , texture.Height/2 ) , SpriteEffects.None , 0);
                a_spriteBatch.End();
            }
        }

        public virtual void CalculateDrawRect()
        {
            drawRect.X = (int)(v2Position.X - (fCircleSize / 2) );
            drawRect.Y = (int)(v2Position.Y + (fCircleSize / 2));
            drawRect.Width = (int)(fCircleSize);
            drawRect.Height = (int)(fCircleSize);
        }

        public virtual void Collide()
        {
            active = false;
        }
    }

    class Ship : Gameobject
    {
        //variables
        Bullet[] bullets;
        float fTimeWaited = 0;
        public float fTimeToWait;
        bool fTriggerPressedLastCycle = false;
        
        //functions
        public Ship(float a_size, GameManager a_manager, Vector2 a_pos, float a_fMoveSpeed, Vector2 a_v2Rot, Texture2D a_texture) :
            base( a_size,  a_manager,  a_pos,  a_fMoveSpeed,  a_v2Rot, a_texture)
        {
            bullets = new Bullet[25];
            fTimeToWait = gameManager.fShipFireWait;
        }

        public override void Update(float a_deltaTime)
        {
            //get gamepad state
            GamePadState input = GamePad.GetState(PlayerIndex.One);
            if(  input.ThumbSticks.Left != Vector2.Zero)
                v2Rotation = input.ThumbSticks.Left;

            if (input.Triggers.Left != 0)
            {
                v2MoveVelocity += new Vector2(fMoveSpeed * v2Rotation.X * a_deltaTime, fMoveSpeed * -v2Rotation.Y * a_deltaTime);
            }

            base.Update(a_deltaTime); //handles movement

            //check for fire!
            CheckForFire( input );

            UpdateBullets( a_deltaTime );
        }

        public override void Draw(SpriteBatch a_spriteBatch)
        {
            base.Draw(a_spriteBatch);

            foreach (Bullet b in bullets)
                if( b != null)
                    b.Draw(a_spriteBatch);
        }

        protected override void Move(float a_deltaTime)
        {
            base.Move(a_deltaTime);

            //drag in x
            if (v2MoveVelocity.X > 0)
                if (v2MoveVelocity.X - gameManager.fDrag > 0)
                    v2MoveVelocity.X -= gameManager.fDrag;
                else
                    v2MoveVelocity.X = 0;

            else if (v2MoveVelocity.X < 0)
                if (v2MoveVelocity.X + gameManager.fDrag < 0)
                    v2MoveVelocity.X += gameManager.fDrag;
                else
                    v2MoveVelocity.X = 0;

            //drag in Y
            if (v2MoveVelocity.Y > 0)
                if (v2MoveVelocity.Y - gameManager.fDrag > 0)
                    v2MoveVelocity.Y -= gameManager.fDrag;
                else
                    v2MoveVelocity.Y = 0;

            else if (v2MoveVelocity.Y < 0)
                if (v2MoveVelocity.Y + gameManager.fDrag < 0)
                    v2MoveVelocity.Y += gameManager.fDrag;
                else
                    v2MoveVelocity.Y = 0;
        }

        void CheckForFire( GamePadState a_input)
        {
            if (fTriggerPressedLastCycle)
            {
                //trigger still pressed, wait for release
                if (a_input.Triggers.Right == 0)
                    fTriggerPressedLastCycle = false;
            }
            else
            {
                if (a_input.Triggers.Right != 0)
                {
                    fTriggerPressedLastCycle = true;

                    Fire();
                }
            }
        }

        void Fire()
        {
            for (int i = 0; i < bullets.Length; i++)
            {
                if (bullets[i] == null )//&& !bullets[i].active)
                {
                    bullets[i] = new Bullet(gameManager.fBulletSize, gameManager, v2Position, fMoveSpeed, v2Rotation, gameManager.bulletTexture);
                    break;
                }
            }
        }

        void UpdateBullets( float a_deltaTime )
        {
            for( int i = 0 ; i < bullets.Length ; i++ )
            {
                if (bullets[i] != null)
                {
                    bullets[i].Update(a_deltaTime);
                    if (!bullets[i].active)
                        bullets[i] = null;
                }
         
            }
        }
    }

    class Bullet : Gameobject
    {
        //variables
        float fLifeTime = 0; //amount of time alive
        float fLifeSpan;    //amount of time allow to be alive


        public Bullet(float a_size, GameManager a_manager, Vector2 a_pos, float a_fMoveSpeed, Vector2 a_v2Rot, Texture2D a_texture) :
            base( a_size,  a_manager,  a_pos,  a_fMoveSpeed + a_manager.fBulletMoveSpeed,  a_v2Rot, a_texture)
        {
            fLifeSpan = gameManager.fBulletLifeSpan;
        }

        public override void Update(float a_deltaTime)
        {
            v2MoveVelocity = new Vector2(fMoveSpeed * v2Rotation.X * a_deltaTime, fMoveSpeed * -v2Rotation.Y * a_deltaTime);

            //update life time
            fLifeTime += a_deltaTime;
            if (fLifeTime > fLifeSpan)
                active = false;
            base.Update(a_deltaTime);
        }
    }

    class Asteroid : Gameobject
    {
        //variables
        int iSizeStage = 3;


        public Asteroid(float a_size, GameManager a_manager, Vector2 a_pos, float a_fMoveSpeed, Vector2 a_v2Rot, Texture2D a_texture) :
            base( a_size,  a_manager,  a_pos,  a_fMoveSpeed ,  a_v2Rot, a_texture)
        {
        }

        public override void  Update(float a_deltaTime)
        {
            v2MoveVelocity = new Vector2(fMoveSpeed * v2Rotation.X * a_deltaTime, fMoveSpeed * -v2Rotation.Y * a_deltaTime);
            base.Update(a_deltaTime);
        }

        void Break()
        {
            iSizeStage--;
            if (iSizeStage <= 0)
            {
                active = false;
            }
            else
            {
                v2Rotation *= -1;
            }
        }
    }
}
