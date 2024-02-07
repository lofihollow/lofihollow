using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using LofiHollow.Managers; 
using LofiHollow.DataTypes;
using System.Linq;
using Steamworks;

namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptOut)]
    public class Player : StatBall { 
        public TimeManager Clock = new(); 

        public List<Item> Inventory;
        public int InventoryCapacity = 9;
         
        [JsonIgnore]
        public double TimeLastTicked = 0;

        public Dictionary<string, int> MiscData = new();
        public Dictionary<string, uint> Timers = new();
         
         
        public int Zeri = 0;
        public int MagitechAptitude = 0;
        public int AlignmentLaw = 0;
        public int AlignmentGood = 0;

        public int Strength = 10;
        public int Dexterity = 10;
        public int Constitution = 10;
        public int Intelligence = 10;
        public int Wisdom = 10;
        public int Charisma = 10;

        public string StartScenario = "Shipwrecked";

        public string Race = "Human";
        public string SecondaryRace = "";

        public bool RealTime = true;
        public bool CanFastTravel = true;
        public bool CanUseShops = true;
        public bool CanUseArmor = true;
        public bool CanUseWeapons = true;
        public bool CanUseMagic = true;
        public bool BlackoutIsDeath = false;

        public Dictionary<string, int> NeedBars = new();
        public bool HungerEnabled = true;
        public int HungerBar = 100;
        public bool ThirstEnabled = false;
        public int ThirstBar = 100;
        public bool ThermalEnabled = false;
        public int ThermalBar = 100;
        public bool BathroomEnabled = true;
        public int BathroomBar = 100;
        public bool HygieneEnabled = true;
        public int HygieneBar = 100;
        public bool EntertainmentEnabled = false;
        public int EntertainmentBar = 100;
        public bool SocialEnabled = false;
        public int SocialBar = 100;
        public bool EnvironmentEnabled = false;
        public int EnvironmentBar = 100;
        public bool SleepEnabled = false;
        public int SleepBar = 100;

        public int WakeupHour = 6;
        public int BlackoutHour = 24;
        public bool BlackoutsOn = true;

        public int DropsOnDeath = -1;
        public int DropsOnBlackout = -1;
        public int LivesRemaining = -1; 

        [JsonIgnore]
        public double DayStart = 0; 

        public Player(Color foreground) : base(foreground) {
            Inventory = new();

            NeedBars.Add("Hunger", 100);
            NeedBars.Add("Thirst", 100);
            NeedBars.Add("Thermal", 100);
            NeedBars.Add("Bathroom", 100);
            NeedBars.Add("Hygiene", 100);
            NeedBars.Add("Entertainment", 100);
            NeedBars.Add("Social", 100);
            NeedBars.Add("Environment", 100);
            NeedBars.Add("Sleep", 100);
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
            }
        }
         

        public int CheckRel(string NPC) {
            if (Helper.PlayerHasData("NPC_" + NPC)) {
                return Helper.GetPlayerData("NPC_" + NPC);
            }
            return 0; 
        } 

        public void ModRel(string NPC, int amount) {
            Helper.ModifyPlayerData("NPC_" + NPC, amount);
        }

        public bool HasInventorySlotOpen(string stackID = "") {
            for (int i = 0; i < Inventory.Count; i++) {
                if (Inventory[i].Name == stackID && stackID != "") {
                    return true;
                }
            }

            if (Inventory.Count < InventoryCapacity)
                return true;

            return false;
        }
         

        public void PlayerDied() { 
            GameLoop.UIManager.AddMsg(new ColoredString(Name + " died!", Color.Red, Color.Black)); 

            if (DropsOnDeath == 1) {
                for (int i = 0; i < Inventory.Count; i++) {
                    if (Inventory[i].Name != "(EMPTY)") {
                        // TODO: Rewrite item drop on death
                    }
                } 
            }

            if (DropsOnDeath >= 0) {
                Item zeri = Item.Copy("lh:Zeri");
                zeri.Quantity = 0;  

                if (Zeri > 0) {
                    zeri.Quantity = Zeri;
                    Zeri = 0;
                    
                } 
                 
                if (zeri.Quantity > 0) { 
                    // TODO: Drop money on death
                }
            } 

            LivesRemaining--;

            if (LivesRemaining != 0) {
                NavLoc = RespawnLoc;
                CurrentHP = MaxHP;

                if (this == GameLoop.World.Player) {
                    GameLoop.UIManager.AddMsg(new ColoredString("Oh no, you died!", Color.Red, Color.Black));
                    GameLoop.UIManager.AddMsg(new ColoredString("A warm yellow light fills your vision.", Color.Yellow, Color.Black));
                    GameLoop.UIManager.AddMsg(new ColoredString("As it fades, you find yourself in the Cemetary.", Color.Yellow, Color.Black));
 
                }

                if (LivesRemaining > 1)
                    GameLoop.UIManager.AddMsg(new ColoredString("You have " + LivesRemaining + " lives left.", Color.Red, Color.Black));
                else if (LivesRemaining == 1)
                    GameLoop.UIManager.AddMsg(new ColoredString("You have " + LivesRemaining + " life left!", Color.Red, Color.Black));



                if (NavLoc == GameLoop.World.Player.NavLoc && this != GameLoop.World.Player) {
                    GameLoop.UIManager.AddMsg(new ColoredString(Name + " appears in a flash of yellow light!", Color.Yellow, Color.Black));
                }
            } else {
                // Player had lives previously and now is out of lives
                 

                // Switch this to a death stats display instead of just dumping to the main menu 
                GameLoop.UIManager.HandleMenuChange(true); 


                if (System.IO.Directory.Exists("./saves/" + Name + "/"))
                    System.IO.Directory.Delete("./saves/" + Name + "/", true);
                GameLoop.World.CreatePlayer();

            }
        }

        public void CalculateAptitude() {
            int magicMax = 1;
            int techMax = 1;

            foreach (var kv in Skills) {
                if (kv.Value.IsMagic) {
                    if (magicMax < kv.Value.Level) {
                        magicMax = kv.Value.Level;
                    }
                }

                if (kv.Value.IsTech) {
                    if (techMax < kv.Value.Level) {
                        techMax = kv.Value.Level;
                    }
                }
            }

            int tempApt = magicMax - techMax;
            
            if (tempApt != 0) {
                if (tempApt < 0)
                    tempApt -= 2;
                else
                    tempApt += 2;
            }

            MagitechAptitude = tempApt; 
        } 

        public void TryMoveTo(ConnectionNode node) {
            if (node.AllConditionsPassed() || GameLoop.DevMode) {
                NavLoc = node.LocationID;
                GameLoop.UIManager.Nav.MovedMaps();
            }
        }

        public void TryUseObject(NodeObject? obj, string locID) {
            if (obj != null) { 
                if (obj.zeriCost != 0)
                    Zeri -= obj.zeriCost;

                if (obj.timeCost > 0) {
                    for (int i = 0; i < obj.timeCost; i++) {
                        Clock.TickTime();
                    }
                }

                if (obj.TimerID != null && obj.TimerID != "") {
                    Timers.Add(locID + ";" + obj.TimerID, Math.Max(1, obj.TimerHours));
                }

                if (obj.DataMods.Count > 0) {
                    for (int i = 0; i < obj.DataMods.Count; i++) {
                        obj.DataMods[i].DoMod();
                    }
                } 

                if (obj.ExpGranted.Count > 0) {
                    for (int i = 0; i < obj.ExpGranted.Count; i++) {
                        string[] split = obj.ExpGranted[i].Split(";");
                        if (split.Length >= 2) {
                            int exp = int.Parse(split[1]);

                            TryAddExp(exp, split[0]);
                        }
                    }
                }

                if (obj.ActionString != null && obj.ActionString != "") {
                    if (obj.ActionString == "fullSleep") {
                        Clock.NextDay(false);
                    }
                }

                if (obj.UseMessage != null && obj.UseMessage != "") {
                    GameLoop.UIManager.AddMsg(new ColoredString(obj.UseMessage, new Color(obj.UseMR, obj.UseMG, obj.UseMB), Color.Black));
                }
            } 
        }

        public void TryPickupNodeItem(NodeItem item) {
            if (item != null) { 
                GameLoop.UIManager.AddMsg("dong " + item.itemID);
            }
        }

        public void TrySpeakNPC(NPC.NPC target) {
            GameLoop.UIManager.AddMsg("ping " + target.Name);
        }

        public void PickupItem(Location loc, int index) {
            if (loc.ItemsOnGround.Count > index) {
                Item toGrab = loc.ItemsOnGround[index];

                loc.ItemsOnGround.RemoveAt(index);
                AddItemToInventory(toGrab); 
            }
        }

        public void AddItemToInventory(Item toGrab) {
            if (HasInventorySlotOpen(toGrab.FullName())) {  
                for (int i = 0; i < Inventory.Count; i++) {
                    if (Inventory[i].StacksWith(toGrab)) {
                        Inventory[i].Quantity += toGrab.Quantity;
                        return;
                    }
                }

                if (Inventory.Count < InventoryCapacity) {
                    Inventory.Add(toGrab);
                    return;
                }
            }

            Location? loc = Helper.ResolveLoc(NavLoc);
            loc.ItemsOnGround.Add(toGrab);
        }
    }
}
