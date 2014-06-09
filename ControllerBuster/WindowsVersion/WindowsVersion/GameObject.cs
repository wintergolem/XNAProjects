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
using Microsoft.Xna.Framework.GamerServices;

namespace WindowsVersion
{
    //enums
    public enum InputType
    {
        A, B, X, Y, DpadUp,
        DpadLeft , DpadDown, DpadRight, LeftStickUp, LeftStickLeft,
        LeftStickDown, LeftStickRight, RightStickUp, RightStickLeft, RightStickDown,
        RightStickRight, LeftStickPress, RightStickPress, LeftShoulder, RightShoulder,
        Start, LeftTrigger, RightTrigger
        
    };//23 total options
    public enum InputCat { Button, ThumbStick, Trigger, DPad };
    public enum Gamestate { MainMenu, CoreGame, Multiplayer, Exit, ScoreScreen };
    public enum MenuState { PressStart, Single, VS };

    //classes
    public class TileSpriteManager
    {

        protected Texture2D texture;

        public Vector2 position = Vector2.Zero;
        public Color color = Color.White;
        public Vector2 origin;
        public float rotation = 0f;
        public float scale = 1f;
        public SpriteEffects spriteEffect;
        protected Dictionary< InputType , Rectangle > dRectangles;
        protected int frameIndex = 0;

        public TileSpriteManager(Texture2D a_texture, int framesHorizontal, int framesVertical , int a_iMaxTextures)
        {
            this.texture = a_texture;
            int iXBuffer = 66;
            int iYBuffer = 60;
            int iBetweenBufferX = 0;
            int iBetweenBufferY = 0;
            int width = texture.Width;
            width = width - (70 + iXBuffer) ;
            width -= (iBetweenBufferX * 4);
            width /= framesHorizontal;
            int height =    ((texture.Height - (91 + iYBuffer) ) - (iBetweenBufferY * 4)) / framesVertical;
            dRectangles = new Dictionary<InputType, Rectangle>();
            int index = 0;
            for (int j = 0; j < framesVertical; j++)
            {
                for (int i = 0; i < framesHorizontal ; i++)
                {
                    dRectangles.Add((InputType)index, new Rectangle( (i * iBetweenBufferX) + ( i *width) + iXBuffer, (j *iBetweenBufferY)  + ( j * height) + iYBuffer, width, height));
                    index++;
                    if (index >= a_iMaxTextures) return;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch , InputType inputWanted , Rectangle a_destRect)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, a_destRect, dRectangles[inputWanted], color);
            spriteBatch.End();
        }
    }

    class GameObject
    {
        public Vector4 v4Position { get; set; }
        public bool bIsActive { get; set; }
        public Texture2D texture { get; set; }

        public GameObject() 
        {
            v4Position = Vector4.Zero;
            bIsActive = false;
        }
    }

    class Tile : GameObject
    {
        public enum TileState { Quarry, Active, Defaulted, Dead };
        public TileState state;
        public InputType inputWanted;
        InputCat inputCat; //helps narrow down the check for input given
        //int spriteIndex;

        public Tile(InputType a_input, Vector4 a_position, TileState a_state)
        {
            inputWanted = a_input;
            CalculateCategory();
            v4Position = a_position;
            state = a_state;
            bIsActive = true;
            //TODO: find texture

        }

        public void Update()
        {
            //Adjust position on Screen
            //called by TileManager

        }

        public void Draw(SpriteBatch a_spriteBatch , TileSpriteManager a_SpriteManager )
        {
            a_SpriteManager.Draw(a_spriteBatch, inputWanted, new Rectangle((int)v4Position.X, (int)v4Position.Y, (int)v4Position.Z, (int)v4Position.W));
            //spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            ////spriteBatch.Draw(texture, v2Position, Color.White);
            //spriteBatch.Draw(texture, new Rectangle((int)v4Position.X, (int)v4Position.Y, (int)v4Position.Z, (int)v4Position.W), Color.White);
            //spriteBatch.End();
        }

        public bool CheckInput(GamePadState a_state)
        {
            bool temp;
            switch (inputCat)
            {
                case InputCat.Button:
                    temp = CheckButton(a_state);
                    break;
                case InputCat.DPad:
                    temp = CheckDpad(a_state);
                    break;
                case InputCat.ThumbStick:
                    temp = CheckThumbStick(a_state);
                    break;
                case InputCat.Trigger:
                    temp = CheckTrigger(a_state);
                    break;
                default:
                    temp = false;
                    break;
            }
            if (temp)
            {
                state = TileState.Dead;
            }
            return temp;
        }

        bool CheckButton(GamePadState a_state)
        {
            switch (inputWanted)
            {
                case InputType.A:
                    if (a_state.Buttons.A == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case InputType.B:
                    if (a_state.Buttons.B == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case InputType.X:
                    if (a_state.Buttons.X == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case InputType.Y:
                    if (a_state.Buttons.Y == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                //case InputType.Back:
                //    if (a_state.Buttons.Back == ButtonState.Pressed)
                //        return true;
                //    else
                //        return false;
                case InputType.Start:
                    if (a_state.Buttons.Start == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case InputType.LeftShoulder:
                    if (a_state.Buttons.LeftShoulder == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case InputType.RightShoulder:
                    if (a_state.Buttons.RightShoulder == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case InputType.LeftStickPress:
                    if (a_state.Buttons.LeftStick == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case InputType.RightStickPress:
                    if (a_state.Buttons.RightStick == ButtonState.Pressed)
                        return true;
                    else
                        return false;
            }
            return false;
        }

        bool CheckDpad(GamePadState a_state)
        {
            switch (inputWanted)
            {
                case InputType.DpadDown:
                    if (a_state.DPad.Down == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case InputType.DpadUp:
                    if (a_state.DPad.Up == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case InputType.DpadLeft:
                    if (a_state.DPad.Left == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                case InputType.DpadRight:
                    if (a_state.DPad.Right == ButtonState.Pressed)
                        return true;
                    else
                        return false;
            }
            return false;
        }

        bool CheckTrigger(GamePadState a_state)
        {
            if (inputWanted == InputType.LeftTrigger)
            {
                if (a_state.Triggers.Left != 0)
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (a_state.Triggers.Right != 0)
                {
                    return true;
                }
                return false;
            }
        }

        bool CheckThumbStick(GamePadState a_state)
        {
            switch (inputWanted)
            {
                case InputType.LeftStickDown:
                    if (a_state.ThumbSticks.Left.Y < 0)
                        return true;
                    return false;
                case InputType.LeftStickUp:
                    if (a_state.ThumbSticks.Left.Y > 0)
                        return true;
                    return false;
                case InputType.LeftStickLeft:
                    if (a_state.ThumbSticks.Left.X < 0)
                        return true;
                    return false;
                case InputType.LeftStickRight:
                    if (a_state.ThumbSticks.Left.X > 0)
                        return true;
                    return false;
                case InputType.RightStickDown:
                    if (a_state.ThumbSticks.Right.Y < 0)
                        return true;
                    return false;
                case InputType.RightStickUp:
                    if (a_state.ThumbSticks.Right.Y > 0)
                        return true;
                    return false;
                case InputType.RightStickLeft:
                    if (a_state.ThumbSticks.Right.X < 0)
                        return true;
                    return false;
                case InputType.RightStickRight:
                    if (a_state.ThumbSticks.Right.X > 0)
                        return true;
                    return false;
                default:
                    return false;
            }
        }

        void CalculateCategory()
        {
            //search name for keywords like "trigger"
            //to figure out which category of input this tile checks for
            string temp = inputWanted.ToString();
            if (temp.Contains("pad"))
            {
                inputCat = InputCat.DPad;
            }
            else if (temp.Contains("Trigger"))
            {
                inputCat = InputCat.Trigger;
            }
            else if (temp.Contains("Stick") && !temp.Contains("Press"))
            {
                inputCat = InputCat.ThumbStick;
            }
            else
            {
                inputCat = InputCat.Button;
            }
        }

    }

    struct TileSet
    {
        public float timeInBetweenTilesAdd;
        public float timeToSubtractFromWait;
        public float timeSinceLastTile;
    }

    class TileManager
    {
        GameManager manager;
        TileSpriteManager tileSprites;
        Texture2D vsBackgroundTexture;
        Texture2D singleBackgroundTexture;
        Rectangle vsSourceRect;
        Rectangle singleSourceRect;
        Queue<Tile> qTilesInWait;
        Tile[] aTilesOnField;
        Vector4[] av4TileLocationsAndSize; //x an y position , z and w? for size
        int iMaxActiveTiles;
        public bool bMulitplayer = false;
        TileSet tileSetOne;
        TileSet tileSetTwo;
        GameTime lastGameTime;

        Random rand;

        public TileManager(int a_MaxTiles, GameManager a_manager, float a_between, float a_subtract)
        {
            iMaxActiveTiles = a_MaxTiles;
            aTilesOnField = new Tile[iMaxActiveTiles];
            qTilesInWait = new Queue<Tile>();
            rand = new Random();
            AddToQueue(10);
            //av2TileSpots = new Vector2[iMaxActiveTiles];
            manager = a_manager;
            lastGameTime = new GameTime();
            tileSetOne.timeInBetweenTilesAdd = a_between;
            tileSetOne.timeSinceLastTile = 0;
            tileSetOne.timeToSubtractFromWait = a_subtract;

            tileSetTwo.timeInBetweenTilesAdd = a_between;
            tileSetTwo.timeSinceLastTile = 0;
            tileSetTwo.timeToSubtractFromWait = a_subtract;
            //timeInBetweenTilesAdd = a_between;
            //timeToSubtractFromWait = a_subtract;
        }

        void AddToQueue(int iAmountToAdd)
        {
            if (av4TileLocationsAndSize == null) return; //array still needs to be filled
            for (int i = 0; i < iAmountToAdd; i++)
            {
                //TODO change Vector.zero to actual spot on screen
                Tile t = new Tile((InputType)rand.Next(22), Vector4.Zero, Tile.TileState.Quarry);
                qTilesInWait.Enqueue(t);
            }
        }

        void FromQueueToArray(int aiIndex)
        {
            //take oldest tile from queue and add it to the array
            if (qTilesInWait.Count < 1) return; //nothing in queue, then we're done here

            Tile temp = qTilesInWait.Dequeue();
            //add position
            temp.v4Position = av4TileLocationsAndSize[aiIndex];
            //temp.texture = CalculateTexture(temp.inputWanted);
            aTilesOnField[aiIndex] = temp;
            aTilesOnField[aiIndex].state = Tile.TileState.Active;
        }

        void CheckForInput( GamePadState a_stateOne, GamePadState a_stateTwo)
        {
            //TODO: learn to use threading
            //if (!bMulitplayer)
            //{
                for (int i = 0; i < aTilesOnField.Length; i++)
                {
                    Tile t = aTilesOnField[i];
                    //checks through state to check input pressed
                    if (t == null) continue; //if blank, then it doesn't care about input
                    if (t.CheckInput(a_stateOne) && ( !bMulitplayer || i < (aTilesOnField.Length / 2 ) ) )
                    {//inputwanted was given, remove from list of active tiles
                        //think this can be handled better
                        aTilesOnField[i] = null;
                        manager.Scored(1);
                        tileSetOne.timeInBetweenTilesAdd -= tileSetOne.timeToSubtractFromWait;//speed up the tiles
                        //TODO: add min time between
                    }
                    if (bMulitplayer && (i > aTilesOnField.Length / 2) && t.CheckInput(a_stateTwo))
                    {
                        aTilesOnField[i] = null;
                        manager.Scored(2);
                        tileSetTwo.timeInBetweenTilesAdd -= tileSetTwo.timeToSubtractFromWait;
                    }
                }
            //}
        }

        public void Update(GamePadState a_currentStateOne , GamePadState a_currentStateTwo, GameTime a_gameTime)
        {
            //check for input
            CheckForInput(a_currentStateOne, a_currentStateTwo);
            //TODO: add check for game lost

            //Add tiles to blank spaces
            float deltaTime = (float)(a_gameTime.ElapsedGameTime.TotalSeconds);// - lastGameTime.ElapsedGameTime.TotalMilliseconds);
            tileSetOne.timeSinceLastTile += deltaTime;
            if (tileSetOne.timeSinceLastTile >= tileSetOne.timeInBetweenTilesAdd)
            {
                bool bFull = true; //if remains true, then game is lost
                for (int i = 0; i < iMaxActiveTiles; i++)
                {
                    if (aTilesOnField[i] == null) //tile in wait to be filled
                    {
                        bFull = false;
                        tileSetOne.timeSinceLastTile = 0;
                        FromQueueToArray(i);
                        break; // never add more than one tile per update
                    }
                }
                if (bFull)
                {
                    manager.ChangeState(Gamestate.ScoreScreen);
                }

            }
            if( bMulitplayer )
            {
                tileSetTwo.timeSinceLastTile += deltaTime;
                if (tileSetTwo.timeSinceLastTile >= tileSetTwo.timeInBetweenTilesAdd)
                {
                    bool bFull2 = true; //if remains true, then game is lost
                    for (int i = iMaxActiveTiles / 2; i < iMaxActiveTiles; i++)
                    {
                        if (aTilesOnField[i] == null) //tile in wait to be filled
                        {
                            bFull2 = false;
                            tileSetTwo.timeSinceLastTile = 0;
                            FromQueueToArray(i);
                            break; // never add more than one tile per update
                        }
                    }
                    if (bFull2)
                    {
                        manager.ChangeState(Gamestate.ScoreScreen);
                    }

                }
            }
            
                //ensure queue have plenty of backups
                if (qTilesInWait.Count < 8)
                    AddToQueue(2);
        }

        public void DrawTiles(SpriteBatch a_spriteBatch)
        {
            a_spriteBatch.Begin();
            if (bMulitplayer)
                a_spriteBatch.Draw(vsBackgroundTexture, new Rectangle(0, 0, manager.graphics.GraphicsDevice.Viewport.Width, manager.graphics.GraphicsDevice.Viewport.Height), vsSourceRect, Color.White);
            else
                a_spriteBatch.Draw(singleBackgroundTexture, new Rectangle(0, 0, manager.graphics.GraphicsDevice.Viewport.Width, manager.graphics.GraphicsDevice.Viewport.Height), singleSourceRect, Color.White);
            a_spriteBatch.End();
            foreach (Tile t in aTilesOnField)
            {
                if (t != null)
                    t.Draw(a_spriteBatch , tileSprites);
            }
        }

        public void AssignLocationsArray(ref Vector4[] a_locations)
        {
            av4TileLocationsAndSize = new Vector4[a_locations.Length];
            for (int i = 0; i < av4TileLocationsAndSize.Length; i++)
            {
                av4TileLocationsAndSize[i] = new Vector4();
                av4TileLocationsAndSize[i].X = a_locations[i].X;
                av4TileLocationsAndSize[i].Y = a_locations[i].Y;
                av4TileLocationsAndSize[i].Z = a_locations[i].Z;
                av4TileLocationsAndSize[i].W = a_locations[i].W;
            }
            AddToQueue(6);
        }

        public void LoadTextures(ContentManager a_content)
        {
            tileSprites = new TileSpriteManager(a_content.Load<Texture2D>("Tiles/CB_tiles_2k_d"), 5 ,5 , 23);
            vsBackgroundTexture = a_content.Load<Texture2D>("Backgrounds/background_2_d");
            vsSourceRect = new Rectangle(0, 0, vsBackgroundTexture.Width, vsBackgroundTexture.Height);
            singleBackgroundTexture = a_content.Load<Texture2D>("Backgrounds/background_4_d");
            singleSourceRect = new Rectangle(0, 0, singleBackgroundTexture.Width, singleBackgroundTexture.Height / 2);
        }

        public void Reset(float a_timeBetween)
        {
            tileSetOne.timeInBetweenTilesAdd = a_timeBetween;
            tileSetOne.timeSinceLastTile = 0;
            tileSetTwo.timeInBetweenTilesAdd = a_timeBetween;
            tileSetTwo.timeSinceLastTile = 0;
            aTilesOnField = new Tile[iMaxActiveTiles];
            qTilesInWait = new Queue<Tile>();

            
        }

        /*Texture2D*/ void CalculateTexture(InputType a_input)
        {
            //switch (a_input)
            //{
            //    case InputType.A:
            //        return lTilesTextures[0];
            //    case InputType.B:
            //        return lTilesTextures[1];
            //    case InputType.X:
            //        return lTilesTextures[2];
            //    case InputType.Y:
            //        return lTilesTextures[3];
            //    default:
            //        return lTilesTextures[4];
            //}
        }
    }

    class MainMenu
    {
        Texture2D backGroundTexture;
        Texture2D logoTexture;
        Rectangle spriteSourceRect;
        public SpriteFont spriteFont;
        Color fontColor;
        int opacityChange;
        GameManager manager;
        bool bTextDrawn = false;
        MenuState state;
        int fInputDelay = 1000; //milliseconds
        int fTimeSince = 0;   //milliseconds

        public MainMenu(GameManager a_manager)
        {
            manager = a_manager;
            fontColor = Color.White;
            opacityChange = 5;
            state = MenuState.PressStart;
        }

        public void LoadContent(ContentManager content , int a_iHalfWanted)
        {
            backGroundTexture = content.Load<Texture2D>("Backgrounds/background_1_d");
            logoTexture = content.Load<Texture2D>("ControllerBustterLogo");
            //assumes texture is in two halfs
            //int height = (backGroundTexture.Height / 2) -200;
            spriteSourceRect = new Rectangle(0, backGroundTexture.Height * (a_iHalfWanted -1), backGroundTexture.Width, backGroundTexture.Height/2);
            spriteFont = content.Load<SpriteFont>("SpriteFonts/SpriteFont1");
        }

        public void Update(GamePadState a_gamePadState, GameTime a_gameTime)
        {
            fTimeSince += a_gameTime.ElapsedGameTime.Milliseconds;
            if (fTimeSince > fInputDelay)
            {
                //get from press start to single state
                if (a_gamePadState.Buttons.Start == ButtonState.Pressed)
                {
                    if (state == MenuState.PressStart)
                    {
                        state = MenuState.Single;
                        fTimeSince = 0;
                    }
                }
                if (a_gamePadState.Buttons.A == ButtonState.Pressed)
                {
                    if (state == MenuState.Single )//|| state == MenuState.VS)
                    {
                        manager.ChangeState(Gamestate.CoreGame);
                        fTimeSince = 0;
                    }
                    else if (state == MenuState.VS)
                    {
                        manager.ChangeState(Gamestate.Multiplayer);
                        fTimeSince = 0;
                    }
                }
                //pop between single VS
                if (( a_gamePadState.ThumbSticks.Left.Y != 0 && state != MenuState.PressStart) )// && !manager.bTrialVersion)
                {
                    if (state == MenuState.Single) state = MenuState.VS;
                    else if (state == MenuState.VS) state = MenuState.Single;

                    fTimeSince = 0;
                }
                if (a_gamePadState.Buttons.Back == ButtonState.Pressed || a_gamePadState.Buttons.B == ButtonState.Pressed)
                {
                    if (state == MenuState.PressStart)
                    {
                        manager.ChangeState(Gamestate.Exit);
                    }
                    else
                    {
                        state = MenuState.PressStart;
                        fTimeSince = 0;
                    }
                }
            }
            AdjustOpacity();
        }

        void AdjustOpacity()
        {
            if (!bTextDrawn) return; //wait til bTextDrawn is true before running next bit
            //due to bytes never being negative, store value as int, manipulate, then reassign
            int opacity = fontColor.A;
            if (opacityChange < 0 && opacity + opacityChange <= opacityChange)// || opacity + opacityChange >= 255 + opacityChange)
            {
                opacity = 0;
                opacityChange *= -1;
            }
            if (opacityChange > 0 && /*opacity + opacityChange > 0 + opacityChange)||*/ opacity + opacityChange >= 255 - opacityChange)
            {
                opacity = 255;
                opacityChange *= -1;
            }
            else
            {
                opacity += opacityChange;
            }
            if( manager.bTrialVersion && state == MenuState.VS)
                fontColor = new Color(255, opacity, opacity, opacity);
            else
                fontColor = new Color(opacity, opacity, opacity, opacity);
            bTextDrawn = false;
        }

        public void Draw(SpriteBatch a_spriteBatch, GraphicsDevice a_device)
        {
            //Viewport view = manager.graphics.GraphicsDevice.Viewport;
            Vector2 temp = new Vector2(manager.graphics.GraphicsDevice.Viewport.Width * 0.05f, manager.graphics.GraphicsDevice.Viewport.Height * 0.2f);
            
            if (state == MenuState.PressStart)
            {
                a_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                a_spriteBatch.Draw(backGroundTexture, new Rectangle(0, 0, a_device.Viewport.Width, a_device.Viewport.Height), spriteSourceRect , Color.White);
                a_spriteBatch.Draw(logoTexture, temp, Color.White);
                a_spriteBatch.DrawString(spriteFont, "Press Start To Begin", new Vector2(manager.graphics.GraphicsDevice.Viewport.Width / 2.8f, manager.graphics.GraphicsDevice.Viewport.Height / 1.8f), fontColor);
                a_spriteBatch.End();
                bTextDrawn = true;
            }
            else
            {
                a_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                a_spriteBatch.Draw(backGroundTexture, new Rectangle(0, 0, a_device.Viewport.Width, a_device.Viewport.Height), spriteSourceRect , Color.White);
                a_spriteBatch.Draw(logoTexture, temp, Color.White);
                if (state == MenuState.Single)
                    a_spriteBatch.DrawString(spriteFont, "Single", new Vector2(manager.graphics.GraphicsDevice.Viewport.Width / 2.25f, manager.graphics.GraphicsDevice.Viewport.Height / 1.8f), fontColor);
                else
                    a_spriteBatch.DrawString(spriteFont, "Single", new Vector2(manager.graphics.GraphicsDevice.Viewport.Width / 2.25f, manager.graphics.GraphicsDevice.Viewport.Height / 1.8f), Color.White);
                if (manager.bTrialVersion)
                {
                    if( state == MenuState.VS) //disable multiplier for trail version
                        a_spriteBatch.DrawString(spriteFont, "VS - Locked In Trial", new Vector2(manager.graphics.GraphicsDevice.Viewport.Width / 2.8f, manager.graphics.GraphicsDevice.Viewport.Height / 1.4f), fontColor);//new Color( 0.5f , 0.5f , 0.5f , 0.5f) );
                    else
                        a_spriteBatch.DrawString(spriteFont, "VS - Locked In Trial", new Vector2(manager.graphics.GraphicsDevice.Viewport.Width / 2.8f, manager.graphics.GraphicsDevice.Viewport.Height / 1.4f), Color.Red);//new Color( 0.5f , 0.5f , 0.5f , 0.5f) );
                }
                else if (state == MenuState.VS)
                    a_spriteBatch.DrawString(spriteFont, "VS", new Vector2(manager.graphics.GraphicsDevice.Viewport.Width / 2.1f, manager.graphics.GraphicsDevice.Viewport.Height / 1.4f), fontColor);
                else
                    a_spriteBatch.DrawString(spriteFont, "VS", new Vector2(manager.graphics.GraphicsDevice.Viewport.Width / 2.1f, manager.graphics.GraphicsDevice.Viewport.Height / 1.4f), Color.White);
                
                a_spriteBatch.End();
                bTextDrawn = true;
            }
        }
    }

    class GameManager
    {
        
        public MainMenu mainMenu;
        public GraphicsDeviceManager graphics;
        public PlayerIndex playerOneIndex;
        public PlayerIndex playerTwoIndex;
        public bool bTrialVersion = false;

        TileManager tileManager;
        Gamestate state;
        Texture2D scoreScreenTexture;
        int iMaxTiles;
        int iMaxTilesPerRow;
        int iPlayerOneScore;
        int iPlayerTwoScore;
        bool bViewPortCalculated = false;
        bool bPlayerOneFound = false;
        float timeInBetweenTilesAdd;
        float timeToSubtractFromWait;

        //testing
        IAsyncResult syncResult;

        public GameManager( float a_TimeInBetween, float a_TimeToSubtract, int a_MaxTiles, int a_MaxTilesPerRow, 
                            ref GraphicsDeviceManager a_Graphics , bool a_bTrialVersion 
                            /*, PlayerIndex a_playerOneIndex, PlayerIndex a_playerTwoIndex*/)
        {
            graphics = a_Graphics;
            timeInBetweenTilesAdd = a_TimeInBetween;
            timeToSubtractFromWait = a_TimeToSubtract;
            iMaxTiles = a_MaxTiles;
            iMaxTilesPerRow = a_MaxTilesPerRow;
            //Vector4[] v4Locations = CalculateLocations(a_MaxTiles, a_MaxTilesPerRow);
            mainMenu = new MainMenu(this);
            tileManager = new TileManager(a_MaxTiles, this, timeInBetweenTilesAdd, timeToSubtractFromWait); //check if this is needed, if so, replace "this"
            state = Gamestate.MainMenu;
            bTrialVersion = a_bTrialVersion;

            playerOneIndex = PlayerIndex.One;
            playerTwoIndex = PlayerIndex.Two;
        }

        Vector4[] CalculateLocations(int a_MaxTiles, int a_MaxTilesPerRow)
        {
            //based on max tiles, figure out location and size each tile 
            Vector4[] locations = new Vector4[a_MaxTiles];
            //for each x tiles, add new line
            int iNumOfRows = (int)MathHelper.Clamp((float)(a_MaxTilesPerRow / 10), 1, 10);
            //I want tiles to take up 3/4 of the screen -fucking magic number!
            float XSpace = (graphics.GraphicsDevice.Viewport.Width) / a_MaxTilesPerRow;
            float YSpace = (graphics.GraphicsDevice.Viewport.Height) / (iNumOfRows + 1);//+1 due to 1 row to divide by 2

            //size of tile
            float XSize = XSpace / 1.2f;
            float YSize = YSpace / 2;
            float size = Math.Min(XSize, YSize); // make them square

            //fill array
            float buffer = 10;
            for (int i = 0; i <= iNumOfRows; i++)
                for (int j = 0; j < a_MaxTilesPerRow; j++)
                {
                    int iIndex = a_MaxTilesPerRow * i + j;
                    if (iIndex >= a_MaxTiles) break;
                    locations[iIndex] = new Vector4(
                        XSpace * j + buffer,
                        YSpace * i + buffer,
                        size,
                        size
                        );
                }
            return locations;
        }

        public bool Update(GameTime gameTime) // return true if game needs to end
        {
            if( !bPlayerOneFound )
                CheckForStart(); // only do this for checking which controller should be player one

            GamePadState currentState = GamePad.GetState( playerOneIndex);
            GamePadState playerTwoState = GamePad.GetState( playerTwoIndex );
            switch (state)
            {
                case Gamestate.CoreGame:
                    if (!bViewPortCalculated)
                    {
                        Vector4[] v4Locations = CalculateLocations(iMaxTiles, iMaxTilesPerRow);
                        tileManager.AssignLocationsArray(ref v4Locations);
                        bViewPortCalculated = true;
                    }
                    tileManager.Update(currentState , playerTwoState, gameTime);
                    break;
                case Gamestate.Multiplayer:
                    if (!bViewPortCalculated)
                    {
                        Vector4[] v4Locations = CalculateLocations(iMaxTiles, iMaxTilesPerRow);
                        tileManager.AssignLocationsArray(ref v4Locations);
                        bViewPortCalculated = true;
                    }
                    tileManager.Update(currentState , playerTwoState, gameTime);
                    break;
                case Gamestate.MainMenu:
                    mainMenu.Update(currentState, gameTime);
                    break;
                case Gamestate.ScoreScreen:
                    if (currentState.Buttons.A == ButtonState.Pressed || currentState.Buttons.Start == ButtonState.Pressed)
                    {
                        ChangeState(Gamestate.MainMenu);
                    }
                    break;
                case Gamestate.Exit:
                    return true;
                default:

                    break;
            }
            return false;
        }

        public void DrawObjects(SpriteBatch a_spriteBatch)
        {
            switch (state)
            {
                case Gamestate.CoreGame:
                    tileManager.DrawTiles(a_spriteBatch);
                    //in coregame  draw score
                    a_spriteBatch.Begin();
                    a_spriteBatch.DrawString(mainMenu.spriteFont, "Score: " + iPlayerOneScore.ToString(), new Vector2(graphics.GraphicsDevice.Viewport.Width * 0.8f, graphics.GraphicsDevice.Viewport.Height * 0.2f), Color.White);
                    a_spriteBatch.End();
                    break;
                case Gamestate.Multiplayer:
                    tileManager.DrawTiles(a_spriteBatch);
                    //in coregame  draw score
                    a_spriteBatch.Begin();
                    a_spriteBatch.DrawString(mainMenu.spriteFont, "Player 1 Score: " + iPlayerOneScore.ToString(), new Vector2(graphics.GraphicsDevice.Viewport.Width * 0.0f, graphics.GraphicsDevice.Viewport.Height * 0.4f), Color.White);
                    a_spriteBatch.DrawString(mainMenu.spriteFont, "Player 2 Score: " + iPlayerTwoScore.ToString(), new Vector2(graphics.GraphicsDevice.Viewport.Width * 0.4f, graphics.GraphicsDevice.Viewport.Height * 0.4f), Color.White);
                    a_spriteBatch.End();
                    break;
                case Gamestate.MainMenu:
                    mainMenu.Draw(a_spriteBatch, graphics.GraphicsDevice);
                    break;
                case Gamestate.ScoreScreen:
                    a_spriteBatch.Begin();
                    a_spriteBatch.Draw(scoreScreenTexture, new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height), new Rectangle(0 , 0 , scoreScreenTexture.Width , scoreScreenTexture.Height / 2) , Color.White) ;
                    a_spriteBatch.DrawString(mainMenu.spriteFont, "Game Over", new Vector2(graphics.GraphicsDevice.Viewport.Width * 0.425f, graphics.GraphicsDevice.Viewport.Height * 0.3f), Color.White);
                    a_spriteBatch.DrawString(mainMenu.spriteFont, "Player 1 : " + iPlayerOneScore.ToString() + " " , new Vector2(graphics.GraphicsDevice.Viewport.Width * 0.4f, graphics.GraphicsDevice.Viewport.Height * 0.5f), Color.White);
                    if (tileManager.bMulitplayer)
                        a_spriteBatch.DrawString(mainMenu.spriteFont, "Player 2 : " + iPlayerTwoScore.ToString(), new Vector2(graphics.GraphicsDevice.Viewport.Width * 0.4f, graphics.GraphicsDevice.Viewport.Height * 0.6f), Color.White);
                    a_spriteBatch.End();
                    break;
                default:
                    break;
            }
        }

        public void Scored( int iPlayerScored)
        {
            //add to score
            if (iPlayerScored == 1)
                iPlayerOneScore += 1;
            else if (iPlayerScored == 2)
                iPlayerTwoScore += 1;

        }

        public void LoadContent(ContentManager a_content)
        {
            scoreScreenTexture = a_content.Load<Texture2D>("Backgrounds/background_3_d");
            mainMenu.LoadContent(a_content , 1);
            tileManager.LoadTextures(a_content);
        }

        public void ChangeState(Gamestate a_state)
        {
            if (a_state == Gamestate.Multiplayer)
            {
                //setup multiplayer
                //Guide.ShowSignIn(2, false);
                if (bTrialVersion)
                {
                    //TODO: bring up purchase menu
                    if (!Gamer.SignedInGamers.Cast<SignedInGamer>().Any(x => x.PlayerIndex == playerOneIndex))
                    {
                        List<string> MBOptions = new List<string>();
                        MBOptions.Add("OK, Sign Me In");
                        MBOptions.Add("I Don't Want to Sign In");
                        Guide.BeginShowMessageBox("Not connected to LIVE", "You must be connected to Live in order to access the marketplace. Please sign in.", MBOptions,
                            0, Microsoft.Xna.Framework.GamerServices.MessageBoxIcon.Warning, GetMBResult, null);
                            
                    }
                    Guide.ShowMarketplace(playerOneIndex);
                }
                else
                {
                    tileManager.bMulitplayer = true;
                    state = Gamestate.Multiplayer;
                }
            }
            else if (a_state == Gamestate.CoreGame)
            {
                tileManager.bMulitplayer = false;
                state = Gamestate.CoreGame;
            }
            else
            {
                state = a_state;
                tileManager.Reset(timeInBetweenTilesAdd);
            }
        }

        public void CheckForStart()
        {
            for (int i = 0; i < 4; i++)
            {
                if (GamePad.GetState((PlayerIndex)i).Buttons.Start == ButtonState.Pressed)
                {
                    playerOneIndex = (PlayerIndex)i;
                    bPlayerOneFound = true;
                    break;
                }
            }
        }

        void GetMBResult(IAsyncResult r)
        {
            int? b = Guide.EndShowMessageBox(r);
            if (b == 0)
            {
                string test = "testing";
            }
        }
    }//end of GameManager class
}//end of WindowsVersion namespace
