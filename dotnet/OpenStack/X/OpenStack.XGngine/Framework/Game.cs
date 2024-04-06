using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Render;
using System.NumericsX.OpenStack.Gngine.UI;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public struct GameReturn
    {
        public string sessionCommand;  // "map", "disconnect", "victory", etc
        public int consistencyHash;                    // used to check for network game divergence
        public int health;
        public int heartRate;
        public int vibrationLow0;
        public int vibrationLow1;
        public int vibrationHigh0;
        public int vibrationHigh1;
        public int stamina;
        public int combat;
        public bool syncNextGameFrame;                 // used when cinematics are skipped to prevent session from simulating several game frames to
    }

    public enum AllowReply
    {
        ALLOW_YES = 0,
        ALLOW_BADPASS,  // core will prompt for password and connect again
        ALLOW_NOTYET,   // core will wait with transmitted message
        ALLOW_NO        // core will abort with transmitted message
    }

    public enum EscReply
    {
        ESC_IGNORE = 0, // do nothing
        ESC_MAIN,       // start main menu GUI
        ESC_GUI         // set an explicit GUI
    }

    public interface IGame
    {
        // Initialize the game for the first time.
        void Init();

        // Shut down the entire game.
        void Shutdown();

        // Set the local client number. Distinguishes listen ( == 0 ) / dedicated ( == -1 )
        void SetLocalClient(int clientNum);

        void SetVRClientInfo(VRClientInfo vrClientInfo);
        void CheckRenderCvars();

        void EvaluateVRMoveMode(in Vector3 viewangles, Usercmd cmd, int buttonCurrentlyClicked, float snapTurn);
        bool CMDButtonsAttackCall(int teleportCanceled);
        bool CMDButtonsPhysicalCrouch();

        bool InCinematic { get; }

        // Release the mouse when the PDA is open
        bool IsPDAOpen { get; }

        //bool AnimatorGetJointTransform(Animator animator, JointHandle jointHandle, int currentTime, in Vector3 offset, in Matrix3x3 axis);
        const bool IsVR = true;

        // Sets the user info for a client.
        // if canModify is true, the game can modify the user info in the returned dictionary pointer, server will forward the change back canModify is never true on network client
        Dictionary<string, string> SetUserInfo(int clientNum, Dictionary<string, string> userInfo, bool isClient, bool canModify);

        // Retrieve the game's userInfo dict for a client.
        Dictionary<string, string> GetUserInfo(int clientNum);

        // The game gets a chance to alter userinfo before they are emitted to server.
        void ThrottleUserInfo();

        // Sets the serverinfo at map loads and when it changes.
        void SetServerInfo(Dictionary<string, string> serverInfo);

        // The session calls this before moving the single player game to a new level.
        Dictionary<string, string> GetPersistentPlayerInfo(int clientNum);

        // The session calls this right before a new level is loaded.
        void SetPersistentPlayerInfo(int clientNum, Dictionary<string, string> playerInfo);

        // Loads a map and spawns all the entities.
        void InitFromNewMap(string mapName, IRenderWorld renderWorld, ISoundWorld soundWorld, bool isServer, bool isClient, int randseed);

        // Loads a map from a savegame file.
        bool InitFromSaveGame(string mapName, IRenderWorld renderWorld, ISoundWorld soundWorld, VFile saveGameFile);

        // Saves the current game state, the session may have written some data to the file already.
        void SaveGame(VFile saveGameFile);

        // Shut down the current map.
        void MapShutdown();

        // Caches media referenced from in key/value pairs in the given dictionary.
        void CacheDictionaryMedia(Dictionary<string, string> dict);

        // Spawns the player entity to be used by the client.
        void SpawnPlayer(int clientNum);

        // Runs a game frame, may return a session command for level changing, etc
        GameReturn RunFrame(Usercmd[] clientCmds);

        // Indicates to the game library that the frame has now ended
        void EndFrame();

        // Makes rendering and sound system calls to display for a given clientNum.
        bool Draw(int clientNum);

        // Let the game do it's own UI when ESCAPE is used
        EscReply HandleESC(IUserInterface gui);

        // get the games menu if appropriate ( multiplayer )
        IUserInterface StartMenu { get; }

        // When the game is running it's own UI fullscreen, GUI commands are passed through here return NULL once the fullscreen UI mode should stop, or "main" to go to main menu
        string HandleGuiCommands(string menuCommand);

        // main menu commands not caught in the engine are passed here
        void HandleMainMenuCommands(string menuCommand, IUserInterface gui);

        // Early check to deny connect.
        AllowReply ServerAllowClient(int numClients, string ip, string guid, string password, out string reason);

        // Connects a client.
        void ServerClientConnect(int clientNum, string guid);

        // Spawns the player entity to be used by the client.
        void ServerClientBegin(int clientNum);

        // Disconnects a client and removes the player entity from the game.
        void ServerClientDisconnect(int clientNum);

        // Writes initial reliable messages a client needs to recieve when first joining the game.
        void ServerWriteInitialReliableMessages(int clientNum);

        // Writes a snapshot of the server game state for the given client.
        void ServerWriteSnapshot(int clientNum, int sequence, BitMsg msg, byte[] clientInPVS, int numPVSClients);

        // Patches the network entity states at the server with a snapshot for the given client.
        bool ServerApplySnapshot(int clientNum, int sequence);

        // Processes a reliable message from a client.
        void ServerProcessReliableMessage(int clientNum, BitMsg msg);

        // Reads a snapshot and updates the client game state.
        void ClientReadSnapshot(int clientNum, int sequence, int gameFrame, int gameTime, int dupeUsercmds, int aheadOfServer, BitMsg msg);

        // Patches the network entity states at the client with a snapshot.
        bool ClientApplySnapshot(int clientNum, int sequence);

        // Processes a reliable message from the server.
        void ClientProcessReliableMessage(int clientNum, BitMsg msg);

        // Runs prediction on entities at the client.
        GameReturn ClientPrediction(int clientNum, Usercmd[] clientCmds, bool lastPredictFrame);

        // Used to manage divergent time-lines
        void SelectTimeGroup(int timeGroup);
        int GetTimeGroupTime(int timeGroup);

        void GetBestGameType(string map, string gametype, out string buf);

        // Returns a summary of stats for a given client
        void GetClientStats(int clientNum, byte[] data, int len);

        // Switch a player to a particular team
        void SwitchTeam(int clientNum, int team);

        bool DownloadRequest(string ip, string guid, string paks, string urls);

        void GetMapLoadingGUI(string gui);

        // Added by Emile
        bool InGameGuiActive { get; }
        bool ObjectiveSystemActive { get; }
    }
}