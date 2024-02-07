using SadRogue.Primitives;
using SadConsole; 
using System;
using Key = SadConsole.Input.Keys;
using SadConsole.UI;
using Color = SadRogue.Primitives.Color; 
using LofiHollow.Managers; 
using LofiHollow.DataTypes;
using System.Collections.Generic;
using LofiHollow.Entities;
using Steamworks;
using LofiHollow.Minigames.Electronics;
using SadConsole.Input;
using System.Linq;

namespace LofiHollow.UI {
    public class UIManager : ScreenObject {
        public SadConsole.UI.Colors CustomColors;
         
        public UI_Sidebar Sidebar; 
        public UI_MenuBackdrop MenuBackdrop;
        public UI_Inventory Inventory; 
        public UI_Skills Skills;
        public UI_Minigame Minigames; 
        public UI_Help Help; 
        public UI_ScriptMini ScriptMini;  
        public UI_Nametag Nametag;
        public UI_Options Options;
        public UI_Navigation Nav;
        public UI_MessageLog MsgLog;

        public UI_MainMenu MainMenu;
        public UI_CharGen CharGen;
        public UI_LoadFile LoadFile;
        public UI_FakeStart FakeStart;
        public UI_MapEditor MapEditor;
        
        public GamePocket GamePocket;


        public Dictionary<string, InstantUI> Interfaces = new();  

        public bool clientAndConnected = true;
         
        public UIManager() { 
            IsVisible = true;
            IsFocused = true; 
            Parent = GameHost.Instance.Screen;
        }

        public InstantUI? GetUI(string name) {
            if (Interfaces.ContainsKey(name))
                return Interfaces[name];
            return null;
        }

        public void ToggleUI(string name) {
            if (Interfaces.ContainsKey(name)) {
                Interfaces[name].Win.IsVisible = !Interfaces[name].Win.IsVisible;

                if (Interfaces[name].Win.IsVisible) {
                    Interfaces[name].Win.IsFocused = true;
                } 
            }
        }


        public void AddMsg(string msg) { MsgLog.Log.Add(new(new ColoredString(msg))); MsgLog.Top = MsgLog.Log.Count - 1; }
        public void AddMsg(ColoredString msg) { MsgLog.Log.Add(new(msg)); MsgLog.Top = MsgLog.Log.Count - 1; }
        public void AddMsg(DecoratedString msg) { MsgLog.Log.Add(msg); MsgLog.Top = MsgLog.Log.Count - 1; }

        public override void Update(TimeSpan timeElapsed) {
            foreach (KeyValuePair<string, InstantUI> kv in Interfaces) {
                if (kv.Value.Win.IsVisible) {
                    kv.Value.Update();
                    kv.Value.Input();
                    kv.Value.Win.IsFocused = true;
                }
            }

            CheckKeyboard();
            Helper.ClearKeys();
            base.Update(timeElapsed);
        }

        public void Init() {
            SetupCustomColors();
             
            Sidebar = new UI_Sidebar(40, GameLoop.GameHeight); 

            Inventory = new UI_Inventory(GameLoop.GameWidth / 2, GameLoop.GameHeight / 2, ""); 
             
            MenuBackdrop = new UI_MenuBackdrop();
            MainMenu = new UI_MainMenu(20, 28);
            CharGen = new UI_CharGen(102, 42);
            LoadFile = new UI_LoadFile(20, 28);
            FakeStart = new UI_FakeStart(50, 10);


            Skills = new UI_Skills(72, 42, "Skills");
            Minigames = new UI_Minigame(72, 42, "Minigames"); 
            Help = new UI_Help(72, 42, "Help");  
            ScriptMini = new UI_ScriptMini(72, 42, "ScriptMini");  
            Nametag = new UI_Nametag(20, 5, "Nametag");
            Options = new UI_Options(72, 42, "Options");
            GamePocket = new GamePocket(34, 50, "GamePocket");

            Nav = new UI_Navigation(143, 40);
            MsgLog = new UI_MessageLog(143, 20);
            MapEditor = new UI_MapEditor(80, 20);

            UseMouse = true; 

            MainMenu.Win.Position += new Point(0, 4);
            ToggleUI("MainMenu"); 
        } 


        public void HandleMenuChange(bool toMainMenu) {
            if (toMainMenu) {
                foreach (var kv in Interfaces) {
                    kv.Value.Win.IsVisible = false;
                }

                
                MenuBackdrop.MenuBackdrop.IsVisible = true;
                MenuBackdrop.MenuBackdrop.IsFocused = true;
                
            } else {
                foreach (var kv in Interfaces) {
                    kv.Value.Win.IsVisible = false;
                }

                ToggleUI("Navigation");
                ToggleUI("MessageLog");
                ToggleUI("Sidebar");
                MenuBackdrop.MenuBackdrop.IsVisible = false;
            }

        }


        private void CheckKeyboard() { 
            if (GameLoop.DevMode) {
                if (Helper.HotkeyDown(Key.F2)) {
                    ToggleUI("MapEditor");
                }

                if (Helper.HotkeyDown(Key.F3)) {
                    List<KeyValuePair<string, Location>> locs = GameLoop.World.atlas.ToList();
                     
                    Location randomLoc = locs[GameLoop.rand.Next(locs.Count - 1)].Value;

                    while (!randomLoc.InDevLottery) {
                        randomLoc = locs[GameLoop.rand.Next(locs.Count - 1)].Value;
                    } 

                    AddMsg(randomLoc.DisplayName);
                    GameLoop.World.Player.NavLoc = randomLoc.ID;
                }

                if (Helper.HotkeyDown(Key.F8)) {
                    GameLoop.World.Player.Zeri += 100;
                }
            }

            if (!MapEditor.Win.IsVisible) {

                if (Helper.EitherShift() && Helper.HotkeyDown(Key.OemQuestion)) {
                    Help.ToggleHelp("Hotkeys");
                    GameLoop.SoundManager.PlaySound("openGuide");
                }



                if (Helper.HotkeyDown(Key.I)) {
                    ToggleUI("Inventory");
                    GameLoop.SoundManager.PlaySound("openGuide");
                }

                if (Helper.HotkeyDown(Key.K)) {
                    GameLoop.SteamManager.PullSkillBoards();
                    ToggleUI("Skills");
                    GameLoop.SoundManager.PlaySound("openGuide");
                }

                if (Helper.HotkeyDown(Key.Q)) {
                    ToggleUI("Missions");
                    GameLoop.SoundManager.PlaySound("openGuide");
                }

                if (Helper.HotkeyDown(Key.F1)) {
                    Help.ToggleHelp("Guide");
                    GameLoop.SoundManager.PlaySound("openGuide");
                }

                if (Helper.HotkeyDown(Key.Escape)) {
                    //ToggleUI("Options");
                    AddMsg(GameHost.Instance.GameRunningTotalTime.TotalMilliseconds.ToString());
                }
            }
        } 
         
        private void SetupCustomColors() {
            CustomColors = SadConsole.UI.Colors.CreateAnsi(); 
            CustomColors.ControlHostBackground = new AdjustableColor(Color.Black, "Black");  
            CustomColors.Lines = new AdjustableColor(Color.White, "White");  
            CustomColors.Title = new AdjustableColor(Color.White, "White");

            CustomColors.RebuildAppearances(); 
            SadConsole.UI.Themes.Library.Default.Colors = CustomColors; 
        }
         
    }
}
