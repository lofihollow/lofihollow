using LofiHollow.Entities;
using LofiHollow.EntityData;
using LofiHollow.Managers;
using Newtonsoft.Json;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Sidebar {
        public Window SidebarWindow;
        public SadConsole.Console SidebarConsole; 
        public MessageLogWindow BattleLog;

        public int tileIndex = 0;
        public int monIndex = 0;
        public int hotbarSelect = 0;
        public int ChargeBar = 0;
        public bool Harvesting = false;
        public Point LureSpot;

        public FishingLure LocalLure;

        public Point tileSelected = new(0, 0);

        public Dictionary<Point3D, MinimapTile> minimap = new();



        public UI_Sidebar(int width, int height, string title) {
            int sidebarConsoleWidth = width - 2;
            int sidebarConsoleHeight = height - 2;

            SidebarConsole = new(sidebarConsoleWidth, sidebarConsoleHeight);
            SidebarWindow = new(width, height);
            SidebarWindow.CanDrag = false;
            SidebarWindow.Position = new(72, 0); 

            SidebarConsole.Position = new Point(1, 1);
            SidebarWindow.Title = title.Align(HorizontalAlignment.Center, sidebarConsoleWidth, (char)196);


            SidebarWindow.Children.Add(SidebarConsole);
            GameLoop.UIManager.Children.Add(SidebarWindow);

            SidebarWindow.Show();

            SidebarWindow.IsVisible = false;

            BattleLog = new MessageLogWindow(18, 11, "Combat");
            SidebarWindow.Children.Add(BattleLog);
            BattleLog.Show();
            BattleLog.Position = new Point(0, 0);
            BattleLog.IsVisible = false;
        }

        public void SidebarInput() { 
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F11)) {
                if (GameLoop.UIManager.selectedMenu == "Map Editor") {
                    GameLoop.UIManager.selectedMenu = "None";
                } else {
                    GameLoop.UIManager.selectedMenu = "Map Editor";
                }
            }

            if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                if (hotbarSelect + 1 < GameLoop.World.Player.Inventory.Length)
                    hotbarSelect++;
            } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                if (hotbarSelect > 0)
                    hotbarSelect--;
            }

            bool madeCoinThisFrame = false;
            Point sidebarMouse = new MouseScreenObjectState(SidebarConsole, GameHost.Instance.Mouse).CellPosition;
            if (sidebarMouse.X > 0) { // Clicked in Sidebar
                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (sidebarMouse.Y >= 48) {
                        int slot = sidebarMouse.Y - 48;
                        if (slot >= 0 && slot <= 9)
                            CommandManager.UnequipItem(GameLoop.World.Player, slot);
                    }

                    

                    if (sidebarMouse.X < 12) {
                        if (sidebarMouse.Y == 13) {
                            if (GameLoop.World.Player.CopperCoins > 0) { 
                                GameLoop.World.Player.CopperCoins -= 1;
                                Item copper = new("lh:Copper Coin");
                                CommandManager.AddItemToInv(GameLoop.World.Player, copper);
                                madeCoinThisFrame = true;
                            }
                        }

                        if (sidebarMouse.Y == 14) {
                            if (GameLoop.World.Player.SilverCoins > 0) {
                                GameLoop.World.Player.SilverCoins--;
                                Item silver = new("lh:Silver Coin");
                                CommandManager.AddItemToInv(GameLoop.World.Player, silver);
                                madeCoinThisFrame = true;
                            }
                        }
                    } else { 
                        if (sidebarMouse.Y == 13) {
                            if (GameLoop.World.Player.GoldCoins > 0) {
                                GameLoop.World.Player.GoldCoins--;
                                Item gold = new("lh:Gold Coin");
                                CommandManager.AddItemToInv(GameLoop.World.Player, gold);
                                madeCoinThisFrame = true;
                            }
                        }

                        if (sidebarMouse.Y == 14) {
                            if (GameLoop.World.Player.JadeCoins > 0) {
                                GameLoop.World.Player.JadeCoins--;
                                Item jade = new("lh:Jade Coin");
                                CommandManager.AddItemToInv(GameLoop.World.Player, jade);
                                madeCoinThisFrame = true;
                            }
                        } 
                    }

                }
            }


            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F6)) {
                for (int i = 0; i < GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles.Length; i++) {
                    TileBase tile = GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i];
                    if (tile.Name == "Dirt") {
                        tile.ForegroundR = 152;
                        tile.ForegroundG = 118;
                        tile.ForegroundB = 84;
                        tile.UpdateAppearance();
                    }
                }
            }

            Point mapPos = new MouseScreenObjectState(GameLoop.UIManager.Map.MapConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Mouse.LeftButtonDown) {
                if (GameLoop.World.Player.Inventory[hotbarSelect].Name == "(EMPTY)") { 

                    int distance = GoRogue.Lines.Get(new GoRogue.Coord(mapPos.X, mapPos.Y), new GoRogue.Coord(GameLoop.World.Player.Position.X, GameLoop.World.Player.Position.Y)).Count();
                    if (distance < 5) {
                        if (mapPos.X >= 0 && mapPos.X <= GameLoop.MapWidth && mapPos.Y >= 0 && mapPos.Y <= GameLoop.MapHeight) { 
                            TileBase tile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos); 
                            if (tile.Plant != null) {
                                if (tile.Plant.ProduceName == GameLoop.World.Player.Inventory[hotbarSelect].Name || GameLoop.World.Player.Inventory[hotbarSelect].Name == "(EMPTY)") {
                                    if (tile.Plant.CurrentStage != 0 && tile.Plant.CurrentStage != -1)
                                        Harvesting = true;
                                } 

                                tile.Plant.Harvest(GameLoop.World.Player);
                                if (tile.Plant.CurrentStage == -1) {
                                    TileBase tilled = new(8);
                                    tilled.Name = "Tilled Dirt";
                                    tilled.TileGlyph = 34;
                                    tilled.UpdateAppearance(); 


                                    GameLoop.World.maps[GameLoop.World.Player.MapPos].SetTile(mapPos, tilled);
                                    GameLoop.UIManager.Map.MapConsole.SetEffect(mapPos.X, mapPos.X, null);
                                    tile.UpdateAppearance();

                                    string json = JsonConvert.SerializeObject(tilled, Formatting.Indented);
                                    GameLoop.SendMessageIfNeeded(new string[] { "updateTile", mapPos.X.ToString(), mapPos.Y.ToString(), GameLoop.World.Player.MapPos.ToString(), json }, false, false);
                                     
                                } else {
                                    tile.UpdateAppearance();

                                    string json = JsonConvert.SerializeObject(tile, Formatting.Indented);
                                    GameLoop.SendMessageIfNeeded(new string[] { "updateTile", mapPos.X.ToString(), mapPos.Y.ToString(), GameLoop.World.Player.MapPos.ToString(), json }, false, false);

                                }
                            }
                        }
                    }
                }
            }

            if (!GameHost.Instance.Mouse.LeftButtonDown) {
                Harvesting = false;
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                int tempCat = GameLoop.World.Player.Inventory[hotbarSelect].ItemCategory;

                int distToClick = GoRogue.Lines.Get(new GoRogue.Coord(mapPos.X, mapPos.Y), new GoRogue.Coord(GameLoop.World.Player.Position.X, GameLoop.World.Player.Position.Y)).Count();
                 if (distToClick < 5) {
                    TileBase clickedTile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos);
                    if (clickedTile.MiscString != "") {
                        string[] split = clickedTile.MiscString.Split(",");
                        if (split[0] == "Skill") {
                            if (split.Length > 2)
                                GameLoop.UIManager.Crafting.SetupCrafting(split[1], split[2], Int32.Parse(split[3]));
                            else if (split.Length > 1)
                                GameLoop.UIManager.Crafting.SetupCrafting(split[1], split[2], 1);
                            else
                                GameLoop.UIManager.Crafting.SetupCrafting(split[1], "None", 1);
                        }
                    }

                    if (clickedTile.Lock != null) {
                        GameLoop.World.maps[GameLoop.World.Player.MapPos].ToggleLock(mapPos, GameLoop.World.Player.MapPos);
                    }
                }

                if (tempCat == 13 && !madeCoinThisFrame) { // Clicked with a coin
                    if (GameLoop.World.Player.Inventory[hotbarSelect].Name == "Copper Coin") {
                        GameLoop.World.Player.CopperCoins++;
                        CommandManager.RemoveOneItem(GameLoop.World.Player, hotbarSelect);
                    }

                    if (GameLoop.World.Player.Inventory[hotbarSelect].Name == "Silver Coin") {
                        GameLoop.World.Player.SilverCoins++;
                        CommandManager.RemoveOneItem(GameLoop.World.Player, hotbarSelect);
                    }

                    if (GameLoop.World.Player.Inventory[hotbarSelect].Name == "Gold Coin") {
                        GameLoop.World.Player.GoldCoins++;
                        CommandManager.RemoveOneItem(GameLoop.World.Player, hotbarSelect);
                    }

                    if (GameLoop.World.Player.Inventory[hotbarSelect].Name == "Jade Coin") {
                        GameLoop.World.Player.JadeCoins++;
                        CommandManager.RemoveOneItem(GameLoop.World.Player, hotbarSelect);
                    }
                }

                if (tempCat == 4 || tempCat == 9) {
                    if (GameLoop.World.Player.MapPos == new Point3D(-1, 0, 0) && !GameLoop.CheckFlag("farm")) {
                        GameLoop.UIManager.AddMsg(new ColoredString("You need to buy this land from the town hall first.", Color.Red, Color.Black));
                    } else if (GameLoop.World.Player.MapPos != new Point3D(-1, 0, 0)) {
                        GameLoop.UIManager.AddMsg(new ColoredString("You probably shouldn't do that here.", Color.Red, Color.Black));
                    }
                }

                if (tempCat == 5) { // Clicked with a hammer
                    int distance = GoRogue.Lines.Get(new GoRogue.Coord(mapPos.X, mapPos.Y), new GoRogue.Coord(GameLoop.World.Player.Position.X, GameLoop.World.Player.Position.Y)).Count();
                    if (distance < 5) {
                        if (mapPos.X >= 0 && mapPos.X <= GameLoop.MapWidth && mapPos.Y >= 0 && mapPos.Y <= GameLoop.MapHeight) {
                            if (GameLoop.World.Player.MapPos == new Point3D(-1, 0, 0)) {
                                if (GameLoop.CheckFlag("farm")) {
                                    TileBase tile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos);

                                    if (GameLoop.UIManager.Construction.SelectedConstructible != -1) {
                                        if (GameLoop.World.Player.Skills["Construction"].Level >= GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].RequiredLevel) {
                                            if (UI_Construction.CheckValidConstruction(GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible])) {
                                                Constructible con = GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible];
                                                if (tile.Name.ToLower().Contains("floor") || tile.Name.ToLower().Contains("grass")) {
                                                    if (tile.Name != con.Name) {
                                                        for (int i = 0; i < GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].MaterialsNeeded.Count; i++) {
                                                            int needed = GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].MaterialsNeeded[i].ItemQuantity;

                                                            for (int j = 0; j < GameLoop.World.Player.Inventory.Length; j++) {
                                                                if (needed > 0) {
                                                                    if (con.MaterialsNeeded[i].Name.Contains("Nails")) {
                                                                        if (GameLoop.World.Player.Inventory[j].Name == con.MaterialsNeeded[i].Name) {
                                                                            if (GameLoop.World.Player.Inventory[j].SubID >= con.MaterialsNeeded[i].SubID) {
                                                                                if (GameLoop.World.Player.Inventory[j].ItemQuantity > con.MaterialsNeeded[i].ItemQuantity) {
                                                                                    GameLoop.World.Player.Inventory[j].ItemQuantity -= con.MaterialsNeeded[i].ItemQuantity;
                                                                                    needed -= con.MaterialsNeeded[i].ItemQuantity;
                                                                                } else if (GameLoop.World.Player.Inventory[j].ItemQuantity == con.MaterialsNeeded[i].ItemQuantity) {
                                                                                    GameLoop.World.Player.Inventory[j] = new("lh:(EMPTY)");
                                                                                    needed = 0;
                                                                                } else {
                                                                                    needed -= GameLoop.World.Player.Inventory[j].ItemQuantity;
                                                                                    GameLoop.World.Player.Inventory[j] = new("lh:(EMPTY)");
                                                                                }
                                                                            }
                                                                        }
                                                                    } else {
                                                                        if (GameLoop.World.Player.Inventory[j].Name == con.MaterialsNeeded[i].Name) {
                                                                            if (GameLoop.World.Player.Inventory[j].SubID == con.MaterialsNeeded[i].SubID) {
                                                                                if (GameLoop.World.Player.Inventory[j].ItemQuantity > con.MaterialsNeeded[i].ItemQuantity) {
                                                                                    GameLoop.World.Player.Inventory[j].ItemQuantity -= con.MaterialsNeeded[i].ItemQuantity;
                                                                                    needed -= con.MaterialsNeeded[i].ItemQuantity;
                                                                                } else if (GameLoop.World.Player.Inventory[j].ItemQuantity == con.MaterialsNeeded[i].ItemQuantity) {
                                                                                    GameLoop.World.Player.Inventory[j] = new("lh:(EMPTY)");
                                                                                    needed = 0;
                                                                                } else {
                                                                                    needed -= GameLoop.World.Player.Inventory[j].ItemQuantity;
                                                                                    GameLoop.World.Player.Inventory[j] = new("lh:(EMPTY)");
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }


                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).Name = con.Name;
                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).TileGlyph = con.Glyph;
                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).ForegroundR = con.ForegroundR;
                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).ForegroundG = con.ForegroundG;
                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).ForegroundB = con.ForegroundB;
                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).Dec = con.Dec;
                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).IsBlockingLOS = con.BlocksLOS;
                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).IsBlockingMove = con.BlocksMove;
                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).Container = con.Container;
                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).Lock = con.Lock;
                                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).UpdateAppearance();

                                                        MissionManager.Increment("Construction", con.Name, 1);

                                                        GameLoop.World.Player.ExpendStamina(1);

                                                        GameLoop.UIManager.Map.UpdateVision();

                                                        GameLoop.World.Player.Skills["Construction"].GrantExp(con.ExpGranted);

                                                        string json =  JsonConvert.SerializeObject(GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos), Formatting.Indented);
                                                        GameLoop.SendMessageIfNeeded(new string[] { "updateTile", mapPos.X.ToString(), mapPos.Y.ToString(), GameLoop.World.Player.MapPos.ToString(), json }, false, false);
                                                        
                                                    } else {
                                                        GameLoop.UIManager.AddMsg("That's already been built there!");
                                                    }
                                                } else {
                                                    GameLoop.UIManager.AddMsg("There's already something there.");
                                                }
                                            } else {
                                                GameLoop.UIManager.AddMsg("That needs: " + GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].Materials);
                                            }
                                        } else {
                                            GameLoop.UIManager.AddMsg("You need " + GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].RequiredLevel + " Construction to build that.");
                                        }
                                    } else {
                                        if (tile.Name.ToLower().Contains("floor") || tile.Name.ToLower().Contains("grass") || tile.TileID == 8) {
                                            GameLoop.UIManager.AddMsg("Right click to select something to build first.");
                                        } else {
                                            if (!tile.DeconstructFlag) {
                                                GameLoop.UIManager.AddMsg("Click again to deconstruct the " + tile.Name + ".");
                                                GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).DeconstructFlag = true;
                                            } else {
                                                GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[mapPos.X + (mapPos.Y * GameLoop.MapWidth)] = new TileBase(0);
                                                GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[mapPos.X + (mapPos.Y * GameLoop.MapWidth)].UpdateAppearance();
                                                GameLoop.UIManager.Map.UpdateVision();

                                                string json = JsonConvert.SerializeObject(GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[mapPos.X + (mapPos.Y * GameLoop.MapWidth)], Formatting.Indented);
                                                GameLoop.SendMessageIfNeeded(new string[] { "updateTile", mapPos.X.ToString(), mapPos.Y.ToString(), GameLoop.World.Player.MapPos.ToString(), json }, false, false);
                                            }
                                        }
                                    }
                                } else {
                                    GameLoop.UIManager.AddMsg("You need to buy the Farm Permit first.");
                                }
                            } else {
                                GameLoop.UIManager.AddMsg("You probably shouldn't build here.");
                            }
                        }
                    }
                }

                if (tempCat == 12) { // Clicked with a permit in-hand
                    if (GameLoop.World.Player.Inventory[hotbarSelect].Name == "Farm Permit") { // Permit is for the farm
                        if (GameLoop.SingleOrHosting()) {
                            if (!GameLoop.CheckFlag("farm")) {
                                GameLoop.World.Player.OwnsFarm = true;
                                CommandManager.RemoveOneItem(GameLoop.World.Player, hotbarSelect);
                                GameLoop.UIManager.AddMsg(new ColoredString("You unlocked the farm!", Color.Cyan, Color.Black));

                                if (!GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0)))
                                    GameLoop.World.LoadMapAt(new Point3D(-1, 0, 0));

                                GameLoop.World.maps[new Point3D(-1, 0, 0)].MonsterWeights.Clear();
                                GameLoop.World.maps[new Point3D(-1, 0, 0)].MaximumMonsters = 0;
                                GameLoop.World.maps[new Point3D(-1, 0, 0)].MinimumMonsters = 0;
                                GameLoop.World.maps[new Point3D(-1, 0, 0)].MinimapTile.name = "Your Farm";

                                GameLoop.SendMessageIfNeeded(new string[] { "usedPermit", "farm" }, false, false);
                            } else {
                                GameLoop.UIManager.AddMsg(new ColoredString("You already own the farm!", Color.Red, Color.Black));
                            }
                        } else if (!GameLoop.SingleOrHosting()) {
                            if (!GameLoop.CheckFlag("farm")) {
                                if (GameLoop.World.Player.Inventory[hotbarSelect].Weight != 0f) {
                                    GameLoop.UIManager.AddMsg("Use again to unlock for the HOST player (not yourself!)");
                                    GameLoop.World.Player.Inventory[hotbarSelect].Weight = 0f;
                                } else {
                                    GameLoop.UIManager.AddMsg(new ColoredString("You unlocked the farm!", Color.Cyan, Color.Black));

                                    if (!GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0)))
                                        GameLoop.World.LoadMapAt(new Point3D(-1, 0, 0));

                                    GameLoop.World.maps[new Point3D(-1, 0, 0)].MonsterWeights.Clear();
                                    GameLoop.World.maps[new Point3D(-1, 0, 0)].MaximumMonsters = 0;
                                    GameLoop.World.maps[new Point3D(-1, 0, 0)].MinimumMonsters = 0;
                                    GameLoop.World.maps[new Point3D(-1, 0, 0)].MinimapTile.name = "Your Farm";

                                    GameLoop.SendMessageIfNeeded(new string[] { "usedPermit", "farm" }, false, false); 
                                    CommandManager.RemoveOneItem(GameLoop.World.Player, hotbarSelect);
                                }
                            } else {
                                GameLoop.UIManager.AddMsg(new ColoredString("The host already owns the farm!", Color.Red, Color.Black));
                            }
                        }
                    }
                }

                if (GameLoop.World.Player.Inventory[hotbarSelect].Name == "Medium Backpack") { // Used Medium Backpack
                    CommandManager.RemoveOneItem(GameLoop.World.Player, hotbarSelect);
                    List<Item> previous = GameLoop.World.Player.Inventory.ToList();
                    GameLoop.World.Player.Inventory = new Item[18];

                    for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                        GameLoop.World.Player.Inventory[i] = new("lh:(EMPTY)");
                    }

                    for (int i = 0; i < previous.Count; i++) {
                        GameLoop.World.Player.Inventory[i] = previous[i];
                    }
                }

                if (GameLoop.World.Player.Inventory[hotbarSelect].Name == "Large Backpack") { // Used Medium Backpack
                    CommandManager.RemoveOneItem(GameLoop.World.Player, hotbarSelect);
                    List<Item> previous = GameLoop.World.Player.Inventory.ToList();
                    GameLoop.World.Player.Inventory = new Item[27];

                    for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                        GameLoop.World.Player.Inventory[i] = new("lh:(EMPTY)");
                    }

                    for (int i = 0; i < previous.Count; i++) {
                        GameLoop.World.Player.Inventory[i] = previous[i];
                    }
                }
            }


            if (GameHost.Instance.Mouse.LeftButtonDown) {
                if (GameLoop.World.Player.Inventory[hotbarSelect].Name != "(EMPTY)") {
                    if (GameLoop.World.Player.Inventory[hotbarSelect].ItemCategory == 7) { // Fishing rod
                        if (ChargeBar < 100) {
                            ChargeBar++;
                        }
                    }

                    if (GameLoop.World.Player.Inventory[hotbarSelect].ItemCategory == 4) { // Clicked with a hoe
                        int distance = GoRogue.Lines.Get(new GoRogue.Coord(mapPos.X, mapPos.Y), new GoRogue.Coord(GameLoop.World.Player.Position.X, GameLoop.World.Player.Position.Y)).Count();
                        if (distance < 5) {
                            if (mapPos.X >= 0 && mapPos.X <= GameLoop.MapWidth && mapPos.Y >= 0 && mapPos.Y <= GameLoop.MapHeight) {
                                TileBase tile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos);
                                if (tile.Name == "Dirt") {
                                    if (GameLoop.World.Player.MapPos == new Point3D(-1, 0, 0) && GameLoop.CheckFlag("farm")) {
                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).Name = "Tilled Dirt";
                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).TileGlyph = 34;
                                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos).UpdateAppearance();
                                        GameLoop.World.Player.ExpendStamina(1);

                                        string json = JsonConvert.SerializeObject(GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos), Formatting.Indented);
                                        GameLoop.SendMessageIfNeeded(new string[] { "updateTile", mapPos.X.ToString(), mapPos.Y.ToString(), GameLoop.World.Player.MapPos.ToString(), json }, false, false);
                                    }
                                }
                            }
                        }
                    }

                    if (GameLoop.World.Player.Inventory[hotbarSelect].ItemCategory == 1) { // Clicked with a watering can
                        int distance = GoRogue.Lines.Get(new GoRogue.Coord(mapPos.X, mapPos.Y), new GoRogue.Coord(GameLoop.World.Player.Position.X, GameLoop.World.Player.Position.Y)).Count();
                        if (distance < 5) {
                            if (mapPos.X >= 0 && mapPos.X <= GameLoop.MapWidth && mapPos.Y >= 0 && mapPos.Y <= GameLoop.MapHeight) {
                                TileBase tile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos);
                                if (tile.Name == "Well" || tile.Name.ToLower().Contains("water")) {
                                    GameLoop.World.Player.Inventory[hotbarSelect].Durability = GameLoop.World.Player.Inventory[hotbarSelect].MaxDurability;
                                } else {
                                    if (tile.Plant != null) {
                                        if (!tile.Plant.WateredToday && GameLoop.World.Player.Inventory[hotbarSelect].Durability > 0) {
                                            tile.Plant.WateredToday = true;
                                            GameLoop.World.Player.Inventory[hotbarSelect].Durability--;
                                            GameLoop.UIManager.Map.MapConsole.SetEffect(mapPos.X, mapPos.Y, null);
                                            tile.UpdateAppearance();
                                            GameLoop.UIManager.Map.MapConsole.SetCellAppearance(mapPos.X, mapPos.Y, tile);
                                            GameLoop.World.Player.ExpendStamina(1);
                                        }
                                        string json = JsonConvert.SerializeObject(tile, Formatting.Indented);
                                        GameLoop.SendMessageIfNeeded(new string[] { "updateTile", mapPos.X.ToString(), mapPos.Y.ToString(), GameLoop.World.Player.MapPos.ToString(), json }, false, false);
                                    }
                                }
                            }
                        }
                    }

                    if (GameLoop.World.Player.Inventory[hotbarSelect].Plant != null && !Harvesting) { // Clicked with a seed
                        int distance = GoRogue.Lines.Get(new GoRogue.Coord(mapPos.X, mapPos.Y), new GoRogue.Coord(GameLoop.World.Player.Position.X, GameLoop.World.Player.Position.Y)).Count();
                        if (distance < 5) {
                            if (mapPos.X >= 0 && mapPos.X <= GameLoop.MapWidth && mapPos.Y >= 0 && mapPos.Y <= GameLoop.MapHeight) {
                                TileBase tile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mapPos);
                                if (tile.Name == "Tilled Dirt") {
                                    if (GameLoop.World.Player.MapPos == new Point3D(-1, 0, 0) && GameLoop.CheckFlag("farm")) {
                                        if (GameLoop.World.Player.Inventory[hotbarSelect].Plant.RequiredLevel <= GameLoop.World.Player.Skills["Farming"].Level) {
                                            if (GameLoop.World.Player.Inventory[hotbarSelect].Plant.GrowthSeason == "Any" || GameLoop.World.Player.Inventory[hotbarSelect].Plant.GrowthSeason == GameLoop.World.Player.Clock.GetSeason()) {
                                                tile.Plant = new Plant(GameLoop.World.Player.Inventory[hotbarSelect].Plant);
                                                tile.Name = tile.Plant.ProduceName + " Plant";
                                                tile.UpdateAppearance();
                                                GameLoop.UIManager.Map.MapConsole.SetEffect(mapPos.X, mapPos.Y, new CustomBlink(168, Color.Blue));
                                                CommandManager.RemoveOneItem(GameLoop.World.Player, hotbarSelect);

                                                string json = JsonConvert.SerializeObject(tile, Formatting.Indented);
                                                GameLoop.SendMessageIfNeeded(new string[] { "updateTile", mapPos.X.ToString(), mapPos.Y.ToString(), GameLoop.World.Player.MapPos.ToString(), json }, false, false);
                                            } else {
                                                Harvesting = true;
                                                GameLoop.UIManager.AddMsg(new ColoredString("You can't plant that right now. (" + GameLoop.World.Player.Inventory[hotbarSelect].Plant.GrowthSeason + ")", Color.Red, Color.Black));
                                            }
                                        } else {
                                            Harvesting = true;
                                            GameLoop.UIManager.AddMsg(new ColoredString("You aren't high enough level to plant that. (" + GameLoop.World.Player.Inventory[hotbarSelect].Plant.RequiredLevel + ")", Color.Red, Color.Black));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!GameHost.Instance.Mouse.LeftButtonDown) {
                if (ChargeBar != 0) {
                    if (LocalLure != null) {
                        if (GameLoop.World.maps[GameLoop.World.Player.MapPos].GetEntityAt<ItemWrapper>(LocalLure.Position) != null) {
                            ItemWrapper wrap = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetEntityAt<ItemWrapper>(LocalLure.Position);
                            GameLoop.UIManager.AddMsg("Snagged the " + wrap.item.Name + "!");
                            CommandManager.AddItemToInv(GameLoop.World.Player, wrap.item);
                            //  GameLoop.CommandManager.DestroyItem(item);
                            GameLoop.World.Player.ExpendStamina(1);
                            wrap.Position = new Point(-1, -2);
                        } else if (LocalLure.FishOnHook) {
                            GameLoop.UIManager.Minigames.FishingManager.InitiateFishing(GameLoop.World.Player.Clock.GetSeason(), 
                                GameLoop.World.maps[GameLoop.World.Player.MapPos].MinimapTile.name, 
                                GameLoop.World.Player.Clock.GetCurrentTime(),
                                GameLoop.World.Player.Skills["Fishing"].Level);
                            GameLoop.World.Player.ExpendStamina(1);
                        } else { 
                            GameLoop.UIManager.Map.MapConsole.ClearDecorators(LocalLure.Position.X, LocalLure.Position.Y, 1);
                            LocalLure.Position = new Point(-1, -1);
                        }
                    }
                    if (ChargeBar >= 10) {
                        if (GameLoop.World.Player.Inventory[hotbarSelect].Name != "(EMPTY)") {
                            if (GameLoop.World.Player.Inventory[hotbarSelect].ItemCategory == 7) { // Fishing rod
                                Point target = LureSpot * new Point(2, 2);
                                int xDist = (int)((double)((target.X / 10) * (ChargeBar / 10)));
                                int yDist = (int)((double)((target.Y / 10) * (ChargeBar / 10)));

                                LocalLure = new();
                                LocalLure.Position = GameLoop.World.Player.Position;
                                LocalLure.SetVelocity(xDist, yDist); 
                                GameLoop.UIManager.Map.EntityRenderer.Add(LocalLure);
                            }
                        }
                    }

                    ChargeBar = 0;
                }
            }



            if (GameHost.Instance.Mouse.RightClicked) {
                if (GameLoop.World.Player.Inventory[hotbarSelect].ItemCategory == 5) {
                    GameLoop.UIManager.Construction.ToggleConstruction();
                }
            }
        }

        public void MapEditorInput() {
            Map mapData = GameLoop.World.maps[GameLoop.World.Player.MapPos];
            MinimapTile thisMap = mapData.MinimapTile;


            foreach (var key in GameHost.Instance.Keyboard.KeysReleased) {
                if (key.Character >= 'A' && key.Character <= 'z') {
                    thisMap.name += key.Character;
                }
            }
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Back)) {
                if (thisMap.name.Length > 0) { thisMap.name = thisMap.name[0..^1]; }
            } else if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Space)) {
                thisMap.name += " ";
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F11)) {
                if (GameLoop.UIManager.selectedMenu == "Map Editor") {
                    GameLoop.UIManager.selectedMenu = "None";
                } else {
                    GameLoop.UIManager.selectedMenu = "Map Editor";
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F9)) {
                World.SaveMapToFile(GameLoop.World.maps[GameLoop.World.Player.MapPos], GameLoop.World.Player.MapPos);
            }

            Point sidebarMouse = new MouseScreenObjectState(SidebarConsole, GameHost.Instance.Mouse).CellPosition;
            TileBase selectedTile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(tileSelected);

            if (selectedTile.Lock != null) {
                if (selectedTile.Lock.LockTime > 1440)
                    selectedTile.Lock.LockTime = 1440;

                if (selectedTile.Lock.UnlockTime > 1440)
                    selectedTile.Lock.UnlockTime = 1440;
            }


            if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                if (sidebarMouse.Y == 19) {
                    if (monIndex < GameLoop.World.monsterLibrary.Count)
                        monIndex++;
                    else
                        monIndex = 0;
                } else if (sidebarMouse.Y == 26) {
                    if (tileIndex < GameLoop.World.tileLibrary.Count)
                        tileIndex++;
                    else
                        tileIndex = 0;
                } else if (sidebarMouse.Y == 11) {
                    thisMap.ch++;
                } else if (sidebarMouse.Y == 38) {
                    if (selectedTile.Lock != null && selectedTile.Lock.RelationshipUnlock < 100)
                        selectedTile.Lock.RelationshipUnlock++;
                } else if (sidebarMouse.Y == 39) {
                    if (selectedTile.Lock != null)
                        selectedTile.Lock.MissionUnlock++;
                } else if (sidebarMouse.Y == 37) {
                    if (selectedTile.Lock != null) {
                        selectedTile.Lock.OwnerID++;
                        selectedTile.Lock.UpdateOwner();
                    }
                } else if (sidebarMouse.Y == 41) {
                    if (selectedTile.Lock != null)
                        if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                            selectedTile.Lock.UnlockTime += 60;
                        else
                            selectedTile.Lock.UnlockTime++;
                } else if (sidebarMouse.Y == 42) {
                    if (selectedTile.Lock != null)
                        if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                            selectedTile.Lock.LockTime += 60;
                        else
                            selectedTile.Lock.LockTime++;
                }
            } else if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                if (sidebarMouse.Y == 19) {
                    if (monIndex > 0)
                        monIndex--;
                    else
                        monIndex = GameLoop.World.monsterLibrary.Count;
                } else if (sidebarMouse.Y == 26) {
                    if (tileIndex > 0)
                        tileIndex--;
                    else
                        tileIndex = GameLoop.World.tileLibrary.Count;
                } else if (sidebarMouse.Y == 11) {
                    if (thisMap.ch > 0)
                        thisMap.ch--;
                } else if (sidebarMouse.Y == 38) {
                    if (selectedTile.Lock != null && selectedTile.Lock.RelationshipUnlock > 0)
                        selectedTile.Lock.RelationshipUnlock--;
                } else if (sidebarMouse.Y == 39) {
                    if (selectedTile.Lock != null && selectedTile.Lock.MissionUnlock > -1)
                        selectedTile.Lock.MissionUnlock--;
                } else if (sidebarMouse.Y == 37) {
                    if (selectedTile.Lock != null && selectedTile.Lock.OwnerID > -1) {
                        selectedTile.Lock.OwnerID--;
                        selectedTile.Lock.UpdateOwner();
                    }
                } else if (sidebarMouse.Y == 41) {
                    if (selectedTile.Lock != null && selectedTile.Lock.UnlockTime > 0)
                        if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                            selectedTile.Lock.UnlockTime -= 60;
                        else
                            selectedTile.Lock.UnlockTime--;
                } else if (sidebarMouse.Y == 42) {
                    if (selectedTile.Lock != null && selectedTile.Lock.LockTime > 0)
                        if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                            selectedTile.Lock.LockTime -= 60;
                        else
                            selectedTile.Lock.LockTime--;
                }
            }

            /*
                        SidebarConsole.Print(0, 37, "Owner: " + tile.Lock.Owner);
                        SidebarConsole.Print(0, 38, "Rel Unlock: " + tile.Lock.RelationshipUnlock);
                        SidebarConsole.Print(0, 39, "Mission Unlock: " + tile.Lock.MissionUnlock);
                        SidebarConsole.Print(0, 40, "Always Locked: " + tile.Lock.AlwaysLocked);
                        SidebarConsole.Print(0, 41, "Unlocks at: " + tile.Lock.UnlockTime);
                        SidebarConsole.Print(0, 42, "Locks at: " + tile.Lock.LockTime);
                        SidebarConsole.Print(0, 43, "Key SubID: " + tile.Lock.KeySubID);
            */
             
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F4)) {
                GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.Clear();
                GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.Add(1, 45);
                GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.Add(2, 45);
                GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.Add(3, 10);
                GameLoop.World.maps[GameLoop.World.Player.MapPos].MinimumMonsters = 5;
                GameLoop.World.maps[GameLoop.World.Player.MapPos].MaximumMonsters = 10;

            }

            Point mousePos = GameHost.Instance.Mouse.ScreenPosition.PixelLocationToSurface(12, 12) - new Point(1, 1);
            if (mousePos.X < 72 && mousePos.Y < 41 && mousePos.X >= 0 && mousePos.Y >= 0) {
                if (GameHost.Instance.Mouse.LeftButtonDown) {
                    if (GameLoop.World.tileLibrary.ContainsKey(tileIndex)) {
                        TileBase tile = new(tileIndex);
                        tile.TileID = tileIndex;
                        if (mousePos.ToIndex(GameLoop.MapWidth) < GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles.Length) { 
                            GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[mousePos.ToIndex(GameLoop.MapWidth)] = tile;

                            string json = JsonConvert.SerializeObject(tile, Formatting.Indented);
                            GameLoop.SendMessageIfNeeded(new string[] { "updateTile", mousePos.X.ToString(), mousePos.Y.ToString(), GameLoop.World.Player.MapPos.ToString(), json }, false, false);
                        }
                    }
                }

                if (GameHost.Instance.Mouse.RightClicked) {
                    tileSelected = mousePos;
                }
            } else {
                mousePos -= new Point(72, 0);
                if (GameHost.Instance.Mouse.LeftButtonDown) {
                    if (mousePos == new Point(8, 14)) { thisMap.fg = new Color(thisMap.fg.R - 1, thisMap.fg.G, thisMap.fg.B); }
                    if (mousePos == new Point(14, 14)) { thisMap.fg = new Color(thisMap.fg.R + 1, thisMap.fg.G, thisMap.fg.B); }
                    if (mousePos == new Point(8, 15)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G - 1, thisMap.fg.B); }
                    if (mousePos == new Point(14, 15)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G + 1, thisMap.fg.B); }
                    if (mousePos == new Point(8, 16)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G, thisMap.fg.B - 1); }
                    if (mousePos == new Point(14, 16)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G, thisMap.fg.B + 1); }

                    if (GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.ContainsKey(monIndex)) {
                        if (mousePos == new Point(8, 20)) { 
                            GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights[monIndex]--;
                            if (GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights[monIndex] == 0) {
                                GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.Remove(monIndex);
                            }
                        }
                        if (mousePos == new Point(14, 20)) { GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights[monIndex]++; }
                    } else {
                        if (mousePos.Y == 20) {
                            GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.Add(monIndex, 1);
                        }
                    }
                }

                if (GameHost.Instance.Mouse.RightClicked) {
                    if (mousePos == new Point(8, 14)) { thisMap.fg = new Color(thisMap.fg.R - 1, thisMap.fg.G, thisMap.fg.B); }
                    if (mousePos == new Point(14, 14)) { thisMap.fg = new Color(thisMap.fg.R + 1, thisMap.fg.G, thisMap.fg.B); }
                    if (mousePos == new Point(8, 15)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G - 1, thisMap.fg.B); }
                    if (mousePos == new Point(14, 15)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G + 1, thisMap.fg.B); }
                    if (mousePos == new Point(8, 16)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G, thisMap.fg.B - 1); }
                    if (mousePos == new Point(14, 16)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G, thisMap.fg.B + 1); }
                    if (mousePos == new Point(14, 17)) { GameLoop.World.maps[GameLoop.World.Player.MapPos].MinimumMonsters--; }
                    if (mousePos == new Point(20, 17)) { GameLoop.World.maps[GameLoop.World.Player.MapPos].MinimumMonsters++; }
                    if (mousePos == new Point(14, 18)) { GameLoop.World.maps[GameLoop.World.Player.MapPos].MaximumMonsters--; }
                    if (mousePos == new Point(20, 18)) { GameLoop.World.maps[GameLoop.World.Player.MapPos].MaximumMonsters++; }

                    if (GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.ContainsKey(monIndex)) {
                        if (mousePos == new Point(8, 20)) { 
                            GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights[monIndex]--; 
                            if (GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights[monIndex] == 0) {
                                GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.Remove(monIndex);
                            }
                        }
                        if (mousePos == new Point(14, 20)) { GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights[monIndex]++; }
                    }
                    
                }
            }
        }

        public void RenderSidebar() {
            Point mousePos = new MouseScreenObjectState(SidebarConsole, GameHost.Instance.Mouse).CellPosition;
            SidebarConsole.Clear();
            string time = "";
            string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };


            if (LocalLure != null)
                LocalLure.Update();

            if (GameLoop.World.Player.Clock != null) {
                string timeHour = GameLoop.World.Player.Clock.Hours.ToString();
                if (timeHour.Length == 1)
                    timeHour = "0" + timeHour;

                string timeMinute = GameLoop.World.Player.Clock.Minutes.ToString();
                if (timeMinute.Length == 1)
                    timeMinute = "0" + timeMinute;

                time = timeHour + ":" + timeMinute;
                 
            }

            SidebarConsole.DrawLine(new Point(16, 0), new Point(16, 9), (char)179, Color.White, Color.Black);
            // The minimap area (top-right)

            for (int x = GameLoop.World.Player.MapPos.X - 4; x < GameLoop.World.Player.MapPos.X + 5; x++) {
                for (int y = GameLoop.World.Player.MapPos.Y - 4; y < GameLoop.World.Player.MapPos.Y + 5; y++) {
                    if (minimap.ContainsKey(new Point3D(x, y, GameLoop.World.Player.MapPos.Z))) {
                        Point3D modifiedPos = new Point3D(x, y, 0) - GameLoop.World.Player.MapPos;
                        SidebarConsole.Print(modifiedPos.X + 21, modifiedPos.Y + 4, minimap[new Point3D(x, y, GameLoop.World.Player.MapPos.Z)].AsColoredGlyph());
                    }
                }
            }

            SidebarConsole.Print(21, 4, GameLoop.World.Player.GetAppearance());
            SidebarConsole.DrawLine(new Point(0, 9), new Point(25, 9), (char)196, Color.White, Color.Black);


            if (GameLoop.UIManager.selectedMenu == "Map Editor") {
                MinimapTile thisMap = GameLoop.World.maps[GameLoop.World.Player.MapPos].MinimapTile;
                SidebarConsole.Print(0, 10, "Map Name: " + thisMap.name);
                SidebarConsole.Print(0, 11, "Map Icon: ");
                SidebarConsole.Print(10, 11, thisMap.AsColoredGlyph());
                SidebarConsole.Print(0, 13, "Position: " + GameLoop.World.Player.MapPos.ToString());

                SidebarConsole.Print(0, 14, "Map fR: - " + thisMap.fg.R);
                SidebarConsole.Print(14, 14, "+");
                SidebarConsole.Print(0, 15, "Map fG: - " + thisMap.fg.G);
                SidebarConsole.Print(14, 15, "+");
                SidebarConsole.Print(0, 16, "Map fB: - " + thisMap.fg.B);
                SidebarConsole.Print(14, 16, "+");
                 
                
                SidebarConsole.Print(0, 17, "Min Monsters: - " + GameLoop.World.maps[GameLoop.World.Player.MapPos].MinimumMonsters.ToString().Align(HorizontalAlignment.Center, 3) + " +");
                SidebarConsole.Print(0, 18, "Max Monsters: - " + GameLoop.World.maps[GameLoop.World.Player.MapPos].MaximumMonsters.ToString().Align(HorizontalAlignment.Center, 3) + " +");

                SidebarConsole.Print(0, 19, "Monster ID: " + monIndex + "(" + (GameLoop.World.monsterLibrary.ContainsKey(monIndex) ? GameLoop.World.monsterLibrary[monIndex].Name : "None") + ")");
                if (GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.ContainsKey(monIndex)) {
                    SidebarConsole.Print(0, 20, "Weight: -" + GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights[monIndex].ToString().Align(HorizontalAlignment.Center, 5) + "+");
                } else {
                    SidebarConsole.Print(0, 20, "Not on this map [Add]");
                }

                int weight = 0;

                if (GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.Count > 1) {
                    foreach (KeyValuePair<int, int> kv in GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights) {
                        weight += kv.Value;
                    }
                } else if (GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.Count == 1) {
                    weight = GameLoop.World.maps[GameLoop.World.Player.MapPos].MonsterWeights.ElementAt(0).Value;
                }

                SidebarConsole.Print(0, 21, "Total Map Weight: " + weight);


                SidebarConsole.Print(0, 26, "Tile Index: " + tileIndex);

                if (GameLoop.World.tileLibrary.ContainsKey(tileIndex)) {
                    TileBase tile = GameLoop.World.tileLibrary[tileIndex];

                    SidebarConsole.Print(0, 27, "Tile Name: " + tile.Name);
                    SidebarConsole.Print(0, 28, "Tile Appearance: ");
                    SidebarConsole.Print(17, 28, tile.AsColoredGlyph());
                }

                if (tileSelected != new Point(-1, -1)) {
                    TileBase tile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(tileSelected);
                    SidebarConsole.Print(0, 35, "Selected Tile: " + tile.Name);
                    SidebarConsole.Print(0, 36, "Tile Appearance: ");
                    SidebarConsole.Print(17, 36, tile.AsColoredGlyph());
                    if (tile.Lock != null) {
                        SidebarConsole.Print(0, 37, "Owner: " + tile.Lock.Owner);
                        SidebarConsole.Print(0, 38, "Rel Unlock: " + tile.Lock.RelationshipUnlock);
                        SidebarConsole.Print(0, 39, "Mission Unlock: " + tile.Lock.MissionUnlock);
                        SidebarConsole.Print(0, 40, "Always Locked: " + tile.Lock.AlwaysLocked);
                        SidebarConsole.Print(0, 41, "Unlocks at: " + GameLoop.World.Player.Clock.MinutesToTime(tile.Lock.UnlockTime));
                        SidebarConsole.Print(0, 42, "Locks at: " + GameLoop.World.Player.Clock.MinutesToTime(tile.Lock.LockTime));
                        SidebarConsole.Print(0, 43, "Key Name: " + tile.Lock.UnlockKeyName);
                    }
                }
            } else { // Print non-map editor stuff
                if (GameLoop.World != null && GameLoop.World.DoneInitializing) {
                    for (int i = 0; i < GameLoop.World.Player.killList.Count; i++) {
                        SidebarConsole.Print(i % 26, 10 + (i / 26), GameLoop.World.Player.killList.ToList()[i]);
                    }

                    SidebarConsole.DrawLine(new Point(0, 12), new Point(25, 12), (char)196, Color.White, Color.Black);

                    ColoredString copperString = new("CP:" + GameLoop.World.Player.CopperCoins, new Color(184, 115, 51), Color.Black);
                    ColoredString silverString = new("SP:" + GameLoop.World.Player.SilverCoins, Color.Silver, Color.Black);
                    ColoredString goldString = new("GP:" + GameLoop.World.Player.GoldCoins, Color.Yellow, Color.Black);
                    ColoredString JadeString = new("JP:" + GameLoop.World.Player.JadeCoins, new Color(0, 168, 107), Color.Black);

                    SidebarConsole.Print(0, 13, copperString);
                    SidebarConsole.Print(0, 14, silverString);
                    SidebarConsole.Print(13, 13, goldString);
                    SidebarConsole.Print(13, 14, JadeString);

                    SidebarConsole.DrawLine(new Point(0, 15), new Point(25, 15), (char)196, Color.White, Color.Black);



                    int y = 16;

                    SidebarConsole.Print(0, y, new ColoredString(((char)3).ToString(), Color.Red, Color.Black));
                    SidebarConsole.Print(1, y, new ColoredString((GameLoop.World.Player.CurrentHP + "/" + GameLoop.World.Player.MaxHP).Align(HorizontalAlignment.Right, 5), Color.Red, Color.Black));

                    SidebarConsole.Print(8, y, new ColoredString(((char)175).ToString(), Color.Lime, Color.Black));
                    SidebarConsole.Print(9, y, new ColoredString((GameLoop.World.Player.CurrentStamina + "/" + GameLoop.World.Player.MaxStamina).Align(HorizontalAlignment.Right, 7), Color.Lime, Color.Black));

                    GameLoop.World.Player.CalculateCombatLevel();
                    SidebarConsole.Print(0, y + 1, "Combat Lv: " + GameLoop.World.Player.CombatLevel);
                    SidebarConsole.Print(0, y + 2, "Mode: " + GameLoop.World.Player.CombatMode);
                    SidebarConsole.Print(0, y + 3, "Damage: " + GameLoop.World.Player.GetDamageType());

                    if (GameLoop.World.Player.Clock != null) {
                        SidebarConsole.Print(18, y, time);
                        SidebarConsole.Print(24, y++, GameLoop.World.Player.Clock.AM ? "AM" : "PM");
                        SidebarConsole.Print(18, y++, (months[GameLoop.World.Player.Clock.Month - 1] + " " + GameLoop.World.Player.Clock.Day).Align(HorizontalAlignment.Right, 8));
                        SidebarConsole.Print(19, y++, ("Year " + GameLoop.World.Player.Clock.Year).Align(HorizontalAlignment.Right, 7));
                    }

                    SidebarConsole.DrawLine(new Point(0, y), new Point(25, y), (char)196, Color.White, Color.Black);
                    y++;

                    SidebarConsole.Print(0, y++, "Mouse-Look");
                    Point mouseOverMap = new MouseScreenObjectState(GameLoop.UIManager.Map.MapConsole, GameHost.Instance.Mouse).CellPosition;
                    if (mouseOverMap.X >= 0 && mouseOverMap.Y >= 0 && mouseOverMap.X <= GameLoop.MapWidth && mouseOverMap.Y <= GameLoop.MapHeight) {
                        if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(new GoRogue.Coord(mouseOverMap.X, mouseOverMap.Y))) {
                            TileBase tile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mouseOverMap);

                            SidebarConsole.Print(0, y++, tile.AsColoredGlyph() + new ColoredString(" " + tile.Name, Color.White, Color.Black));
                            if (tile.Dec != null) 
                                SidebarConsole.SetDecorator(0, y - 1, 1, tile.GetDecorator());
                            y++;
                            
                            if (GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mouseOverMap).Container == null) {
                                SidebarConsole.Print(0, y++, "Monsters");
                                List<Monster> MonstersOnTile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetAllEntities<Monster>(mouseOverMap);

                                if (MonstersOnTile != null && MonstersOnTile.Count > 0) {
                                    for (int i = 0; i < MonstersOnTile.Count; i++) {
                                        SidebarConsole.Print(0, y++, MonstersOnTile[i].GetAppearance() + new ColoredString(" " + MonstersOnTile[i].Name, Color.White, Color.Black));
                                    }
                                } else {
                                    SidebarConsole.Print(0, y++, new ColoredString(" (None)", Color.DarkSlateGray, Color.Black));
                                }

                                y++;

                            
                                SidebarConsole.Print(0, y++, "Items");
                                List<ItemWrapper> ItemsOnTile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetAllEntities<ItemWrapper>(mouseOverMap);

                                if (ItemsOnTile != null && ItemsOnTile.Count > 0) {
                                    for (int i = 0; i < ItemsOnTile.Count; i++) {
                                        string qty = "";
                                        if (ItemsOnTile[i].item.ItemQuantity > 1)
                                            qty = ItemsOnTile[i].item.ItemQuantity + "x ";

                                        ColoredString LetterGrade = new("");
                                        if (ItemsOnTile[i].item.Quality > 0)
                                            LetterGrade = new ColoredString(" [") + ItemsOnTile[i].item.LetterGrade() + new ColoredString("]");

                                        SidebarConsole.Print(0, y++, ItemsOnTile[i].AsColoredGlyph() + new ColoredString(" " + qty + ItemsOnTile[i].item.Name, Color.White, Color.Black) + LetterGrade);
                                        if (ItemsOnTile[i].item.Dec != null)
                                            SidebarConsole.SetDecorator(0, y - 1, 1, ItemsOnTile[i].GetDecorator());

                                        if (y >= 35)
                                            break;
                                    }
                                } else {
                                    SidebarConsole.Print(0, y++, new ColoredString(" (None)", Color.DarkSlateGray, Color.Black));
                                }
                            } else {
                                SidebarConsole.Print(0, y - 2, tile.AsColoredGlyph() + new ColoredString((" " + tile.Container.Name).Align(HorizontalAlignment.Left, 20), Color.White, Color.Black));
                                if (tile.Dec != null)
                                    SidebarConsole.SetDecorator(0, y - 2, 1, tile.GetDecorator());

                                SidebarConsole.Print(0, y++, "Container");
                                List<Item> ItemsOnTile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(mouseOverMap).Container.Items;

                                if (ItemsOnTile != null && ItemsOnTile.Count > 0) {
                                    for (int i = 0; i < ItemsOnTile.Count; i++) { 
                                        string qty = "";
                                        if (ItemsOnTile[i].ItemQuantity > 1)
                                            qty = ItemsOnTile[i].ItemQuantity + "x ";

                                        ColoredString LetterGrade = new("");
                                        if (ItemsOnTile[i].Quality > 0)
                                            LetterGrade = new ColoredString(" [") + ItemsOnTile[i].LetterGrade() + new ColoredString("]");

                                        SidebarConsole.Print(0, y++, ItemsOnTile[i].AsColoredGlyph() + new ColoredString(" " + qty + ItemsOnTile[i].Name, Color.White, Color.Black) + LetterGrade);
                                        if (ItemsOnTile[i].Dec != null)
                                            SidebarConsole.SetDecorator(0, y - 1, 1, ItemsOnTile[i].GetDecorator());

                                        if (y >= 35)
                                            break;
                                    }
                                } else {
                                    SidebarConsole.Print(0, y++, new ColoredString(" (Empty)", Color.DarkSlateGray, Color.Black));
                                }
                            }
                        }
                    } 
                    y = 35;

                    SidebarConsole.DrawLine(new Point(0, y), new Point(25, y), (char)196, Color.White, Color.Black);
                    y++;
                    SidebarConsole.Print(0, y, "Backpack");
                    y++;

                    for (int i = 0; i < 9; i++) {
                        Item item = GameLoop.World.Player.Inventory[i];

                        SidebarConsole.Print(0, y, "|");
                        SidebarConsole.Print(1, y, item.AsColoredGlyph());
                        if (item.Dec != null) {
                            SidebarConsole.SetDecorator(1, y, 1, new CellDecorator(new Color(item.Dec.R, item.Dec.G, item.Dec.B), item.Dec.Glyph, Mirror.None));
                        }

                        ColoredString LetterGrade = new("");
                        if (item.Quality > 0)
                            LetterGrade = new ColoredString(" [") + item.LetterGrade() + new ColoredString("]");


                        if (!item.IsStackable || (item.IsStackable && item.ItemQuantity == 1))
                            SidebarConsole.Print(3, y, new ColoredString(item.Name, i == hotbarSelect ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.TransparentBlack) + LetterGrade);
                        else
                            SidebarConsole.Print(3, y, new ColoredString(("(" + item.ItemQuantity + ") " + item.Name), i == hotbarSelect ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.TransparentBlack) + LetterGrade);

                        if (i == hotbarSelect && item.ItemCategory == 7) {
                            SidebarConsole.Print(15, y, "[");
                            int chargePercent = ChargeBar / 10;

                            for (int j = 0; j < 10; j++) {
                                if (j < chargePercent) {
                                    SidebarConsole.Print(16 + j, y, "X");
                                } else {
                                    SidebarConsole.Print(16 + j, y, "-");
                                }
                            }

                            SidebarConsole.Print(25, y, "]");
                        }

                        if (i == hotbarSelect && item.ItemCategory == 5 && GameLoop.UIManager.Construction.SelectedConstructible != -1) {
                            SidebarConsole.Print(23, y, "["); 
                            SidebarConsole.Print(24, y, GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].Appearance());
                            SidebarConsole.Print(25, y, "]");
                        }

                        y++;
                    }


                    y++;
                    SidebarConsole.Print(0, y, "Equipment");
                    y++;

                    for (int i = 0; i < 10; i++) {
                        Item item = GameLoop.World.Player.Equipment[i];
                          
                        SidebarConsole.Print(0, y, "|");
                        SidebarConsole.Print(1, y, item.AsColoredGlyph());
                        if (item.Dec != null) {
                            SidebarConsole.SetDecorator(1, y, 1, new CellDecorator(new Color(item.Dec.R, item.Dec.G, item.Dec.B), item.Dec.Glyph, Mirror.None));
                        }
                        SidebarConsole.Print(3, y, new ColoredString(item.Name, mousePos.Y == y ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.TransparentBlack));
                        y++;
                    }
                }
            }
        }
    }
}
