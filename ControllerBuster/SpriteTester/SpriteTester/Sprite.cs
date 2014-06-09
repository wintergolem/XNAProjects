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

namespace SpriteTester
{
    class Sprite
    {
    }


    public abstract class SpriteManager
    {

        protected Texture2D texture;

        public Vector2 position = Vector2.Zero;
        public Color color = Color.White;
        public Vector2 origin;
        public float rotation = 0f;
        public float scale = 1f;
        public SpriteEffects spriteEffect;
        protected Rectangle[] rectangles;
        protected int frameIndex = 0;

        public SpriteManager(Texture2D a_texture, int frames)
        {
            this.texture = a_texture;
            int width = texture.Width / frames;
            rectangles = new Rectangle[frames];
            for (int i = 0; i < frames; i++)
            {
                rectangles[i] = new Rectangle(i * width, 0, width, texture.Height);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, rectangles[frameIndex], color, rotation, origin, scale, spriteEffect, 0f);
        }
    }

    public class FrameAnimation : SpriteManager
    {
        public FrameAnimation(Texture2D a_texture, int a_iframes)
            : base(a_texture, a_iframes)
        {

        }

        public void SetFrame(int a_iFrame)
        {
            if (a_iFrame < rectangles.Length)
            {
                frameIndex = a_iFrame;
            }
        }
    }

    public class SpriteAnimation : SpriteManager
    {
        private float timeElapsed;
        public bool bIsLooping = false;
        private float timeToUpdate = 0.05f;

        public SpriteAnimation(Texture2D a_texture, int a_iFrames)
            : base(a_texture , a_iFrames)
        {

        }

        public int FramesPerSecond
        {
            set { timeToUpdate = (1f / value); }
        }

        public void Update(GameTime gameTime)
        {
            timeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timeElapsed > timeToUpdate)
            {
                timeElapsed -= timeToUpdate;

                if (frameIndex < rectangles.Length - 1)
                    frameIndex++;
                else if (bIsLooping)
                    frameIndex = 0;
            }
        }
    }


}