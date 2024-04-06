using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Render;
using System.Runtime.CompilerServices;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    enum POWERUP
    {
        NONE = 0,
        BIGPADDLE,
        MULTIBALL
    }

    class BOEntity
    {
        public bool visible;
        public string materialName;
        public Material material;
        public float width, height;
        public Vector4 color;
        public Vector2 position;
        public Vector2 velocity;
        public POWERUP powerup;
        public bool removed;
        public bool fadeOut;
        public GameBustOutWindow game;

        public BOEntity(GameBustOutWindow game)
        {
            this.game = game;
            visible = true;

            materialName = "";
            material = null;
            width = height = 8;
            color = colorWhite;
            powerup = POWERUP.NONE;

            position.Zero();
            velocity.Zero();

            removed = false;
            fadeOut = false;
        }

        public virtual void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(visible);

            game.WriteSaveGameString(materialName, savefile);

            savefile.Write(width);
            savefile.Write(height);

            savefile.WriteT(color);
            savefile.WriteT(position);
            savefile.WriteT(velocity);

            savefile.Write(powerup);
            savefile.Write(removed);
            savefile.Write(fadeOut);
        }

        public virtual void ReadFromSaveGame(VFile savefile, GameBustOutWindow game)
        {
            this.game = game;

            savefile.Read(out visible);

            game.ReadSaveGameString(out materialName, savefile); SetMaterial(materialName);

            savefile.Read(out width);
            savefile.Read(out height);

            savefile.ReadT(out color);
            savefile.ReadT(out position);
            savefile.ReadT(out velocity);

            savefile.Read(out powerup);
            savefile.Read(out removed);
            savefile.Read(out fadeOut);
        }

        public void SetMaterial(string name)
        {
            materialName = name;
            material = declManager.FindMaterial(name);
            material.Sort = (float)SS.GUI;
        }

        public void SetSize(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        public void SetColor(float r, float g, float b, float a)
        {
            color.x = r;
            color.y = g;
            color.z = b;
            color.w = a;
        }

        public void SetVisible(bool visible)
            => this.visible = visible;

        public virtual void Update(float timeslice, int guiTime)
        {
            if (!visible)
                return;

            // Move the entity
            position += velocity * timeslice;

            // Fade out the ent
            if (fadeOut)
            {
                color.w -= timeslice * 2.5f;
                if (color.w <= 0f) { color.w = 0f; removed = true; }
            }
        }

        public virtual void Draw(DeviceContext dc)
        {
            if (visible)
                dc.DrawMaterialRotated(position.x, position.y, width, height, material, color, 1f, 1f, MathX.DEG2RAD(0f));
        }
    }

    enum COLLIDE
    {
        NONE = 0,
        DOWN,
        UP,
        LEFT,
        RIGHT
    }

    class BOBrick
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public POWERUP powerup;
        public bool isBroken;
        public BOEntity ent;

        public BOBrick()
        {
            ent = null;
            x = y = width = height = 0;
            powerup = POWERUP.NONE;
            isBroken = false;
        }
        public BOBrick(BOEntity ent, float x, float y, float width, float height)
        {
            this.ent = ent;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            powerup = POWERUP.NONE;

            isBroken = false;

            ent.position.x = x;
            ent.position.y = y;
            ent.SetSize(width, height);
            ent.SetMaterial("game/bustout/brick");

            ent.game.entities.Add(ent);
        }

        public virtual void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(x);
            savefile.Write(y);
            savefile.Write(width);
            savefile.Write(height);

            savefile.Write(powerup);
            savefile.Write(isBroken);

            var index = ent.game.entities.FindIndex(x => x == ent);
            savefile.Write(index);
        }

        public virtual void ReadFromSaveGame(VFile savefile, GameBustOutWindow game)
        {
            savefile.Read(out x);
            savefile.Read(out y);
            savefile.Read(out width);
            savefile.Read(out height);

            savefile.Read(out powerup);
            savefile.Read(out isBroken);

            savefile.Read(out int index);
            ent = game.entities[index];
        }

        public void SetColor(Vector4 bcolor)
            => ent.SetColor(bcolor.x, bcolor.y, bcolor.z, bcolor.w);

        public COLLIDE checkCollision(Vector2 pos, Vector2 vel)
        {
            Vector2 ptA, ptB; float dist;

            var result = COLLIDE.NONE;
            if (isBroken)
                return result;

            // Check for collision with each edge
            Vector2 vec;

            // Bottom
            ptA.x = x;
            ptA.y = y + height;

            ptB.x = x + width;
            ptB.y = y + height;

            if (vel.y < 0 && pos.y > ptA.y)
                if (pos.x > ptA.x && pos.x < ptB.x)
                {
                    dist = pos.y - ptA.y;
                    if (dist < GameBustOutWindow.BALL_RADIUS)
                        result = COLLIDE.DOWN;
                }
                else
                {
                    vec = pos.x <= ptA.x ? pos - ptA : pos - ptB;
                    if (MathX.Fabs(vec.y) > MathX.Fabs(vec.x) && vec.LengthFast < GameBustOutWindow.BALL_RADIUS)
                        result = COLLIDE.DOWN;
                }

            if (result != COLLIDE.NONE)
                return result;

            // Top
            ptA.y = y;
            ptB.y = y;

            if (vel.y > 0 && pos.y < ptA.y)
                if (pos.x > ptA.x && pos.x < ptB.x)
                {
                    dist = ptA.y - pos.y;
                    if (dist < GameBustOutWindow.BALL_RADIUS)
                        result = COLLIDE.UP;
                }
                else
                {
                    vec = pos.x <= ptA.x ? pos - ptA : pos - ptB;
                    if (MathX.Fabs(vec.y) > MathX.Fabs(vec.x) && vec.LengthFast < GameBustOutWindow.BALL_RADIUS)
                        result = COLLIDE.UP;
                }

            if (result != COLLIDE.NONE)
                return result;

            // Left side
            ptA.x = x;
            ptA.y = y;

            ptB.x = x;
            ptB.y = y + height;

            if (vel.x > 0 && pos.x < ptA.x)
            {
                if (pos.y > ptA.y && pos.y < ptB.y)
                {
                    dist = ptA.x - pos.x;
                    if (dist < GameBustOutWindow.BALL_RADIUS)
                        result = COLLIDE.LEFT;
                }
                else
                {
                    vec = pos.y <= ptA.y ? pos - ptA : pos - ptB;
                    if (MathX.Fabs(vec.x) >= MathX.Fabs(vec.y) && vec.LengthFast < GameBustOutWindow.BALL_RADIUS)
                        result = COLLIDE.LEFT;
                }
            }

            if (result != COLLIDE.NONE)
                return result;

            // Right side
            ptA.x = x + width;
            ptB.x = x + width;

            if (vel.x < 0 && pos.x > ptA.x)
                if (pos.y > ptA.y && pos.y < ptB.y)
                {
                    dist = pos.x - ptA.x;
                    if (dist < GameBustOutWindow.BALL_RADIUS)
                        result = COLLIDE.LEFT;
                }
                else
                {
                    vec = pos.y <= ptA.y ? pos - ptA : pos - ptB;
                    if (MathX.Fabs(vec.x) >= MathX.Fabs(vec.y) && vec.LengthFast < GameBustOutWindow.BALL_RADIUS)
                        result = COLLIDE.LEFT;
                }

            return result;
        }
    }

    public class GameBustOutWindow : Window
    {
        public const int BOARD_ROWS = 12;
        internal const float BALL_RADIUS = 12f;
        const float BALL_SPEED = 250f;
        const float BALL_MAXSPEED = 450f;
        const int S_UNIQUE_CHANNEL = 6;

        WinBool gamerunning;
        WinBool onFire;
        WinBool onContinue;
        WinBool onNewGame;
        WinBool onNewLevel;

        float timeSlice;
        bool gameOver;

        int numLevels;
        byte[] levelBoardData;
        bool boardDataLoaded;

        int numBricks;
        int currentLevel;

        bool updateScore;
        int gameScore;
        int nextBallScore;

        int bigPaddleTime;
        float paddleVelocity;

        float ballSpeed;
        int ballsRemaining;
        int ballsInPlay;
        bool ballHitCeiling;

        List<BOEntity> balls;
        List<BOEntity> powerUps;

        BOBrick paddle;
        List<BOBrick>[] board = new List<BOBrick>[BOARD_ROWS];
        internal List<BOEntity> entities = new();

        protected override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "gamerunning", StringComparison.OrdinalIgnoreCase)) { gamerunning = src.ParseBool(); return true; }
            if (string.Equals(name, "onFire", StringComparison.OrdinalIgnoreCase)) { onFire = src.ParseBool(); return true; }
            if (string.Equals(name, "onContinue", StringComparison.OrdinalIgnoreCase)) { onContinue = src.ParseBool(); return true; }
            if (string.Equals(name, "onNewGame", StringComparison.OrdinalIgnoreCase)) { onNewGame = src.ParseBool(); return true; }
            if (string.Equals(name, "onNewLevel", StringComparison.OrdinalIgnoreCase)) { onNewLevel = src.ParseBool(); return true; }
            if (string.Equals(name, "numLevels", StringComparison.OrdinalIgnoreCase))
            {
                numLevels = src.ParseInt();
                // Load all the level images
                LoadBoardFiles();
                return true;
            }
            return base.ParseInternalVar(name, src);
        }

        public override WinVar GetWinVarByName(string name, bool winLookup = false, Action<DrawWin> owner = null)
        {
            WinVar retVar = null;
            if (string.Equals(name, "gamerunning", StringComparison.OrdinalIgnoreCase)) retVar = gamerunning;
            else if (string.Equals(name, "onFire", StringComparison.OrdinalIgnoreCase)) retVar = onFire;
            else if (string.Equals(name, "onContinue", StringComparison.OrdinalIgnoreCase)) retVar = onContinue;
            else if (string.Equals(name, "onNewGame", StringComparison.OrdinalIgnoreCase)) retVar = onNewGame;
            else if (string.Equals(name, "onNewLevel", StringComparison.OrdinalIgnoreCase)) retVar = onNewLevel;
            if (retVar != null) return retVar;
            return base.GetWinVarByName(name, winLookup, owner);
        }

        void CommonInit()
        {
            BOEntity ent;

            // Precache images
            declManager.FindMaterial("game/bustout/ball");
            declManager.FindMaterial("game/bustout/doublepaddle");
            declManager.FindMaterial("game/bustout/powerup_bigpaddle");
            declManager.FindMaterial("game/bustout/powerup_multiball");
            declManager.FindMaterial("game/bustout/brick");

            // Precache sounds
            declManager.FindSound("arcade_ballbounce");
            declManager.FindSound("arcade_brickhit");
            declManager.FindSound("arcade_missedball");
            declManager.FindSound("arcade_sadsound");
            declManager.FindSound("arcade_extraball");
            declManager.FindSound("arcade_powerup");

            ResetGameState();

            numLevels = 0;
            boardDataLoaded = false;
            levelBoardData = null;

            // Create Paddle
            ent = new BOEntity(this);
            paddle = new BOBrick(ent, 260f, 440f, 96f, 24f);
            paddle.ent.SetMaterial("game/bustout/paddle");
        }

        public GameBustOutWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public GameBustOutWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        public override void WriteToSaveGame(VFile savefile)
        {
            base.WriteToSaveGame(savefile);

            gamerunning.WriteToSaveGame(savefile);
            onFire.WriteToSaveGame(savefile);
            onContinue.WriteToSaveGame(savefile);
            onNewGame.WriteToSaveGame(savefile);
            onNewLevel.WriteToSaveGame(savefile);

            savefile.Write(timeSlice);
            savefile.Write(gameOver);
            savefile.Write(numLevels);

            // Board Data is loaded when GUI is loaded, don't need to save

            savefile.Write(numBricks);
            savefile.Write(currentLevel);

            savefile.Write(updateScore);
            savefile.Write(gameScore);
            savefile.Write(nextBallScore);

            savefile.Write(bigPaddleTime);
            savefile.Write(paddleVelocity);

            savefile.Write(ballSpeed);
            savefile.Write(ballsRemaining);
            savefile.Write(ballsInPlay);
            savefile.Write(ballHitCeiling);

            // Write Entities
            int i;
            var numberOfEnts = entities.Count;
            savefile.Write(numberOfEnts);
            for (i = 0; i < numberOfEnts; i++)
                entities[i].WriteToSaveGame(savefile);

            // Write Balls
            numberOfEnts = balls.Count;
            savefile.Write(numberOfEnts);
            for (i = 0; i < numberOfEnts; i++)
            {
                var ballIndex = entities.FindIndex(x => x == balls[i]);
                savefile.Write(ballIndex);
            }

            // Write Powerups
            numberOfEnts = powerUps.Count;
            savefile.Write(numberOfEnts);
            for (i = 0; i < numberOfEnts; i++)
            {
                var powerIndex = entities.FindIndex(x => x == powerUps[i]);
                savefile.Write(powerIndex);
            }

            // Write paddle
            paddle.WriteToSaveGame(savefile);

            // Write Bricks
            int row;
            for (row = 0; row < BOARD_ROWS; row++)
            {
                numberOfEnts = board[row].Count;
                savefile.Write(numberOfEnts);
                for (i = 0; i < numberOfEnts; i++)
                    board[row][i].WriteToSaveGame(savefile);
            }
        }

        public override void ReadFromSaveGame(VFile savefile)
        {
            base.ReadFromSaveGame(savefile);

            // Clear out existing paddle and entities from GUI load
            paddle = null;
            entities.Clear();

            gamerunning.ReadFromSaveGame(savefile);
            onFire.ReadFromSaveGame(savefile);
            onContinue.ReadFromSaveGame(savefile);
            onNewGame.ReadFromSaveGame(savefile);
            onNewLevel.ReadFromSaveGame(savefile);

            savefile.Read(out timeSlice);
            savefile.Read(out gameOver);
            savefile.Read(out numLevels);

            // Board Data is loaded when GUI is loaded, don't need to save

            savefile.Read(out numBricks);
            savefile.Read(out currentLevel);

            savefile.Read(out updateScore);
            savefile.Read(out gameScore);
            savefile.Read(out nextBallScore);

            savefile.Read(out bigPaddleTime);
            savefile.Read(out paddleVelocity);

            savefile.Read(out ballSpeed);
            savefile.Read(out ballsRemaining);
            savefile.Read(out ballsInPlay);
            savefile.Read(out ballHitCeiling);

            int i, numberOfEnts;

            // Read entities
            savefile.Read(out numberOfEnts);
            for (i = 0; i < numberOfEnts; i++)
            {
                var ent = new BOEntity(this);
                ent.ReadFromSaveGame(savefile, this);
                entities.Add(ent);
            }

            // Read balls
            savefile.Read(out numberOfEnts);
            for (i = 0; i < numberOfEnts; i++)
            {
                savefile.Read(out int ballIndex);
                balls.Add(entities[ballIndex]);
            }

            // Read powerups
            savefile.Read(out numberOfEnts);
            for (i = 0; i < numberOfEnts; i++)
            {
                savefile.Read(out int powerIndex);
                balls.Add(entities[powerIndex]);
            }

            // Read paddle
            paddle = new BOBrick();
            paddle.ReadFromSaveGame(savefile, this);

            // Read board
            int row;
            for (row = 0; row < BOARD_ROWS; row++)
            {
                savefile.Read(out numberOfEnts);
                for (i = 0; i < numberOfEnts; i++)
                {
                    var brick = new BOBrick();
                    brick.ReadFromSaveGame(savefile, this);
                    board[row].Add(brick);
                }
            }
        }

        void ResetGameState()
        {
            gamerunning = false;
            gameOver = false;
            onFire = false;
            onContinue = false;
            onNewGame = false;
            onNewLevel = false;

            // Game moves forward 16 milliseconds every frame
            timeSlice = 0.016f;
            ballsRemaining = 3;
            ballSpeed = BALL_SPEED;
            ballsInPlay = 0;
            updateScore = false;
            numBricks = 0;
            currentLevel = 1;
            gameScore = 0;
            bigPaddleTime = 0;
            nextBallScore = gameScore + 10000;

            ClearBoard();
        }

        public override string HandleEvent(SysEvent ev, Action<bool> updateVisuals)
        {
            var key = (Key)ev.evValue;

            // need to call this to allow proper focus and capturing on embedded children
            var ret = base.HandleEvent(ev, updateVisuals);

            if (ev.evType == SE.KEY)
            {
                if (ev.evValue2 == 0)
                    return ret;
                if (key == K_MOUSE1)
                {
                    // Mouse was clicked
                    if (ballsInPlay == 0)
                    {
                        var ball = CreateNewBall();

                        ball.SetVisible(true);
                        ball.position.x = paddle.ent.position.x + 48f;
                        ball.position.y = 430f;

                        ball.velocity.x = ballSpeed;
                        ball.velocity.y = -ballSpeed * 2f;
                        ball.velocity.NormalizeFast();
                        ball.velocity *= ballSpeed;
                    }
                }
                else return ret;
            }

            return ret;
        }

        public override void Draw(int time, float x, float y)
        {
            int i;

            //Update the game every frame before drawing
            UpdateGame();

            for (i = entities.Count - 1; i >= 0; i--)
                entities[i].Draw(dc);
        }

        void ClearBoard()
        {
            int i, j;

            ClearPowerups();

            ballHitCeiling = false;

            for (i = 0; i < BOARD_ROWS; i++)
            {
                for (j = 0; j < board[i].Count; j++)
                {
                    var brick = board[i][j];
                    brick.ent.removed = true;
                }
                board[i].Clear();
            }
        }

        void ClearPowerups()
        {
            while (powerUps.Count != 0)
            {
                powerUps[0].removed = true;
                powerUps.RemoveAt(0);
            }
        }

        void ClearBalls()
        {
            while (balls.Count != 0)
            {
                balls[0].removed = true;
                balls.RemoveAt(0);
            }

            ballsInPlay = 0;
        }

        void LoadBoardFiles()
        {
            int i, w, h; DateTime time; int boardSize; byte[] currentBoard;

            if (boardDataLoaded)
                return;

            boardSize = 9 * 12 * 4;
            levelBoardData = new byte[boardSize * numLevels];

            currentBoard = levelBoardData; var currentBoardIdx = 0;

            for (i = 0; i < numLevels; i++)
            {
                var name = "guis/assets/bustout/level";
                name += i + 1;
                name += ".tga";

                R.LoadImage(name, out var pic, out w, out h, out time, false);

                if (pic != null)
                {
                    if (w != 9 || h != 12)
                        common.DWarning($"Hell Bust-Out level image not correct dimensions! ({w} x {h})");

                    Unsafe.CopyBlock(ref currentBoard[currentBoardIdx], ref pic[0], (uint)boardSize);
                }

                currentBoardIdx += boardSize;
            }

            boardDataLoaded = true;
        }


        void SetCurrentBoard()
        {
            int i, j, realLevel = (currentLevel - 1) % numLevels;
            float bx = 11f, by = 24f, stepx = 619f / 9f, stepy = 256 / 12f;

            var boardSize = 9 * 12 * 4;
            var currentBoard = levelBoardData.AsSpan(realLevel * boardSize);

            for (j = 0; j < BOARD_ROWS; j++)
            {
                bx = 11f;

                for (i = 0; i < 9; i++)
                {
                    var pixelindex = (j * 9 * 4) + (i * 4);

                    if (currentBoard[pixelindex + 3] != 0)
                    {
                        Vector4 bcolor; var pType = 0f;

                        var bent = new BOEntity(this);
                        var brick = new BOBrick(bent, bx, by, stepx, stepy);

                        bcolor.x = currentBoard[pixelindex + 0] / 255f;
                        bcolor.y = currentBoard[pixelindex + 1] / 255f;
                        bcolor.z = currentBoard[pixelindex + 2] / 255f;
                        bcolor.w = 1f;
                        brick.SetColor(bcolor);

                        pType = currentBoard[pixelindex + 3] / 255f;
                        if (pType > 0f && pType < 1f)
                            brick.powerup = pType < 0.5f ? POWERUP.BIGPADDLE : POWERUP.MULTIBALL;

                        board[j].Add(brick);
                        numBricks++;
                    }

                    bx += stepx;
                }

                by += stepy;
            }
        }

        void UpdateGame()
        {
            int i;

            if (onNewGame)
            {
                ResetGameState();

                // Create Board
                SetCurrentBoard();

                gamerunning = true;
            }
            if (onContinue)
            {
                gameOver = false;
                ballsRemaining = 3;

                onContinue = false;
            }
            if (onNewLevel)
            {
                currentLevel++;

                ClearBoard();
                SetCurrentBoard();

                ballSpeed = BALL_SPEED * (1f + (currentLevel / 5f));
                if (ballSpeed > BALL_MAXSPEED) ballSpeed = BALL_MAXSPEED;
                updateScore = true;
                onNewLevel = false;
            }

            if (gamerunning == true)
            {
                UpdatePaddle();
                UpdateBall();
                UpdatePowerups();

                for (i = 0; i < entities.Count; i++)
                    entities[i].Update(timeSlice, gui.Time);

                // Delete entities that need to be deleted
                for (i = entities.Count - 1; i >= 0; i--)
                    if (entities[i].removed)
                    {
                        var ent = entities[i];
                        entities.RemoveAt(i);
                    }

                if (updateScore)
                {
                    UpdateScore();
                    updateScore = false;
                }
            }
        }

        void UpdatePowerups()
        {
            Vector2 pos;

            for (var i = 0; i < powerUps.Count; i++)
            {
                var pUp = powerUps[i];

                // Check for powerup falling below screen
                if (pUp.position.y > 480)
                {
                    powerUps.RemoveAt(i);
                    pUp.removed = true;
                    continue;
                }

                // Check for the paddle catching a powerup
                pos.x = pUp.position.x + (pUp.width / 2);
                pos.y = pUp.position.y + (pUp.height / 2);

                var collision = paddle.checkCollision(pos, pUp.velocity);
                if (collision != COLLIDE.NONE)
                {
                    BOEntity ball;

                    // Give the powerup to the player
                    switch (pUp.powerup)
                    {
                        case POWERUP.BIGPADDLE:
                            bigPaddleTime = gui.Time + 15000;
                            break;
                        case POWERUP.MULTIBALL:
                            // Create 2 new balls in the spot of the existing ball
                            for (var b = 0; b < 2; b++)
                            {
                                ball = CreateNewBall();
                                ball.position = balls[0].position;
                                ball.velocity = balls[0].velocity;

                                if (b == 0) ball.velocity.x -= 35f;
                                else ball.velocity.x += 35f;
                                ball.velocity.NormalizeFast();
                                ball.velocity *= ballSpeed;

                                ball.SetVisible(true);
                            }
                            break;
                        default: break;
                    }

                    // Play the sound
                    session.sw.PlayShaderDirectly("arcade_powerup", S_UNIQUE_CHANNEL);

                    // Remove it
                    powerUps.RemoveAt(i);
                    pUp.removed = true;
                }
            }
        }

        void UpdatePaddle()
        {
            Vector2 cursorPos;
            var oldPos = paddle.x;

            cursorPos.x = gui.CursorX;
            cursorPos.y = gui.CursorY;

            if (bigPaddleTime > gui.Time)
            {
                paddle.x = cursorPos.x - 80f;
                paddle.width = 160;
                paddle.ent.width = 160;
                paddle.ent.SetMaterial("game/bustout/doublepaddle");
            }
            else
            {
                paddle.x = cursorPos.x - 48f;
                paddle.width = 96;
                paddle.ent.width = 96;
                paddle.ent.SetMaterial("game/bustout/paddle");
            }
            paddle.ent.position.x = paddle.x;

            paddleVelocity = paddle.x - oldPos;
        }

        static int UpdateBall_bounceChannel = 1;
        void UpdateBall()
        {
            int ballnum, i, j;
            var playSoundBounce = false;
            var playSoundBrick = false;

            if (ballsInPlay == 0)
                return;

            for (ballnum = 0; ballnum < balls.Count; ballnum++)
            {
                var ball = balls[ballnum];

                // Check for ball going below screen, lost ball
                if (ball.position.y > 480f)
                {
                    ball.removed = true;
                    continue;
                }

                // Check world collision
                if (ball.position.y < 20 && ball.velocity.y < 0)
                {
                    ball.velocity.y = -ball.velocity.y;

                    // Increase ball speed when it hits ceiling
                    if (!ballHitCeiling)
                    {
                        ballSpeed *= 1.25f;
                        ballHitCeiling = true;
                    }
                    playSoundBounce = true;
                }

                if (ball.position.x > 608 && ball.velocity.x > 0)
                {
                    ball.velocity.x = -ball.velocity.x;
                    playSoundBounce = true;
                }
                else if (ball.position.x < 8 && ball.velocity.x < 0)
                {
                    ball.velocity.x = -ball.velocity.x;
                    playSoundBounce = true;
                }

                // Check for Paddle collision
                var ballCenter = ball.position + new Vector2(BALL_RADIUS, BALL_RADIUS);
                var collision = paddle.checkCollision(ballCenter, ball.velocity);

                if (collision == COLLIDE.UP)
                {
                    if (ball.velocity.y > 0)
                    {
                        var paddleVec = new Vector2(paddleVelocity * 2, 0);
                        float centerX = bigPaddleTime > gui.Time ? paddle.x + 80f : paddle.x + 48f;
                        ball.velocity.y = -ball.velocity.y;

                        paddleVec.x += (ball.position.x - centerX) * 2;

                        ball.velocity += paddleVec;
                        ball.velocity.NormalizeFast();
                        ball.velocity *= ballSpeed;

                        playSoundBounce = true;
                    }
                }
                else if (collision == COLLIDE.LEFT || collision == COLLIDE.RIGHT)
                {
                    if (ball.velocity.y > 0)
                    {
                        ball.velocity.x = -ball.velocity.x;
                        playSoundBounce = true;
                    }
                }

                collision = COLLIDE.NONE;

                // Check for collision with bricks
                for (i = 0; i < BOARD_ROWS; i++)
                {
                    var num = board[i].Count;

                    for (j = 0; j < num; j++)
                    {
                        var brick = board[i][j];

                        collision = brick.checkCollision(ballCenter, ball.velocity);
                        if (collision != 0)
                        {
                            // Now break the brick if there was a collision
                            brick.isBroken = true;
                            brick.ent.fadeOut = true;

                            if (brick.powerup > POWERUP.NONE)
                                CreatePowerup(brick);

                            numBricks--;
                            gameScore += 100;
                            updateScore = true;

                            // Go ahead an forcibly remove the last brick, no fade
                            if (numBricks == 0)
                                brick.ent.removed = true;
                            board[i].Remove(brick);
                            break;
                        }
                    }

                    if (collision != 0)
                    {
                        playSoundBrick = true;
                        break;
                    }
                }

                if (collision == COLLIDE.DOWN || collision == COLLIDE.UP) ball.velocity.y *= -1;
                else if (collision == COLLIDE.LEFT || collision == COLLIDE.RIGHT) ball.velocity.x *= -1;

                if (playSoundBounce) session.sw.PlayShaderDirectly("arcade_ballbounce", UpdateBall_bounceChannel);
                else if (playSoundBrick) session.sw.PlayShaderDirectly("arcade_brickhit", UpdateBall_bounceChannel);

                if (playSoundBounce || playSoundBrick)
                {
                    UpdateBall_bounceChannel++;
                    if (UpdateBall_bounceChannel == 4) UpdateBall_bounceChannel = 1;
                }
            }

            // Check to see if any balls were removed from play
            for (ballnum = 0; ballnum < balls.Count; ballnum++)
            {
                if (balls[ballnum].removed)
                {
                    ballsInPlay--;
                    balls.RemoveAt(ballnum);
                }
            }

            // If all the balls were removed, update the game accordingly
            if (ballsInPlay == 0)
            {
                if (ballsRemaining == 0)
                {
                    gameOver = true;

                    // Game Over sound
                    session.sw.PlayShaderDirectly("arcade_sadsound", S_UNIQUE_CHANNEL);
                }
                else
                {
                    ballsRemaining--;

                    // Ball was lost, but game is not over
                    session.sw.PlayShaderDirectly("arcade_missedball", S_UNIQUE_CHANNEL);
                }

                ClearPowerups();
                updateScore = true;
            }
        }

        void UpdateScore()
        {
            if (gameOver)
            {
                gui.HandleNamedEvent("GameOver");
                return;
            }

            // Check for level progression
            if (numBricks == 0)
            {
                ClearBalls();

                gui.HandleNamedEvent("levelComplete");
            }

            // Check for new ball score
            if (gameScore >= nextBallScore)
            {
                ballsRemaining++;
                gui.HandleNamedEvent("extraBall");

                // Play sound
                session.sw.PlayShaderDirectly("arcade_extraball", S_UNIQUE_CHANNEL);

                nextBallScore = gameScore + 10000;
            }

            gui.SetStateString("player_score", gameScore.ToString());
            gui.SetStateString("balls_remaining", ballsRemaining.ToString());
            gui.SetStateString("current_level", currentLevel.ToString());
            gui.SetStateString("next_ball_score", nextBallScore.ToString());
        }

        BOEntity CreateNewBall()
        {
            var ball = new BOEntity(this);
            ball.position.x = 300f;
            ball.position.y = 416f;
            ball.SetMaterial("game/bustout/ball");
            ball.SetSize(BALL_RADIUS * 2f, BALL_RADIUS * 2f);
            ball.SetVisible(false);

            ballsInPlay++;

            balls.Add(ball);
            entities.Add(ball);

            return ball;
        }

        BOEntity CreatePowerup(BOBrick brick)
        {
            var powerEnt = new BOEntity(this);

            powerEnt.position.x = brick.x;
            powerEnt.position.y = brick.y;
            powerEnt.velocity.x = 0f;
            powerEnt.velocity.y = 64f;

            powerEnt.powerup = brick.powerup;

            switch (powerEnt.powerup)
            {
                case POWERUP.BIGPADDLE: powerEnt.SetMaterial("game/bustout/powerup_bigpaddle"); break;
                case POWERUP.MULTIBALL: powerEnt.SetMaterial("game/bustout/powerup_multiball"); break;
                default: powerEnt.SetMaterial("textures/common/nodraw"); break;
            }

            powerEnt.SetSize(619 / 9, 256 / 12);
            powerEnt.SetVisible(true);

            powerUps.Add(powerEnt);
            entities.Add(powerEnt);

            return powerEnt;
        }
    }
}

