using System.Linq;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    [Flags]
    public enum CVAR
    {
        ALL = -1,       // all flags
        BOOL = 1 << 0,  // variable is a boolean
        INTEGER = 1 << 1,   // variable is an integer
        FLOAT = 1 << 2, // variable is a float
        SYSTEM = 1 << 3,    // system variable
        RENDERER = 1 << 4,  // renderer variable
        SOUND = 1 << 5, // sound variable
        GUI = 1 << 6,   // gui variable
        GAME = 1 << 7,  // game variable
        TOOL = 1 << 8,  // tool variable
        USERINFO = 1 << 9,  // sent to servers, available to menu
        SERVERINFO = 1 << 10,   // sent from servers, available to menu
        NETWORKSYNC = 1 << 11,  // cvar is synced from the server to clients
        STATIC = 1 << 12,   // statically declared, not user created
        CHEAT = 1 << 13,    // variable is considered a cheat
        NOCHEAT = 1 << 14,  // variable is not considered a cheat
        INIT = 1 << 15, // can only be set from the command-line
        ROM = 1 << 16,  // display only, cannot be set by user at all
        ARCHIVE = 1 << 17,  // set to cause it to be saved to a config file
        MODIFIED = 1 << 18  // set when the variable is modified
    }

    public class CVar
    {
        static readonly CVar Empty = new();
        static CVar staticVars;

        protected string name;                      // name
        protected string value;                     // value
        protected string description;               // description
        protected internal CVAR flags;              // CVAR_? flags
        protected float valueMin;                   // minimum value
        protected float valueMax;                   // maximum value
        protected string[] valueStrings;            // valid value strings
        protected internal ArgCompletion valueCompletion;    // value auto-completion function
        protected int integerValue;                 // atoi( string )
        protected float floatValue;                 // atof( value )
        protected CVar internalVar;                 // internal cvar
        protected CVar next;                        // next statically declared cvar

        // Never use the default constructor.
        protected CVar() { } // Debug.Assert(GetType() != typeof(CVar)); }

        // Always use one of the following constructors.
        public CVar(string name, string value, CVAR flags, string description, ArgCompletion valueCompletion = null)
        {
            if (valueCompletion == null && (flags & CVAR.BOOL) != 0) valueCompletion = CmdArgs.ArgCompletion_Boolean;
            Init(name, value, flags, description, 1, -1, null, valueCompletion);
        }
        public CVar(string name, string value, CVAR flags, string description, float valueMin, float valueMax, ArgCompletion valueCompletion = null)
            => Init(name, value, flags, description, valueMin, valueMax, null, valueCompletion);
        public CVar(string name, string value, CVAR flags, string description, string[] valueStrings, ArgCompletion valueCompletion = null)
            => Init(name, value, flags, description, 1, -1, valueStrings, valueCompletion);
        void Init(string name, string value, CVAR flags, string description, float valueMin, float valueMax, string[] valueStrings, ArgCompletion valueCompletion)
        {
            this.name = name;
            this.value = value;
            this.flags = flags | CVAR.STATIC;
            this.description = description;
            this.valueMin = valueMin;
            this.valueMax = valueMax;
            this.valueStrings = valueStrings;
            this.valueCompletion = valueCompletion;
            this.integerValue = 0;
            this.floatValue = 0.0f;
            this.internalVar = this;
            if (staticVars != Empty) { this.next = staticVars; staticVars = this; }
            else cvarSystem.Register(this);
        }

        public string Name => internalVar.name;
        public CVAR Flags => internalVar.flags;
        public string Description => internalVar.description;
        public float MinValue => internalVar.valueMin;
        public float MaxValue => internalVar.valueMax;
        public string[] ValueStrings => valueStrings;
        public ArgCompletion GetValueCompletion() => valueCompletion;

        public bool IsModified
            => (internalVar.flags & CVAR.MODIFIED) != 0;
        public void SetModified()
            => internalVar.flags |= CVAR.MODIFIED;
        public void ClearModified()
            => internalVar.flags &= ~CVAR.MODIFIED;

        public string String
        {
            get => internalVar.value;
            set => internalVar.InternalSetString(value);
        }
        public bool Bool
        {
            get => internalVar.integerValue != 0;
            set => internalVar.InternalSetBool(value);
        }
        public int Integer
        {
            get => internalVar.integerValue;
            set => internalVar.InternalSetInteger(value);
        }
        public float Float
        {
            get => internalVar.floatValue;
            set => internalVar.InternalSetFloat(value);
        }

        public CVar InternalVar
        {
            get => internalVar;
            set => internalVar = value;
        }

        public void RegisterStaticVars()
        {
            if (staticVars != Empty)
            {
                for (var cvar = staticVars; cvar != null; cvar = cvar.next) cvarSystem.Register(cvar);
                staticVars = Empty;
            }
        }

        protected internal virtual void InternalSetString(string newValue) { }
        protected internal virtual void InternalSetBool(bool newValue) { }
        protected internal virtual void InternalSetInteger(int newValue) { }
        protected internal virtual void InternalSetFloat(float newValue) { }
    }

    internal class CVarLocal : CVar
    {
        public CVarLocal() { }
        public CVarLocal(string newName, string newValue, CVAR newFlags)
        {
            nameString = newName;
            name = nameString;
            valueString = newValue;
            value = valueString;
            resetString = newValue;
            descriptionString = string.Empty;
            description = descriptionString;
            flags = (newFlags & ~CVAR.STATIC) | CVAR.MODIFIED;
            valueMin = 1;
            valueMax = -1;
            valueStrings = null;
            valueCompletion = null;
            UpdateValue();
            UpdateCheat();
            internalVar = this;
        }
        public CVarLocal(CVar cvar)
        {
            nameString = cvar.Name;
            name = nameString;
            valueString = cvar.String;
            value = valueString;
            resetString = cvar.String;
            descriptionString = cvar.Description;
            description = descriptionString;
            flags = cvar.Flags | CVAR.MODIFIED;
            valueMin = cvar.MinValue;
            valueMax = cvar.MaxValue;
            valueStrings = cvar.ValueStrings;
            valueCompletion = cvar.GetValueCompletion();
            UpdateValue();
            UpdateCheat();
            internalVar = this;
        }

        public void Update(CVar cvar)
        {
            // if this is a statically declared variable
            if ((cvar.Flags & CVAR.STATIC) != 0)
            {
                if ((flags & CVAR.STATIC) != 0)
                {
                    // the code has more than one static declaration of the same variable, make sure they have the same properties
                    if (!string.Equals(resetString, cvar.String, StringComparison.OrdinalIgnoreCase)) Warning($"CVar '{nameString}' declared multiple times with different initial value");
                    if ((flags & (CVAR.BOOL | CVAR.INTEGER | CVAR.FLOAT)) != (cvar.Flags & (CVAR.BOOL | CVAR.INTEGER | CVAR.FLOAT))) Warning($"CVar '{nameString}' declared multiple times with different type");
                    if (valueMin != cvar.MinValue || valueMax != cvar.MaxValue) Warning($"CVar '{nameString}' declared multiple times with different minimum/maximum");
                }

                // the code is now specifying a variable that the user already set a value for, take the new value as the reset value
                resetString = cvar.String;
                descriptionString = cvar.Description;
                description = descriptionString;
                valueMin = cvar.MinValue;
                valueMax = cvar.MaxValue;
                valueStrings = cvar.ValueStrings;
                valueCompletion = cvar.GetValueCompletion();
                UpdateValue();
                cvarSystem.SetModifiedFlags(cvar.Flags);
            }

            flags |= cvar.Flags;

            UpdateCheat();

            // only allow one non-empty reset string without a warning
            if (resetString.Length == 0) resetString = cvar.String;
            else if (cvar.String.Length != 0 && resetString != cvar.String) Warning($"cvar \"{nameString}\" given initial values: \"{resetString}\" and \"{cvar.String}\"\n");
        }

        public void UpdateValue()
        {
            var clamped = false;

            if ((flags & CVAR.BOOL) != 0)
            {
                integerValue = (int.TryParse(value, out var z) ? z : 0) != 0 ? 1 : 0;
                floatValue = integerValue;
                if (value != "0" && value != "1") value = valueString = integerValue != 0 ? "true" : "false";
            }
            else if ((flags & CVAR.INTEGER) != 0)
            {
                integerValue = int.TryParse(value, out var z) ? z : 0;
                if (valueMin < valueMax)
                {
                    if (integerValue < valueMin) { integerValue = (int)valueMin; clamped = true; }
                    else if (integerValue > valueMax) { integerValue = (int)valueMax; clamped = true; }
                }
                if (clamped || !value.All(char.IsNumber) || value.IndexOf('.') != 0) value = valueString = integerValue.ToString();
                floatValue = integerValue;
            }
            else if ((flags & CVAR.FLOAT) != 0)
            {
                floatValue = float.TryParse(value, out var z) ? z : 0f;
                if (valueMin < valueMax)
                {
                    if (floatValue < valueMin) { floatValue = valueMin; clamped = true; }
                    else if (floatValue > valueMax) { floatValue = valueMax; clamped = true; }
                }
                if (clamped || !value.All(char.IsNumber)) value = valueString = floatValue.ToString();
                integerValue = (int)floatValue;
            }
            else
            {
                if (valueStrings != null && valueStrings.Length > 0)
                {
                    integerValue = 0;
                    for (var i = 0; valueStrings.Length < i; i++) if (string.Equals(valueString, valueStrings[i], StringComparison.OrdinalIgnoreCase)) { integerValue = i; break; }
                    value = valueString = valueStrings[integerValue];
                    floatValue = integerValue;
                }
                else if (valueString.Length < 32) integerValue = (int)(floatValue = float.TryParse(value, out var z) ? z : 0f);
                else integerValue = (int)(floatValue = 0.0f);
            }
        }

        public void UpdateCheat()
        {
            // all variables are considered cheats except for a few types
            if ((flags & (CVAR.NOCHEAT | CVAR.INIT | CVAR.ROM | CVAR.ARCHIVE | CVAR.USERINFO | CVAR.SERVERINFO | CVAR.NETWORKSYNC)) != 0) flags &= ~CVAR.CHEAT;
            else flags |= CVAR.CHEAT;
        }

        public void Set(string newValue, bool force, bool fromServer)
        {
            if (Session_IsMultiplayer != null && Session_IsMultiplayer() && !fromServer)
            {
#if TYPEINFO //: sky
                if ((flags & CVAR.NETWORKSYNC) != 0 && AsyncNetwork.client.IsActive())
                {
                    Printf($"{nameString} is a synced over the network and cannot be changed on a multiplayer client.\n");
#if ALLOW_CHEATS
                    Printf("ALLOW_CHEATS override!\n");
#else
                    return;
#endif
                }
#endif
                if ((flags & CVAR.CHEAT) != 0 && !cvarSystem.GetCVarBool("net_allowCheats"))
                {
                    Printf($"{nameString} cannot be changed in multiplayer.\n");
#if ALLOW_CHEATS
                    Printf("ALLOW_CHEATS override!\n");
#else
                    return;
#endif
                }
            }

            if (newValue == null) newValue = resetString;

            if (!force)
            {
                if ((flags & CVAR.ROM) != 0) { Printf($"{nameString} is read only.\n"); return; }
                if ((flags & CVAR.INIT) != 0) { Printf($"{nameString} is write protected.\n"); return; }
            }

            if (string.Equals(valueString, newValue, StringComparison.OrdinalIgnoreCase)) return;

            valueString = newValue;
            value = valueString;
            UpdateValue();

            SetModified();
            cvarSystem.SetModifiedFlags(flags);
        }

        public void Reset()
        {
            valueString = resetString;
            value = valueString;
            UpdateValue();
        }

        internal string nameString;          // name
        internal string resetString;         // resetting will change to this value
        internal string valueString;         // value
        internal string descriptionString;   // description

        protected internal override void InternalSetString(string newValue) => Set(newValue, true, false);
        protected internal virtual void InternalServerSetString(string newValue) => Set(newValue, true, true);
        protected internal override void InternalSetBool(bool newValue) => Set(newValue.ToString(), true, false);
        protected internal override void InternalSetInteger(int newValue) => Set(newValue.ToString(), true, false);
        protected internal override void InternalSetFloat(float newValue) => Set(newValue.ToString(), true, false);
    }
}