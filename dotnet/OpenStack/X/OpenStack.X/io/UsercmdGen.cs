using System.Runtime.InteropServices;

namespace System.NumericsX.OpenStack
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Usercmd
    {
        // usercmd_t->button bits
        public const byte BUTTON_ATTACK = 1 << 0;
        public const byte BUTTON_RUN = 1 << 1;
        public const byte BUTTON_ZOOM = 1 << 2;
        public const byte BUTTON_SCORES = 1 << 3;
        public const byte BUTTON_MLOOK = 1 << 4;
        public const byte BUTTON_JUMP = 1 << 5;
        public const byte BUTTON_CROUCH = 1 << 6;
        public const byte BUTTON_USE = 1 << 7;

        // usercmd_t->impulse commands
        const int IMPULSE_0 = 0;            // weap 0
        const int IMPULSE_1 = 1;            // weap 1
        const int IMPULSE_2 = 2;            // weap 2
        const int IMPULSE_3 = 3;            // weap 3
        const int IMPULSE_4 = 4;            // weap 4
        const int IMPULSE_5 = 5;            // weap 5
        const int IMPULSE_6 = 6;            // weap 6
        const int IMPULSE_7 = 7;            // weap 7
        const int IMPULSE_8 = 8;            // weap 8
        const int IMPULSE_9 = 9;            // weap 9
        const int IMPULSE_10 = 10;          // weap 10
        const int IMPULSE_11 = 11;          // weap 11
        const int IMPULSE_12 = 12;          // weap 12
        const int IMPULSE_13 = 13;          // weap reload
        const int IMPULSE_14 = 14;          // weap next
        const int IMPULSE_15 = 15;          // weap prev
        const int IMPULSE_16 = 16;          // <unused>
        const int IMPULSE_17 = 17;          // ready to play ( toggles ui_ready )
        const int IMPULSE_18 = 18;          // center view
        const int IMPULSE_19 = 19;          // show PDA/INV/MAP
        const int IMPULSE_20 = 20;          // toggle team ( toggles ui_team )
        const int IMPULSE_21 = 21;          // <unused>
        const int IMPULSE_22 = 22;          // spectate
        const int IMPULSE_23 = 23;          // <unused>
        const int IMPULSE_24 = 24;          // <unused>
        const int IMPULSE_25 = 25;          // <unused>
        const int IMPULSE_26 = 26;          // Carl: Fists
        const int IMPULSE_27 = 27;          // Chainsaw
        const int IMPULSE_28 = 28;          // quick 0
        const int IMPULSE_29 = 29;          // quick 1
        const int IMPULSE_30 = 30;          // quick 2
        const int IMPULSE_31 = 31;          // quick 3

        // Koz
        const int IMPULSE_32 = 32;          // reset HMD/Body orientation
        const int IMPULSE_33 = 33;          // toggle lasersight
        const int IMPULSE_34 = 34;          // comfort turn right
        const int IMPULSE_35 = 35;          // comfort turn left
        const int IMPULSE_36 = 36;          // toggle hud
        const int IMPULSE_37 = 37;          // free
        const int IMPULSE_38 = 38;          // walk in place
        const int IMPULSE_39 = 39;          // freelook
        const int IMPULSE_40 = 40;          // Vehicle
        const int IMPULSE_41 = 41;          // click to move
                                            // Koz end
        const int IMPULSE_44 = 44;          // Carl: computer, freeze program
        const int IMPULSE_PAUSE = IMPULSE_44;
        const int IMPULSE_45 = 45;          // Carl: computer, resume program
        const int IMPULSE_RESUME = IMPULSE_45;

        // usercmd_t->flags
        internal const int UCF_IMPULSE_SEQUENCE = 0x0001;    // toggled every time an impulse command is sent

        public int gameFrame;                       // frame number
        public int gameTime;                        // game time
        public int duplicateCount;                  // duplication count for networking
        public byte buttons;                        // buttons
        public char forwardmove;                    // forward/backward movement
        public char rightmove;                      // left/right movement
        public char upmove;                         // up/down movement
        public short angles0;                       // view angles
        public short angles1;                       // view angles
        public short angles2;                       // view angles
        public short mx;                            // mouse delta x
        public short my;                            // mouse delta y
        public sbyte impulse;                       // impulse command
        public byte flags;                          // additional flags
        public int sequence;                        // just for debugging

        /// <summary>
        /// on big endian systems, byte swap the shorts and ints
        /// </summary>
        public void ByteSwap()
        {
            angles0 = Platform.LittleShort(angles0);
            angles1 = Platform.LittleShort(angles1);
            angles2 = Platform.LittleShort(angles2);
            sequence = Platform.LittleInt(sequence);
        }

        public override bool Equals(object obj)
        {
            var rhs = (Usercmd)obj;
            return buttons == rhs.buttons &&
                forwardmove == rhs.forwardmove &&
                rightmove == rhs.rightmove &&
                upmove == rhs.upmove &&
                angles0 == rhs.angles0 &&
                angles1 == rhs.angles1 &&
                angles2 == rhs.angles2 &&
                impulse == rhs.impulse &&
                flags == rhs.flags &&
                mx == rhs.mx &&
                my == rhs.my;
        }
        public override int GetHashCode()
            => base.GetHashCode();
    }

    public enum INHIBIT
    {
        SESSION = 0,
        ASYNC
    }

    public interface IUsercmd
    {
        const int USERCMD_HZ = 60;
        public const int USERCMD_MSEC = 1000 / USERCMD_HZ;
        //static int USERCMD_MSEC() => (1000 / (renderSystem != null ? renderSystem.GetRefresh() : 60));
        public const int MAX_BUFFERED_USERCMD = 64;

        // Sets up all the cvars and console commands.
        void Init();

        // Prepares for a new map.
        void InitForNewMap();

        // Shut down.
        void Shutdown();

        // Clears all key states and face straight.
        void Clear();

        // Clears view angles.
        void ClearAngles();

        // When the console is down or the menu is up, only emit default usercmd, so the player isn't moving around.
        // Each subsystem (session and game) may want an inhibit will OR the requests.
        void InhibitUsercmd(INHIBIT subsystem, bool inhibit);

        // Returns a buffered command for the given game tic.
        ref Usercmd TicCmd(int ticNumber);

        // Called async at regular intervals.
        void UsercmdInterrupt();

        // Set a value that can safely be referenced by UsercmdInterrupt() for each key binding.
        int CommandStringUsercmdData(string cmdString);

        // Returns the number of user commands.
        int NumUserCommand { get; }

        // Returns the name of a user command via index.
        string GetUserCommandName(int index);

        // Continuously modified, never reset. For full screen guis.
        void MouseState(out int x, out int y, out int button, out bool down);

        // Directly sample a button.
        int ButtonState(int key);

        // Directly sample a keystate.
        int KeyState(int key);

        // Directly sample a usercmd.
        ref Usercmd GetDirectUsercmd();
    }
}