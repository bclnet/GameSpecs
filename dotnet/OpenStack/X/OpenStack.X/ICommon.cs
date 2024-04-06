using System.Collections.Generic;
using System.IO;

namespace System.NumericsX.OpenStack
{
    public delegate object FunctionPointer(object arg); // needs to be cast to/from real type!

    public enum CallbackType
    {
        // called on reloadImages and vid_restart commands (before anything "real" happens)
        // expecting callback to be like void cb(void* userarg, const idCmdArgs& cmdArgs)
        // where cmdArgs contains the command+arguments that was called
        CB_ReloadImages = 1,
    }

    public enum FunctionType
    {
        // the function's signature is bool fn(void) - no arguments.
        // it returns true if we're currently running the doom3 demo not relevant for mods, only for game/ aka base.dll/base.so/...
        FT_IsDemo = 1,
    }

    [Flags]
    public enum EDITOR
    {
        NONE = 0,
        RADIANT = 1 << 1,
        GUI = 1 << 2,
        DEBUGGER = 1 << 3,
        SCRIPT = 1 << 4,
        LIGHT = 1 << 5,
        SOUND = 1 << 6,
        DECL = 1 << 7,
        AF = 1 << 8,
        PARTICLE = 1 << 9,
        PDA = 1 << 10,
        AAS = 1 << 11,
        MATERIAL = 1 << 12
    }

    /// <summary>
    /// ICommon
    /// </summary>
    public interface ICommon
    {
        /// <summary>
        /// Initialize everything.
        /// if the OS allows, pass argc/argv directly (without executable name) otherwise pass the command line in a single string (without executable name)
        /// </summary>
        /// <param name="argc">The argc.</param>
        /// <param name="argv">The argv.</param>
        void Init(int argc, Span<string> argv);

        /// <summary>
        /// Shuts down everything.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Shuts down everything.
        /// </summary>
        void Quit();

        /// <summary>
        /// Returns true if common initialization is complete.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </returns>
        bool IsInitialized { get; }

        /// <summary>
        /// Called repeatedly as the foreground thread for rendering and game logic.
        /// </summary>
        void Frame();

        /// <summary>
        /// Called repeatedly by blocking function calls with GUI interactivity.
        /// </summary>
        /// <param name="execCmd">if set to <c>true</c> [execute command].</param>
        /// <param name="network">if set to <c>true</c> [network].</param>
        void GUIFrame(bool execCmd, bool network);

        /// <summary>
        /// Called 60 times a second from a background thread for sound mixing, and input generation. Not called until idCommon::Init() has completed.
        /// </summary>
        void Async();

        /// <summary>
        /// Checks for and removes command line "+set var arg" constructs. If match is NULL, all set commands will be executed, otherwise
        /// only a set with the exact name.  Only used during startup. set once to clear the cvar from +set for early init code
        /// </summary>
        /// <param name="match">The match.</param>
        /// <param name="once">if set to <c>true</c> [once].</param>
        void StartupVariable(string match, bool once);

        /// <summary>
        /// Initializes a tool with the given dictionary.
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <param name="dict">The dictionary.</param>
        void InitTool(EDITOR tool, Dictionary<string, object> dict);

        /// <summary>
        /// Activates or deactivates a tool.
        /// </summary>
        /// <param name="active">if set to <c>true</c> [active].</param>
        void ActivateTool(bool active);

        /// <summary>
        /// Writes the user's configuration to a file
        /// </summary>
        /// <param name="filename">The filename.</param>
        void WriteConfigToFile(string filename);

        /// <summary>
        /// Writes cvars with the given flags to a file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="setCmd">The set command.</param>
        void WriteFlaggedCVarsToFile(string filename, int flags, string setCmd);

        /// <summary>
        /// Begins redirection of console output to the given buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="buffersize">The buffersize.</param>
        /// <param name="flush">The flush.</param>
        void BeginRedirect(byte[] buffer, int buffersize, Action<string> flush);

        /// <summary>
        /// Stops redirection of console output.
        /// </summary>
        void EndRedirect();

        /// <summary>
        /// Update the screen with every message printed.
        /// </summary>
        /// <param name="set">if set to <c>true</c> [set].</param>
        void SetRefreshOnPrint(bool set);

        /// <summary>
        /// Prints message to the console, which may cause a screen update if com_refreshOnPrint is set.
        /// </summary>
        /// <param name="fmt">The FMT.</param>
        /// <param name="args">The arguments.</param>
        void Printf(string fmt, params object[] args);

        ///// <summary>
        ///// Same as Printf, with a more usable API - Printf pipes to this.
        ///// </summary>
        ///// <param name="fmt">The FMT.</param>
        ///// <param name="arg">The argument.</param>
        //void VPrintf(string fmt, object[] arg);

        /// <summary>
        /// Prints message that only shows up if the "developer" cvar is set, and NEVER forces a screen update, which could cause reentrancy problems.
        /// </summary>
        /// <param name="fmt">The FMT.</param>
        /// <param name="args">The arguments.</param>
        void DPrintf(string fmt, params object[] args);

        /// <summary>
        /// Prints WARNING %s message and adds the warning message to a queue for printing later on.
        /// </summary>
        /// <param name="fmt">The FMT.</param>
        /// <param name="args">The arguments.</param>
        void Warning(string fmt, params object[] args);

        /// <summary>
        /// Prints WARNING %s message in yellow that only shows up if the "developer" cvar is set.
        /// </summary>
        /// <param name="fmt">The FMT.</param>
        /// <param name="args">The arguments.</param>
        void DWarning(string fmt, params object[] args);

        /// <summary>
        /// Prints all queued warnings.
        /// </summary>
        void PrintWarnings();

        /// <summary>
        /// Removes all queued warnings.
        /// </summary>
        /// <param name="reason">The reason.</param>
        void ClearWarnings(string reason);

        /// <summary>
        /// Issues a throw. Normal errors just abort to the game loop, which is appropriate for media or dynamic logic errors.
        /// </summary>
        /// <param name="fmt">The FMT.</param>
        /// <param name="args">The arguments.</param>
        void Error(string fmt, params object[] args);

        /// <summary>
        /// Fatal errors quit all the way to a system dialog box, which is appropriate for static internal errors or cases where the system may be corrupted.
        /// </summary>
        /// <param name="fmt">The FMT.</param>
        /// <param name="args">The arguments.</param>
        void FatalError(string fmt, params object[] args);

        /// <summary>
        /// Returns a pointer to the dictionary with language specific strings.
        /// </summary>
        /// <returns></returns>
        //Dictionary<string, string> LanguageDict { get; }
        string LanguageDictGetString(string key);


        /// <summary>
        /// Returns key bound to the command
        /// </summary>
        /// <returns></returns>
        string KeysFromBinding(string bind);

        /// <summary>
        /// Returns the binding bound to the key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        string BindingFromKey(string key);

        /// <summary>
        /// Directly sample a button.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        int ButtonState(int key);

        /// <summary>
        /// Directly sample a keystate.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        int KeyState(int key);

        int GetFrameNumber();

        /// <summary>
        /// Haptic Feedback
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="low">The low.</param>
        /// <param name="high">The high.</param>
        void Vibrate(int channel, float low, float high);

        /* Some Mods (like Ruiner and DarkMod when it still was a mod) used "SourceHook" to override Doom3 Methods to call their own code before the original method
         * was executed.. this is super ugly and probably not super portable either.
         *
         * So let's offer something that's slightly less ugly: A function pointer based interface to provide similar (but known!) hacks.
         * For example, Ruiner used SourceHook to intercept idCmdSystem::BufferCommandText() and recreate some cooked rendering data in case reloadImages or vid_restart was executed.
         * Now, instead of doing ugly hacks with SourceHook, Ruiner can just call
         *   common->SetCallback( idCommon::CB_ReloadImages,
         *                        (idCommon::FunctionPointer)functionToCall,
         *                        (void*)argForFunctionToCall );
         *
         * (the Mod needs to check if SetCallback() returned true; if it didn't the used version of dhewm3 doesn't support the given CallBackType and the Mod must either error out
         *  or handle the case that the callback doesn't work)
         *
         * Of course this means that for every new SourceHook hack a Mod (that's ported to dhewm3) uses, a corresponding entry must be added to enum CallbackType and it must be handled,
         * which implies that the Mod will only properly work with the latest dhewm3 git code or the next release..
         * I guess most mods don't need this hack though, so I think it's feasible.
         *
         * Note that this allows adding new types of callbacks without breaking the API and ABI between dhewm3 and the Game DLLs; the alternative would be something like
         * idCommon::RegisterReloadImagesCallback(), and maybe other similar methods later, which would break the ABI and API each time and all Mods would have to be adjusted, even if
         * they don't even need that functionality (because they never needed SourceHook or similar).
         *
         * Similar to SetCallback() I've also added GetAdditionalFunction() to get a function pointer from dhewm3 that Mods can call (and that's not exported via the normal interface classes).
         * Right now it's only used for a Doom3 Demo specific hack only relevant for base.dll (not for Mods)
         */

        /// <summary>
        /// When a game DLL is unloaded the callbacks are automatically removed from the Engine so you usually don't have to worry about that; but you can call this with cb = NULL
        /// and userArg = NULL to remove a callback manually (e.g. if userArg refers to an object you deleted)
        /// </summary>
        /// <param name="cbt">The CBT.</param>
        /// <param name="cb">The cb.</param>
        /// <param name="userArg">The user argument.</param>
        /// <returns>returns true if setting the callback was successful, else false</returns>
        bool SetCallback(CallbackType cbt, FunctionPointer cb, object userArg);

        /// <summary>
        /// Gets the additional function.
        /// </summary>
        /// <param name="ft">The ft.</param>
        /// <param name="out_fnptr">out_fnptr will be the function (you'll have to cast it probably)</param>
        /// <param name="out_userArg">out_userArg will be an argument you have to pass to the function, if appropriate (else NULL)</param>
        /// <returns>returns true if that function is available in this version of dhewm3</returns>
        bool GetAdditionalFunction(FunctionType ft, out FunctionPointer out_fnptr, out object out_userArg);
    }
}