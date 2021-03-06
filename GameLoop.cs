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
        public static FirebaseManager FirebaseManager;

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
                    if (World.Player.Clock != null && UIManager.Sidebar.SidebarWindow.IsVisible) {
                        if ((NetworkManager == null && !UIManager.Help.Win.IsVisible) || (NetworkManager != null && NetworkManager.isHost)) {
                            World.Player.TimeLastTicked++;
                            if (World.Player.TimeLastTicked >= 60) {
                                World.Player.TimeLastTicked = 0;
                                World.Player.Clock.TickTime();
                            }
                        }

                        Map farm = Helper.ResolveMap(new Point3D("Overworld", -1, 0, 0));

                        if (farm != null) {
                            foreach(Entity ent in farm.Entities.Items) {
                                if (ent is FarmAnimal farmAnimal) {
                                    farmAnimal.Update();
                                }
                            }
                        }

                        if (World.Player.NoonbreezeApt != null) {
                            foreach(Entity ent in World.Player.NoonbreezeApt.map.Entities.Items) {
                                if (ent is FarmAnimal ani) {
                                    ani.Update();
                                }
                            }
                        }

                        foreach (KeyValuePair<SteamId, Player> kv in World.otherPlayers) {
                            if (kv.Value.NoonbreezeApt != null) {
                                foreach (Entity ent in kv.Value.NoonbreezeApt.map.Entities.Items) {
                                    if (ent is FarmAnimal ani) {
                                        ani.Update();
                                    }
                                }
                            }
                        }
                    }
                }

                SoundManager.PickMusic(); 
            }

            if (SoundManager != null)
                SoundManager.UpdateSounds();


            /*
            if (UIManager != null && UIManager.DevConsole != null && UIManager.DevConsole.DevWindow.IsVisible) {
                UIManager.DevConsole.RenderDevConsole();
                UIManager.DevConsole.DevConsoleInput();
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(SadConsole.Input.Keys.OemTilde)) {
                UIManager.DevConsole.Toggle();
            } */

            if (SteamManager != null) {
                SteamManager.Update(); 
                SteamManager.MoneyAchieves();
            }
        }

        private static void Init() {
        //    SadConsole.Game.Instance.SetSplashScreens(new SadConsole.SplashScreens.PCBoot());
            rand = new Random(); 
            SoundManager = new SoundManager(); 
            SteamManager = new SteamManager();
            World = new World(); 

            SteamManager.Start();

            UIManager = new UIManager();
            UIManager.Init();
            FirebaseManager = new();


            
            CommandManager = new CommandManager();
            ScriptManager = new ScriptManager();

            //  World.LoadExistingMaps();
            World.LoadMapAt(new Point3D(1, 3, 0));
            World.InitPlayer();

            Game.Instance.MonoGameInstance.Window.Title = "Lofi Hollow"; 
        }

        public static bool EitherShift() {
            if (GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.RightShift))
                return true;
            return false;
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
            if (NetworkManager != null && SteamClient.IsValid) {
                if (!OnlyIfHost || (OnlyIfHost && NetworkManager.isHost)) {
                    if (AddOwnID)
                        msg.senderID = SteamClient.SteamId;

                    if (OnlyToID == 0) {
                        NetworkManager.BroadcastMsg(msg);
                    } else if (OnlyToID == 1) {
                        var lobbyOwnerId = NetworkManager.GetLobbyOwner();
                        msg.recipient = lobbyOwnerId;
                        NetworkManager.BroadcastMsg(msg); 
                    } else {
                        msg.recipient = (SteamId) OnlyToID;
                        NetworkManager.BroadcastMsg(msg); 
                    }
                }
            }
        }
    }
}
