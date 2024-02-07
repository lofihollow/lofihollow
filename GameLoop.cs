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
    public class GameLoop {
        public const int GameWidth = 183;
        public const int GameHeight = 60;

        public static UIManager UIManager;
        public static World World;
        public static CommandManager CommandManager; 
        public static SoundManager SoundManager;
        public static ScriptManager ScriptManager;
        public static SteamManager SteamManager;

        public static bool DevMode = true;

        public static SadFont SquareFont;
        public static Random rand;

        public static void Main() {
            // Setup the engine and create the main window.
            SadConsole.Game.Create(GameWidth, GameHeight, "./fonts/ThinExtended.font");
             

            // Hook the start event so we can add consoles to the system.
            GameHost.Instance.OnStart = Init;
            GameHost.Instance.FrameUpdate += Update;
            
            // Start the game.
            SadConsole.Game.Instance.Run();

            SadConsole.Game.Instance.Dispose();
            
        } 

        private static void Update(object sender, GameHost e) {
            if (UIManager != null) {  
                if (World.Player.Clock != null && UIManager.Sidebar.Win.IsVisible && !UIManager.MapEditor.Win.IsVisible) { 
                    World.Player.TimeLastTicked++;
                    if (World.Player.TimeLastTicked >= 60) {
                        World.Player.TimeLastTicked = 0;
                        World.Player.Clock.TickTime();
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
            }
        }

        private static void Init() {
            SquareFont = (SadFont)GameHost.Instance.LoadFont("./fonts/CheepicusExtended.font"); 
            rand = new Random(); 
            SoundManager = new SoundManager(); 
            SteamManager = new SteamManager();
            World = new World(); 

            SteamManager.Start();

            UIManager = new UIManager();
            UIManager.Init(); 


            
            CommandManager = new CommandManager();
            ScriptManager = new ScriptManager();

            //  World.LoadExistingMaps(); 
            World.InitPlayer();

            Game.Instance.MonoGameInstance.Window.Title = "Lofi Hollow"; 
        } 
    }
}
