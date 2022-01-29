using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using LofiHollow.Managers;

namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptIn)]
    public class Player : Actor {
        [JsonProperty]
        public TimeManager Clock = new();

        [JsonProperty]
        public Dictionary<string, int> MetNPCs = new();

        [JsonProperty]
        public Dictionary<string, Missions.Mission> MissionLog = new();


        [JsonProperty]
        public bool OwnsFarm = false;

        public List<Point3D> VisitedMaps = new();

        public double TimeLastTicked = 0;
        public int MapsClearedToday = 0;
        public Stack<ColoredString> killList = new(52);

        public string MineLocation = "None";
        public int MineDepth = 0;
        public bool MineVisible = false;
        public Point MineEnteredAt = new Point(0, 0);

        public string CurrentKillTask = "";
        public int KillTaskProgress = 0;
        public int KillTaskGoal = 0;

        public string CurrentDeliveryTask = "";

        [JsonProperty]
        public int CourierGuildRank = 1;
        [JsonProperty]
        public int AdventurerGuildRank = 1;

        public bool Sleeping = false;

        [JsonProperty]
        public int LivesRemaining = -1;
        [JsonProperty]
        public int DropsOnDeath = -1; // -1: Nothing, 0: Gold, 1: Items and Gold

        public Player(Color foreground) : base(foreground, '@', true) {
            ActorGlyph = '@';
        }
         

        public void PlayerDied() {
            if (MapPos == GameLoop.World.Player.MapPos && this != GameLoop.World.Player) {
                GameLoop.UIManager.AddMsg(new ColoredString(Name + " died!", Color.Red, Color.Black));
            }

            if (DropsOnDeath == 1) {
                for (int i = 0; i < Inventory.Length; i++) {
                    if (Inventory[i].Name != "(EMPTY)") {
                        if (MineLocation == "None") {
                            CommandManager.DropItem(this, i);
                        } else {
                            if (MineLocation == "Mountain") {
                                GameLoop.UIManager.Minigames.MineManager.MountainMine.DropItem(this, i, MineDepth);
                            } else if (MineLocation == "Lake") {
                                GameLoop.UIManager.Minigames.MineManager.LakeMine.DropItem(this, i, MineDepth);
                            }
                        }
                    }
                }

                for (int i = 0; i < Equipment.Length; i++) {
                    if (Equipment[i].Name != "(EMPTY)") {
                        CommandManager.UnequipItem(this, i);

                        if (MineLocation == "None") {
                            CommandManager.DropItem(this, 0);
                        } else {
                            if (MineLocation == "Mountain") {
                                GameLoop.UIManager.Minigames.MineManager.MountainMine.DropItem(this, 0, MineDepth);
                            } else if (MineLocation == "Lake") {
                                GameLoop.UIManager.Minigames.MineManager.LakeMine.DropItem(this, 0, MineDepth);
                            }
                        }
                    }
                }
            }

            if (DropsOnDeath >= 0) {
                Item copper = new("lh:Copper Coin");
                copper.ItemQuantity = 0;
                Item silver = new("lh:Silver Coin");
                silver.ItemQuantity = 0;
                Item gold = new("lh:Gold Coin");
                gold.ItemQuantity = 0;
                Item jade = new("lh:Jade Coin");
                jade.ItemQuantity = 0;

                ItemWrapper copperWrap = new(copper);
                ItemWrapper silverWrap = new(silver);
                ItemWrapper goldWrap = new(gold);
                ItemWrapper jadeWrap = new(jade);

                if (CopperCoins > 0) {
                    copperWrap.item.ItemQuantity = CopperCoins;
                    copperWrap.Position = Position;
                    copperWrap.MapPos = MapPos;
                    CopperCoins = 0;
                }

                if (SilverCoins > 0) {
                    silverWrap.item.ItemQuantity = SilverCoins;
                    silverWrap.Position = Position;
                    silverWrap.MapPos = MapPos;
                    SilverCoins = 0;
                }

                if (GoldCoins > 0) {
                    goldWrap.item.ItemQuantity = GoldCoins;
                    goldWrap.Position = Position;
                    goldWrap.MapPos = MapPos;
                    GoldCoins = 0;
                }

                if (JadeCoins > 0) {
                    jadeWrap.item.ItemQuantity = JadeCoins;
                    jadeWrap.Position = Position;
                    jadeWrap.MapPos = MapPos;
                    JadeCoins = 0;
                }

                if (MineLocation == "None") {
                    if (copper.ItemQuantity > 0)
                        CommandManager.SpawnItem(copperWrap);
                    if (silver.ItemQuantity > 0)
                        CommandManager.SpawnItem(silverWrap);
                    if (gold.ItemQuantity > 0)
                        CommandManager.SpawnItem(goldWrap);
                    if (jade.ItemQuantity > 0)
                        CommandManager.SpawnItem(jadeWrap);
                } else {
                    copperWrap.Position = Position / 12;
                    silverWrap.Position = Position / 12;
                    goldWrap.Position = Position / 12;
                    jadeWrap.Position = Position / 12;
                    if (MineLocation == "Mountain") {
                        if (copper.ItemQuantity > 0)
                            GameLoop.UIManager.Minigames.MineManager.MountainMine.SpawnItem(copperWrap, MineDepth);
                        if (silver.ItemQuantity > 0)
                            GameLoop.UIManager.Minigames.MineManager.MountainMine.SpawnItem(silverWrap, MineDepth);
                        if (gold.ItemQuantity > 0)
                            GameLoop.UIManager.Minigames.MineManager.MountainMine.SpawnItem(goldWrap, MineDepth);
                        if (jade.ItemQuantity > 0)
                            GameLoop.UIManager.Minigames.MineManager.MountainMine.SpawnItem(jadeWrap, MineDepth);
                    } else if (MineLocation == "Lake") {
                        if (copper.ItemQuantity > 0)
                            GameLoop.UIManager.Minigames.MineManager.LakeMine.SpawnItem(copperWrap, MineDepth);
                        if (silver.ItemQuantity > 0)
                            GameLoop.UIManager.Minigames.MineManager.LakeMine.SpawnItem(silverWrap, MineDepth);
                        if (gold.ItemQuantity > 0)
                            GameLoop.UIManager.Minigames.MineManager.LakeMine.SpawnItem(goldWrap, MineDepth);
                        if (jade.ItemQuantity > 0)
                            GameLoop.UIManager.Minigames.MineManager.LakeMine.SpawnItem(jadeWrap, MineDepth);
                    }
                }
            }

            GameLoop.World.Player.UsePixelPositioning = false;

            LivesRemaining--;

            if (LivesRemaining != 0) {

                MoveTo(new Point(35, 6), new Point3D(0, 0, 0));
                CurrentHP = MaxHP;

                if (this == GameLoop.World.Player) {
                    GameLoop.UIManager.AddMsg(new ColoredString("Oh no, you died!", Color.Red, Color.Black));
                    GameLoop.UIManager.AddMsg(new ColoredString("A warm yellow light fills your vision.", Color.Yellow, Color.Black));
                    GameLoop.UIManager.AddMsg(new ColoredString("As it fades, you find yourself in the Cemetary.", Color.Yellow, Color.Black));

                    if (GameLoop.UIManager.selectedMenu == "Minigame") {
                        GameLoop.UIManager.Minigames.ToggleMinigame();
                        GameLoop.UIManager.selectedMenu = "None";
                        GameLoop.UIManager.Minigames.CurrentGame = "None";
                    }
                }

                if (LivesRemaining > 1)
                    GameLoop.UIManager.AddMsg(new ColoredString("You have " + LivesRemaining + " lives left.", Color.Red, Color.Black));
                else if (LivesRemaining == 1)
                    GameLoop.UIManager.AddMsg(new ColoredString("You have " + LivesRemaining + " life left!", Color.Red, Color.Black));



                if (MapPos == GameLoop.World.Player.MapPos && this != GameLoop.World.Player) {
                    GameLoop.UIManager.AddMsg(new ColoredString(Name + " appears in a flash of yellow light!", Color.Yellow, Color.Black));
                }
            } else {
                // Player had lives previously and now is out of lives

                if (GameLoop.NetworkManager != null) {
                    if (GameLoop.NetworkManager.isHost) {
                        GameLoop.SendMessageIfNeeded(new string[] { "hostDead" }, true, false); 
                    } else {
                        GameLoop.SendMessageIfNeeded(new string[] { "outOfLives" }, false, true);
                        GameLoop.NetworkManager.lobbyManager.DisconnectLobby(GameLoop.NetworkManager.lobbyID, (Discord.Result result) => {

                        });
                    }
                }


                // Switch this to a death stats display instead of just dumping to the main menu
                GameLoop.UIManager.MainMenu.RemakeMenu();
                GameLoop.UIManager.selectedMenu = "MainMenu";
                GameLoop.UIManager.MainMenu.MainMenuWindow.IsVisible = true;
                GameLoop.UIManager.Map.MapWindow.IsVisible = false;
                GameLoop.UIManager.Map.MessageLog.IsVisible = false;
                GameLoop.UIManager.Sidebar.BattleLog.IsVisible = false;
                GameLoop.UIManager.Sidebar.SidebarWindow.IsVisible = false;
                GameLoop.UIManager.MainMenu.NameBox.Text = "";


                if (System.IO.Directory.Exists("./saves/" + Name + "/"))
                    System.IO.Directory.Delete("./saves/" + Name + "/", true);
                GameLoop.World.CreatePlayer();

            }
        }


        public int GetToolTier(int Category) {
            if (Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemCategory == Category) {
                return Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemTier;
            }

            return 0;
        }
        
    }
}
