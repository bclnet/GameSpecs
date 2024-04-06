using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    class BSEntity
    {
        public Material material;
        public string materialName;
        public float width, height;
        public bool visible;

        public Vector4 entColor;
        public Vector2 position;
        public float rotation;
        public float rotationSpeed;
        public Vector2 velocity;

        public bool fadeIn;
        public bool fadeOut;

        public GameBearShootWindow game;

        public BSEntity(GameBearShootWindow game)
        {
            this.game = game;
            visible = true;

            entColor = colorWhite;
            materialName = "";
            material = null;
            width = height = 8;
            rotation = 0f;
            rotationSpeed = 0f;
            fadeIn = false;
            fadeOut = false;

            position.Zero();
            velocity.Zero();
        }

        public virtual void WriteToSaveGame(VFile savefile)
        {
            game.WriteSaveGameString(materialName, savefile);

            savefile.Write(width);
            savefile.Write(height);
            savefile.Write(visible);

            savefile.WriteT(entColor);
            savefile.WriteT(position);
            savefile.Write(rotation);
            savefile.Write(rotationSpeed);
            savefile.WriteT(velocity);

            savefile.Write(fadeIn);
            savefile.Write(fadeOut);
        }

        public virtual void ReadFromSaveGame(VFile savefile, GameBearShootWindow game)
        {
            this.game = game;

            game.ReadSaveGameString(out materialName, savefile); SetMaterial(materialName);

            savefile.Read(out width);
            savefile.Read(out height);
            savefile.Read(out visible);

            savefile.ReadT(out entColor);
            savefile.ReadT(out position);
            savefile.Read(out rotation);
            savefile.Read(out rotationSpeed);
            savefile.ReadT(out velocity);

            savefile.Read(out fadeIn);
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

        public void SetVisible(bool isVisible)
            => visible = isVisible;

        public virtual void Update(float timeslice)
        {
            if (!visible)
                return;

            // Fades
            if (fadeIn && entColor.w < 1f)
            {
                entColor.w += 1 * timeslice;
                if (entColor.w >= 1f) { entColor.w = 1f; fadeIn = false; }
            }
            if (fadeOut && entColor.w > 0f)
            {
                entColor.w -= 1 * timeslice;
                if (entColor.w <= 0f) { entColor.w = 0f; fadeOut = false; }
            }

            // Move the entity
            position += velocity * timeslice;

            // Rotate Entity
            rotation += rotationSpeed * timeslice;
        }

        public virtual void Draw(DeviceContext dc)
        {
            if (visible)
                dc.DrawMaterialRotated(position.x, position.y, width, height, material, entColor, 1f, 1f, MathX.DEG2RAD(rotation));
        }
    }

    class GameBearShootWindow : Window
    {
        const float BEAR_GRAVITY = 240;
        const float BEAR_SIZE = 24f;
        const float BEAR_SHRINK_TIME = 2000f;

        const float MAX_WINDFORCE = 100f;

        static readonly CVar bearTurretAngle = new("bearTurretAngle", "0", CVAR.FLOAT, "");
        static readonly CVar bearTurretForce = new("bearTurretForce", "200", CVAR.FLOAT, "");

        WinBool gamerunning;
        WinBool onFire;
        WinBool onContinue;
        WinBool onNewGame;

        float timeSlice;
        float timeRemaining;
        bool gameOver;

        int currentLevel;
        int goalsHit;
        bool updateScore;
        bool bearHitTarget;

        float bearScale;
        bool bearIsShrinking;
        int bearShrinkStartTime;

        float turretAngle;
        float turretForce;

        float windForce;
        int windUpdateTime;

        List<BSEntity> entities = new();

        BSEntity turret;
        BSEntity bear;
        BSEntity helicopter;
        BSEntity goal;
        BSEntity wind;
        BSEntity gunblast;

        protected override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "gamerunning", StringComparison.OrdinalIgnoreCase)) { gamerunning = src.ParseBool(); return true; }
            if (string.Equals(name, "onFire", StringComparison.OrdinalIgnoreCase)) { onFire = src.ParseBool(); return true; }
            if (string.Equals(name, "onContinue", StringComparison.OrdinalIgnoreCase)) { onContinue = src.ParseBool(); return true; }
            if (string.Equals(name, "onNewGame", StringComparison.OrdinalIgnoreCase)) { onNewGame = src.ParseBool(); return true; }
            return base.ParseInternalVar(name, src);
        }

        public override WinVar GetWinVarByName(string name, bool winLookup = false, Action<DrawWin> owner = null)
        {
            WinVar retVar = null;
            if (string.Equals(name, "gamerunning", StringComparison.OrdinalIgnoreCase)) retVar = gamerunning;
            else if (string.Equals(name, "onFire", StringComparison.OrdinalIgnoreCase)) retVar = onFire;
            else if (string.Equals(name, "onContinue", StringComparison.OrdinalIgnoreCase)) retVar = onContinue;
            else if (string.Equals(name, "onNewGame", StringComparison.OrdinalIgnoreCase)) retVar = onNewGame;
            if (retVar != null) return retVar;
            return base.GetWinVarByName(name, winLookup, owner);
        }

        void CommonInit()
        {
            BSEntity ent;

            // Precache sounds
            declManager.FindSound("arcade_beargroan");
            declManager.FindSound("arcade_sargeshoot");
            declManager.FindSound("arcade_balloonpop");
            declManager.FindSound("arcade_levelcomplete1");

            // Precache dynamically used materials
            declManager.FindMaterial("game/bearshoot/helicopter_broken");
            declManager.FindMaterial("game/bearshoot/goal_dead");
            declManager.FindMaterial("game/bearshoot/gun_blast");

            ResetGameState();

            ent = turret = new BSEntity(this);
            ent.SetMaterial("game/bearshoot/turret");
            ent.SetSize(272f, 144f);
            ent.position.x = -44f; ent.position.y = 260f;
            entities.Add(ent);

            ent = new BSEntity(this);
            ent.SetMaterial("game/bearshoot/turret_base");
            ent.SetSize(144f, 160f);
            ent.position.x = 16f; ent.position.y = 280f;
            entities.Add(ent);

            ent = bear = new BSEntity(this);
            ent.SetMaterial("game/bearshoot/bear");
            ent.SetSize(BEAR_SIZE, BEAR_SIZE);
            ent.SetVisible(false);
            ent.position.x = 0f; ent.position.y = 0f;
            entities.Add(ent);

            ent = helicopter = new BSEntity(this);
            ent.SetMaterial("game/bearshoot/helicopter");
            ent.SetSize(64f, 64f);
            ent.position.x = 550f; ent.position.y = 100f;
            entities.Add(ent);

            ent = goal = new BSEntity(this);
            ent.SetMaterial("game/bearshoot/goal");
            ent.SetSize(64f, 64f);
            ent.position.x = 550f; ent.position.y = 164f;
            entities.Add(ent);

            ent = wind = new BSEntity(this);
            ent.SetMaterial("game/bearshoot/wind");
            ent.SetSize(100f, 40f);
            ent.position.x = 500f; ent.position.y = 430f;
            entities.Add(ent);

            ent = gunblast = new BSEntity(this);
            ent.SetMaterial("game/bearshoot/gun_blast");
            ent.SetSize(64f, 64f);
            ent.SetVisible(false);
            entities.Add(ent);
        }

        public GameBearShootWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public GameBearShootWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
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

            savefile.Write(timeSlice);
            savefile.Write(timeRemaining);
            savefile.Write(gameOver);

            savefile.Write(currentLevel);
            savefile.Write(goalsHit);
            savefile.Write(updateScore);
            savefile.Write(bearHitTarget);

            savefile.Write(bearScale);
            savefile.Write(bearIsShrinking);
            savefile.Write(bearShrinkStartTime);

            savefile.Write(turretAngle);
            savefile.Write(turretForce);

            savefile.Write(windForce);
            savefile.Write(windUpdateTime);

            var numberOfEnts = entities.Count; savefile.Write(numberOfEnts);

            for (var i = 0; i < numberOfEnts; i++)
                entities[i].WriteToSaveGame(savefile);

            int index;
            index = entities.FindIndex(x => x == turret); savefile.Write(index);
            index = entities.FindIndex(x => x == bear); savefile.Write(index);
            index = entities.FindIndex(x => x == helicopter); savefile.Write(index);
            index = entities.FindIndex(x => x == goal); savefile.Write(index);
            index = entities.FindIndex(x => x == wind); savefile.Write(index);
            index = entities.FindIndex(x => x == gunblast); savefile.Write(index);
        }

        public override void ReadFromSaveGame(VFile savefile)
        {
            base.ReadFromSaveGame(savefile);

            // Remove all existing entities
            entities.Clear();

            gamerunning.ReadFromSaveGame(savefile);
            onFire.ReadFromSaveGame(savefile);
            onContinue.ReadFromSaveGame(savefile);
            onNewGame.ReadFromSaveGame(savefile);

            savefile.Read(out timeSlice);
            savefile.Read(out timeRemaining);
            savefile.Read(out gameOver);

            savefile.Read(out currentLevel);
            savefile.Read(out goalsHit);
            savefile.Read(out updateScore);
            savefile.Read(out bearHitTarget);

            savefile.Read(out bearScale);
            savefile.Read(out bearIsShrinking);
            savefile.Read(out bearShrinkStartTime);

            savefile.Read(out turretAngle);
            savefile.Read(out turretForce);

            savefile.Read(out windForce);
            savefile.Read(out windUpdateTime);

            savefile.Read(out int numberOfEnts);
            for (var i = 0; i < numberOfEnts; i++)
            {
                var ent = new BSEntity(this);
                ent.ReadFromSaveGame(savefile, this);
                entities.Add(ent);
            }

            int index;
            savefile.Read(out index); turret = entities[index];
            savefile.Read(out index); bear = entities[index];
            savefile.Read(out index); helicopter = entities[index];
            savefile.Read(out index); goal = entities[index];
            savefile.Read(out index); wind = entities[index];
            savefile.Read(out index); gunblast = entities[index];
        }

        void ResetGameState()
        {
            gamerunning = false;
            gameOver = false;
            onFire = false;
            onContinue = false;
            onNewGame = false;

            // Game moves forward 16 milliseconds every frame
            timeSlice = 0.016f;
            timeRemaining = 60f;
            goalsHit = 0;
            updateScore = false;
            bearHitTarget = false;
            currentLevel = 1;
            turretAngle = 0f;
            turretForce = 200f;
            windForce = 0f;
            windUpdateTime = 0;

            bearIsShrinking = false;
            bearShrinkStartTime = 0;
            bearScale = 1f;
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
                if (key == K_MOUSE1) { } // Mouse was clicked
                else return ret;
            }
            return ret;
        }

        public override void Draw(int time, float x, float y)
        {
            // Update the game every frame before drawing
            UpdateGame();

            for (var i = entities.Count - 1; i >= 0; i--)
                entities[i].Draw(dc);
        }

        void UpdateTurret()
        {
            Vector2 pt, turretOrig = new(), right; float dot, angle;

            pt.x = gui.CursorX; pt.y = gui.CursorY;
            turretOrig.Set(80f, 348f);

            pt -= turretOrig;
            pt.NormalizeFast();

            right.x = 1f; right.y = 0f;

            dot = pt * right;

            angle = MathX.RAD2DEG((float)Math.Acos(dot));

            turretAngle = MathX.ClampFloat(0f, 90f, angle);
        }

        void UpdateBear()
        {
            var time = gui.Time;
            var startShrink = false;

            // Apply gravity
            bear.velocity.y += BEAR_GRAVITY * timeSlice;
            // Apply wind
            bear.velocity.x += windForce * timeSlice;

            // Check for collisions
            if (!bearHitTarget && !gameOver)
            {
                Vector2 bearCenter;
                var collision = false;

                bearCenter.x = bear.position.x + bear.width / 2;
                bearCenter.y = bear.position.y + bear.height / 2;

                if (bearCenter.x > (helicopter.position.x + 16) && bearCenter.x < (helicopter.position.x + helicopter.width - 29))
                    if (bearCenter.y > (helicopter.position.y + 12) && bearCenter.y < (helicopter.position.y + helicopter.height - 7))
                        collision = true;

                if (collision)
                {
                    // balloons pop and bear tumbles to ground
                    helicopter.SetMaterial("game/bearshoot/helicopter_broken");
                    helicopter.velocity.y = 230f;
                    goal.velocity.y = 230f;
                    session.sw.PlayShaderDirectly("arcade_balloonpop");

                    bear.SetVisible(false);
                    if (bear.velocity.x > 0)
                        bear.velocity.x *= -1f;
                    bear.velocity *= 0.666f;
                    bearHitTarget = true;
                    updateScore = true;
                    startShrink = true;
                }
            }

            // Check for ground collision
            if (bear.position.y > 380)
            {
                bear.position.y = 380;

                if (bear.velocity.Length < 25)
                    bear.velocity.Zero();
                else
                {
                    startShrink = true;

                    bear.velocity.y *= -1f;
                    bear.velocity *= 0.5f;

                    if (bearScale != 0f)
                        session.sw.PlayShaderDirectly("arcade_balloonpop");
                }
            }

            // Bear rotation is based on velocity
            float angle; Vector2 dir;

            dir = bear.velocity;
            dir.NormalizeFast();

            angle = MathX.RAD2DEG((float)Math.Atan2(dir.x, dir.y));
            bear.rotation = angle - 90;

            // Update Bear scale
            if (bear.position.x > 650)
                startShrink = true;

            if (!bearIsShrinking && bearScale != 0f && startShrink)
            {
                bearShrinkStartTime = time;
                bearIsShrinking = true;
            }

            if (bearIsShrinking)
            {
                bearScale = bearHitTarget
                    ? 1 - ((float)(time - bearShrinkStartTime) / BEAR_SHRINK_TIME)
                    : 1 - ((float)(time - bearShrinkStartTime) / 750);
                bearScale *= BEAR_SIZE;
                bear.SetSize(bearScale, bearScale);

                if (bearScale < 0)
                {
                    gui.HandleNamedEvent("EnableFireButton");
                    bearIsShrinking = false;
                    bearScale = 0f;

                    if (bearHitTarget)
                    {
                        goal.SetMaterial("game/bearshoot/goal");
                        goal.position.x = 550f; goal.position.y = 164f;
                        goal.velocity.Zero(); goal.velocity.y = (currentLevel - 1) * 30;
                        goal.entColor.w = 0f;
                        goal.fadeIn = true;
                        goal.fadeOut = false;

                        helicopter.SetVisible(true);
                        helicopter.SetMaterial("game/bearshoot/helicopter");
                        helicopter.position.x = 550f; helicopter.position.y = 100f;
                        helicopter.velocity.Zero(); helicopter.velocity.y = goal.velocity.y;
                        helicopter.entColor.w = 0f;
                        helicopter.fadeIn = true;
                        helicopter.fadeOut = false;
                    }
                }
            }
        }

        void UpdateHelicopter()
        {
            if (bearHitTarget && bearIsShrinking)
            {
                if (helicopter.velocity.y != 0f && helicopter.position.y > 264f)
                {
                    helicopter.velocity.y = 0f;
                    goal.velocity.y = 0f;

                    helicopter.SetVisible(false);
                    goal.SetMaterial("game/bearshoot/goal_dead");
                    session.sw.PlayShaderDirectly("arcade_beargroan", 1);

                    helicopter.fadeOut = true;
                    goal.fadeOut = true;
                }
            }
            else if (currentLevel > 1)
            {
                var height = (int)helicopter.position.y;
                var speed = (currentLevel - 1) * 30f;

                if (height > 240f)
                {
                    helicopter.velocity.y = -speed;
                    goal.velocity.y = -speed;
                }
                else if (height < 30f)
                {
                    helicopter.velocity.y = speed;
                    goal.velocity.y = speed;
                }
            }
        }

        void UpdateButtons()
        {
            if (onFire)
            {
                Vector2 vec;

                gui.HandleNamedEvent("DisableFireButton");
                session.sw.PlayShaderDirectly("arcade_sargeshoot");

                bear.SetVisible(true);
                bearScale = 1f;
                bear.SetSize(BEAR_SIZE, BEAR_SIZE);

                vec.x = MathX.Cos(MathX.DEG2RAD(turretAngle)); vec.x += (1 - vec.x) * 0.18f;
                vec.y = -MathX.Sin(MathX.DEG2RAD(turretAngle));

                turretForce = bearTurretForce.Float;

                bear.position.x = 80 + (96 * vec.x); bear.position.y = 334 + (96 * vec.y);
                bear.velocity.x = vec.x * turretForce; bear.velocity.y = vec.y * turretForce;

                gunblast.position.x = 55 + (96 * vec.x); gunblast.position.y = 310 + (100 * vec.y);
                gunblast.SetVisible(true);
                gunblast.entColor.w = 1f;
                gunblast.rotation = turretAngle;
                gunblast.fadeOut = true;

                bearHitTarget = false;

                onFire = false;
            }
        }

        void UpdateScore()
        {
            if (gameOver)
            {
                gui.HandleNamedEvent("GameOver");
                return;
            }

            goalsHit++;
            gui.SetStateString("player_score", goalsHit.ToString());

            // Check for level progression
            if ((goalsHit % 5) == 0)
            {
                currentLevel++;
                gui.SetStateString("current_level", currentLevel.ToString());
                session.sw.PlayShaderDirectly("arcade_levelcomplete1", 3);

                timeRemaining += 30f;
            }
        }

        void UpdateGame()
        {
            int i;

            if (onNewGame)
            {
                ResetGameState();

                goal.position.x = 550f; goal.position.y = 164f;
                goal.velocity.Zero();
                helicopter.position.x = 550f; helicopter.position.y = 100f;
                helicopter.velocity.Zero();
                bear.SetVisible(false);

                bearTurretAngle.Float = 0f;
                bearTurretForce.Float = 200f;

                gamerunning = true;
            }
            if (onContinue)
            {
                gameOver = false;
                timeRemaining = 60f;

                onContinue = false;
            }

            if (gamerunning == true)
            {
                var current_time = gui.Time;
                var rnd = new RandomX(current_time);

                // Check for button presses
                UpdateButtons();

                if (bear != null)
                    UpdateBear();
                if (helicopter != null && goal != null)
                    UpdateHelicopter();

                // Update Wind
                if (windUpdateTime < current_time)
                {
                    float scale; int width;

                    windForce = rnd.CRandomFloat() * (MAX_WINDFORCE * 0.75f);
                    if (windForce > 0)
                    {
                        windForce += MAX_WINDFORCE * 0.25f;
                        wind.rotation = 0;
                    }
                    else
                    {
                        windForce -= MAX_WINDFORCE * 0.25f;
                        wind.rotation = 180;
                    }

                    scale = 1f - ((MAX_WINDFORCE - MathX.Fabs(windForce)) / MAX_WINDFORCE);
                    width = (int)(100 * scale);

                    wind.position.x = windForce < 0 ? 500 - width + 1 : 500;
                    wind.SetSize(width, 40);

                    windUpdateTime = current_time + 7000 + rnd.RandomInt(5000);
                }

                // Update turret rotation angle
                if (turret != null)
                {
                    turretAngle = bearTurretAngle.Float;
                    turret.rotation = turretAngle;
                }

                for (i = 0; i < entities.Count; i++)
                    entities[i].Update(timeSlice);

                // Update countdown timer
                timeRemaining -= timeSlice;
                timeRemaining = MathX.ClampFloat(0f, 99999f, timeRemaining);
                gui.SetStateString("time_remaining", $"{timeRemaining:2.1}");

                if (timeRemaining <= 0f && !gameOver)
                {
                    gameOver = true;
                    updateScore = true;
                }

                if (updateScore)
                {
                    UpdateScore();
                    updateScore = false;
                }
            }
        }
    }
}
