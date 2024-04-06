using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    class SSDCrossHair
    {
        public const string CROSSHAIR_STANDARD_MATERIAL = "game/SSD/crosshair_standard";
        public const string CROSSHAIR_SUPER_MATERIAL = "game/SSD/crosshair_super";

        public enum CROSSHAIR
        {
            STANDARD = 0,
            SUPER,
            COUNT
        }
        public Material[] crosshairMaterial = new Material[(int)CROSSHAIR.COUNT];
        public int currentCrosshair;
        public float crosshairWidth, crosshairHeight;

        public SSDCrossHair() { }

        public virtual void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(currentCrosshair);
            savefile.Write(crosshairWidth);
            savefile.Write(crosshairHeight);
        }

        public virtual void ReadFromSaveGame(VFile savefile)
        {
            InitCrosshairs();
            savefile.Read(out currentCrosshair);
            savefile.Read(out crosshairWidth);
            savefile.Read(out crosshairHeight);
        }

        public void InitCrosshairs()
        {
            crosshairMaterial[(int)CROSSHAIR.STANDARD] = declManager.FindMaterial(CROSSHAIR_STANDARD_MATERIAL);
            crosshairMaterial[(int)CROSSHAIR.SUPER] = declManager.FindMaterial(CROSSHAIR_SUPER_MATERIAL);
            crosshairWidth = 64;
            crosshairHeight = 64;
            currentCrosshair = (int)CROSSHAIR.STANDARD;
        }

        public void Draw(DeviceContext dc, Vector2 cursor)
        {
            var x = cursor.x - (crosshairWidth / 2);
            var y = cursor.y - (crosshairHeight / 2);
            dc.DrawMaterial(x, y, crosshairWidth, crosshairHeight, crosshairMaterial[currentCrosshair], colorWhite, 1f, 1f);
        }
    }

    enum SSD_ENTITY
    {
        BASE = 0,
        ASTEROID,
        ASTRONAUT,
        EXPLOSION,
        POINTS,
        PROJECTILE,
        POWERUP
    }

    class SSDEntity
    {
        // SSDEntity Information
        public int type;
        public int id;
        public string materialName;
        public Material material;
        public Vector3 position;
        public Vector2 size;
        public float radius;
        public float hitRadius;
        public float rotation;

        public Vector4 matColor;

        public string text;
        public float textScale;
        public Vector4 foreColor;

        public GameSSDWindow game;
        public int currentTime;
        public int lastUpdate;
        public int elapsed;

        public bool destroyed;
        public bool noHit;
        public bool noPlayerDamage;

        public bool inUse;

        public SSDEntity()
            => EntityInit();

        public virtual void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(type);
            game.WriteSaveGameString(materialName, savefile);
            savefile.WriteT(position);
            savefile.WriteT(size);
            savefile.Write(radius);
            savefile.Write(hitRadius);
            savefile.Write(rotation);

            savefile.WriteT(matColor);

            game.WriteSaveGameString(text, savefile);
            savefile.Write(textScale);
            savefile.WriteT(foreColor);

            savefile.Write(currentTime);
            savefile.Write(lastUpdate);
            savefile.Write(elapsed);

            savefile.Write(destroyed);
            savefile.Write(noHit);
            savefile.Write(noPlayerDamage);

            savefile.Write(inUse);
        }

        public virtual void ReadFromSaveGame(VFile savefile, GameSSDWindow game)
        {
            savefile.Read(out type);
            game.ReadSaveGameString(out materialName, savefile);
            SetMaterial(materialName);
            savefile.ReadT(out position);
            savefile.ReadT(out size);
            savefile.Read(out radius);
            savefile.Read(out hitRadius);
            savefile.Read(out rotation);

            savefile.ReadT(out matColor);

            game.ReadSaveGameString(out text, savefile);
            savefile.Read(out textScale);
            savefile.ReadT(out foreColor);

            this.game = game;
            savefile.Read(out currentTime);
            savefile.Read(out lastUpdate);
            savefile.Read(out elapsed);

            savefile.Read(out destroyed);
            savefile.Read(out noHit);
            savefile.Read(out noPlayerDamage);

            savefile.Read(out inUse);
        }

        public void EntityInit()
        {
            inUse = false;

            type = (int)SSD_ENTITY.BASE;

            materialName = "";
            material = null;
            position.Zero();
            size.Zero();
            radius = 0f;
            hitRadius = 0f;
            rotation = 0f;

            currentTime = 0;
            lastUpdate = 0;

            destroyed = false;
            noHit = false;
            noPlayerDamage = false;

            matColor.Set(1, 1, 1, 1);

            text = "";
            textScale = 1f;
            foreColor.Set(1, 1, 1, 1);
        }

        public void SetGame(GameSSDWindow game)
            => this.game = game;

        public void SetMaterial(string name)
        {
            materialName = name;
            material = declManager.FindMaterial(name);
            material.Sort = (float)SS.GUI;
        }

        public void SetPosition(Vector3 position)
            => this.position = position;

        public void SetSize(Vector2 size)
            => this.size = size;

        public void SetRadius(float radius, float hitFactor = 1f)
        {
            this.radius = radius;
            hitRadius = radius * hitFactor;
        }

        public void SetRotation(float rotation)
            => this.rotation = rotation;

        public void Update()
        {
            currentTime = game.ssdTime;

            // Is this the first update
            if (lastUpdate == 0)
            {
                lastUpdate = currentTime;
                return;
            }

            elapsed = currentTime - lastUpdate;

            EntityUpdate();

            lastUpdate = currentTime;
        }

        public bool HitTest(Vector2 pt)
        {
            if (noHit)
                return false;

            var screenPos = WorldToScreen(position);

            // Scale the radius based on the distance from the player
            var scale = 1f - ((screenPos.z - GameSSDWindow.Z_NEAR) / (GameSSDWindow.Z_FAR - GameSSDWindow.Z_NEAR));
            var scaledRad = scale * hitRadius;

            // So we can compare against the square of the length between two points
            var scaleRadSqr = scaledRad * scaledRad;

            var diff = screenPos.ToVec2() - pt;
            var dist = MathX.Fabs(diff.LengthSqr);

            return dist < scaleRadSqr;
        }

        public virtual void EntityUpdate() { }

        public virtual void Draw(DeviceContext dc)
        {
            Vector2 persize; float x, y;

            Bounds bounds = new();
            bounds[0] = new Vector3(position.x - (size.x / 2f), position.y - (size.y / 2f), position.z);
            bounds[1] = new Vector3(position.x + (size.x / 2f), position.y + (size.y / 2f), position.z);

            var screenBounds = WorldToScreen(bounds);
            persize.x = MathX.Fabs(screenBounds[1].x - screenBounds[0].x);
            persize.y = MathX.Fabs(screenBounds[1].y - screenBounds[0].y);

            x = screenBounds[0].x;
            y = screenBounds[1].y;
            dc.DrawMaterialRotated(x, y, persize.x, persize.y, material, matColor, 1f, 1f, MathX.DEG2RAD(rotation));

            if (text.Length > 0)
            {
                var rect = new Rectangle(x, y, DeviceContext.VIRTUAL_WIDTH, DeviceContext.VIRTUAL_HEIGHT);
                dc.DrawText(text, textScale, 0, foreColor, rect, false);
            }
        }

        public virtual void DestroyEntity()
            => inUse = false;

        public virtual void OnHit(int key) { }

        public virtual void OnStrikePlayer() { }

        public Bounds WorldToScreen(Bounds worldBounds)
        {
            var screenMin = WorldToScreen(worldBounds[0]);
            var screenMax = WorldToScreen(worldBounds[1]);
            var screenBounds = new Bounds(screenMin, screenMax);
            return screenBounds;
        }

        public Vector3 WorldToScreen(Vector3 worldPos)
        {
            var d = 0.5f * GameSSDWindow.V_WIDTH * MathX.Tan(MathX.DEG2RAD(90f) / 2f);

            // World To Camera Coordinates
            var cameraTrans = new Vector3(0, 0, d);
            Vector3 cameraPos;
            cameraPos = worldPos + cameraTrans;

            // Camera To Screen Coordinates
            Vector3 screenPos;
            screenPos.x = d * cameraPos.x / cameraPos.z + (0.5f * GameSSDWindow.V_WIDTH - 0.5f);
            screenPos.y = -d * cameraPos.y / cameraPos.z + (0.5f * GameSSDWindow.V_HEIGHT - 0.5f);
            screenPos.z = cameraPos.z;
            return screenPos;
        }

        public Vector3 ScreenToWorld(Vector3 screenPos)
        {
            Vector3 worldPos;
            worldPos.x = screenPos.x - 0.5f * GameSSDWindow.V_WIDTH;
            worldPos.y = -(screenPos.y - 0.5f * GameSSDWindow.V_HEIGHT);
            worldPos.z = screenPos.z;
            return worldPos;
        }
    }

    // SSDMover
    class SSDMover : SSDEntity
    {
        public Vector3 speed;
        public float rotationSpeed;

        public SSDMover() { }

        public override void WriteToSaveGame(VFile savefile)
        {
            base.WriteToSaveGame(savefile);
            savefile.WriteT(speed);
            savefile.Write(rotationSpeed);
        }

        public override void ReadFromSaveGame(VFile savefile, GameSSDWindow game)
        {
            base.ReadFromSaveGame(savefile, game);
            savefile.ReadT(out speed);
            savefile.Read(out rotationSpeed);
        }

        public void MoverInit(Vector3 speed, float rotationSpeed)
        {
            this.speed = speed;
            this.rotationSpeed = rotationSpeed;
        }

        public override void EntityUpdate()
        {
            base.EntityUpdate();
            // Move forward based on speed (units per second)
            var moved = (elapsed / 1000f) * speed;
            position += moved;

            var rotated = (elapsed / 1000f) * rotationSpeed * 360f;
            rotation += rotated;
            if (rotation >= 360) rotation -= 360f;
            if (rotation < 0) rotation += 360f;
        }
    }

    // SSDAsteroid
    class SSDAsteroid : SSDMover
    {
        internal const string ASTEROID_MATERIAL = "game/SSD/asteroid";
        public const int MAX_ASTEROIDS = 64;
        public int health;
        protected static SSDAsteroid[] asteroidPool = new SSDAsteroid[MAX_ASTEROIDS];

        public SSDAsteroid() { }

        public override void WriteToSaveGame(VFile savefile)
        {
            base.WriteToSaveGame(savefile);
            savefile.Write(health);
        }

        public override void ReadFromSaveGame(VFile savefile, GameSSDWindow game)
        {
            base.ReadFromSaveGame(savefile, game);
            savefile.Read(out health);
        }

        public void Init(GameSSDWindow game, Vector3 startPosition, Vector2 size, float speed, float rotate, int health)
        {
            EntityInit();
            MoverInit(new Vector3(0, 0, -speed), rotate);

            SetGame(game);

            type = (int)SSD_ENTITY.ASTEROID;

            SetMaterial(ASTEROID_MATERIAL);
            SetSize(size);
            SetRadius(Math.Max(size.x, size.y), 0.3f);
            SetRotation(GameSSDWindow.random.RandomInt(360));

            position = startPosition;
            this.health = health;
        }

        public override void EntityUpdate()
            => base.EntityUpdate();

        public static SSDAsteroid GetNewAsteroid(GameSSDWindow game, Vector3 startPosition, Vector2 size, float speed, float rotate, int health)
        {
            for (var i = 0; i < MAX_ASTEROIDS; i++)
                if (!asteroidPool[i].inUse)
                {
                    asteroidPool[i].Init(game, startPosition, size, speed, rotate, health);
                    asteroidPool[i].inUse = true;
                    asteroidPool[i].id = i;
                    return asteroidPool[i];
                }
            return null;
        }

        public static SSDAsteroid GetSpecificAsteroid(int id)
            => asteroidPool[id];

        public static void WriteAsteroids(VFile savefile)
        {
            var count = 0;
            for (var i = 0; i < MAX_ASTEROIDS; i++)
                if (asteroidPool[i].inUse)
                    count++;
            savefile.Write(count);
            for (var i = 0; i < MAX_ASTEROIDS; i++)
                if (asteroidPool[i].inUse)
                {
                    savefile.Write(asteroidPool[i].id);
                    asteroidPool[i].WriteToSaveGame(savefile);
                }
        }

        public static void ReadAsteroids(VFile savefile, GameSSDWindow game)
        {
            savefile.Read(out int count);
            for (var i = 0; i < count; i++)
            {
                savefile.Read(out int id);
                var ent = GetSpecificAsteroid(id);
                ent.ReadFromSaveGame(savefile, game);
            }
        }
    }

    // SSDAstronaut
    class SSDAstronaut : SSDMover
    {
        internal const string ASTRONAUT_MATERIAL = "game/SSD/astronaut";
        public const int MAX_ASTRONAUT = 8;
        public int health;
        protected static SSDAstronaut[] astronautPool = new SSDAstronaut[MAX_ASTRONAUT];

        public SSDAstronaut() { }

        public override void WriteToSaveGame(VFile savefile)
        {
            base.WriteToSaveGame(savefile);
            savefile.Write(health);
        }

        public override void ReadFromSaveGame(VFile savefile, GameSSDWindow game)
        {
            base.ReadFromSaveGame(savefile, game);
            savefile.Read(out health);
        }

        public void Init(GameSSDWindow game, Vector3 startPosition, float speed, float rotate, int health)
        {
            EntityInit();
            MoverInit(new Vector3(0, 0, -speed), rotate);

            SetGame(game);

            type = (int)SSD_ENTITY.ASTRONAUT;

            SetMaterial(ASTRONAUT_MATERIAL);
            SetSize(new Vector2(256, 256));
            SetRadius(Math.Max(size.x, size.y), 0.3f);
            SetRotation(GameSSDWindow.random.RandomInt(360));

            position = startPosition;
            this.health = health;
        }

        public static SSDAstronaut GetNewAstronaut(GameSSDWindow game, Vector3 startPosition, float speed, float rotate, int health)
        {
            for (var i = 0; i < MAX_ASTRONAUT; i++)
                if (!astronautPool[i].inUse)
                {
                    astronautPool[i].Init(game, startPosition, speed, rotate, health);
                    astronautPool[i].inUse = true;
                    astronautPool[i].id = i;
                    return astronautPool[i];
                }
            return null;
        }

        public static SSDAstronaut GetSpecificAstronaut(int id)
            => astronautPool[id];

        public static void WriteAstronauts(VFile savefile)
        {
            var count = 0;
            for (var i = 0; i < MAX_ASTRONAUT; i++)
                if (astronautPool[i].inUse)
                    count++;
            savefile.Write(count);
            for (var i = 0; i < MAX_ASTRONAUT; i++)
                if (astronautPool[i].inUse)
                {
                    savefile.Write(astronautPool[i].id);
                    astronautPool[i].WriteToSaveGame(savefile);
                }
        }
        public static void ReadAstronauts(VFile savefile, GameSSDWindow game)
        {
            savefile.Read(out int count);
            for (var i = 0; i < count; i++)
            {
                savefile.Read(out int id);
                var ent = GetSpecificAstronaut(id);
                ent.ReadFromSaveGame(savefile, game);
            }
        }
    }

    // SSDExplosion
    class SSDExplosion : SSDEntity
    {
        internal static string[] explosionMaterials = {
            "game/SSD/fball",
            "game/SSD/teleport"
        };

        public enum EXPLOSION
        {
            NORMAL = 0,
            TELEPORT = 1
        }

        public const int MAX_EXPLOSIONS = 64;
        public Vector2 finalSize;
        public int length;
        public int beginTime;
        public int endTime;
        public int explosionType;

        // The entity that is exploding
        public SSDEntity buddy;
        public bool killBuddy;
        public bool followBuddy;

        protected static SSDExplosion[] explosionPool = new SSDExplosion[MAX_EXPLOSIONS];

        public SSDExplosion()
            => type = (int)SSD_ENTITY.EXPLOSION;

        public override void WriteToSaveGame(VFile savefile)
        {
            base.WriteToSaveGame(savefile);

            savefile.WriteT(finalSize);
            savefile.Write(length);
            savefile.Write(beginTime);
            savefile.Write(endTime);
            savefile.Write(explosionType);

            savefile.Write(buddy.type);
            savefile.Write(buddy.id);

            savefile.Write(killBuddy);
            savefile.Write(followBuddy);
        }

        public override void ReadFromSaveGame(VFile savefile, GameSSDWindow game)
        {
            base.ReadFromSaveGame(savefile, game);
            savefile.ReadT(out finalSize);
            savefile.Read(out length);
            savefile.Read(out beginTime);
            savefile.Read(out endTime);
            savefile.Read(out explosionType);

            savefile.Read(out int type);
            savefile.Read(out int id);

            // Get a pointer to my buddy
            buddy = game.GetSpecificEntity(type, id);

            savefile.Read(out killBuddy);
            savefile.Read(out followBuddy);
        }

        public void Init(GameSSDWindow game, Vector3 position, Vector2 size, int length, int type, SSDEntity buddy, bool killBuddy = true, bool followBuddy = true)
        {
            EntityInit();

            SetGame(game);

            this.type = (int)SSD_ENTITY.EXPLOSION;
            explosionType = type;

            SetMaterial(explosionMaterials[explosionType]);
            SetPosition(position);
            position.z -= 50;

            finalSize = size;
            this.length = length;
            beginTime = game.ssdTime;
            endTime = beginTime + length;

            this.buddy = buddy;
            this.killBuddy = killBuddy;
            this.followBuddy = followBuddy;

            // Explosion Starts from nothing and will increase in size until it gets to final size
            size.Zero();

            noPlayerDamage = true;
            noHit = true;
        }

        public override void EntityUpdate()
        {
            base.EntityUpdate();

            // Always set my position to my buddies position except change z to be on top
            if (followBuddy) { position = buddy.position; position.z -= 50; }
            // Only mess with the z if we are not following
            else position.z = buddy.position.z - 50;

            //Scale the image based on the time
            size = finalSize * ((float)(currentTime - beginTime) / (float)length);

            // Destroy myself after the explosion is done
            if (currentTime > endTime)
            {
                destroyed = true;
                // Destroy the exploding object
                if (killBuddy)
                    buddy.destroyed = true;
            }
        }

        public static SSDExplosion GetNewExplosion(GameSSDWindow game, Vector3 position, Vector2 size, int length, int type, SSDEntity buddy, bool killBuddy = true, bool followBuddy = true)
        {
            for (var i = 0; i < MAX_EXPLOSIONS; i++)
                if (!explosionPool[i].inUse)
                {
                    explosionPool[i].Init(game, position, size, length, type, buddy, killBuddy, followBuddy);
                    explosionPool[i].inUse = true;
                    return explosionPool[i];
                }
            return null;
        }

        public static SSDExplosion GetSpecificExplosion(int id)
            => explosionPool[id];

        public static void WriteExplosions(VFile savefile)
        {
            var count = 0;
            for (var i = 0; i < MAX_EXPLOSIONS; i++)
                if (explosionPool[i].inUse)
                    count++;
            savefile.Write(count);
            for (var i = 0; i < MAX_EXPLOSIONS; i++)
            {
                if (explosionPool[i].inUse)
                {
                    savefile.Write(explosionPool[i].id);
                    explosionPool[i].WriteToSaveGame(savefile);
                }
            }
        }

        public static void ReadExplosions(VFile savefile, GameSSDWindow game)
        {
            savefile.Read(out int count);
            for (var i = 0; i < count; i++)
            {
                savefile.Read(out int id);
                var ent = GetSpecificExplosion(id);
                ent.ReadFromSaveGame(savefile, game);
            }
        }
    }

    class SSDPoints : SSDEntity
    {
        public const int MAX_POINTS = 16;
        public int length;
        public int distance;
        public int beginTime;
        public int endTime;

        public Vector3 beginPosition;
        public Vector3 endPosition;

        public Vector4 beginColor;
        public Vector4 endColor;
        protected static SSDPoints[] pointsPool = new SSDPoints[MAX_POINTS];

        public SSDPoints()
            => type = (int)SSD_ENTITY.POINTS;

        public override void WriteToSaveGame(VFile savefile)
        {
            base.WriteToSaveGame(savefile);

            savefile.Write(length);
            savefile.Write(distance);
            savefile.Write(beginTime);
            savefile.Write(endTime);

            savefile.WriteT(beginPosition);
            savefile.WriteT(endPosition);

            savefile.WriteT(beginColor);
            savefile.WriteT(endColor);
        }

        public override void ReadFromSaveGame(VFile savefile, GameSSDWindow game)
        {
            base.ReadFromSaveGame(savefile, game);

            savefile.Read(out length);
            savefile.Read(out distance);
            savefile.Read(out beginTime);
            savefile.Read(out endTime);

            savefile.ReadT(out beginPosition);
            savefile.ReadT(out endPosition);

            savefile.ReadT(out beginColor);
            savefile.ReadT(out endColor);
        }

        public void Init(GameSSDWindow game, SSDEntity _ent, int points, int length, int distance, Vector4 color)
        {
            EntityInit();

            SetGame(game);

            this.length = length;
            this.distance = distance;
            beginTime = game.ssdTime;
            endTime = beginTime + length;

            textScale = 0.4f;
            text = points.ToString();

            var width = 0f;
            for (var i = 0; i < text.Length; i++)
                width += game.DC.CharWidth(text[i], textScale);

            size.Set(0, 0);

            //Set the start position at the top of the passed in entity
            position = WorldToScreen(_ent.position);
            position = ScreenToWorld(position);

            position.z = 0;
            position.x -= width / 2f;

            beginPosition = position;

            endPosition = beginPosition;
            endPosition.y += distance;

            //beginColor.Set(0,1,0,1);
            endColor.Set(1, 1, 1, 0);

            beginColor = color;
            beginColor.w = 1;

            noPlayerDamage = true;
            noHit = true;
        }

        public override void EntityUpdate()
        {
            var t = (float)(currentTime - beginTime) / (float)length;

            // Move up from the start position
            position.Lerp(beginPosition, endPosition, t);

            // Interpolate the color
            foreColor.Lerp(beginColor, endColor, t);

            if (currentTime > endTime)
                destroyed = true;
        }

        public static SSDPoints GetNewPoints(GameSSDWindow game, SSDEntity ent, int points, int length, int distance, Vector4 color)
        {
            for (var i = 0; i < MAX_POINTS; i++)
                if (!pointsPool[i].inUse)
                {
                    pointsPool[i].Init(game, ent, points, length, distance, color);
                    pointsPool[i].inUse = true;
                    return pointsPool[i];
                }
            return null;
        }

        public static SSDPoints GetSpecificPoints(int id)
            => pointsPool[id];

        public static void WritePoints(VFile savefile)
        {
            var count = 0;
            for (var i = 0; i < MAX_POINTS; i++)
                if (pointsPool[i].inUse)
                    count++;
            savefile.Write(count);
            for (var i = 0; i < MAX_POINTS; i++)
                if (pointsPool[i].inUse)
                {
                    savefile.Write(pointsPool[i].id);
                    pointsPool[i].WriteToSaveGame(savefile);
                }
        }

        public static void ReadPoints(VFile savefile, GameSSDWindow game)
        {
            savefile.Read(out int count);
            for (var i = 0; i < count; i++)
            {
                savefile.Read(out int id);
                var ent = GetSpecificPoints(id);
                ent.ReadFromSaveGame(savefile, game);
            }
        }
    }

    class SSDProjectile : SSDEntity
    {
        internal const string PROJECTILE_MATERIAL = "game/SSD/fball";
        public const int MAX_PROJECTILES = 64;

        public Vector3 dir;
        public Vector3 speed;
        public int beginTime;
        public int endTime;

        public Vector3 endPosition;
        protected static SSDProjectile[] projectilePool = new SSDProjectile[MAX_PROJECTILES];

        public SSDProjectile()
            => type = (int)SSD_ENTITY.PROJECTILE;

        public override void WriteToSaveGame(VFile savefile)
        {
            base.WriteToSaveGame(savefile);

            savefile.WriteT(dir);
            savefile.WriteT(speed);
            savefile.Write(beginTime);
            savefile.Write(endTime);

            savefile.WriteT(endPosition);
        }

        public override void ReadFromSaveGame(VFile savefile, GameSSDWindow game)
        {
            base.ReadFromSaveGame(savefile, game);

            savefile.ReadT(out dir);
            savefile.ReadT(out speed);
            savefile.Read(out beginTime);
            savefile.Read(out endTime);

            savefile.ReadT(out endPosition);
        }

        public void Init(GameSSDWindow game, Vector3 beginPosition, Vector3 endPosition, float speed, float size)
        {
            EntityInit();

            SetGame(game);

            SetMaterial(PROJECTILE_MATERIAL);
            this.size.Set(size, size);

            position = beginPosition;
            this.endPosition = endPosition;

            dir = endPosition - position;
            dir.Normalize();

            //speed.Zero();
            this.speed.x = this.speed.y = this.speed.z = speed;

            noHit = true;
        }

        public override void EntityUpdate()
        {
            base.EntityUpdate();

            //Move forward based on speed (units per second)
            var moved = dir * ((float)elapsed / 1000f) * speed.z;
            position += moved;

            // We have reached our position
            if (position.z > endPosition.z)
                destroyed = true;
        }

        public static SSDProjectile GetNewProjectile(GameSSDWindow game, Vector3 beginPosition, Vector3 endPosition, float speed, float size)
        {
            for (var i = 0; i < MAX_PROJECTILES; i++)
                if (!projectilePool[i].inUse)
                {
                    projectilePool[i].Init(game, beginPosition, endPosition, speed, size);
                    projectilePool[i].inUse = true;
                    return projectilePool[i];
                }
            return null;
        }

        public static SSDProjectile GetSpecificProjectile(int id)
            => projectilePool[id];

        public static void WriteProjectiles(VFile savefile)
        {
            var count = 0;
            for (var i = 0; i < MAX_PROJECTILES; i++)
                if (projectilePool[i].inUse)
                    count++;
            savefile.Write(count);
            for (var i = 0; i < MAX_PROJECTILES; i++)
                if (projectilePool[i].inUse)
                {
                    savefile.Write(projectilePool[i].id);
                    projectilePool[i].WriteToSaveGame(savefile);
                }
        }

        public static void ReadProjectiles(VFile savefile, GameSSDWindow game)
        {
            savefile.Read(out int count);
            for (var i = 0; i < count; i++)
            {
                savefile.Read(out int id);
                var ent = GetSpecificProjectile(id);
                ent.ReadFromSaveGame(savefile, game);
            }
        }
    }

    // Powerups work in two phases:
    //	1.) Closed container hurls at you If you shoot the container it open
    //	3.) If an opened powerup hits the player he aquires the powerup
    // Powerup Types:
    //	Health - Give a specific amount of health
    //	Super Blaster - Increases the power of the blaster (lasts a specific amount of time)
    //	Asteroid Nuke - Destroys all asteroids on screen as soon as it is aquired
    //	Rescue Powerup - Rescues all astronauts as soon as it is acquited
    //	Bonus Points - Gives some bonus points when acquired
    class SSDPowerup : SSDMover
    {
        internal static string[][] powerupMaterials = {
            new []{ "game/SSD/powerupHealthClosed",         "game/SSD/powerupHealthOpen" },
            new []{ "game/SSD/powerupSuperBlasterClosed",   "game/SSD/powerupSuperBlasterOpen" },
            new []{ "game/SSD/powerupNukeClosed",           "game/SSD/powerupNukeOpen" },
            new []{ "game/SSD/powerupRescueClosed",         "game/SSD/powerupRescueOpen" },
            new []{ "game/SSD/powerupBonusPointsClosed",    "game/SSD/powerupBonusPointsOpen" },
            new []{ "game/SSD/powerupDamageClosed",         "game/SSD/powerupDamageOpen" },
        };

        public const int MAX_POWERUPS = 64;

        enum POWERUP_STATE
        {
            CLOSED = 0,
            OPEN
        }

        enum POWERUP_TYPE
        {
            HEALTH = 0,
            SUPER_BLASTER,
            ASTEROID_NUKE,
            RESCUE_ALL,
            BONUS_POINTS,
            DAMAGE,
            MAX
        }

        public int powerupState;
        public int powerupType;
        protected static SSDPowerup[] powerupPool = new SSDPowerup[MAX_POWERUPS];

        public SSDPowerup() { }

        public override void WriteToSaveGame(VFile savefile)
        {
            base.WriteToSaveGame(savefile);
            savefile.Write(powerupState);
            savefile.Write(powerupType);
        }

        public override void ReadFromSaveGame(VFile savefile, GameSSDWindow game)
        {
            base.ReadFromSaveGame(savefile, game);
            savefile.Read(out powerupState);
            savefile.Read(out powerupType);
        }

        public override void OnHit(int key)
        {
            if (powerupState == (int)POWERUP_STATE.CLOSED)
            {
                // Small explosion to indicate it is opened
                var explosion = SSDExplosion.GetNewExplosion(game, position, size * 2f, 300, (int)SSDExplosion.EXPLOSION.NORMAL, this, false, true);
                game.entities.Add(explosion);

                powerupState = (int)POWERUP_STATE.OPEN;
                SetMaterial(powerupMaterials[powerupType][powerupState]);
            }
            else
            {
                // Destory the powerup with a big explosion
                var explosion = SSDExplosion.GetNewExplosion(game, position, size * 2, 300, (int)SSDExplosion.EXPLOSION.NORMAL, this);
                game.entities.Add(explosion);
                game.PlaySound("arcade_explode");

                noHit = true;
                noPlayerDamage = true;
            }
        }
        public override void OnStrikePlayer()
        {
            // The powerup was open so activate it
            if (powerupState == (int)POWERUP_STATE.OPEN)
                OnActivatePowerup();

            // Just destroy the powerup
            destroyed = true;
        }

        public void OnOpenPowerup() { }

        public void OnActivatePowerup()
        {
            switch ((POWERUP_TYPE)powerupType)
            {
                case POWERUP_TYPE.HEALTH: game.AddHealth(10); break;
                case POWERUP_TYPE.SUPER_BLASTER: game.OnSuperBlaster(); break;
                case POWERUP_TYPE.ASTEROID_NUKE: game.OnNuke(); break;
                case POWERUP_TYPE.RESCUE_ALL: game.OnRescueAll(); break;
                case POWERUP_TYPE.BONUS_POINTS: var points = (GameSSDWindow.random.RandomInt(5) + 1) * 100; game.AddScore(this, points); break;
                case POWERUP_TYPE.DAMAGE: game.AddDamage(10); game.PlaySound("arcade_explode"); break;
            }
        }

        public void Init(GameSSDWindow game, float speed, float rotation)
        {
            EntityInit();
            MoverInit(new Vector3(0, 0, -speed), rotation);

            SetGame(game);
            SetSize(new Vector2(200, 200));
            SetRadius(Math.Max(size.x, size.y), 0.3f);

            type = (int)SSD_ENTITY.POWERUP;

            Vector3 startPosition;
            startPosition.x = GameSSDWindow.random.RandomInt((int)GameSSDWindow.V_WIDTH) - (GameSSDWindow.V_WIDTH / 2f);
            startPosition.y = GameSSDWindow.random.RandomInt((int)GameSSDWindow.V_HEIGHT) - (GameSSDWindow.V_HEIGHT / 2f);
            startPosition.z = GameSSDWindow.ENTITY_START_DIST;

            position = startPosition;

            powerupState = (int)POWERUP_STATE.CLOSED;
            powerupType = GameSSDWindow.random.RandomInt((int)POWERUP_TYPE.MAX + 1);
            if (powerupType >= (int)POWERUP_TYPE.MAX) powerupType = 0;

            SetMaterial(powerupMaterials[powerupType][powerupState]);
        }

        public static SSDPowerup GetNewPowerup(GameSSDWindow game, float speed, float rotation)
        {
            for (var i = 0; i < MAX_POWERUPS; i++)
                if (!powerupPool[i].inUse)
                {
                    powerupPool[i].Init(game, speed, rotation);
                    powerupPool[i].inUse = true;
                    return powerupPool[i];
                }
            return null;
        }

        public static SSDPowerup GetSpecificPowerup(int id)
            => powerupPool[id];

        public static void WritePowerups(VFile savefile)
        {
            var count = 0;
            for (var i = 0; i < MAX_POWERUPS; i++)
                if (powerupPool[i].inUse)
                    count++;
            savefile.Write(count);
            for (var i = 0; i < MAX_POWERUPS; i++)
                if (powerupPool[i].inUse)
                {
                    savefile.Write(powerupPool[i].id);
                    powerupPool[i].WriteToSaveGame(savefile);
                }
        }

        public static void ReadPowerups(VFile savefile, GameSSDWindow game)
        {
            savefile.Read(out int count);
            for (var i = 0; i < count; i++)
            {
                savefile.Read(out int id);
                var ent = GetSpecificPowerup(id);
                ent.ReadFromSaveGame(savefile, game);
            }
        }
    }

    struct SSDLevelData
    {
        public float spawnBuffer;
        public int needToWin;
    }

    struct SSDAsteroidData
    {
        public float speedMin, speedMax;
        public float sizeMin, sizeMax;
        public float rotateMin, rotateMax;
        public int spawnMin, spawnMax;
        public int asteroidHealth;
        public int asteroidPoints;
        public int asteroidDamage;
    }

    struct SSDAstronautData
    {
        public float speedMin, speedMax;
        public float rotateMin, rotateMax;
        public int spawnMin, spawnMax;
        public int health;
        public int points;
        public int penalty;
    }

    struct SSDPowerupData
    {
        public float speedMin, speedMax;
        public float rotateMin, rotateMax;
        public int spawnMin, spawnMax;
    }

    struct SSDWeaponData
    {
        public float speed;
        public int damage;
        public int size;
    }

    // Data that is used for each level. This data is reset each new level.
    struct SSDLevelStats
    {
        public int shotCount;
        public int hitCount;
        public int destroyedAsteroids;
        public int nextAsteroidSpawnTime;

        public int killedAstronauts;
        public int savedAstronauts;

        // Astronaut Level Data
        public int nextAstronautSpawnTime;

        // Powerup Level Data
        public int nextPowerupSpawnTime;

        public SSDEntity targetEnt;

        internal void memset()
        {
            throw new NotImplementedException();
        }
    }

    //	Data that is used for the game that is currently running. Memset this to completely reset the game
    struct SSDGameStats
    {
        public bool gameRunning;

        public int score;
        public int prebonusscore;

        public int health;

        public int currentWeapon;
        public int currentLevel;
        public int nextLevel;

        public SSDLevelStats levelStats;

        internal void memset()
        {
            gameRunning = false;
        }
    }

    class GameSSDWindow : Window
    {
        public const float Z_NEAR = 100f;
        public const float Z_FAR = 4000f;
        public const int ENTITY_START_DIST = 3000;
        public const float V_WIDTH = 640f;
        public const float V_HEIGHT = 480f;

        public static RandomX random = new();
        public int ssdTime;
        // WinVars used to call functions from the guis
        public WinBool beginLevel;
        public WinBool resetGame;
        public WinBool continueGame;
        public WinBool refreshGuiData;

        public SSDCrossHair crosshair;
        public Bounds screenBounds;

        // Level Data
        public int levelCount;
        public List<SSDLevelData> levelData = new();
        public List<SSDAsteroidData> asteroidData = new();
        public List<SSDAstronautData> astronautData = new();
        public List<SSDPowerupData> powerupData = new();

        // Weapon Data
        public int weaponCount;
        public List<SSDWeaponData> weaponData = new();

        public int superBlasterTimeout;

        // All current game data is stored in this structure (except the entity list)
        public SSDGameStats gameStats;
        public List<SSDEntity> entities = new();

        public int currentSound;

        protected override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "beginLevel", StringComparison.OrdinalIgnoreCase)) { beginLevel = src.ParseBool(); return true; }
            if (string.Equals(name, "resetGame", StringComparison.OrdinalIgnoreCase)) { resetGame = src.ParseBool(); return true; }
            if (string.Equals(name, "continueGame", StringComparison.OrdinalIgnoreCase)) { continueGame = src.ParseBool(); return true; }
            if (string.Equals(name, "refreshGuiData", StringComparison.OrdinalIgnoreCase)) { refreshGuiData = src.ParseBool(); return true; }
            if (string.Equals(name, "levelcount", StringComparison.OrdinalIgnoreCase))
            {
                levelCount = src.ParseInt();
                for (var i = 0; i < levelCount; i++)
                {
                    levelData.Add(new SSDLevelData());
                    asteroidData.Add(new SSDAsteroidData());
                    astronautData.Add(new SSDAstronautData());
                    powerupData.Add(new SSDPowerupData());
                }
                return true;
            }
            if (string.Equals(name, "weaponCount", StringComparison.OrdinalIgnoreCase))
            {
                weaponCount = src.ParseInt();
                for (var i = 0; i < weaponCount; i++)
                    weaponData.Add(new SSDWeaponData());
                return true;
            }
            if (name.Contains("leveldata", StringComparison.OrdinalIgnoreCase))
            {
                var level = intX.Parse(name[^2..]) - 1;
                ParseString(src, out var levelData);
                ParseLevelData(level, levelData);
                return true;
            }
            if (name.Contains("asteroiddata", StringComparison.OrdinalIgnoreCase))
            {
                var level = intX.Parse(name[^2..]) - 1;
                ParseString(src, out var asteroidData);
                ParseAsteroidData(level, asteroidData);
                return true;
            }
            if (name.Contains("weapondata", StringComparison.OrdinalIgnoreCase))
            {
                var weapon = intX.Parse(name[^2..]) - 1;
                ParseString(src, out var weaponData);
                ParseWeaponData(weapon, weaponData);
                return true;
            }
            if (name.Contains("astronautdata", StringComparison.OrdinalIgnoreCase))
            {
                var level = intX.Parse(name[^2..]) - 1;
                ParseString(src, out var astronautData);
                ParseAstronautData(level, astronautData);
                return true;
            }
            if (name.Contains("powerupdata", StringComparison.OrdinalIgnoreCase))
            {
                var level = intX.Parse(name[^2..]) - 1;
                ParseString(src, out var powerupData);
                ParsePowerupData(level, powerupData);
                return true;
            }
            return base.ParseInternalVar(name, src);
        }

        void ParseLevelData(int level, string levelDataString)
        {
            Parser parser = new();
            parser.LoadMemory(levelDataString, levelDataString.Length, "LevelData");
            levelData[level] = new SSDLevelData
            {
                spawnBuffer = parser.ParseFloat(),
                needToWin = parser.ParseInt(), // Required Destroyed
            };
        }

        void ParseAsteroidData(int level, string asteroidDataString)
        {
            Parser parser = new();
            parser.LoadMemory(asteroidDataString, asteroidDataString.Length, "AsteroidData");
            asteroidData[level] = new SSDAsteroidData
            {
                speedMin = parser.ParseFloat(), // Speed Min
                speedMax = parser.ParseFloat(), // Speed Max
                sizeMin = parser.ParseFloat(), // Size Min
                sizeMax = parser.ParseFloat(), // Size Max
                rotateMin = parser.ParseFloat(), // Rotate Min (rotations per second)
                rotateMax = parser.ParseFloat(), // Rotate Max (rotations per second)
                spawnMin = parser.ParseInt(), // Spawn Min
                spawnMax = parser.ParseInt(), // Spawn Max
                asteroidHealth = parser.ParseInt(), // Health of the asteroid
                asteroidDamage = parser.ParseInt(), // Asteroid Damage
                asteroidPoints = parser.ParseInt(), // Points awarded for destruction
            };
        }
        void ParseWeaponData(int weapon, string weaponDataString)
        {
            Parser parser = new();
            parser.LoadMemory(weaponDataString, weaponDataString.Length, "WeaponData");
            weaponData[weapon] = new SSDWeaponData
            {
                speed = parser.ParseFloat(),
                damage = (int)parser.ParseFloat(),
                size = (int)parser.ParseFloat(),
            };
        }
        void ParseAstronautData(int level, string astronautDataString)
        {
            Parser parser = new();
            parser.LoadMemory(astronautDataString, astronautDataString.Length, "AstronautData");
            astronautData[level] = new SSDAstronautData
            {
                speedMin = parser.ParseFloat(), // Speed Min
                speedMax = parser.ParseFloat(), // Speed Max
                rotateMin = parser.ParseFloat(), // Rotate Min (rotations per second)
                rotateMax = parser.ParseFloat(), // Rotate Max (rotations per second)
                spawnMin = parser.ParseInt(), // Spawn Min
                spawnMax = parser.ParseInt(), // Spawn Max
                health = parser.ParseInt(), // Health of the asteroid
                points = parser.ParseInt(), // Asteroid Damage
                penalty = parser.ParseInt(), // Points awarded for destruction
            };
        }
        void ParsePowerupData(int level, string powerupDataString)
        {
            Parser parser = new();
            parser.LoadMemory(powerupDataString, powerupDataString.Length, "PowerupData");
            powerupData[level] = new SSDPowerupData
            {
                speedMin = parser.ParseFloat(), // Speed Min
                speedMax = parser.ParseFloat(), // Speed Max
                rotateMin = parser.ParseFloat(), // Rotate Min (rotations per second)
                rotateMax = parser.ParseFloat(), // Rotate Max (rotations per second)
                spawnMin = parser.ParseInt(), // Spawn Min
                spawnMax = parser.ParseInt(), // Spawn Max
            };
        }

        public override WinVar GetWinVarByName(string name, bool winLookup = false, Action<DrawWin> owner = null)
        {
            WinVar retVar = null;
            if (string.Equals(name, "beginLevel", StringComparison.OrdinalIgnoreCase)) retVar = beginLevel;
            if (string.Equals(name, "resetGame", StringComparison.OrdinalIgnoreCase)) retVar = resetGame;
            if (string.Equals(name, "continueGame", StringComparison.OrdinalIgnoreCase)) retVar = continueGame;
            if (string.Equals(name, "refreshGuiData", StringComparison.OrdinalIgnoreCase)) retVar = refreshGuiData;
            if (retVar != null) return retVar;
            return base.GetWinVarByName(name, winLookup, owner);
        }

        void CommonInit()
        {
            crosshair.InitCrosshairs();

            beginLevel = false;
            resetGame = false;
            continueGame = false;
            refreshGuiData = false;

            ssdTime = 0;
            levelCount = 0;
            weaponCount = 0;
            screenBounds = new Bounds(new Vector3(-320, -240, 0), new Vector3(320, 240, 0));

            superBlasterTimeout = 0;

            currentSound = 0;

            // Precahce all assets that are loaded dynamically
            declManager.FindMaterial(SSDAsteroid.ASTEROID_MATERIAL);
            declManager.FindMaterial(SSDAstronaut.ASTRONAUT_MATERIAL);

            for (var i = 0; i < SSDExplosion.explosionMaterials.Length; i++)
                declManager.FindMaterial(SSDExplosion.explosionMaterials[i]);
            declManager.FindMaterial(SSDProjectile.PROJECTILE_MATERIAL);
            for (var i = 0; i < SSDPowerup.powerupMaterials.Length; i++)
            {
                declManager.FindMaterial(SSDPowerup.powerupMaterials[i][0]);
                declManager.FindMaterial(SSDPowerup.powerupMaterials[i][1]);
            }

            // Precache sounds
            declManager.FindSound("arcade_blaster");
            declManager.FindSound("arcade_capture ");
            declManager.FindSound("arcade_explode");

            ResetGameStats();
        }

        public GameSSDWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public GameSSDWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        public override void WriteToSaveGame(VFile savefile)
        {
            base.WriteToSaveGame(savefile);

            savefile.Write(ssdTime);

            beginLevel.WriteToSaveGame(savefile);
            resetGame.WriteToSaveGame(savefile);
            continueGame.WriteToSaveGame(savefile);
            refreshGuiData.WriteToSaveGame(savefile);

            crosshair.WriteToSaveGame(savefile);
            savefile.WriteT(screenBounds);

            savefile.Write(levelCount);
            for (var i = 0; i < levelCount; i++)
            {
                savefile.WriteT(levelData[i]);
                savefile.WriteT(asteroidData[i]);
                savefile.WriteT(astronautData[i]);
                savefile.WriteT(powerupData[i]);
            }

            savefile.Write(weaponCount);
            for (var i = 0; i < weaponCount; i++)
                savefile.WriteT(weaponData[i]);

            savefile.Write(superBlasterTimeout);
            savefile.WriteT(gameStats);

            // Write All Static Entities
            SSDAsteroid.WriteAsteroids(savefile);
            SSDAstronaut.WriteAstronauts(savefile);
            SSDExplosion.WriteExplosions(savefile);
            SSDPoints.WritePoints(savefile);
            SSDProjectile.WriteProjectiles(savefile);
            SSDPowerup.WritePowerups(savefile);

            var entCount = entities.Count;
            savefile.Write(entCount);
            for (var i = 0; i < entCount; i++)
            {
                savefile.Write(entities[i].type);
                savefile.Write(entities[i].id);
            }
        }

        public override void ReadFromSaveGame(VFile savefile)
        {
            base.ReadFromSaveGame(savefile);

            savefile.Read(out ssdTime);

            beginLevel.ReadFromSaveGame(savefile);
            resetGame.ReadFromSaveGame(savefile);
            continueGame.ReadFromSaveGame(savefile);
            refreshGuiData.ReadFromSaveGame(savefile);

            crosshair.ReadFromSaveGame(savefile);
            savefile.ReadT(out screenBounds);

            savefile.Read(out levelCount);
            for (var i = 0; i < levelCount; i++)
            {
                savefile.ReadT(out SSDLevelData newLevel);
                levelData.Add(newLevel);

                savefile.ReadT(out SSDAsteroidData newAsteroid);
                asteroidData.Add(newAsteroid);

                savefile.ReadT(out SSDAstronautData newAstronaut);
                astronautData.Add(newAstronaut);

                savefile.ReadT(out SSDPowerupData newPowerup);
                powerupData.Add(newPowerup);
            }

            savefile.Read(out weaponCount);
            for (var i = 0; i < weaponCount; i++)
            {
                savefile.ReadT(out SSDWeaponData newWeapon);
                weaponData.Add(newWeapon);
            }

            savefile.Read(out superBlasterTimeout);

            savefile.ReadT(out gameStats);

            // Reset this because it is no longer valid
            gameStats.levelStats.targetEnt = null;

            SSDAsteroid.ReadAsteroids(savefile, this);
            SSDAstronaut.ReadAstronauts(savefile, this);
            SSDExplosion.ReadExplosions(savefile, this);
            SSDPoints.ReadPoints(savefile, this);
            SSDProjectile.ReadProjectiles(savefile, this);
            SSDPowerup.ReadPowerups(savefile, this);

            savefile.Read(out int entCount);
            for (var i = 0; i < entCount; i++)
            {
                savefile.Read(out int type);
                savefile.Read(out int id);

                var ent = GetSpecificEntity(type, id);
                if (ent != null)
                    entities.Add(ent);
            }
        }

        public override string HandleEvent(SysEvent ev, Action<bool> updateVisuals)
        {
            // need to call this to allow proper focus and capturing on embedded children
            var ret = base.HandleEvent(ev, updateVisuals);

            if (!gameStats.gameRunning)
                return ret;

            var key = (Key)ev.evValue;
            if (ev.evType == SE.KEY)
            {
                if (ev.evValue2 == 0)
                    return ret;

                if (key == K_MOUSE1 || key == K_MOUSE2) FireWeapon((int)key);
                else return ret;
            }
            return ret;
        }

        public override void Draw(int time, float x, float y)
        {
            // Update the game every frame before drawing
            UpdateGame();

            RefreshGuiData();

            if (gameStats.gameRunning)
            {
                ZOrderEntities();

                // Draw from back to front
                for (var i = entities.Count - 1; i >= 0; i--)
                    entities[i].Draw(dc);

                // The last thing to draw is the crosshair
                Vector2 cursor;
                cursor.x = gui.CursorX;
                cursor.y = gui.CursorY;

                crosshair.Draw(dc, cursor);
            }
        }

        void ResetGameStats()
        {
            ResetEntities();

            // Reset the gamestats structure
            gameStats.memset();
            gameStats.health = 100;
        }
        void ResetLevelStats()
        {
            ResetEntities();

            // Reset the level statistics structure
            gameStats.levelStats.memset();
        }
        void ResetEntities()
        {
            // Destroy all of the entities
            for (var i = 0; i < entities.Count; i++)
                entities[i].DestroyEntity();
            entities.Clear();
        }

        // Game Running Methods
        void StartGame()
            => gameStats.gameRunning = true;
        void StopGame()
            => gameStats.gameRunning = false;
        void GameOver()
        {
            StopGame();
            gui.HandleNamedEvent("gameOver");
        }

        // Starting the Game
        void BeginLevel(int level)
        {
            ResetLevelStats();
            gameStats.currentLevel = level;
            StartGame();
        }
        // Continue game resets the players health
        void ContinueGame()
        {
            gameStats.health = 100;
            StartGame();
        }

        // Stopping the Game
        void LevelComplete()
        {

            gameStats.prebonusscore = gameStats.score;

            // Add the bonuses
            var accuracy = gameStats.levelStats.shotCount == 0 ? 0 : (int)((float)gameStats.levelStats.hitCount / (float)gameStats.levelStats.shotCount * 100f);
            var accuracyPoints = Math.Max(0, accuracy - 50) * 20;

            gui.SetStateString("player_accuracy_score", accuracyPoints.ToString());

            gameStats.score += accuracyPoints;

            var totalAst = gameStats.levelStats.savedAstronauts + gameStats.levelStats.killedAstronauts;
            var saveAccuracy = totalAst == 0 ? 0 : (int)((float)gameStats.levelStats.savedAstronauts / (float)totalAst * 100f);
            accuracyPoints = Math.Max(0, saveAccuracy - 50) * 20;

            gui.SetStateString("save_accuracy_score", accuracyPoints.ToString());

            gameStats.score += accuracyPoints;

            StopSuperBlaster();

            gameStats.nextLevel++;

            // Have they beaten the game
            if (gameStats.nextLevel >= levelCount) GameComplete();
            // Make sure we don't go above the levelcount
            else
            {
                //Math.Min(gameStats.nextLevel, levelCount - 1);
                StopGame();
                gui.HandleNamedEvent("levelComplete");
            }
        }
        void GameComplete()
        {
            StopGame();
            gui.HandleNamedEvent("gameComplete");
        }

        void UpdateGame()
        {
            // Check to see if and functions where called by the gui
            if (beginLevel == true) { beginLevel = false; BeginLevel(gameStats.nextLevel); }
            if (resetGame == true) { resetGame = false; ResetGameStats(); }
            if (continueGame == true) { continueGame = false; ContinueGame(); }
            if (refreshGuiData == true) { refreshGuiData = false; RefreshGuiData(); }

            if (gameStats.gameRunning)
            {
                // We assume an upate every 16 milliseconds
                ssdTime += 16;

                if (superBlasterTimeout != 0 && ssdTime > superBlasterTimeout)
                    StopSuperBlaster();

                // Find if we are targeting and enemy
                Vector2 cursor;
                cursor.x = gui.CursorX;
                cursor.y = gui.CursorY;
                gameStats.levelStats.targetEnt = EntityHitTest(cursor);

                // Update from back to front
                for (var i = entities.Count - 1; i >= 0; i--)
                    entities[i].Update();

                CheckForHits();

                // Delete entities that need to be deleted
                for (var i = entities.Count - 1; i >= 0; i--)
                    if (entities[i].destroyed)
                    {
                        var ent = entities[i];
                        ent.DestroyEntity();
                        entities.RemoveAt(i);
                    }

                // Check if we can spawn an asteroid
                SpawnAsteroid();

                // Check if we should spawn an astronaut
                SpawnAstronaut();

                // Check if we should spawn an asteroid
                SpawnPowerup();
            }
        }
        void CheckForHits()
        {
            // See if the entity has gotten close enough
            for (var i = 0; i < entities.Count; i++)
            {
                var ent = entities[i];
                if (ent.position.z <= Z_NEAR)
                    if (!ent.noPlayerDamage)
                    {
                        // Is the object still in the screen
                        var entPos = ent.position;
                        entPos.z = 0;

                        var entBounds = new Bounds(entPos);
                        entBounds.ExpandSelf(ent.hitRadius);

                        if (screenBounds.IntersectsBounds(entBounds))
                        {
                            ent.OnStrikePlayer();
                            // The entity hit the player figure out what is was and act appropriately
                            if (ent.type == (int)SSD_ENTITY.ASTEROID) AsteroidStruckPlayer((SSDAsteroid)ent);
                            else if (ent.type == (int)SSD_ENTITY.ASTRONAUT) AstronautStruckPlayer((SSDAstronaut)ent);
                        }
                        // Tag for removal later in the frame
                        else ent.destroyed = true;
                    }
            }
        }
        void ZOrderEntities()
        {
            // Z-Order the entities using a simple sorting method
            for (var i = entities.Count - 1; i >= 0; i--)
            {
                var flipped = false;
                for (var j = 0; j < i; j++)
                    if (entities[j].position.z > entities[j + 1].position.z)
                    {
                        var ent = entities[j];
                        entities[j] = entities[j + 1];
                        entities[j + 1] = ent;
                        flipped = true;
                    }
                // Jump out because it is sorted
                if (!flipped)
                    break;
            }
        }

        void SpawnAsteroid()
        {
            var currentTime = ssdTime;

            if (currentTime < gameStats.levelStats.nextAsteroidSpawnTime)
                return; // Not time yet

            // Lets spawn it
            var spawnBuffer = levelData[gameStats.currentLevel].spawnBuffer * 2f;
            Vector3 startPosition;
            startPosition.x = random.RandomInt((int)(V_WIDTH + spawnBuffer)) - ((V_WIDTH / 2f) + spawnBuffer);
            startPosition.y = random.RandomInt((int)(V_HEIGHT + spawnBuffer)) - ((V_HEIGHT / 2f) + spawnBuffer);
            startPosition.z = ENTITY_START_DIST;

            var speed = (float)random.RandomInt((int)(asteroidData[gameStats.currentLevel].speedMax - asteroidData[gameStats.currentLevel].speedMin)) + asteroidData[gameStats.currentLevel].speedMin;
            var size = (float)random.RandomInt((int)(asteroidData[gameStats.currentLevel].sizeMax - asteroidData[gameStats.currentLevel].sizeMin)) + asteroidData[gameStats.currentLevel].sizeMin;
            var rotate = (random.RandomFloat() * (asteroidData[gameStats.currentLevel].rotateMax - asteroidData[gameStats.currentLevel].rotateMin)) + asteroidData[gameStats.currentLevel].rotateMin;

            var asteroid = SSDAsteroid.GetNewAsteroid(this, startPosition, new Vector2(size, size), speed, rotate, asteroidData[gameStats.currentLevel].asteroidHealth);
            entities.Add(asteroid);

            gameStats.levelStats.nextAsteroidSpawnTime = currentTime + random.RandomInt(asteroidData[gameStats.currentLevel].spawnMax - asteroidData[gameStats.currentLevel].spawnMin) + asteroidData[gameStats.currentLevel].spawnMin;
        }

        void FireWeapon(int key)
        {
            var cursorWorld = GetCursorWorld();
            Vector2 cursor;
            cursor.x = gui.CursorX;
            cursor.y = gui.CursorY;

            if (key == (int)K_MOUSE1)
            {
                gameStats.levelStats.shotCount++;

                if (gameStats.levelStats.targetEnt != null)
                {
                    // Aim the projectile from the bottom of the screen directly at the ent
                    var newProj = SSDProjectile.GetNewProjectile(this, new Vector3(0, -180, 0), gameStats.levelStats.targetEnt.position, weaponData[gameStats.currentWeapon].speed, weaponData[gameStats.currentWeapon].size);
                    entities.Add(newProj);

                    // We hit something
                    gameStats.levelStats.hitCount++;
                    gameStats.levelStats.targetEnt.OnHit(key);
                    if (gameStats.levelStats.targetEnt.type == (int)SSD_ENTITY.ASTEROID) HitAsteroid((SSDAsteroid)gameStats.levelStats.targetEnt, key);
                    else if (gameStats.levelStats.targetEnt.type == (int)SSD_ENTITY.ASTRONAUT) HitAstronaut((SSDAstronaut)gameStats.levelStats.targetEnt, key);
                    else if (gameStats.levelStats.targetEnt.type == (int)SSD_ENTITY.ASTRONAUT) { }
                }
                else
                {
                    // Aim the projectile so it crosses the cursor 1/4 of screen
                    var vec = new Vector3(cursorWorld.x, cursorWorld.y, (Z_FAR - Z_NEAR) / 8f);
                    vec *= 8;
                    var newProj = SSDProjectile.GetNewProjectile(this, new Vector3(0, -180, 0), vec, weaponData[gameStats.currentWeapon].speed, weaponData[gameStats.currentWeapon].size);
                    entities.Add(newProj);
                }

                // Play the blaster sound
                PlaySound("arcade_blaster");
            }
        }
        SSDEntity EntityHitTest(Vector2 pt)
        {
            for (var i = 0; i < entities.Count; i++)
                // Since we ZOrder the entities every frame we can stop at the first entity we hit. TODO: Make sure this assumption is true
                if (entities[i].HitTest(pt))
                    return entities[i];
            return null;
        }

        void HitAsteroid(SSDAsteroid asteroid, int key)
        {
            asteroid.health -= weaponData[gameStats.currentWeapon].damage;

            if (asteroid.health <= 0)
            {
                // The asteroid has been destroyed
                var explosion = SSDExplosion.GetNewExplosion(this, asteroid.position, asteroid.size * 2, 300, (int)SSDExplosion.EXPLOSION.NORMAL, asteroid);
                entities.Add(explosion);
                PlaySound("arcade_explode");

                AddScore(asteroid, asteroidData[gameStats.currentLevel].asteroidPoints);

                // Don't let the player hit it anymore because
                asteroid.noHit = true;

                gameStats.levelStats.destroyedAsteroids++;
            }
            else
            {
                // This was a damage hit so create a real small quick explosion
                var explosion = SSDExplosion.GetNewExplosion(this, asteroid.position, asteroid.size / 2f, 200, (int)SSDExplosion.EXPLOSION.NORMAL, asteroid, false, false);
                entities.Add(explosion);
            }
        }
        void AsteroidStruckPlayer(SSDAsteroid asteroid)
        {
            asteroid.noPlayerDamage = true;
            asteroid.noHit = true;

            AddDamage(asteroidData[gameStats.currentLevel].asteroidDamage);

            var explosion = SSDExplosion.GetNewExplosion(this, asteroid.position, asteroid.size * 2, 300, (int)SSDExplosion.EXPLOSION.NORMAL, asteroid);
            entities.Add(explosion);
            PlaySound("arcade_explode");
        }

        public void AddHealth(int health)
        {
            gameStats.health += health;
            gameStats.health = Math.Min(100, gameStats.health);
        }
        public void AddScore(SSDEntity ent, int points)
        {
            var pointsEnt = SSDPoints.GetNewPoints(this, ent, points, 1000, 50, points > 0 ? new Vector4(0, 1, 0, 1) : new Vector4(1, 0, 0, 1));
            entities.Add(pointsEnt);

            gameStats.score += points;
            gui.SetStateString("player_score", gameStats.score.ToString());
        }
        public void AddDamage(int damage)
        {
            gameStats.health -= damage;
            gui.SetStateString("player_health", gameStats.health.ToString());

            gui.HandleNamedEvent("playerDamage");

            // The player is dead
            if (gameStats.health <= 0)
                GameOver();
        }

        public void OnNuke()
        {
            gui.HandleNamedEvent("nuke");

            // Destory All Asteroids
            for (var i = 0; i < entities.Count; i++)
                if (entities[i].type == (int)SSD_ENTITY.ASTEROID)
                {
                    // The asteroid has been destroyed
                    var explosion = SSDExplosion.GetNewExplosion(this, entities[i].position, entities[i].size * 2, 300, (int)SSDExplosion.EXPLOSION.NORMAL, entities[i]);
                    entities.Add(explosion);

                    AddScore(entities[i], asteroidData[gameStats.currentLevel].asteroidPoints);

                    // Don't let the player hit it anymore because
                    entities[i].noHit = true;

                    gameStats.levelStats.destroyedAsteroids++;
                }
            PlaySound("arcade_explode");
        }
        public void OnRescueAll()
        {
            gui.HandleNamedEvent("rescueAll");

            // Rescue All Astronauts
            for (var i = 0; i < entities.Count; i++)
                if (entities[i].type == (int)SSD_ENTITY.ASTRONAUT)
                    AstronautStruckPlayer((SSDAstronaut)entities[i]);
        }
        public void OnSuperBlaster()
            => StartSuperBlaster();

        void RefreshGuiData()
        {
            gui.SetStateString("nextLevel", (gameStats.nextLevel + 1).ToString());
            gui.SetStateString("currentLevel", (gameStats.currentLevel + 1).ToString());

            var accuracy = gameStats.levelStats.shotCount == 0 ? 0f : (float)gameStats.levelStats.hitCount / (float)gameStats.levelStats.shotCount * 100f;
            gui.SetStateString("player_accuracy", $"{(int)accuracy}%");

            var totalAst = gameStats.levelStats.savedAstronauts + gameStats.levelStats.killedAstronauts;
            var saveAccuracy = totalAst == 0 ? 0f : (float)gameStats.levelStats.savedAstronauts / (float)totalAst * 100f;
            gui.SetStateString("save_accuracy", $"{(int)saveAccuracy}%");

            if (gameStats.levelStats.targetEnt != null)
            {
                var dist = (int)(gameStats.levelStats.targetEnt.position.z / 100f);
                dist *= 100;
                gui.SetStateString("target_info", $"{dist} meters");
            }
            else gui.SetStateString("target_info", "No Target");

            gui.SetStateString("player_health", gameStats.health.ToString());
            gui.SetStateString("player_score", gameStats.score.ToString());
            gui.SetStateString("player_prebonusscore", gameStats.prebonusscore.ToString());
            gui.SetStateString("level_complete", $"{gameStats.levelStats.savedAstronauts}/{levelData[gameStats.currentLevel].needToWin}");

            if (superBlasterTimeout != 0)
            {
                var timeRemaining = (superBlasterTimeout - ssdTime) / 1000f;
                gui.SetStateString("super_blaster_time", $"{timeRemaining:.2}");
            }
        }

        Vector2 GetCursorWorld()
        {
            Vector2 cursor;
            cursor.x = gui.CursorX;
            cursor.y = gui.CursorY;
            cursor.x = cursor.x - 0.5f * V_WIDTH;
            cursor.y = -(cursor.y - 0.5f * V_HEIGHT);
            return cursor;
        }

        // Astronaut Methods
        void SpawnAstronaut()
        {
            var currentTime = ssdTime;

            // Not time yet
            if (currentTime < gameStats.levelStats.nextAstronautSpawnTime)
                return;

            // Lets spawn it
            Vector3 startPosition;
            startPosition.x = random.RandomInt((int)V_WIDTH) - (V_WIDTH / 2f);
            startPosition.y = random.RandomInt((int)V_HEIGHT) - (V_HEIGHT / 2f);
            startPosition.z = ENTITY_START_DIST;

            var speed = random.RandomInt((int)(astronautData[gameStats.currentLevel].speedMax - astronautData[gameStats.currentLevel].speedMin)) + astronautData[gameStats.currentLevel].speedMin;
            var rotate = (random.RandomFloat() * (astronautData[gameStats.currentLevel].rotateMax - astronautData[gameStats.currentLevel].rotateMin)) + astronautData[gameStats.currentLevel].rotateMin;

            var astronaut = SSDAstronaut.GetNewAstronaut(this, startPosition, speed, rotate, astronautData[gameStats.currentLevel].health);
            entities.Add(astronaut);

            gameStats.levelStats.nextAstronautSpawnTime = currentTime + random.RandomInt(astronautData[gameStats.currentLevel].spawnMax - astronautData[gameStats.currentLevel].spawnMin) + astronautData[gameStats.currentLevel].spawnMin;
        }
        void HitAstronaut(SSDAstronaut astronaut, int key)
        {
            if (key == (int)K_MOUSE1)
            {
                astronaut.health -= weaponData[gameStats.currentWeapon].damage;
                if (astronaut.health <= 0)
                {
                    gameStats.levelStats.killedAstronauts++;

                    // The astronaut has been destroyed
                    var explosion = SSDExplosion.GetNewExplosion(this, astronaut.position, astronaut.size * 2, 300, (int)SSDExplosion.EXPLOSION.NORMAL, astronaut);
                    entities.Add(explosion);
                    PlaySound("arcade_explode");

                    // Add the penalty for killing the astronaut
                    AddScore(astronaut, astronautData[gameStats.currentLevel].penalty);

                    // Don't let the player hit it anymore
                    astronaut.noHit = true;
                }
                else
                {
                    // This was a damage hit so create a real small quick explosion
                    var explosion = SSDExplosion.GetNewExplosion(this, astronaut.position, astronaut.size / 2f, 200, (int)SSDExplosion.EXPLOSION.NORMAL, astronaut, false, false);
                    entities.Add(explosion);
                }
            }
        }
        void AstronautStruckPlayer(SSDAstronaut astronaut)
        {
            gameStats.levelStats.savedAstronauts++;

            astronaut.noPlayerDamage = true;
            astronaut.noHit = true;

            // We are saving an astronaut
            var explosion = SSDExplosion.GetNewExplosion(this, astronaut.position, astronaut.size * 2, 300, (int)SSDExplosion.EXPLOSION.TELEPORT, astronaut);
            entities.Add(explosion);
            PlaySound("arcade_capture");

            // Give the player points for saving the astronaut
            AddScore(astronaut, astronautData[gameStats.currentLevel].points);

            if (gameStats.levelStats.savedAstronauts >= levelData[gameStats.currentLevel].needToWin)
                LevelComplete();
        }

        // Powerup Methods
        void SpawnPowerup()
        {
            var currentTime = ssdTime;

            // Not time yet
            if (currentTime < gameStats.levelStats.nextPowerupSpawnTime)
                return;

            var speed = random.RandomInt((int)(powerupData[gameStats.currentLevel].speedMax - powerupData[gameStats.currentLevel].speedMin)) + powerupData[gameStats.currentLevel].speedMin;
            var rotate = (random.RandomFloat() * (powerupData[gameStats.currentLevel].rotateMax - powerupData[gameStats.currentLevel].rotateMin)) + powerupData[gameStats.currentLevel].rotateMin;

            var powerup = SSDPowerup.GetNewPowerup(this, speed, rotate);
            entities.Add(powerup);

            gameStats.levelStats.nextPowerupSpawnTime = currentTime + random.RandomInt(powerupData[gameStats.currentLevel].spawnMax - powerupData[gameStats.currentLevel].spawnMin) + powerupData[gameStats.currentLevel].spawnMin;
        }

        void StartSuperBlaster()
        {
            gui.HandleNamedEvent("startSuperBlaster");
            gameStats.currentWeapon = 1;
            superBlasterTimeout = ssdTime + 10000;
        }
        void StopSuperBlaster()
        {
            gui.HandleNamedEvent("stopSuperBlaster");
            gameStats.currentWeapon = 0;
            superBlasterTimeout = 0;
        }

        //void FreeSoundEmitter(bool immediate);

        public SSDEntity GetSpecificEntity(int type, int id)
            => (SSD_ENTITY)type switch
            {
                SSD_ENTITY.ASTEROID => SSDAsteroid.GetSpecificAsteroid(id),
                SSD_ENTITY.ASTRONAUT => SSDAstronaut.GetSpecificAstronaut(id),
                SSD_ENTITY.EXPLOSION => SSDExplosion.GetSpecificExplosion(id),
                SSD_ENTITY.POINTS => SSDPoints.GetSpecificPoints(id),
                SSD_ENTITY.PROJECTILE => SSDProjectile.GetSpecificProjectile(id),
                SSD_ENTITY.POWERUP => SSDPowerup.GetSpecificPowerup(id),
                _ => null,
            };

        const int MAX_SOUND_CHANNEL = 8;
        public void PlaySound(string sound)
        {
            session.sw.PlayShaderDirectly(sound, currentSound);

            currentSound++;
            if (currentSound >= MAX_SOUND_CHANNEL) currentSound = 0;
        }
    }
}
