using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using LofiHollow.EntityData;
using LofiHollow.Managers;
using LofiHollow.Minigames.Photo; 
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;

namespace LofiHollow.UI {
    public class UI_Sidebar {
        public Window SidebarWindow;
        public SadConsole.Console SidebarConsole;  

        public int tileIndex = 0;
        public int monIndex = 0;
        public int hotbarSelect = 0; 
        public bool MadeCoinThisFrame = false;
        public int ChargeBar = 0;
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
        }

        public void SidebarInput() {

            
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F11)) {
                if (GameLoop.UIManager.selectedMenu == "Map Editor") {
                    GameLoop.UIManager.selectedMenu = "None";
                } else {
                    GameLoop.UIManager.selectedMenu = "Map Editor";
                }
            } 
            
            Point sidebarMouse = new MouseScreenObjectState(SidebarConsole, GameHost.Instance.Mouse).CellPosition;
            Point mapPos = new MouseScreenObjectState(GameLoop.UIManager.Map.MapConsole, GameHost.Instance.Mouse).CellPosition;

            if (sidebarMouse != new Point(0, 0) || mapPos != new Point(0, 0)) {
                if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                    if (hotbarSelect + 1 < GameLoop.World.Player.Inventory.Length)
                        hotbarSelect++;
                }
                else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                    if (hotbarSelect > 0)
                        hotbarSelect--;
                }
            }

           
            if (sidebarMouse.X > 0) { // Clicked in Sidebar
                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (sidebarMouse.X < 12) {
                        if (sidebarMouse.Y == 13) {
                            if (GameLoop.World.Player.CopperCoins > 0) { 
                                GameLoop.World.Player.CopperCoins -= 1;
                                Item copper = Item.Copy("lh:Copper Coin");
                                CommandManager.AddItemToInv(GameLoop.World.Player, copper);
                                MadeCoinThisFrame = true;
                            }
                        }

                        if (sidebarMouse.Y == 14) {
                            if (GameLoop.World.Player.SilverCoins > 0) {
                                GameLoop.World.Player.SilverCoins--;
                                Item silver = Item.Copy("lh:Silver Coin");
                                CommandManager.AddItemToInv(GameLoop.World.Player, silver);
                                MadeCoinThisFrame = true;
                            }
                        }
                    } else { 
                        if (sidebarMouse.Y == 13) {
                            if (GameLoop.World.Player.GoldCoins > 0) {
                                GameLoop.World.Player.GoldCoins--;
                                Item gold = Item.Copy("lh:Gold Coin");
                                CommandManager.AddItemToInv(GameLoop.World.Player, gold);
                                MadeCoinThisFrame = true;
                            }
                        }

                        if (sidebarMouse.Y == 14) {
                            if (GameLoop.World.Player.JadeCoins > 0) {
                                GameLoop.World.Player.JadeCoins--;
                                Item jade = Item.Copy("lh:Jade Coin");
                                CommandManager.AddItemToInv(GameLoop.World.Player, jade);
                                MadeCoinThisFrame = true;
                            }
                        } 
                    }

                }
            }


            int distance = GoRogue.Lines.Get(mapPos.ToCoord(), GameLoop.World.Player.Position.ToCoord()).Count();
            if (GameHost.Instance.Mouse.LeftButtonDown) {
                ItemHandler.ContinuousUseItem(GameLoop.World.Player.Inventory[hotbarSelect], mapPos, GameLoop.World.Player.MapPos, distance);
            }

            if (GameHost.Instance.Mouse.RightClicked) {
                ItemHandler.RightClickItem(GameLoop.World.Player.Inventory[hotbarSelect], mapPos, GameLoop.World.Player.MapPos, distance);
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                ItemHandler.UseItem(GameLoop.World.Player.Inventory[hotbarSelect], mapPos, GameLoop.World.Player.MapPos, distance);
            }

            if (!GameHost.Instance.Mouse.LeftButtonDown) {
                ItemHandler.ReleaseClick(GameLoop.World.Player.Inventory[hotbarSelect], mapPos, GameLoop.World.Player.MapPos, distance);
            }

            MadeCoinThisFrame = false;
        }

        public void MapEditorInput() {
            Map mapData = Helper.ResolveMap(GameLoop.World.Player.MapPos);

            if (mapData != null) { 
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
                    GameLoop.World.SaveMapToFile(GameLoop.World.Player.MapPos);
                }

                Point sidebarMouse = new MouseScreenObjectState(SidebarConsole, GameHost.Instance.Mouse).CellPosition;
                Tile selectedTile = mapData.GetTile(tileSelected);

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
                        if (tileIndex > 0)
                            tileIndex--;
                        else
                            tileIndex = GameLoop.World.tileLibrary.Count - 1;
                    } else if (sidebarMouse.Y == 11) {
                        thisMap.ch++;
                    } else if (sidebarMouse.Y == 38) {
                        if (selectedTile.Lock != null && selectedTile.Lock.RelationshipUnlock < 100)
                            selectedTile.Lock.RelationshipUnlock++;
                    } else if (sidebarMouse.Y == 39) {
                        if (selectedTile.Lock != null)
                            selectedTile.Lock.MissionUnlock++;
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
                        if (tileIndex < GameLoop.World.tileLibrary.Count)
                            tileIndex++;
                        else
                            tileIndex = 0;
                    } else if (sidebarMouse.Y == 11) {
                        if (thisMap.ch > 0)
                            thisMap.ch--;
                    } else if (sidebarMouse.Y == 38) {
                        if (selectedTile.Lock != null && selectedTile.Lock.RelationshipUnlock > 0)
                            selectedTile.Lock.RelationshipUnlock--;
                    } else if (sidebarMouse.Y == 39) {
                        if (selectedTile.Lock != null && selectedTile.Lock.MissionUnlock > -1)
                            selectedTile.Lock.MissionUnlock--;
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


                Point mousePos = GameHost.Instance.Mouse.ScreenPosition.PixelLocationToSurface(12, 12) - new Point(1, 1);
                if (mousePos.X < 72 && mousePos.Y < 41 && mousePos.X >= 0 && mousePos.Y >= 0) {
                    if (GameHost.Instance.Mouse.LeftButtonDown) {
                        Tile tile = new(GameLoop.World.tileLibrary.ElementAt(tileIndex).Key);
                        if (mousePos.ToIndex(GameLoop.MapWidth) < mapData.Tiles.Length) {
                            mapData.Tiles[mousePos.ToIndex(GameLoop.MapWidth)] = tile;

                            if (GameLoop.World.Player.MapPos.Z <= 0 && GameLoop.World.Player.MapPos.WorldArea == "Overworld") {
                                mapData.Tiles[mousePos.ToIndex(GameLoop.MapWidth)].ExposedToSky = true;
                            }

                            GameLoop.UIManager.Map.SyncMapEntities(mapData);

                            NetMsg updateTile = new("updateTile", tile.ToByteArray());
                            updateTile.SetFullPos(mousePos, GameLoop.World.Player.MapPos);
                            GameLoop.SendMessageIfNeeded(updateTile, false, false);
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

                    }

                    if (GameHost.Instance.Mouse.RightClicked) {
                        if (mousePos == new Point(8, 14)) { thisMap.fg = new Color(thisMap.fg.R - 1, thisMap.fg.G, thisMap.fg.B); }
                        if (mousePos == new Point(14, 14)) { thisMap.fg = new Color(thisMap.fg.R + 1, thisMap.fg.G, thisMap.fg.B); }
                        if (mousePos == new Point(8, 15)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G - 1, thisMap.fg.B); }
                        if (mousePos == new Point(14, 15)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G + 1, thisMap.fg.B); }
                        if (mousePos == new Point(8, 16)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G, thisMap.fg.B - 1); }
                        if (mousePos == new Point(14, 16)) { thisMap.fg = new Color(thisMap.fg.R, thisMap.fg.G, thisMap.fg.B + 1); } 

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
                 
                
                SidebarConsole.Print(0, 26, "Tile Index: " + tileIndex);

                if (tileIndex < GameLoop.World.tileLibrary.Count) {
                    Tile PlacingTile = GameLoop.World.tileLibrary[GameLoop.World.tileLibrary.ElementAt(tileIndex).Key];

                    SidebarConsole.Print(0, 27, "Tile Name: " + PlacingTile.Name);
                    SidebarConsole.Print(0, 28, "Tile Appearance: ");
                    SidebarConsole.Print(17, 28, PlacingTile.AsColoredGlyph());

                    if (tileSelected != new Point(-1, -1)) {
                        Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);

                        if (map != null) {
                            Tile tile = map.GetTile(tileSelected);
                            SidebarConsole.Print(0, 35, "Selected Tile: " + tile.Name);
                            SidebarConsole.Print(0, 36, "Tile Appearance: ");
                            SidebarConsole.Print(17, 36, tile.AsColoredGlyph());
                            if (tile.Lock != null) {
                                SidebarConsole.Print(0, 37, "Owner: " + tile.Lock.Owner);
                                SidebarConsole.Print(0, 38, "Rel Unlock: " + tile.Lock.RelationshipUnlock);
                                SidebarConsole.Print(0, 39, "Mission Unlock: " + tile.Lock.MissionUnlock);
                                SidebarConsole.Print(0, 40, "Always Locked: " + tile.Lock.AlwaysLocked);
                                SidebarConsole.Print(0, 41, "Unlocks at: " + TimeManager.MinutesToTime(tile.Lock.UnlockTime));
                                SidebarConsole.Print(0, 42, "Locks at: " + TimeManager.MinutesToTime(tile.Lock.LockTime));
                                SidebarConsole.Print(0, 43, "Key Name: " + tile.Lock.UnlockKeyName);
                            }
                        }
                    }
                }
            } else { // Print non-map editor stuff
                if (GameLoop.World != null && GameLoop.World.DoneInitializing) {
                    List<ColoredString> kills = GameLoop.World.Player.killList.ToList();
                    kills.Reverse();
                    for (int i = 0; i < kills.Count; i++) {
                        SidebarConsole.Print(i % 26, 10 + (i / 26), kills[i]);
                    }

                    SidebarConsole.DrawLine(new Point(0, 12), new Point(25, 12), (char)196, Color.White, Color.Black);

                    ColoredString copperString = new("CP:" + GameLoop.World.Player.CopperCoins, new Color(184, 115, 51), Color.Black);
                    ColoredString silverString = new("SP:" + GameLoop.World.Player.SilverCoins, Color.Silver, Color.Black);
                    ColoredString goldString = new("GP:" + GameLoop.World.Player.GoldCoins, Color.Yellow, Color.Black);
                    ColoredString JadeString = new("JP:" + GameLoop.World.Player.JadeCoins, new Color(0, 168, 107), Color.Black);

                    SidebarConsole.Print(0, 0, copperString);
                    SidebarConsole.Print(0, 1, silverString);
                    SidebarConsole.Print(0, 2, goldString);
                    SidebarConsole.Print(0, 3, JadeString);

                    SidebarConsole.Print(0, 5, new ColoredString(3.AsString(), Color.Red, Color.Black));
                    SidebarConsole.Print(1, 5, new ColoredString((GameLoop.World.Player.CurrentHP + "/" + GameLoop.World.Player.MaxHP).Align(HorizontalAlignment.Right, 7), Color.Red, Color.Black));

                    SidebarConsole.Print(0, 6, new ColoredString(175.AsString(), Color.Lime, Color.Black));
                    SidebarConsole.Print(1, 6, new ColoredString((GameLoop.World.Player.CurrentStamina + "/" + GameLoop.World.Player.MaxStamina).Align(HorizontalAlignment.Right, 7), Color.Lime, Color.Black));


                    GameLoop.World.Player.CalculateCombatLevel();
                    SidebarConsole.Print(0, 8, "Combat Level: " + GameLoop.World.Player.CombatLevel);

                    if (GameLoop.World.Player.Clock != null) {
                        SidebarConsole.Print(11, 0, time); 
                        SidebarConsole.Print(8, 1, (months[GameLoop.World.Player.Clock.Month - 1] + " " + GameLoop.World.Player.Clock.Day).Align(HorizontalAlignment.Right, 8));
                        SidebarConsole.Print(9, 2, ("Year " + GameLoop.World.Player.Clock.Year).Align(HorizontalAlignment.Right, 7));
                    }


                    int y = 14;   
                    SidebarConsole.Print(0, y++, "Mouse-Look");
                    Point mouseOverMap = new MouseScreenObjectState(GameLoop.UIManager.Map.MapConsole, GameHost.Instance.Mouse).CellPosition;
                    if (mouseOverMap.X >= 0 && mouseOverMap.Y >= 0 && mouseOverMap.X <= GameLoop.MapWidth && mouseOverMap.Y <= GameLoop.MapHeight) {
                        if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(mouseOverMap.ToCoord())) {
                            Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);

                            if (map != null) {
                                Tile tile = map.GetTile(mouseOverMap);

                                if (tile.Name == "Space") {
                                    int depth = 1;
                                    while(tile.Name == "Space") {
                                        Point3D lower = GameLoop.World.Player.MapPos - new Point3D(0, 0, depth);
                                        Map lowerMap = Helper.ResolveMap(lower);
                                        if (lowerMap != null) {
                                            tile = lowerMap.GetTile(mouseOverMap);
                                            depth++;
                                        } else {
                                            break;
                                        }
                                    }
                                }

                                SidebarConsole.Print(0, y++, tile.AsColoredGlyph() + new ColoredString(" " + tile.Name, Color.White, Color.Black));
                                if (tile.Dec != null)
                                    SidebarConsole.SetDecorator(0, y - 1, 1, tile.GetDecorator());
                                y++;

                                if (map.GetTile(mouseOverMap).Container == null) {
                                    SidebarConsole.Print(0, y++, "People");
                                    List<Actor> PeopleOnTile = new();

                                    foreach (KeyValuePair<string, NPC> kv in GameLoop.World.npcLibrary) {
                                        if (kv.Value.Position == mouseOverMap && kv.Value.MapPos == GameLoop.World.Player.MapPos) {
                                            PeopleOnTile.Add(kv.Value);
                                        }
                                    }

                                    foreach (KeyValuePair<Steamworks.SteamId, Player> kv in GameLoop.World.otherPlayers) {
                                        if (kv.Value.Position == mouseOverMap && kv.Value.MapPos == GameLoop.World.Player.MapPos) {
                                            PeopleOnTile.Add(kv.Value);
                                        }
                                    }

                                    Map thisMap = Helper.ResolveMap(GameLoop.World.Player.MapPos);
                                    if (thisMap != null) {
                                        foreach (Entity ent in thisMap.Entities.Items) {
                                            if (ent is FarmAnimal ani) {
                                                if (ani.Position == mouseOverMap) {
                                                    PeopleOnTile.Add(ani); 
                                                }
                                            }
                                        }
                                    }

                                    if (PeopleOnTile != null && PeopleOnTile.Count > 0) {
                                        for (int i = 0; i < PeopleOnTile.Count; i++) {
                                            SidebarConsole.Print(0, y++, PeopleOnTile[i].GetAppearance() + new ColoredString(" " + PeopleOnTile[i].Name, Color.White, Color.Black));
                                        }
                                    } else {
                                        SidebarConsole.Print(0, y++, new ColoredString(" (None)", Color.DarkSlateGray, Color.Black));
                                    }

                                    y++;


                                    SidebarConsole.Print(0, y++, "Items");
                                    List<ItemWrapper> ItemsOnTile = map.GetAllEntities<ItemWrapper>(mouseOverMap);

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
                                    List<Item> ItemsOnTile = map.GetTile(mouseOverMap).Container.Items;

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
                    } 
                    y = 34;

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


                        if (!item.IsStackable || (item.IsStackable && item.ItemQuantity == 1)) { 
                            if (item.ItemCat == "Soul") {
                                string name = item.Name;
                                if (item.SoulPhoto != null) {
                                    name += " (" + item.SoulPhoto.Name() + ")";
                                } 

                                SidebarConsole.Print(3, y, new ColoredString(name, i == hotbarSelect ? Color.Yellow : Color.White, Color.TransparentBlack)); 
                            }
                            else {
                                SidebarConsole.Print(3, y, new ColoredString(item.Name, i == hotbarSelect ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.TransparentBlack) + LetterGrade);
                            }
                        } else
                            SidebarConsole.Print(3, y, new ColoredString(("(" + item.ItemQuantity + ") " + item.Name), i == hotbarSelect ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.TransparentBlack) + LetterGrade);

                        if (i == hotbarSelect && item.ItemCat == "Fishing Rod") {
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

                        if (i == hotbarSelect && item.ItemCat == "Hammer" && GameLoop.UIManager.Construction.SelectedConstructible != -1) {
                            SidebarConsole.Print(23, y, "["); 
                            SidebarConsole.Print(24, y, GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].Appearance());
                            SidebarConsole.Print(25, y, "]");
                        }

                        y++;
                    }


                    y++;
                    SidebarConsole.Print(0, y, "Equipment");
                    y++;

                    for (int i = 0; i < 11; i++) {
                        Item item = GameLoop.World.Player.Equipment[i];
                          
                        SidebarConsole.Print(0, y, "|");
                        SidebarConsole.Print(1, y, item.AsColoredGlyph());
                        if (item.Dec != null) {
                            SidebarConsole.SetDecorator(1, y, 1, new CellDecorator(new Color(item.Dec.R, item.Dec.G, item.Dec.B), item.Dec.Glyph, Mirror.None));
                        }

                        if (item.ItemCat == "Soul") {
                            string name = item.Name;
                            if (item.SoulPhoto != null) {
                                name += " (" + item.SoulPhoto.Name() + ")";
                            }

                            SidebarConsole.PrintClickable(3, y, new ColoredString(name, mousePos.Y == y ? Color.Yellow : Color.White, Color.TransparentBlack), SidebarClick, "Unequip," + i);
                        }
                        else {
                            SidebarConsole.PrintClickable(3, y, new ColoredString(item.Name, mousePos.Y == y ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.TransparentBlack), SidebarClick, "Unequip," + i);
                        }

                        y++;
                    }
                }
            }
        }

        public void SidebarClick(string ID) {
            if (ID.Contains("Unequip")) {
                int slot = Int32.Parse(ID.Split(",")[1]);
                CommandManager.UnequipItem(GameLoop.World.Player, slot);
            } 
        }
    }
}
