﻿using SadRogue.Primitives;
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

namespace LofiHollow.UI {
    public class UIManager : ScreenObject {
        public SadConsole.UI.Colors CustomColors;

        public UI_DialogueWindow DialogueWindow;
        public UI_Sidebar Sidebar; 
        public UI_MainMenu MainMenu;
        public UI_Inventory Inventory;
        public UI_Map Map;
        public UI_Skills Skills;
        public UI_Minigame Minigames;
        public UI_Construction Construction;
        public UI_Crafting Crafting;
        public UI_Container Container;
        public UI_Help Help;
        public UI_MissionLog MissionLog;
        public UI_Photo Photo;
        public UI_Teleport Teleport;
        public UI_DevConsole DevConsole;
        public UI_Security Security;
        public UI_ScriptMini ScriptMini;
        public UI_Combat Combat;
        public UI_Feedback Feedback;
        public UI_ArtifactMaker ArtMaker;
          
        public SadConsole.Console SignConsole;
        public Window SignWindow; 
        public string signText = ""; 

        public Point targetDir = new(0, 0); 
        public string targetType = "None";
        public string selectedMenu = "None";
        public bool flying = false;
        public int RunTicks = 0;

        public bool clientAndConnected = true;
         
        public UIManager() { 
            IsVisible = true;
            IsFocused = true; 
            Parent = GameHost.Instance.Screen;
        }

        public void AddMsg(string msg) { Map.MessageLog.Add(msg); }
        public void AddMsg(ColoredString msg) { Map.MessageLog.Add(msg); }
        public void AddMsg(DecoratedString msg) { Map.MessageLog.Add(msg); }

        public override void Update(TimeSpan timeElapsed) {
            if (clientAndConnected) {
                if (MainMenu.MainMenuWindow.IsVisible) {
                    MainMenu.RenderMainMenu();
                    MainMenu.CaptureMainMenuClicks();
                } else {
                    if (GameLoop.World != null && GameLoop.World.DoneInitializing) {
                        Sidebar.RenderSidebar();
                    }

                    if (selectedMenu == "Inventory") {
                        Inventory.RenderInventory();
                        Inventory.InventoryInput();
                    } else if (selectedMenu == "Map Editor") {
                        Sidebar.MapEditorInput();
                    } else if (selectedMenu == "Skills") {
                        Skills.RenderSkills();
                        Skills.SkillInput();
                    } else if (selectedMenu == "Minigame") {
                        Minigames.RenderMinigame();
                        Minigames.MinigameInput();
                    } else if (selectedMenu == "Construction") {
                        Construction.RenderConstruction();
                        Construction.ConstructionInput();
                    } else if (selectedMenu == "Crafting") {
                        Crafting.RenderCrafting();
                        Crafting.CraftingInput();
                    } else if (selectedMenu == "Container") {
                        Container.RenderContainer();
                        Container.ContainerInput();
                    } else if (selectedMenu == "Help") {
                        Help.RenderHelp();
                        Help.HelpInput();
                    } else if (selectedMenu == "MissionLog") {
                        MissionLog.RenderMissions();
                        MissionLog.MissionInput();
                    } else if (selectedMenu == "Photo") {
                        Photo.RenderPhoto();
                        Photo.PhotoInput();
                    } else if (selectedMenu == "Teleport") {
                        Teleport.RenderTeleports();
                        Teleport.TeleportInput();
                    } else if (selectedMenu == "Security") {
                        Security.RenderSecurity();
                        Security.SecurityInput();
                    } else if (selectedMenu == "ScriptMini") {
                        ScriptMini.RenderScriptMini();
                        ScriptMini.ScriptMiniInput();
                    } else if (selectedMenu == "Combat") {
                        Combat.RenderCombat();
                        Combat.CombatInput();
                    }
                    else if (selectedMenu == "Feedback") {
                        Feedback.RenderFeedback();
                        Feedback.FeedbackInput();
                    }
                    else if (selectedMenu == "ArtMaker") {
                        ArtMaker.RenderArtifactMaker();
                        ArtMaker.ArtifactMakerInput();
                    }
                    else {
                        if (selectedMenu != "Dialogue") {
                            if (selectedMenu == "Sign")
                                RenderSign();
                            CheckFall();
                            if (!DevConsole.DevWindow.IsVisible) {
                                CheckKeyboard();
                                Sidebar.SidebarInput();
                            }
                            Map.UpdateNPCs();

                        } else {
                            DialogueWindow.RenderDialogue();
                            DialogueWindow.CaptureDialogueClicks();
                        }
                    }

                    Map.RenderOverlays();
                }
            } 
            
            base.Update(timeElapsed);
        } 

        public void Init() {
            SetupCustomColors();

            Map = new UI_Map(72, 42);
            Sidebar = new UI_Sidebar(28, GameLoop.GameHeight, ""); 
            Inventory = new UI_Inventory(GameLoop.GameWidth / 2, GameLoop.GameHeight / 2, "");
            CreateSignWindow((GameLoop.MapWidth / 2) - 1, GameLoop.MapHeight / 2, "");

            DialogueWindow = new UI_DialogueWindow(72, 42, ""); 
            MainMenu = new UI_MainMenu();
            Skills = new UI_Skills(72, 42, "[SKILLS]");
            Minigames = new UI_Minigame(72, 42, "");
            Construction = new UI_Construction(72, 42, "");
            Crafting = new UI_Crafting(72, 42, "");
            Container = new UI_Container(75, 29, "");
            Help = new UI_Help(72, 42, "");
            MissionLog = new UI_MissionLog(72, 42, "");
            Photo = new UI_Photo(31, 31, "");
            Teleport = new UI_Teleport(72, 42, "Where to?");
            Security = new UI_Security(72, 42, "Security Panel");
            DevConsole = new UI_DevConsole(72, 42, "DEV CONSOLE");
            ScriptMini = new UI_ScriptMini(72, 42, "");
            Combat = new UI_Combat(72, 42, "");
            Feedback = new UI_Feedback(72, 42, "");
            ArtMaker = new UI_ArtifactMaker(72, 42, "");

            UseMouse = true;
            selectedMenu = "MainMenu";
        } 

        private void CheckKeyboard() {
            if (selectedMenu != "Sign") { 
                if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftControl)) {
                    if (GameHost.Instance.Keyboard.IsKeyPressed(Key.W)) { 
                        CommandManager.MoveActorTo(GameLoop.World.Player, GameLoop.World.Player.Position, GameLoop.World.Player.MapPos + new Point3D(0, -1, 0)); 
                    }
                    if (GameHost.Instance.Keyboard.IsKeyPressed(Key.S)) {
                        CommandManager.MoveActorTo(GameLoop.World.Player, GameLoop.World.Player.Position, GameLoop.World.Player.MapPos + new Point3D(0, 1, 0));
                    }
                    if (GameHost.Instance.Keyboard.IsKeyPressed(Key.A)) {
                        CommandManager.MoveActorTo(GameLoop.World.Player, GameLoop.World.Player.Position, GameLoop.World.Player.MapPos + new Point3D(-1, 0, 0));
                    }
                    if (GameHost.Instance.Keyboard.IsKeyPressed(Key.D)) {
                        CommandManager.MoveActorTo(GameLoop.World.Player, GameLoop.World.Player.Position, GameLoop.World.Player.MapPos + new Point3D(1, 0, 0));
                    }
                } else {
                    if (GameLoop.World.Player.TimeLastActed + (80) < SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                        if (GameHost.Instance.Keyboard.IsKeyDown(Key.W)) {
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(Key.RightShift)) {
                                CommandManager.MoveActorBy(GameLoop.World.Player, new Point(0, -1));
                                RunTicks++;

                                if (RunTicks > 5) {
                                    GameLoop.World.Player.ExpendStamina(1);
                                    RunTicks = 0;
                                }

                                GameLoop.World.Player.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                                Map.UpdateVision();
                            }
                        }
                        else if (GameHost.Instance.Keyboard.IsKeyDown(Key.S)) {
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(Key.RightShift)) {
                                CommandManager.MoveActorBy(GameLoop.World.Player, new Point(0, 1));
                                RunTicks++;

                                if (RunTicks > 5) {
                                    GameLoop.World.Player.ExpendStamina(1);
                                    RunTicks = 0;
                                }

                                GameLoop.World.Player.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                                Map.UpdateVision();
                            }
                        }
                        else if (GameHost.Instance.Keyboard.IsKeyDown(Key.A)) {
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(Key.RightShift)) {
                                CommandManager.MoveActorBy(GameLoop.World.Player, new Point(-1, 0));
                                RunTicks++;

                                if (RunTicks > 5) {
                                    GameLoop.World.Player.ExpendStamina(1);
                                    RunTicks = 0;
                                }

                                GameLoop.World.Player.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                                Map.UpdateVision();
                            }
                        }
                        else if (GameHost.Instance.Keyboard.IsKeyDown(Key.D)) {
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(Key.RightShift)) {
                                CommandManager.MoveActorBy(GameLoop.World.Player, new Point(1, 0));
                                RunTicks++;

                                if (RunTicks > 5) {
                                    GameLoop.World.Player.ExpendStamina(1);
                                    RunTicks = 0;
                                }

                                GameLoop.World.Player.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                                Map.UpdateVision();
                            }
                        }
                    }

                    if (GameLoop.World.Player.TimeLastActed + (120) < SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {

                        if (GameHost.Instance.Keyboard.IsKeyDown(Key.W)) {
                                CommandManager.MoveActorBy(GameLoop.World.Player, new Point(0, -1)); 
                            GameLoop.World.Player.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                            Map.UpdateVision();
                        }
                        else if (GameHost.Instance.Keyboard.IsKeyDown(Key.S)) { 
                                CommandManager.MoveActorBy(GameLoop.World.Player, new Point(0, 1));
                            GameLoop.World.Player.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                            Map.UpdateVision();
                        }
                        else if (GameHost.Instance.Keyboard.IsKeyDown(Key.A)) { 
                                CommandManager.MoveActorBy(GameLoop.World.Player, new Point(-1, 0));
                            GameLoop.World.Player.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                            Map.UpdateVision();
                        }
                        else if (GameHost.Instance.Keyboard.IsKeyDown(Key.D)) { 
                                CommandManager.MoveActorBy(GameLoop.World.Player, new Point(1, 0));
                            GameLoop.World.Player.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                            Map.UpdateVision();
                        }
                    }


                    if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift) && GameHost.Instance.Keyboard.IsKeyPressed(Key.OemPeriod)) {
                        Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);

                        if (map != null) {
                            if (map.GetTile(GameLoop.World.Player.Position).Name == "Down Stairs") {
                                CommandManager.MoveActorTo(GameLoop.World.Player, GameLoop.World.Player.Position, GameLoop.World.Player.MapPos + new Point3D(0, 0, -1));
                                Map.UpdateVision();
                            } else if (map.GetTile(GameLoop.World.Player.Position).Name == "Mine Entrance") {
                                selectedMenu = "Minigame";
                                GameLoop.World.Player.MineEnteredAt = GameLoop.World.Player.Position;
                                Minigames.MineManager.MiningSetup(map.GetTile(GameLoop.World.Player.Position).MiscString);
                                if (GameLoop.SingleOrHosting())
                                    Minigames.ToggleMinigame("Mining");
                                else
                                    AddMsg("Receiving mine data from host, please wait.");
                            } else if (map.GetTile(GameLoop.World.Player.Position).Name == "Bed") {
                                GameLoop.UIManager.AddMsg(new ColoredString("You lie down to sleep...", Color.Green, Color.Black));
                                GameLoop.World.Player.Sleeping = true;

                                NetMsg sleep = new("sleep");
                                sleep.Flag = true;
                                GameLoop.SendMessageIfNeeded(sleep, false, true);
                            }
                        }
                    }

                    if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift) && GameHost.Instance.Keyboard.IsKeyPressed(Key.OemComma)) {
                        Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos); 
                        if (map != null) {
                            if (map.GetTile(GameLoop.World.Player.Position).Name == "Up Stairs") {
                                CommandManager.MoveActorTo(GameLoop.World.Player, GameLoop.World.Player.Position, GameLoop.World.Player.MapPos + new Point3D(0, 0, 1));
                                Map.UpdateVision();
                            }
                        }
                    }

                    if ((GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift) || (GameHost.Instance.Keyboard.IsKeyDown(Key.RightShift))) && GameHost.Instance.Keyboard.IsKeyPressed(Key.OemQuestion)) {
                        Help.ToggleHelp("Hotkeys");
                        GameLoop.SoundManager.PlaySound("openGuide");
                    }



                    if (GameHost.Instance.Keyboard.IsKeyReleased(Key.I)) {
                        Inventory.Toggle();
                        GameLoop.SoundManager.PlaySound("openGuide");
                    }

                    if (GameHost.Instance.Keyboard.IsKeyReleased(Key.K)) {
                        Skills.Toggle();
                        GameLoop.SoundManager.PlaySound("openGuide");
                    }

                    if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Q)) {
                        MissionLog.Toggle();
                        GameLoop.SoundManager.PlaySound("openGuide");
                    }

                    if (GameHost.Instance.Keyboard.IsKeyReleased(Key.G)) {
                        CommandManager.PickupItem(GameLoop.World.Player);
                    }

                    if (GameHost.Instance.Keyboard.IsKeyReleased(Key.C)) {
                        Crafting.SetupCrafting("Crafting", "None", 1);
                    }
                }

                if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F9)) {
                    GameLoop.World.SaveMapToFile(GameLoop.World.Player.MapPos);
                }

                if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F8)) {
                    Feedback.Toggle();
                }

                if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F3)) {
                    // Combat.Toggle();
                    // Combat.StartCombat("Plains", 1);

                    CommandManager.AddItemToInv(GameLoop.World.Player, new("lh:Copper Spade"));
                }

                if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F2)) {
                    /* foreach (KeyValuePair<CSteamID, Player> kv in GameLoop.World.otherPlayers) {
                         GameLoop.World.Player.PartyMembers.Add(kv.Key);
                     } */
                    // CommandManager.AddItemToInv(GameLoop.World.Player, new("lh:Copper SoulCam")); 
                }


                if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F1)) {
                    Help.ToggleHelp("Guide");
                    GameLoop.SoundManager.PlaySound("openGuide");
                }
                
                if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F5)) {
                    Map.LimitedVision = !Map.LimitedVision;

                    if (!Map.LimitedVision) {
                        Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);

                        if (map != null) {
                            for (int i = 0; i < map.Tiles.Length; i++) {
                                map.Tiles[i].Unshade();
                                map.Tiles[i].IsVisible = true;
                            }
                        }
                    } else {
                        Map.UpdateVision();
                    }
                }

                if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Tab)) {
                    GameLoop.World.Player.SwitchMeleeMode();
                    AddMsg(new ColoredString("Switched melee to " + GameLoop.World.Player.CombatMode + ". (" + GameLoop.World.Player.GetDamageType() + ")", Color.Yellow, Color.Black));
                }
            } else if (selectedMenu == "Sign") {
                if (GameHost.Instance.Keyboard.HasKeysPressed) {
                    selectedMenu = "None";
                    SignWindow.IsVisible = false;
                    Map.MapConsole.IsFocused = true;
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F)) {
                flying = !flying;
            }

            if (selectedMenu == "Map Editor") {
                Sidebar.MapEditorInput();
            }
        }

        private void RenderSign() {
            SignConsole.Clear();

            string[] signLines = signText.Split('|');

            int mid = SignConsole.Height / 2;
            int startY = mid - (signLines.Length / 2) - 1;

            for (int i = 0; i < signLines.Length; i++) {
                SignConsole.Print(0, startY + i, signLines[i].Align(HorizontalAlignment.Center, SignConsole.Width));
            } 
        }
         
        public void CreateSignWindow(int width, int height, string title) {
            SignWindow = new(width, height);
            SignWindow.CanDrag = false;
            SignWindow.Position = new(19, 11);

            int signConWidth = width - 2;
            int signConHeight = height - 2;

            SignConsole = new(signConWidth, signConHeight);
            SignConsole.Position = new(1, 1);
            SignWindow.Title = title.Align(HorizontalAlignment.Center, signConWidth, (char)196);


            SignWindow.Children.Add(SignConsole);
            Children.Add(SignWindow);

            SignWindow.Show();
            SignWindow.IsVisible = false;
        } 

        private void SetupCustomColors() {
            CustomColors = SadConsole.UI.Colors.CreateAnsi(); 
            CustomColors.ControlHostBackground = new AdjustableColor(Color.Black, "Black");  
            CustomColors.Lines = new AdjustableColor(Color.White, "White");  
            CustomColors.Title = new AdjustableColor(Color.White, "White");

            CustomColors.RebuildAppearances(); 
            SadConsole.UI.Themes.Library.Default.Colors = CustomColors; 
        }

        public void SignText(Point locInMap, Point3D MapLoc) {
            selectedMenu = "Sign";
            SignWindow.IsVisible = true;
            SignWindow.IsFocused = true;

            if (MapLoc == new Point3D(0, 1, 0)) { // Town Center
                if (locInMap == new Point(31, 9)) { signText = "Tom's Bar"; }
                else if (locInMap == new Point(21, 9)) { signText = "Blacksmith"; } 
                else if (locInMap == new Point(12, 20)) { signText = "Jasper's General Goods"; } 
                else if (locInMap == new Point(21, 30)) { signText = "Adventure Guild"; }
                else if (locInMap == new Point(29, 30)) { signText = "Emerose's Apothecary"; } 
                else if (locInMap == new Point(12, 24)) { signText = "Library"; }
                else if (locInMap == new Point(6, 16)) { signText = "Zephyr's Textiles"; }
                else if (locInMap == new Point(37, 7)) { signText = "Indigo -- Workshop"; }
                else if (locInMap == new Point(49, 9)) { signText = "Indigo -- Residence"; }
                else {
                    AddMsg("Sign at (" + locInMap.X + "," + locInMap.Y + ") has no text.");
                }
            } 

            else if (MapLoc == new Point3D(1, 1, 0)) {
                if (locInMap == new Point(5, 24)) { signText = "Sapphire's Bakery"; } 
                else if (locInMap == new Point(8, 20)) { signText = "Saffron's Farm Supply"; }
                else if (locInMap == new Point(10, 28)) { signText = "Cobalt's House"; }
                else if (locInMap == new Point(16, 28)) { signText = "Tak's House"; }
                else if (locInMap == new Point(21, 24)) { signText = "Clinic"; }
                else if (locInMap == new Point(28, 25)) { signText = "Courier's Guild"; }
                else if (locInMap == new Point(59, 24)) { signText = "Merchant's Guild"; }
                else if (locInMap == new Point(37, 5)) { signText = "Town Hall"; }
                else if (locInMap == new Point(21, 20)) { signText = "Maple's Hardware"; }
                else if (locInMap == new Point(43, 15)) { signText = "Meat Cute"; }
                else if (locInMap == new Point(37, 15)) { signText = "The Inn"; }
                else if (locInMap == new Point(59, 19)) { signText = "Noonbreeze Apartments"; }
                else { AddMsg("Sign at (" + locInMap.X + "," + locInMap.Y + ") has no text."); }
            }
            
            else if (MapLoc == new Point3D(-3, 1, 0)) { signText = "North -- Lake|West -- Mountain Cave|East -- Noonbreeze"; }
            else if (MapLoc == new Point3D(-3, -1, 0)) { signText = "Fisherman's Cabin"; } 
            else if (MapLoc == new Point3D(-5, 1, 0)) { signText = "Mountain Tunnel||Under Construction"; }
            else { AddMsg("Sign at (" + locInMap.X + "," + locInMap.Y + "), map (" + MapLoc.X + "," + MapLoc.Y + "," + MapLoc.Z + ")"); }
        }

        public void CheckFall() {
            if (GameLoop.World.Player.Position.X < 0 || GameLoop.World.Player.Position.Y < 0 || GameLoop.World.Player.Position.X > GameLoop.MapWidth || GameLoop.World.Player.Position.Y > GameLoop.MapHeight)
                return;
            Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);

            if (map != null) {
                if (map.GetTile(GameLoop.World.Player.Position).Name == "Space" && !flying) {
                    CommandManager.MoveActorTo(GameLoop.World.Player, GameLoop.World.Player.Position, GameLoop.World.Player.MapPos + new Point3D(0, 0, -1));
                    AddMsg("You fell down!");
                }
            }
        }
    }
}
