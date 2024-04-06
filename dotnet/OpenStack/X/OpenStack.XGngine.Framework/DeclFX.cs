using System.Collections.Generic;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public enum FX : int
    {
        NONE = -1,
        LIGHT = 0,
        PARTICLE,
        DECAL,
        MODEL,
        SOUND,
        SHAKE,
        ATTACHLIGHT,
        ATTACHENTITY,
        LAUNCH,
        SHOCKWAVE
    }

    // single fx structure
    public class FXSingleAction
    {
        public FX type;
        public int sibling;

        public string data;
        public string name;
        public string fire;

        public float delay;
        public float duration;
        public float restart;
        public float size;
        public float fadeInTime;
        public float fadeOutTime;
        public float shakeTime;
        public float shakeAmplitude;
        public float shakeDistance;
        public float shakeImpulse;
        public float lightRadius;
        public float rotate;
        public float random1;
        public float random2;

        public Vector3 lightColor;
        public Vector3 offset;
        public Matrix3x3 axis;

        public bool soundStarted;
        public bool shakeStarted;
        public bool shakeFalloff;
        public bool shakeIgnoreMaster;
        public bool bindParticles;
        public bool explicitAxis;
        public bool noshadows;
        public bool particleTrackVelocity;
        public bool trackOrigin;
    }

    // grouped fx structures
    public class DeclFX : Decl
    {
        readonly List<FXSingleAction> events = new();
        string joint;

        public override int Size => 0;
        public override string DefaultDefinition =>
@"{
	{
		duration 5
		model _default
	}
}";

        public override bool Parse(string text)
        {
            Lexer src = new();
            src.LoadMemory(text, FileName, LineNum);
            src.Flags = DeclBase.DECL_LEXER_FLAGS;
            src.SkipUntilString("{");

            // scan through, identifying each individual parameter
            while (true)
            {
                if (!src.ReadToken(out var token)) break;
                if (token == "}") break;
                if (string.Equals(token, "bindto", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); joint = token; continue; }
                if (token == "{") { ParseSingleFXAction(src, out var action); events.Add(action); continue; }
            }

            if (src.HadError) { src.Warning($"FX decl '{Name}' had a parse error"); return false; }
            return true;
        }

        public override void FreeData()
            => events.Clear();

        public override void Print()
        {
            DeclFX list = this;

            common.Printf($"{list.events.Count} events\n");
            for (var i = 0; i < list.events.Count; i++)
                switch (list.events[i].type)
                {
                    case FX.LIGHT: common.Printf($"FX_LIGHT {list.events[i].data}\n"); break;
                    case FX.PARTICLE: common.Printf($"FX_PARTICLE {list.events[i].data}\n"); break;
                    case FX.MODEL: common.Printf($"FX_MODEL {list.events[i].data}\n"); break;
                    case FX.SOUND: common.Printf($"FX_SOUND {list.events[i].data}\n"); break;
                    case FX.DECAL: common.Printf($"FX_DECAL {list.events[i].data}\n"); break;
                    case FX.SHAKE: common.Printf($"FX_SHAKE {list.events[i].data}\n"); break;
                    case FX.ATTACHLIGHT: common.Printf($"FX_ATTACHLIGHT {list.events[i].data}\n"); break;
                    case FX.ATTACHENTITY: common.Printf($"FX_ATTACHENTITY {list.events[i].data}\n"); break;
                    case FX.LAUNCH: common.Printf($"FX_LAUNCH {list.events[i].data}\n"); break;
                    case FX.SHOCKWAVE: common.Printf($"FX_SHOCKWAVE {list.events[i].data}\n"); break;
                }
        }

        public override void List()
            => common.Printf($"{Name}, {events.Count} stages\n");

        void ParseSingleFXAction(Lexer src, out FXSingleAction FXAction)
        {
            FXAction = new FXSingleAction
            {
                type = FX.NONE,
                sibling = -1,

                data = "<none>",
                name = "<none>",
                fire = "<none>",

                delay = 0f,
                duration = 0f,
                restart = 0f,
                size = 0f,
                fadeInTime = 0f,
                fadeOutTime = 0f,
                shakeTime = 0f,
                shakeAmplitude = 0f,
                shakeDistance = 0f,
                shakeFalloff = false,
                shakeImpulse = 0f,
                shakeIgnoreMaster = false,
                lightRadius = 0f,
                rotate = 0f,
                random1 = 0f,
                random2 = 0f,

                lightColor = Vector3.origin,
                offset = Vector3.origin,
                axis = Matrix3x3.identity,

                bindParticles = false,
                explicitAxis = false,
                noshadows = false,
                particleTrackVelocity = false,
                trackOrigin = false,
                soundStarted = false
            };

            while (true)
            {
                if (!src.ReadToken(out var token)) break;
                if (token == "}") break;
                if (string.Equals(token, "shake", StringComparison.OrdinalIgnoreCase))
                {
                    FXAction.type = FX.SHAKE;
                    FXAction.shakeTime = src.ParseFloat(); src.ExpectTokenString(",");
                    FXAction.shakeAmplitude = src.ParseFloat(); src.ExpectTokenString(",");
                    FXAction.shakeDistance = src.ParseFloat(); src.ExpectTokenString(",");
                    FXAction.shakeFalloff = src.ParseBool(); src.ExpectTokenString(",");
                    FXAction.shakeImpulse = src.ParseFloat();
                    continue;
                }
                if (string.Equals(token, "noshadows", StringComparison.OrdinalIgnoreCase)) { FXAction.noshadows = true; continue; }
                if (string.Equals(token, "name", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); FXAction.name = token; continue; }
                if (string.Equals(token, "fire", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); FXAction.fire = token; continue; }
                if (string.Equals(token, "random", StringComparison.OrdinalIgnoreCase))
                {
                    FXAction.random1 = src.ParseFloat(); src.ExpectTokenString(",");
                    FXAction.random2 = src.ParseFloat();
                    FXAction.delay = 0f; // check random
                    continue;
                }
                if (string.Equals(token, "delay", StringComparison.OrdinalIgnoreCase)) { FXAction.delay = src.ParseFloat(); continue; }
                if (string.Equals(token, "rotate", StringComparison.OrdinalIgnoreCase)) { FXAction.rotate = src.ParseFloat(); continue; }
                if (string.Equals(token, "duration", StringComparison.OrdinalIgnoreCase)) { FXAction.duration = src.ParseFloat(); continue; }
                if (string.Equals(token, "trackorigin", StringComparison.OrdinalIgnoreCase)) { FXAction.trackOrigin = src.ParseBool(); continue; }
                if (string.Equals(token, "restart", StringComparison.OrdinalIgnoreCase)) { FXAction.restart = src.ParseFloat(); continue; }
                if (string.Equals(token, "fadeIn", StringComparison.OrdinalIgnoreCase)) { FXAction.fadeInTime = src.ParseFloat(); continue; }
                if (string.Equals(token, "fadeOut", StringComparison.OrdinalIgnoreCase)) { FXAction.fadeOutTime = src.ParseFloat(); continue; }
                if (string.Equals(token, "size", StringComparison.OrdinalIgnoreCase)) { FXAction.size = src.ParseFloat(); continue; }
                if (string.Equals(token, "offset", StringComparison.OrdinalIgnoreCase))
                {
                    FXAction.offset.x = src.ParseFloat(); src.ExpectTokenString(",");
                    FXAction.offset.y = src.ParseFloat(); src.ExpectTokenString(",");
                    FXAction.offset.z = src.ParseFloat();
                    continue;
                }
                if (string.Equals(token, "axis", StringComparison.OrdinalIgnoreCase))
                {
                    Vector3 v;
                    v.x = src.ParseFloat(); src.ExpectTokenString(",");
                    v.y = src.ParseFloat(); src.ExpectTokenString(",");
                    v.z = src.ParseFloat();
                    v.Normalize();
                    FXAction.axis = v.ToMat3();
                    FXAction.explicitAxis = true;
                    continue;
                }
                if (string.Equals(token, "angle", StringComparison.OrdinalIgnoreCase))
                {
                    Angles a;
                    a.pitch = src.ParseFloat(); src.ExpectTokenString(",");
                    a.yaw = src.ParseFloat(); src.ExpectTokenString(",");
                    a.roll = src.ParseFloat();
                    FXAction.axis = a.ToMat3();
                    FXAction.explicitAxis = true;
                    continue;
                }
                if (string.Equals(token, "uselight", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    FXAction.data = token;
                    for (var i = 0; i < events.Count; i++)
                        if (string.Equals(events[i].name, FXAction.data, StringComparison.OrdinalIgnoreCase))
                        {
                            FXAction.sibling = i;
                            FXAction.lightColor = events[i].lightColor;
                            FXAction.lightRadius = events[i].lightRadius;
                        }
                    FXAction.type = FX.LIGHT;

                    // precache the light material
                    declManager.FindMaterial(FXAction.data);
                    continue;
                }
                if (string.Equals(token, "attachlight", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    FXAction.data = token;
                    FXAction.type = FX.ATTACHLIGHT;

                    // precache it
                    declManager.FindMaterial(FXAction.data);
                    continue;
                }
                if (string.Equals(token, "attachentity", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    FXAction.data = token;
                    FXAction.type = FX.ATTACHENTITY;

                    // precache the model
                    renderModelManager.FindModel(FXAction.data);
                    continue;
                }
                if (string.Equals(token, "launch", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    FXAction.data = token;
                    FXAction.type = FX.LAUNCH;

                    // precache the entity def
                    declManager.FindType(DECL.ENTITYDEF, FXAction.data);
                    continue;
                }
                if (string.Equals(token, "useModel", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    FXAction.data = token;
                    for (var i = 0; i < events.Count; i++)
                        if (string.Equals(events[i].name, FXAction.data, StringComparison.OrdinalIgnoreCase))
                            FXAction.sibling = i;
                    FXAction.type = FX.MODEL;

                    // precache the model
                    renderModelManager.FindModel(FXAction.data);
                    continue;
                }
                if (string.Equals(token, "light", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    FXAction.data = token; src.ExpectTokenString(",");
                    FXAction.lightColor.x = src.ParseFloat(); src.ExpectTokenString(",");
                    FXAction.lightColor.y = src.ParseFloat(); src.ExpectTokenString(",");
                    FXAction.lightColor.z = src.ParseFloat(); src.ExpectTokenString(",");
                    FXAction.lightRadius = src.ParseFloat();
                    FXAction.type = FX.LIGHT;

                    // precache the light material
                    declManager.FindMaterial(FXAction.data);
                    continue;
                }
                if (string.Equals(token, "model", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    FXAction.data = token;
                    FXAction.type = FX.MODEL;

                    // precache it
                    renderModelManager.FindModel(FXAction.data);
                    continue;
                }
                if (string.Equals(token, "particle", StringComparison.OrdinalIgnoreCase))
                {   // FIXME: now the same as model
                    src.ReadToken(out token);
                    FXAction.data = token;
                    FXAction.type = FX.PARTICLE;

                    // precache it
                    renderModelManager.FindModel(FXAction.data);
                    continue;
                }
                if (string.Equals(token, "decal", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    FXAction.data = token;
                    FXAction.type = FX.DECAL;

                    // precache it
                    declManager.FindMaterial(FXAction.data);
                    continue;
                }
                if (string.Equals(token, "particleTrackVelocity", StringComparison.OrdinalIgnoreCase)) { FXAction.particleTrackVelocity = true; continue; }
                if (string.Equals(token, "sound", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    FXAction.data = token;
                    FXAction.type = FX.SOUND;

                    // precache it
                    declManager.FindSound(FXAction.data);
                    continue;
                }
                if (string.Equals(token, "ignoreMaster", StringComparison.OrdinalIgnoreCase)) { FXAction.shakeIgnoreMaster = true; continue; }
                if (string.Equals(token, "shockwave", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    FXAction.data = token;
                    FXAction.type = FX.SHOCKWAVE;

                    // precache the entity def
                    declManager.FindType(DECL.ENTITYDEF, FXAction.data);
                    continue;
                }

                src.Warning("FX File: bad token");
                continue;
            }
        }
    }
}
