using System;
using System.Collections.Generic;
using SadConsole;
using SadRogue.Primitives;
using LofiHollow.Managers;
using ProtoBuf;
using Newtonsoft.Json;

namespace LofiHollow.Entities {
    [ProtoContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class Player : Actor {
        [ProtoMember(1)]
        public TimeManager Clock = new();

        [ProtoMember(2)]
        public Dictionary<string, int> MetNPCs = new();

        [ProtoMember(3)]
        public Dictionary<string, Missions.Mission> MissionLog = new();


        [ProtoMember(4)]
        public Item[] Inventory;
        [ProtoMember(5)]
        public Item[] Equipment;

        [ProtoMember(6)]
        public bool OwnsFarm = false;

        [ProtoMember(7)]
        public int CourierGuildRank = 1;
        [ProtoMember(8)]
        public int AdventurerGuildRank = 1;

        [ProtoMember(9)]
        public int CopperCoins = 0;
        [ProtoMember(10)]
        public int SilverCoins = 0;
        [ProtoMember(11)]
        public int GoldCoins = 0;
        [ProtoMember(12)]
        public int JadeCoins = 0;

        [ProtoMember(13)]
        public int LivesRemaining = -1;
        [ProtoMember(14)]
        public int DropsOnDeath = -1; // -1: Nothing, 0: Gold, 1: Items and Gold

        [JsonIgnore]
        public List<Point3D> VisitedMaps = new();
        [JsonIgnore]
        public int MapsClearedToday = 0;
        [JsonIgnore]
        public Stack<ColoredString> killList = new(52);
        [JsonIgnore]
        public string MineLocation = "None";
        [JsonIgnore]
        public int MineDepth = 0;
        [JsonIgnore]
        public bool MineVisible = false;
        [JsonIgnore]
        public Point MineEnteredAt = new Point(0, 0);
        [JsonIgnore]
        public string CurrentKillTask = "";
        [JsonIgnore]
        public int KillTaskProgress = 0;
        [JsonIgnore]
        public int KillTaskGoal = 0;
        [JsonIgnore]
        public string CurrentDeliveryTask = "";
        [JsonIgnore]
        public bool Sleeping = false;

        public Player() { }

        public Player(bool newPlayer) {
            ActorGlyph = '@';

            Equipment = new Item[10];
            for (int i = 0; i < Equipment.Length; i++) {
                Equipment[i] = new Item("lh:(EMPTY)");
            }

            Inventory = new Item[9];

            for (int i = 0; i < Inventory.Length; i++) {
                Inventory[i] = new Item("lh:(EMPTY)");
            }

            ForegroundR = 127;
            ForegroundG = 127;
            ForegroundB = 127;
            Position = new(25, 25);
            MapPos = new(3, 1, 0);
            Name = "Player";
        }

        public bool HasInventorySlotOpen(string stackID = "") {
            for (int i = 0; i < Inventory.Length; i++) {
                if (Inventory[i].Name == "(EMPTY)" || (Inventory[i].Name == stackID && stackID != "")) {
                    return true;
                }
            }
            return false;
        }


        public void PlayerDied() {
            if (MapPos == GameLoop.World.Player.player.MapPos && this != GameLoop.World.Player.player) {
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
                    copperWrap.item.Position = Position;
                    copperWrap.item.MapPos = MapPos;
                    CopperCoins = 0;
                }

                if (SilverCoins > 0) {
                    silverWrap.item.ItemQuantity = SilverCoins;
                    silverWrap.item.Position = Position;
                    silverWrap.item.MapPos = MapPos;
                    SilverCoins = 0;
                }

                if (GoldCoins > 0) {
                    goldWrap.item.ItemQuantity = GoldCoins;
                    goldWrap.item.Position = Position;
                    goldWrap.item.MapPos = MapPos;
                    GoldCoins = 0;
                }

                if (JadeCoins > 0) {
                    jadeWrap.item.ItemQuantity = JadeCoins;
                    jadeWrap.item.Position = Position;
                    jadeWrap.item.MapPos = MapPos;
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

                if (this == GameLoop.World.Player.player) {
                    GameLoop.UIManager.AddMsg(new ColoredString("Oh no, you died!", Color.Red, Color.Black));
                    GameLoop.UIManager.AddMsg(new ColoredString("A warm yellow light fills your vision.", Color.Yellow, Color.Black));
                    GameLoop.UIManager.AddMsg(new ColoredString("As it fades, you find yourself in the Cemetary.", Color.Yellow, Color.Black));

                    if (GameLoop.UIManager.selectedMenu == "Minigame") {
                        GameLoop.UIManager.Minigames.ToggleMinigame("None");
                        GameLoop.UIManager.selectedMenu = "None";
                    }
                }

                if (LivesRemaining > 1)
                    GameLoop.UIManager.AddMsg(new ColoredString("You have " + LivesRemaining + " lives left.", Color.Red, Color.Black));
                else if (LivesRemaining == 1)
                    GameLoop.UIManager.AddMsg(new ColoredString("You have " + LivesRemaining + " life left!", Color.Red, Color.Black));



                if (MapPos == GameLoop.World.Player.player.MapPos && this != GameLoop.World.Player.player) {
                    GameLoop.UIManager.AddMsg(new ColoredString(Name + " appears in a flash of yellow light!", Color.Yellow, Color.Black));
                }
            } else {
                // Player had lives previously and now is out of lives

                if (GameLoop.NetworkManager != null) {
                    if (GameLoop.NetworkManager.isHost) {
                        NetMsg died = new("hostDead", null);
                        GameLoop.SendMessageIfNeeded(died, true, false);
                    } else {
                        NetMsg died = new("outOfLives", null);
                        GameLoop.SendMessageIfNeeded(died, false, true);
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


        public int GetToolTier(string Category) {
            if (Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemCat == Category) {
                return Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemTier;
            }

            return 0;
        }

    }
}
