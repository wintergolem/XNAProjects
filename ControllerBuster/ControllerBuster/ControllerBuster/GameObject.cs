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
//using Microsoft.Xna.Framework.GamerServices;

namespace ControllerBuster
{
    //enums
    public enum InputType
    {
        A, B, X, Y, DpadUp,
        DpadLeft, DpadDown, DpadRight, LeftStickUp, LeftStickLeft,
        LeftStickDown, LeftStickRight, RightStickUp, RightStickLeft, RightStickDown,
        RightStickRight, LeftStickPress, RightStickPress, LeftShoulder, RightShoulder,
        Back, LeftTrigger, RightTrigger

    };//23 total options
    public enum InputCat { Button, ThumbStick, Trigger, DPad };
    public enum Gamestate { MainMenu, CoreGame, Multiplayer, Exit, ScoreScreen };
    public enum MenuState { PressStart, Single, VS };

    //structs
    public struct PlayerScoreStruct
    {
        public int iPointsScored;
        public int iPointsPossible;
        public int iNegativePoints;
    }

    struct TileSet
    {
        public float timeInBetweenTilesAdd;
        public float timeToSubtractFromWait;
        public float timeSinceLastTile;
    }

    //classes
    public class InputRecordEntry
    {
        //class based around trying to add a buffer around input to prevent unnessarity negative points
        public InputCat cat;
        public InputType type;
        public float fTimeToKill;

        public InputRecordEntry(InputCat a_cat, InputType a_type, float a_fTime)
        {
            cat = a_cat;
            type = a_type;
            fTimeToKill = a_fTime;
        }

        public bool Update( float a_fTime)
        {//returns true if update was successful, if not, then time kill this
            if (a_fTime >= fTimeToKill)
                return false;
            return true;
        }
    }

    public class TileSpriteManager
    {

        protected Texture2D texture;

        public Vector2 position = Vector2.Zero;
        public Color color = Color.White;
        public Vector2 origin;
        public float rotation = 0f;
        public float scale = 1f;
        public SpriteEffects spriteEffect;
        protected Dictionary<InputType, Rectangle> dRectangles;
        protected int frameIndex = 0;

        public TileSpriteManager(Texture2D a_texture, int framesHorizontal, int framesVertical, int a_iMaxTextures)
        {
            this.texture = a_texture;
            int iXBuffer = 66;
            int iYBuffer = 60;
            int iBetweenBufferX = 0;
            int iBetweenBufferY = 0;
            int width = texture.Width;
            width = width - (70 + iXBuffer);
            width -= (iBetweenBufferX * 4);
            width /= framesHorizontal;
            int height = ((texture.Height - (91 + iYBuffer)) - (iBetweenBufferY * 4)) / framesVertical;
            dRectangles = new Dictionary<InputType, Rectangle>();
            int index = 0;
            for (int j = 0; j < framesVertical; j++)
            {
                for (int i = 0; i < framesHorizontal; i++)
                {
                    dRectangles.Add((InputType)index, new Rectangle((i * iBetweenBufferX) + (i * width) + iXBuffer, (j * iBetweenBufferY) + (j * height) + iYBuffer, width, height));
                    index++;
                    if (index >= a_iMaxTextures) return;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, InputType inputWanted, Rectangle a_destRect)
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
        public InputCat inputCat; //helps narrow down the check for input given and used for record entry in tilemanager
        public bool bMoving { get; protected set; }
        Vector2 v2EndPosition;
        const float fSpeed = 0.1f;

        public Tile(InputType a_input, Vector4 a_positionStart, TileState a_state)
        {
            inputWanted = a_input;
            CalculateCategory();
            v4Position = a_positionStart;
            state = a_state;
            bIsActive = true;
            bMoving = false;
        }

        public void Update( float a_deltaTime)
        {
            //Adjust position on Screen
            //called by TileManager
            if (bMoving)
            {
                Vector2 moveVector = new Vector2(v2EndPosition.X - v4Position.X , v2EndPosition.Y - v4Position.Y );
               
                //moveVector *= fSpeed;
                v4Position += new Vector4( moveVector * a_deltaTime, 0,0);

                Vector2 pos = new Vector2( v4Position.X , v4Position.Y);
                if (Vector2.Distance(pos, v2EndPosition) < 2)
                {
                    bMoving = false;
                    pos = v2EndPosition;
                    v4Position = new Vector4(pos, v4Position.Z, v4Position.W);
                }
            }
        }

        public void Draw(SpriteBatch a_spriteBatch, TileSpriteManager a_SpriteManager)
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
                case InputType.Back:
                    if (a_state.Buttons.Back == ButtonState.Pressed)
                        return true;
                    else
                        return false;
                //case InputType.Start:
                //    if (a_state.Buttons.Start == ButtonState.Pressed)
                //        return true;
                //    else
                //        return false;
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

        public void MoveTo(Vector2 a_pos)
        {
            v2EndPosition = a_pos;
            bMoving = true;
        }
    }

    class TileManager
    {
        List<InputRecordEntry> lInputRecord;
        GamePadState testState;
        GameManager manager;
        GameTime lastGameTime;
        Queue<Tile> qTilesInWait;
        Rectangle vsSourceRect;
        Rectangle singleSourceRect;
        Texture2D vsBackgroundTexture;
        Texture2D singleBackgroundTexture;
        TileSet tileSetOne;
        Tile[] aTilesOnField;
        Tile testTile;
        Vector4[] av4TileLocationsAndSize; //x an y position , z and w? for size
        Vector2 v2TileStartingPoint;

        bool bInputSuccess = false;
        float fTimeBetweenSubtract = 1500; //milliseconds
        float fTimeSinceSubtract = 0;   //seconds
        float fInputRecordTime = 1500; //milliseconds
        int iMaxActiveTiles;
        int i; // used for looping

        public bool bMulitplayer = false;
        public bool bIsMultiManager;
       

        Random rand;

        public TileManager(int a_MaxTiles, GameManager a_manager, float a_between, float a_subtract, Vector2 a_TileStart, bool a_bIsSecondManager)
        {
            iMaxActiveTiles = a_MaxTiles;
            aTilesOnField = new Tile[iMaxActiveTiles];
            qTilesInWait = new Queue<Tile>();
            rand = new Random(DateTime.Now.Millisecond);
            AddToQueue(10);
            //av2TileSpots = new Vector2[iMaxActiveTiles];
            manager = a_manager;
            lastGameTime = new GameTime();
            tileSetOne.timeInBetweenTilesAdd = a_between;
            tileSetOne.timeSinceLastTile = 0;
            tileSetOne.timeToSubtractFromWait = a_subtract;
            v2TileStartingPoint = a_TileStart;
            bIsMultiManager = a_bIsSecondManager;
            testState = new GamePadState();
            testTile = new Tile(InputType.A, Vector4.Zero, Tile.TileState.Defaulted);
            lInputRecord = new List<InputRecordEntry>();
        }

        public void Update(GamePadState a_currentStateOne, GameTime a_gameTime)
        {
            //update tile entries
            do
            {
                bInputSuccess = false; //not its orignal purpose, but there is no harm in using it here
                for (i = 0; i < lInputRecord.Count; i++)
                {
                    if (!lInputRecord[i].Update( (float)a_gameTime.TotalGameTime.TotalMilliseconds) )
                    {
                        lInputRecord.RemoveAt(i);
                        bInputSuccess = true;
                    }
                }
            } while (bInputSuccess);

            //update tiles position
            foreach (Tile t in aTilesOnField)
                if( t != null)
                    t.Update((float)(a_gameTime.ElapsedGameTime.Milliseconds)/100);

            //check for input
            CheckForInput(a_currentStateOne , a_gameTime);

            fTimeSinceSubtract += a_gameTime.ElapsedGameTime.Milliseconds;
            if (fTimeSinceSubtract > fTimeBetweenSubtract)
            {
                fTimeSinceSubtract = 0;
                tileSetOne.timeInBetweenTilesAdd -= tileSetOne.timeToSubtractFromWait;//speed up the tiles
            }
            int iMaxTiles = bMulitplayer ? manager.iMaxTilesMulti : iMaxActiveTiles;
            //Add tiles to blank spaces
            float deltaTime = (float)(a_gameTime.ElapsedGameTime.TotalSeconds);// - lastGameTime.ElapsedGameTime.TotalMilliseconds);
            tileSetOne.timeSinceLastTile += deltaTime;
            if (tileSetOne.timeSinceLastTile >= tileSetOne.timeInBetweenTilesAdd)
            {
                bool bFull = true; //if remains true, then game is lost
                bool bAllStopped = true;
                for (int i = 0; i < iMaxTiles; i++)
                {
                    if (aTilesOnField[i] == null) //tile in wait to be filled
                    {
                        bFull = false;
                        tileSetOne.timeSinceLastTile = 0;
                        FromQueueToArray(i);
                        break; // never add more than one tile per update
                    }
                    else if (aTilesOnField[i].bMoving)
                    {
                        bAllStopped = false;
                    }
                }
                if (bFull && bAllStopped)
                {
                    manager.ChangeState(Gamestate.ScoreScreen);
                }

            }

            //ensure queue have plenty of backups
            if (qTilesInWait.Count < 8)
                AddToQueue(2);
        }

        public void DrawTiles(SpriteBatch a_spriteBatch)
        {
            if (!bIsMultiManager)
            {
                a_spriteBatch.Begin();
                if (bMulitplayer)
                    a_spriteBatch.Draw(vsBackgroundTexture, new Rectangle(0, 0, manager.graphics.GraphicsDevice.Viewport.Width, manager.graphics.GraphicsDevice.Viewport.Height), vsSourceRect, Color.White);
                else
                    a_spriteBatch.Draw(singleBackgroundTexture, new Rectangle(0, 0, manager.graphics.GraphicsDevice.Viewport.Width, manager.graphics.GraphicsDevice.Viewport.Height), singleSourceRect, Color.White);
                a_spriteBatch.End();
            }
            foreach (Tile t in aTilesOnField)
            {
                if (t != null)
                    t.Draw(a_spriteBatch, manager.tileSprites);
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
           // tileSprites = new TileSpriteManager(a_content.Load<Texture2D>("Tiles/CB_tiles_2k_d"), 5, 5, 23);
            if (!bIsMultiManager)
            {
                vsBackgroundTexture = a_content.Load<Texture2D>("Backgrounds/background_2_d");
                vsSourceRect = new Rectangle(0, 0, vsBackgroundTexture.Width, vsBackgroundTexture.Height);
                singleBackgroundTexture = a_content.Load<Texture2D>("Backgrounds/background_4_d");
                singleSourceRect = new Rectangle(0, 0, singleBackgroundTexture.Width, singleBackgroundTexture.Height / 2);
            }
        }

        public void Reset(float a_timeBetween)
        {
            tileSetOne.timeInBetweenTilesAdd = a_timeBetween;
            tileSetOne.timeSinceLastTile = 0;
            //tileSetTwo.timeInBetweenTilesAdd = a_timeBetween;
            //tileSetTwo.timeSinceLastTile = 0;
            aTilesOnField = new Tile[iMaxActiveTiles];
            qTilesInWait = new Queue<Tile>();
            fTimeSinceSubtract = 0;

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

            while (aiIndex > (bMulitplayer ? manager.iMaxTilesMulti : manager.iMaxTilesPerRow - 1))
            {
                //tell index below to move down
                aTilesOnField[aiIndex] = aTilesOnField[aiIndex - manager.iMaxTilesPerRow];
                aTilesOnField[aiIndex - manager.iMaxTilesPerRow] = null;
                aTilesOnField[aiIndex].MoveTo(new Vector2(av4TileLocationsAndSize[aiIndex].X, av4TileLocationsAndSize[aiIndex].Y));
                //adjust array accordingly
                //decrease index
                aiIndex -= manager.iMaxTilesPerRow;
            }

            Tile temp = qTilesInWait.Dequeue();
            //add position
            temp.v4Position = new Vector4(v2TileStartingPoint, av4TileLocationsAndSize[aiIndex].Z, av4TileLocationsAndSize[aiIndex].W);
            //temp.texture = CalculateTexture(temp.inputWanted);
            aTilesOnField[aiIndex] = temp;
            aTilesOnField[aiIndex].state = Tile.TileState.Active;
            aTilesOnField[aiIndex].MoveTo(new Vector2(av4TileLocationsAndSize[aiIndex].X, av4TileLocationsAndSize[aiIndex].Y));

        }

        void CheckForInput(GamePadState a_stateOne , GameTime a_time)
        {
            //check to see if there is input
            if (manager.ComparePadState(a_stateOne, testState))
            {
                bInputSuccess = false;
                for (int i = 0; i < aTilesOnField.Length; i++)
                {
                    Tile t = aTilesOnField[i];
                    //checks through state to check input pressed
                    if (t == null) continue; //if blank, then it doesn't care about input
                    if (t.CheckInput(a_stateOne))
                    {//inputwanted was given, remove from list of active tiles
                        //think this can be handled better
                        AddRecordEntry(aTilesOnField[i].inputCat, aTilesOnField[i].inputWanted, a_time);
                        aTilesOnField[i] = null;
                        manager.UpdateScores(bIsMultiManager ? 2 : 1, 1, 0);
                        bInputSuccess = true;
                    }
                }
                if (!bInputSuccess)
                {
                    //compare input to InputRecord
                    if (CheckAgainstRecord(a_stateOne)) return;
                    //gave input, but it was invalid/incorrect
                    manager.UpdateScores(bInputSuccess ? 2 : 1, 0, 1);

                }
            }
        }

        bool CheckAgainstRecord(GamePadState state)
        {//input given is in the record
            for (i = 0; i < lInputRecord.Count; i++)
            {
                testTile.inputCat = lInputRecord[i].cat;
                testTile.inputWanted = lInputRecord[i].type;
                if (testTile.CheckInput(state))
                {
                    return true;
                }
            }
            return false;
        }

        void CalculateTexture(InputType a_input)
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

        void AddRecordEntry(InputCat a_cat, InputType a_type, GameTime a_time)
        {
            InputRecordEntry entry = new InputRecordEntry(a_cat, a_type, (float)(a_time.TotalGameTime.TotalMilliseconds) + fInputRecordTime);
            lInputRecord.Add(entry);
        }

        void AddFailedInputToRecord(GamePadState state)
        {
            //InputRecordEntry r = new InputRecordEntry(InputCat.Button, InputType.A, 0); // making default entry
            
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
        GamePadState prevPadState;

        float fColorLerpAmount = 0;
        float fColorLerpIncrease = 0.025f;

        public MainMenu(GameManager a_manager)
        {
            manager = a_manager;
            fontColor = Color.White;
            opacityChange = 5;
            state = MenuState.PressStart;
        }

        public void LoadContent(ContentManager content, int a_iHalfWanted)
        {
            backGroundTexture = content.Load<Texture2D>("Backgrounds/background_1_d");
            logoTexture = content.Load<Texture2D>("ControllerBustterLogo");
            //assumes texture is in two halfs
            //int height = (backGroundTexture.Height / 2) -200;
            spriteSourceRect = new Rectangle(0, backGroundTexture.Height * (a_iHalfWanted - 1), backGroundTexture.Width, backGroundTexture.Height / 2);
            spriteFont = content.Load<SpriteFont>("SpriteFonts/SpriteFont1");
        }

        public void Update(GamePadState a_gamePadState, GameTime a_gameTime)
        {
            //get from press start to single state
            if (a_gamePadState.Buttons.Start == ButtonState.Pressed && prevPadState.Buttons.Start == ButtonState.Released)
            {
                if (state == MenuState.PressStart)
                {
                    state = MenuState.Single;
                }
            }
            if (a_gamePadState.Buttons.A == ButtonState.Pressed && prevPadState.Buttons.A == ButtonState.Released)
            {
                if (state == MenuState.Single)//|| state == MenuState.VS)
                {
                    manager.ChangeState(Gamestate.CoreGame);
                }
                else if (state == MenuState.VS)
                {
                    manager.ChangeState(Gamestate.Multiplayer);
                }
            }
            //pop between single VS
            if ((a_gamePadState.ThumbSticks.Left.Y != 0 && prevPadState.ThumbSticks.Left.Y == 0 && state != MenuState.PressStart))
            {
                if (state == MenuState.Single) state = MenuState.VS;
                else if (state == MenuState.VS) state = MenuState.Single;
            }
            //pop back to pressstart or exit game
            if ( (a_gamePadState.Buttons.Back == ButtonState.Pressed && prevPadState.Buttons.Back == ButtonState.Released) || (a_gamePadState.Buttons.B == ButtonState.Pressed && prevPadState.Buttons.B == ButtonState.Released) )
            {
                if (state == MenuState.PressStart)
                {
                    manager.ChangeState(Gamestate.Exit);
                }
                else
                {
                    manager.ChangeState(Gamestate.MainMenu);//do this so user can switch controller is desired
                    state = MenuState.PressStart;
                }
            }
            prevPadState = a_gamePadState;
            //fontColor = Color.Lerp(Color.White, Color.clear, fColorLerpAmount);
            //fColorLerpAmount += fColorLerpIncrease;
            //if (fColorLerpAmount > 1 || fColorLerpAmount < 0)
            //{
            //    fColorLerpIncrease *= -1;
            //    fColorLerpAmount += 2 * fColorLerpIncrease;
            //}
            AdjustOpacity();
        }

        void AdjustOpacity()
        {
            if (!bTextDrawn) return; //wait til bTextDrawn is true before running next bit
            //due to bytes never being negative, store value as int, manipulate, then reassign
            fColorLerpAmount += fColorLerpIncrease;
            if (fColorLerpAmount > 1 || fColorLerpAmount < 0)
            {
                fColorLerpIncrease *= -1;
                fColorLerpAmount += 2 * fColorLerpIncrease;
            }
           
            if (manager.bTrialVersion && state == MenuState.VS)
                fontColor = Color.Lerp(Color.White, Color.Red, fColorLerpAmount);
            else
                fontColor = Color.Lerp(Color.White, new Color(0, 0, 0, 0), fColorLerpAmount);
            bTextDrawn = false;
            //if (!bTextDrawn) return; //wait til bTextDrawn is true before running next bit
            ////due to bytes never being negative, store value as int, manipulate, then reassign
            //int opacity = fontColor.A;
            //if (opacityChange < 0 && opacity + opacityChange <= opacityChange)// || opacity + opacityChange >= 255 + opacityChange)
            //{
            //    opacity = 0;
            //    opacityChange *= -1;
            //}
            //if (opacityChange > 0 && /*opacity + opacityChange > 0 + opacityChange)||*/ opacity + opacityChange >= 255 - opacityChange)
            //{
            //    opacity = 255;
            //    opacityChange *= -1;
            //}
            //else
            //{
            //    opacity += opacityChange;
            //}
            //if (manager.bTrialVersion && state == MenuState.VS)
            //    fontColor = new Color(255, opacity, opacity, opacity);
            //else
            //    fontColor = new Color(opacity, opacity, opacity, opacity);
            //bTextDrawn = false;
        }

        public void Draw(SpriteBatch a_spriteBatch, GraphicsDevice a_device)
        {
            //Viewport view = manager.graphics.GraphicsDevice.Viewport;
            Vector2 temp = new Vector2(manager.graphics.GraphicsDevice.Viewport.TitleSafeArea.Width * 0.25f, manager.graphics.GraphicsDevice.Viewport.TitleSafeArea.Height * 0.25f);

            a_spriteBatch.Begin();
            a_spriteBatch.Draw(backGroundTexture, new Rectangle(0, 0, a_device.Viewport.Width, a_device.Viewport.Height), spriteSourceRect, Color.White);
            a_spriteBatch.Draw(logoTexture, temp, Color.White);
            a_spriteBatch.DrawString(spriteFont, "Art By Patrick Ryan", new Vector2(manager.graphics.GraphicsDevice.Viewport.TitleSafeArea.Width / 9f, manager.graphics.GraphicsDevice.Viewport.TitleSafeArea.Height / 9f), Color.White);

            if (state == MenuState.PressStart)
            {
                a_spriteBatch.DrawString(spriteFont, "Press Start To Begin", new Vector2(manager.graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.X - (int)(spriteFont.MeasureString("Press Start To Begin").X / 2),400), fontColor);
            }
            else
            {
               a_spriteBatch.DrawString(spriteFont, "Single", 
                        new Vector2(manager.graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.X - (int)(spriteFont.MeasureString("Single").X/2), 400),
                        state == MenuState.Single ? fontColor : Color.White);


               if (manager.bTrialVersion)
                   a_spriteBatch.DrawString(spriteFont, "VS - Locked In Trial",
                       new Vector2(manager.graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.X - (int)(spriteFont.MeasureString("VS - Locked In Trial").X / 2), 500),
                        state == MenuState.VS ? fontColor: Color.Red);
               else
                   a_spriteBatch.DrawString(spriteFont, "VS", 
                       new Vector2(manager.graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.X - (int)(spriteFont.MeasureString("VS").X / 2), 500),
                        state == MenuState.VS ? fontColor : Color.White);
            }
            bTextDrawn = true;
            a_spriteBatch.End();
        }
    }

    class GameManager
    {
        TileManager tileManager;
        TileManager tileManagerMult;
        Gamestate state;
        Texture2D scoreScreenTexture;
        PlayerScoreStruct player1Score;
        PlayerScoreStruct player2Score;

        bool bViewPortCalculated = false;
        bool bPlayerOneFound = false;
        bool bPlayerTwoFound = false;

        float timeInBetweenTilesAdd;
        float timeToSubtractFromWait;

        //int iPlayerOneScore;
        //int iPlayerTwoScore;
       
        public MainMenu mainMenu;
        public GraphicsDeviceManager graphics;
        public PlayerIndex playerOneIndex;
        public PlayerIndex playerTwoIndex;
        public TileSpriteManager tileSprites;

        public bool bTrialVersion = false;

        public int iMaxTiles;
        public int iMaxTilesPerRow;
        public int iMaxTilesMulti = 6;

        //functions
        public GameManager(float a_TimeInBetween, float a_TimeToSubtract, int a_MaxTiles, int a_MaxTilesPerRow,
                            ref GraphicsDeviceManager a_Graphics, bool a_bTrialVersion
            /*, PlayerIndex a_playerOneIndex, PlayerIndex a_playerTwoIndex*/)
        {
            graphics = a_Graphics;
            timeInBetweenTilesAdd = a_TimeInBetween;
            timeToSubtractFromWait = a_TimeToSubtract;
            iMaxTiles = a_MaxTiles;
            iMaxTilesPerRow = a_MaxTilesPerRow;
            //Vector4[] v4Locations = CalculateLocations(a_MaxTiles, a_MaxTilesPerRow);
            mainMenu = new MainMenu(this);
            tileManager = new TileManager(a_MaxTiles, this, timeInBetweenTilesAdd, timeToSubtractFromWait, new Vector2( 640, -100), false ); //check if this is needed, if so, replace "this"
            tileManagerMult = new TileManager(a_MaxTiles, this, timeInBetweenTilesAdd, timeToSubtractFromWait, new Vector2(640, 820), true); 
            state = Gamestate.MainMenu;
            bTrialVersion = a_bTrialVersion;

            playerOneIndex = PlayerIndex.One;
            playerTwoIndex = PlayerIndex.Two;
        }

        Vector4[] CalculateLocations(int a_MaxTiles, int a_MaxTilesPerRow, bool a_bMulti)
        {
            //based on max tiles, figure out location and size each tile 
            Vector4[] locations = new Vector4[a_MaxTiles];
            //for each x tiles, add new line
            int iNumOfRows = (int)MathHelper.Clamp((float)(a_MaxTiles / a_MaxTilesPerRow), 1, 10);
            //I want tiles to take up 3/4 of the screen 
            //float XSpace = (graphics.GraphicsDevice.Viewport.Width) / a_MaxTilesPerRow;
            float YSpace = a_bMulti ? ((graphics.GraphicsDevice.Viewport.TitleSafeArea.Height) / (iNumOfRows + 0)) / 2 : (graphics.GraphicsDevice.Viewport.TitleSafeArea.Height) / (iNumOfRows + 0);
            float XSpace = (graphics.GraphicsDevice.Viewport.TitleSafeArea.Width) / a_MaxTilesPerRow;//
            //float YSpace = (graphics.GraphicsDevice.Viewport.TitleSafeArea.Height) / (iNumOfRows + 1);//+1 due to 1 row to divide by 2

            //size of tile
            float XSize = XSpace / 1.6f;
            float YSize = YSpace / 1.6f;
            float size = Math.Min(XSize, YSize); // make them square

            //fill array
            float buffer = 10;
            float fStartingX = graphics.GraphicsDevice.Viewport.TitleSafeArea.Left;
            float fStartingY = graphics.GraphicsDevice.Viewport.TitleSafeArea.Top + 50;
            for (int i = 0; i <= iNumOfRows; i++)
                for (int j = 0; j < a_MaxTilesPerRow; j++)
                {
                    int iIndex = a_MaxTilesPerRow * i + j;
                    if (iIndex >= a_MaxTiles) break;
                    locations[iIndex] = new Vector4(
                        fStartingX + XSpace * j + buffer,
                        fStartingY + YSpace * i + buffer,
                        size,
                        size
                        );
                }
            return locations;
        }

        Vector4[] CalculateLocationMult(int a_MaxTiles, int a_MaxTilesPerRow)
        {
             //based on max tiles, figure out location and size each tile 
            Vector4[] locations = new Vector4[a_MaxTiles];
            //for each x tiles, add new line
            int iNumOfRows = (int)MathHelper.Clamp((float)(a_MaxTiles / a_MaxTilesPerRow), 1, 10);
            //I want tiles to take up 3/4 of the screen 
            //float XSpace = (graphics.GraphicsDevice.Viewport.Width) / a_MaxTilesPerRow;
            float YSpace = ( (graphics.GraphicsDevice.Viewport.TitleSafeArea.Height) / (iNumOfRows + 0) ) /2;//
            float XSpace = (graphics.GraphicsDevice.Viewport.TitleSafeArea.Width) / a_MaxTilesPerRow;
            //float YSpace = (graphics.GraphicsDevice.Viewport.TitleSafeArea.Height) / (iNumOfRows + 1);//+1 due to 1 row to divide by 2

            //size of tile
            float XSize = XSpace / 1.6f;
            float YSize = YSpace / 1.6f;
            float size = Math.Min(XSize, YSize); // make them square

            //fill array
            float buffer = 10;
            float fStartingX = graphics.GraphicsDevice.Viewport.TitleSafeArea.Left ;
            float fStartingY = graphics.GraphicsDevice.Viewport.TitleSafeArea.Top + YSpace +50;

            for (int i = 0; i <= iNumOfRows; i++)
                for (int j = 0; j < a_MaxTilesPerRow; j++)
                {
                    int iIndex = a_MaxTilesPerRow * i + j;
                    if (iIndex >= a_MaxTiles) break;
                    locations[iIndex] = new Vector4(
                        fStartingX + XSpace * j + buffer,
                        fStartingY + YSpace * i + buffer,
                        size,
                        size
                        );
                }
            return locations;
        }

        public bool Update(GameTime gameTime) // return true if game needs to end
        {
            if (!bPlayerOneFound)
                CheckForStart(); // only do this for checking which controller should be player one

            GamePadState currentState = GamePad.GetState(playerOneIndex);
            GamePadState playerTwoState = GamePad.GetState(playerTwoIndex);
            if( !Guide.IsVisible )
                switch (state)
                {
                    case Gamestate.CoreGame:
                        if (!bViewPortCalculated)
                        {
                            Vector4[] v4Locations = CalculateLocations(iMaxTiles, iMaxTilesPerRow , false);
                            tileManager.AssignLocationsArray(ref v4Locations);
                            bViewPortCalculated = true;
                        }
                        tileManager.Update(currentState, gameTime);
                        break;
                    case Gamestate.Multiplayer:
                        if (!bViewPortCalculated)
                        {
                            Vector4[] v4Locations = CalculateLocations(6, 6, true);
                            Vector4[] v4LocationsMulti = CalculateLocationMult(6, 6);
                            tileManager.AssignLocationsArray(ref v4Locations);
                            tileManagerMult.AssignLocationsArray(ref v4LocationsMulti);
                            bViewPortCalculated = true;
                        }
                        if (!bPlayerTwoFound)
                        {
                            CheckForPlayerTwo();
                        }
                        tileManager.Update(currentState, gameTime);
                        tileManagerMult.Update( playerTwoState, gameTime);
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
                    a_spriteBatch.DrawString(mainMenu.spriteFont, "Score: " + (player1Score.iPointsScored - player1Score.iNegativePoints).ToString(), new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.X - mainMenu.spriteFont.MeasureString("Score:   ").X, graphics.GraphicsDevice.Viewport.TitleSafeArea.Top), Color.White);
                    a_spriteBatch.End();
                    break;
                case Gamestate.Multiplayer:
                    tileManager.DrawTiles(a_spriteBatch);
                    tileManagerMult.DrawTiles(a_spriteBatch);
                    //in coregame  draw score
                    a_spriteBatch.Begin();
                    a_spriteBatch.DrawString(mainMenu.spriteFont, "Player 1 Score: " + player1Score.iPointsScored.ToString(), new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Width * 0.0f, graphics.GraphicsDevice.Viewport.TitleSafeArea.Height * 0.4f), Color.White);
                    a_spriteBatch.DrawString(mainMenu.spriteFont, "Player 2 Score: " + player2Score.iPointsScored.ToString(), new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - mainMenu.spriteFont.MeasureString("Player 2 Score:   ").X, graphics.GraphicsDevice.Viewport.TitleSafeArea.Height * 0.4f), Color.White);
                    a_spriteBatch.End();
                    break;
                case Gamestate.MainMenu:
                    mainMenu.Draw(a_spriteBatch, graphics.GraphicsDevice);
                    break;
                case Gamestate.ScoreScreen:
                    a_spriteBatch.Begin();
                    a_spriteBatch.Draw(scoreScreenTexture, new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height), new Rectangle(0, 0, scoreScreenTexture.Width, scoreScreenTexture.Height / 2), Color.White);
                    a_spriteBatch.DrawString(mainMenu.spriteFont, "Game Over", new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Width * 0.425f, graphics.GraphicsDevice.Viewport.TitleSafeArea.Height * 0.3f), Color.White);
                    a_spriteBatch.DrawString(mainMenu.spriteFont, "Player 1 : " + player1Score.iPointsScored.ToString() + " ", new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Width * 0.4f, graphics.GraphicsDevice.Viewport.TitleSafeArea.Height * 0.5f), Color.White);
                    if (tileManager.bMulitplayer)
                        a_spriteBatch.DrawString(mainMenu.spriteFont, "Player 2 : " + player2Score.iPointsScored.ToString(), new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Width * 0.4f, graphics.GraphicsDevice.Viewport.TitleSafeArea.Height * 0.6f), Color.White);
                    a_spriteBatch.End();
                    break;
                default:
                    break;
            }
        }

        public void UpdateScores(int iPlayerScored , int a_iPoints , int a_iNegative)
        {
            //add to score
            if (iPlayerScored == 1)
            {
                player1Score.iPointsScored += a_iPoints;
                player1Score.iPointsPossible += 1;
                player1Score.iNegativePoints += a_iNegative;
                //iPlayerOneScore += 1;
            }
            else if (iPlayerScored == 2)
            {
                player2Score.iPointsScored += a_iPoints;
                player2Score.iPointsPossible += 1;
                player2Score.iNegativePoints += a_iNegative;
                //iPlayerOneScore += 1;
            }

        }

        /*public void Scored(bool bSecondScored)
        {
            if (bSecondScored)
                iPlayerTwoScore += 1;
            else if (!bSecondScored)
                iPlayerOneScore += 1;
        }*/

        public void LoadContent(ContentManager a_content)
        {
            tileSprites = new TileSpriteManager(a_content.Load<Texture2D>("Tiles/CB_tiles_2k_d"), 5, 5, 23);
            scoreScreenTexture = a_content.Load<Texture2D>("Backgrounds/background_3_d");
            mainMenu.LoadContent(a_content, 1);
            tileManager.LoadTextures(a_content);
            tileManagerMult.LoadTextures(a_content);
        }

        public void ChangeState(Gamestate a_state)
        {
            if (a_state == Gamestate.Multiplayer)
            {
                //setup multiplayer
                if (!Guide.IsVisible)
                    Guide.ShowSignIn(2, false);
                bPlayerTwoFound = false;
                bViewPortCalculated = false;
                ZeroOutScores();
                //iPlayerOneScore = 0;
                //iPlayerTwoScore = 0;
                if (bTrialVersion)
                {
                    //if (!Guide.IsVisible)
                    //    Guide.ShowMarketplace(playerOneIndex);
                     //bTrialVersion = false;

                    if (!Gamer.SignedInGamers.Cast<SignedInGamer>().Any( x => x.PlayerIndex == playerOneIndex && x.IsSignedInToLive == true && x.Privileges.AllowPurchaseContent) )
                    {
                        List<string> MBOptions = new List<string>();
                        MBOptions.Add("OK, Sign Me In");
                        MBOptions.Add("I Don't Want to Sign In");
                        if (!Guide.IsVisible)
                            Guide.BeginShowMessageBox("Not connected to LIVE", "You must be connected to Live in order to access the marketplace. Please sign in.", MBOptions,
                            0, Microsoft.Xna.Framework.GamerServices.MessageBoxIcon.Warning, GetMBResult, null);

                    }
                    if (!Guide.IsVisible)
                        Guide.ShowMarketplace(playerOneIndex);
                }
                if( !bTrialVersion )
                {
                    //ensure that playerTwoIndex is not the same as playerOneIndex
                    if (playerOneIndex == PlayerIndex.Four)
                        playerTwoIndex = PlayerIndex.One;
                    else
                        playerTwoIndex = playerOneIndex + 1;
                    //check for any
                    CheckForPlayerTwo();

                    tileManager.bMulitplayer = true;
                    tileManagerMult.bMulitplayer = true;
                    state = Gamestate.Multiplayer;
                }
            }
            else if (a_state == Gamestate.CoreGame)
            {
                bViewPortCalculated = false;
                tileManager.bMulitplayer = false;
                tileManagerMult.bMulitplayer = false;
                state = Gamestate.CoreGame;
                ZeroOutScores();
                //iPlayerOneScore = 0;
                //iPlayerTwoScore = 0;
            }
            else if (a_state == Gamestate.MainMenu)
            {
                bPlayerOneFound = false;
                state = a_state;
                tileManager.Reset(timeInBetweenTilesAdd);
                tileManagerMult.Reset(timeInBetweenTilesAdd);
            }
            else
            {
                state = a_state;
                tileManager.Reset(timeInBetweenTilesAdd);
                tileManagerMult.Reset(timeInBetweenTilesAdd);
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

        void CheckForPlayerTwo()
        {
            bPlayerTwoFound = false;
            GamePadState checkState = new GamePadState();

            if (playerOneIndex != PlayerIndex.One && ComparePadState( checkState , GamePad.GetState(PlayerIndex.One) ) )
            {
                bPlayerTwoFound = true;
                playerTwoIndex = PlayerIndex.One;
            }
            else if (playerOneIndex != PlayerIndex.Two && ComparePadState(checkState, GamePad.GetState(PlayerIndex.Two)))
            {
                bPlayerTwoFound = true;
                playerTwoIndex = PlayerIndex.Two;
            }
            else if (playerOneIndex != PlayerIndex.Three && ComparePadState(checkState, GamePad.GetState(PlayerIndex.Three)))
            {
                bPlayerTwoFound = true;
                playerTwoIndex = PlayerIndex.Three;
            }
            else if (playerOneIndex != PlayerIndex.Four && ComparePadState(checkState, GamePad.GetState(PlayerIndex.Four)))
            {
                bPlayerTwoFound = true;
                playerTwoIndex = PlayerIndex.Four;
            }

        }

        public bool ComparePadState(GamePadState checkState, GamePadState state)
        {//returns true is they are not the same, meant to compare against an empty state to check for any input given
            if (!state.Buttons.Equals(checkState.Buttons) || !state.DPad.Equals(checkState.DPad) || !state.ThumbSticks.Equals(checkState.ThumbSticks) || !state.Triggers.Equals(checkState.Triggers) )
                return true;
            return false;
        }

        bool CheckIndexSignedIn(PlayerIndex a_index)
        {
            if (!Gamer.SignedInGamers.Cast<SignedInGamer>().Any(x => x.PlayerIndex == a_index))
            {
                return true;
            }
            return false;
        }

        void GetMBResult(IAsyncResult r)
        {
            int? b = Guide.EndShowMessageBox(r);
            while(b == 0)
            {
                //string test = "testing";
                if (!Guide.IsVisible)
                {
                    Guide.ShowSignIn(2, false);
                    break;
                }
            }
        }

        void ZeroOutScores()
        {
            player1Score.iPointsScored = 0;
            player1Score.iPointsPossible = 0;
            player1Score.iNegativePoints = 0;
            player2Score.iPointsScored = 0;
            player2Score.iPointsPossible = 0;
            player2Score.iNegativePoints = 0;
        }
    }//end of GameManager class
}//end of WindowsVersion namespace
