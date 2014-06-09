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
    class GameManager
    {
        //gameManager variables
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameManager();
                }
                return instance;
            }
        }
        float fDeltatime;
        public Viewport viewport;
        public float fDrag = 1f;
        //float fPrevTime;

        //bullet variables
        public float fBulletSize;
        public Texture2D bulletTexture;
        public float fBulletMoveSpeed = 0.5f;
        public float fBulletLifeSpan = 1000; //in milliseconds

        //ship variables
        public Ship player;
        public Vector2 v2ShipStartingPoint;
        public float fShipSize;
        public float fShipSpeed;
        public Texture2D shipTexture;
        public float fShipFireWait = 20f;//in millisecond


        //Asteroid variables
        public List<Asteroid> lAsteroids;
        public Dictionary<int, float> dSizeLevels;
        int iStartingAsteroidCount = 5;
        public float fAsteroidMoveSpeed = 0.1f;
        public Texture2D AsteroidTexture;

        private GameManager()
        {
            lAsteroids = new List<Asteroid>();
            dSizeLevels = new Dictionary<int, float>();
            dSizeLevels.Add(1, 20);
            dSizeLevels.Add(2, 30);
            dSizeLevels.Add(3, 40);
        }

        public void LoadTextures ( ContentManager a_content )
        {
            bulletTexture = a_content.Load<Texture2D>("Bullet");
            shipTexture = a_content.Load<Texture2D>("Ship");
            AsteroidTexture = a_content.Load<Texture2D>("Asteriod");
        }

        public void Init(float a_fBulletSize , float a_fShipSpeed , float a_fShipSize , Viewport a_viewport)
        {
            viewport = a_viewport;

            //bullet
            fBulletSize = a_fBulletSize;

            //ship
            v2ShipStartingPoint = new Vector2(viewport.Width / 2, viewport.Height / 2);
            fShipSpeed = a_fShipSpeed;
            fShipSize = a_fShipSize;
            player = new Ship(fShipSize, this, v2ShipStartingPoint, fShipSpeed, new Vector2( 0 , 1) , shipTexture);

            //asteroids
            Random rand = new Random();
            for (int i = 0; i < iStartingAsteroidCount; i++)
            {
                Asteroid a = new Asteroid(dSizeLevels[3], this, GenAstSpawnPoint(rand), fAsteroidMoveSpeed, new Vector2( (float)(rand.NextDouble() ) , (float)(rand.NextDouble() ) ), AsteroidTexture);
                lAsteroids.Add(a);
            }
        }

        public void Update(GameTime a_time, Viewport a_Viewport)
        {
            viewport = a_Viewport;
            CalculateDeltaTime( a_time );
            player.Update( fDeltatime );

            foreach (Asteroid a in lAsteroids)
                a.Update( fDeltatime );
        }

        void CalculateDeltaTime( GameTime a_time)
        {
            fDeltatime = a_time.ElapsedGameTime.Milliseconds;
        }

        public void DrawObjects( SpriteBatch a_spriteBatch)
        {
            player.Draw( a_spriteBatch );

            foreach (Asteroid a in lAsteroids)
                a.Draw(a_spriteBatch);
        }

        Vector2 GenAstSpawnPoint(Random rand)
        {
            //float bufferFromShip = 10;

            return new Vector2( (float)(rand.NextDouble()) * viewport.Width, (float)(rand.NextDouble()) * viewport.Height);
        }
    }
}
