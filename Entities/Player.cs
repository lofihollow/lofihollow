using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using LofiHollow.Managers;
using LofiHollow.EntityData;
using LofiHollow.DataTypes;
using System.Linq;
using Steamworks;

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
        public Item[] Inventory;
        [JsonProperty]
        public Item[] Equipment;

        [JsonProperty]
        public bool OwnsFarm = false;
         
        public double TimeLastTicked = 0; 
        public Stack<ColoredString> killList = new(52);

        public string MineLocation = "None";
        public int MineDepth = 0;
        public bool MineVisible = false;
        public Point MineEnteredAt = new Point(0, 0);

        public string CurrentKillTask = "";
        public int KillTaskProgress = 0;
        public int KillTaskMonLevel = 0;
        public int KillTaskGoal = 0;
        public int KillTaskReward = 0;

        public string CurrentDeliveryTask = "";

        [JsonProperty]
        public int CourierGuildRank = 1;
        [JsonProperty]
        public int AdventurerGuildRank = 1;

        public bool Sleeping = false;

        [JsonProperty]
        public int CopperCoins = 0;
        [JsonProperty]
        public int SilverCoins = 0;
        [JsonProperty]
        public int GoldCoins = 0;
        [JsonProperty]
        public int JadeCoins = 0;

        [JsonProperty]
        public int LivesRemaining = -1;
        [JsonProperty]
        public int DropsOnDeath = -1; // -1: Nothing, 0: Gold, 1: Items and Gold

        [JsonProperty]
        public Dictionary<string, OwnableLocation> OwnedLocations = new();

        [JsonProperty]
        public Apartment NoonbreezeApt;

        [JsonProperty]
        public string InApartment = "None";

        [JsonProperty]
        public int InnDays = 0;


        public bool feed_funny1 = false;
        public bool feed_funny2 = false;
        public bool feed_funny3 = false;
        public bool feed_meticulous1 = false;
        public bool feed_meticulous2 = false;
        public bool feed_ocd = false;
        public bool feed_maturity = false; 

        public double DayStart = 0;

        [JsonProperty]
        public List<SteamId> PartyMembers = new();

        public Player(Color foreground) : base(foreground, '@') {
            ActorGlyph = '@';
             
            Equipment = new Item[11];
            for (int i = 0; i < Equipment.Length; i++) {
                Equipment[i] = Item.Copy("lh:(EMPTY)");
            }

            Inventory = new Item[9];

            for (int i = 0; i < Inventory.Length; i++) {
                Inventory[i] = Item.Copy("lh:(EMPTY)");
            }
        } 

        public CombatParticipant GetCombatParticipant() {
            CombatParticipant part = new();

            CalculateCombatLevel();
            part.Level = CombatLevel;
            part.Attack = Monster.GetStat(Skills["Strength"].Level, AttackGrowth, AttackEXP, part.Level);
            part.Defense = Monster.GetStat(Skills["Defense"].Level, DefenseGrowth, DefenseEXP, part.Level);
            part.Health = Monster.GetStat(Skills["Constitution"].Level, HealthGrowth, HealthEXP, part.Level, true);
            part.Speed = Monster.GetStat(Skills["Agility"].Level, SpeedGrowth, SpeedEXP, part.Level);
            part.MAttack = Monster.GetStat(Skills["Magic"].Level, MAttackGrowth, MAttackEXP, part.Level);
            part.MDefense = Monster.GetStat(Skills["Magic"].Level, MDefenseGrowth, MDefenseEXP, part.Level);

            part.CurrentHP = CurrentHP;
            part.MaxHP = MaxHP; 

            part.Types.Add(ElementalAlignment);

            part.Owner = SteamClient.SteamId;
            part.Name = Name;
            part.ID = "Player";
            part.Species = "Player";
            part.ForeR = ForegroundR;
            part.ForeG = ForegroundG;
            part.ForeB = ForegroundB;
            part.Glyph = ActorGlyph;

            return part;
        }

        public int GetSkillLevel(string which) {
            if (Skills.ContainsKey(which))
                return Skills[which].Level; 
            return -1;
        }

        public void GrantExp(string which, int amount) {
            if (Skills.ContainsKey(which)) {
                int currentLevel = Skills[which].Level;
                Skills[which].GrantExp(amount);

                if (currentLevel != Skills[which].Level && which == "Agility") {
                    UpdateHP();
                    CurrentStamina = Math.Clamp(CurrentStamina + 10, 10, MaxStamina);
                }

                if (currentLevel != Skills[which].Level && which == "Constitution") {
                    UpdateHP();
                    CurrentHP = Math.Clamp(CurrentHP + 1, 1, MaxHP);
                }
            }
        }

        public void GainStatEXP(string which, int amount) {
            int total = HealthEXP + SpeedEXP + AttackEXP + DefenseEXP + MAttackEXP + MDefenseEXP;


            int applying = total + amount > 512 ? (total + amount) - 512 : amount;

            switch (which) {
                case "Health": HealthEXP += applying; break;
                case "Speed": SpeedEXP += applying; break;
                case "Attack": AttackEXP += applying; break;
                case "Defense": DefenseEXP += applying; break;
                case "Magic Attack": MAttackEXP += applying; break;
                case "Magic Defense": MDefenseEXP += applying; break;
            }
        }

        public int CheckRel(string NPC) {
            if (MetNPCs.ContainsKey(NPC)) {
                return MetNPCs[NPC];
            } else {
                return 0;
            }
        }

        public int CopperWealth() {
            return CopperCoins + (SilverCoins * 100) + (GoldCoins * 10000) + (JadeCoins * 1000000);
        }

        public bool HasInventorySlotOpen(string stackID = "") {
            for (int i = 0; i < Inventory.Length; i++) {
                if (Inventory[i].Name == "(EMPTY)" || (Inventory[i].Name == stackID && stackID != "")) {
                    return true;
                }
            }
            return false;
        }

        public string KillFeed() {
            string list = "";

            List<ColoredString> kills = killList.ToList();

            kills.Reverse();

            for (int i = 0; i < kills.Count; i++) {
                list += kills[i].String;
            }

            return list;
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
                Item copper = Item.Copy("lh:Copper Coin");
                copper.ItemQuantity = 0;
                Item silver = Item.Copy("lh:Silver Coin");
                silver.ItemQuantity = 0;
                Item gold = Item.Copy("lh:Gold Coin");
                gold.ItemQuantity = 0;
                Item jade = Item.Copy("lh:Jade Coin");
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

            LivesRemaining--;

            if (LivesRemaining != 0) {

                MoveTo(new Point(35, 6), new Point3D(0, 0, 0));
                CurrentHP = MaxHP;

                if (this == GameLoop.World.Player) {
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



                if (MapPos == GameLoop.World.Player.MapPos && this != GameLoop.World.Player) {
                    GameLoop.UIManager.AddMsg(new ColoredString(Name + " appears in a flash of yellow light!", Color.Yellow, Color.Black));
                }
            } else {
                // Player had lives previously and now is out of lives

                if (GameLoop.NetworkManager != null) {
                    if (GameLoop.NetworkManager.isHost) {
                        NetMsg hostDied = new("hostDead");
                        GameLoop.SendMessageIfNeeded(hostDied, true, false); 
                    } else {
                        NetMsg outOfLives = new("outOfLives");
                        GameLoop.SendMessageIfNeeded(outOfLives, false, true);
                        
                        GameLoop.NetworkManager.LeaveLobby();
                    }
                }


                // Switch this to a death stats display instead of just dumping to the main menu
                GameLoop.UIManager.MainMenu.RemakeMenu();
                GameLoop.UIManager.selectedMenu = "MainMenu";
                GameLoop.UIManager.MainMenu.MainMenuWindow.IsVisible = true;
                GameLoop.UIManager.Map.MapWindow.IsVisible = false; 
                GameLoop.UIManager.Sidebar.SidebarWindow.IsVisible = false;
                GameLoop.UIManager.MainMenu.NameBox.Text = "";


                if (System.IO.Directory.Exists("./saves/" + Name + "/"))
                    System.IO.Directory.Delete("./saves/" + Name + "/", true);
                GameLoop.World.CreatePlayer();

            }
        }

        public int HighestToolTier(string Category) {
            int highest = 0;

            for (int i = 0; i < Inventory.Length; i++) {
                if (Inventory[i].ItemCat == Category) {
                    if (Inventory[i].ItemTier > highest) {
                        if (Skills.ContainsKey(Inventory[i].ItemSkill) && Skills[Inventory[i].ItemSkill].Level >= Inventory[i].ItemTier) {
                            highest = Inventory[i].ItemTier;
                        }
                    }
                }
            }

            return highest;
        }

        public bool HasSoulPhoto() {
            for (int i = 0; i < Inventory.Length; i++) {
                if (Inventory[i].SoulPhoto != null)
                    return true;
            }

            return false;
        }
        public int GetToolTier(string Category) {
            if (Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemCat == Category) {
                return Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemTier;
            }

            return 0;
        }

        public void CalculateCombatLevel() {
            int Attack = Skills["Attack"].Level;
            int Strength = Skills["Strength"].Level;
            int Defense = Skills["Defense"].Level;
            int Constitution = Skills["Constitution"].Level;

            int CombatStat = Attack + Strength;

            CombatLevel = (int)Math.Floor((double)(((13 / 10) * CombatStat) + Defense + Constitution) / 4);
        }

        public void UpdateHP() {
            CalculateCombatLevel();
            MaxHP = Monster.GetStat(Skills["Constitution"].Level, HealthGrowth, HealthEXP, CombatLevel, true);
            MaxStamina = (Skills["Agility"].Level * 10) + 10;
        }

        public string GetDamageType() {
            if (Equipment[0].Stats != null) {
                return Equipment[0].Stats.DamageType;
            }

            return ElementalAlignment;
        }

    }
}
