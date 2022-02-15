using System;
using SadConsole;
using Console = SadConsole.Console;
using SadRogue.Primitives;
using LofiHollow.DataTypes;


using LofiHollow.UI;
using LofiHollow.Managers;
using LofiHollow.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steamworks;

namespace LofiHollow {
    class GameLoop {
        public const int GameWidth = 100;
        public const int GameHeight = 60;  
        public const int MapWidth = 70;
        public const int MapHeight = 40;

        public static UIManager UIManager;
        public static World World;
        public static CommandManager CommandManager;
        public static NetworkManager NetworkManager;
        public static SoundManager SoundManager;
        public static ScriptManager ScriptManager;
        public static SteamManager SteamManager;


        public static Random rand;

        public static void Main() {
            // Setup the engine and create the main window.
            SadConsole.Game.Create(GameWidth, GameHeight, "./fonts/Cheepicus48.font");
             

            // Hook the start event so we can add consoles to the system.
            GameHost.Instance.OnStart = Init;
            GameHost.Instance.FrameUpdate += Update;
            
            // Start the game.
            SadConsole.Game.Instance.Run();

            SadConsole.Game.Instance.Dispose();
            
        } 

        private static void Update(object sender, GameHost e) {
            if (UIManager != null) {
                if (NetworkManager == null || NetworkManager.isHost) {
                    if (World.Player.Clock != null) {
                        World.Player.TimeLastTicked++;
                        if (World.Player.TimeLastTicked >= 60) {
                            World.Player.TimeLastTicked = 0;
                            World.Player.Clock.TickTime();
                        }
                    }
                }

                SoundManager.PickMusic(); 
            }

            if (SoundManager != null)
                SoundManager.UpdateSounds();

            if (UIManager != null && UIManager.DevConsole != null && UIManager.DevConsole.DevWindow.IsVisible) {
                UIManager.DevConsole.RenderDevConsole();
                UIManager.DevConsole.DevConsoleInput();
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(SadConsole.Input.Keys.OemTilde)) {
                UIManager.DevConsole.Toggle();
            }

            if (SteamManager != null) {
                SteamManager.Update();
            }
        }

        private static void Init() { 
            rand = new Random();
            SteamManager = new SteamManager();
            SteamManager.Start();
            World = new World();
            UIManager = new UIManager();
            UIManager.Init();

            
            CommandManager = new CommandManager(); 
            SoundManager = new SoundManager();
            ScriptManager = new ScriptManager();

            //  World.LoadExistingMaps();
            World.LoadMapAt(new Point3D(1, 3, 0));
            World.InitPlayer(); 

            SadConsole.Game.Instance.MonoGameInstance.Window.Title = "Lofi Hollow"; 
        }

        public static bool CheckFlag(string flag) {
            if (flag == "farm") {
                if ((NetworkManager != null && NetworkManager.isHost) || (NetworkManager == null)) {
                    return World.Player.OwnsFarm;
                } else if (NetworkManager != null && !NetworkManager.isHost) {
                    return NetworkManager.HostOwnsFarm;
                }
            }


            return false;
        }

        public static bool SingleOrHosting() {
            if (NetworkManager == null)
                return true;
            return NetworkManager.isHost;
        }

        public static void SendMessageIfNeeded(NetMsg msg, bool OnlyIfHost, bool AddOwnID, ulong OnlyToID = 0) {
            if (NetworkManager != null) {
                if (!OnlyIfHost || (OnlyIfHost && NetworkManager.isHost)) {
                    if (AddOwnID)
                        msg.senderID = Steamworks.SteamUser.GetSteamID();

                    if (OnlyToID == 0) {
                        NetworkManager.BroadcastMsg(msg);
                    } else if (OnlyToID == 1) {
                        var lobbyOwnerId = SteamMatchmaking.GetLobbyOwner((CSteamID) NetworkManager.currentLobby);
                        msg.recipient = lobbyOwnerId;
                        NetworkManager.BroadcastMsg(msg); 
                    } else {
                        msg.recipient = (CSteamID) OnlyToID;
                        NetworkManager.BroadcastMsg(msg); 
                    }
                }
            }
        }
    }
}
