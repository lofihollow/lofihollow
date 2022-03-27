using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives; 
using System.Collections.Generic;
using LofiHollow.DataTypes;
using Steamworks;
using System;

namespace LofiHollow.Managers {
    [JsonObject(MemberSerialization.OptIn)]
    public class TimeManager {
        [JsonProperty]
        public int Hours = 9;
        [JsonProperty]
        public int Minutes = 0;
        [JsonProperty]
        public int Month = 1;
        [JsonProperty]
        public int Day = 1;
        [JsonProperty]
        public int Year = 1;

        [JsonProperty]
        public bool Raining = false;

        public string GetSeason() {
            if (Month == 1) { return "Spring"; } 
            else if (Month == 2) { return "Summer"; }
            else if (Month == 3) { return "Fall"; } 
            else if (Month == 4) { return "Winter"; } 
            else { return "Holiday"; }
        }

        public bool IsItThisDay(int month, int day) { return (Month == month && Day == day); }

        public int GetCurrentTime() { 
            return Minutes + (Hours * 60);
        }

        public static string MinutesToTime(int minutes) {
            return (minutes / 60) + ":" + (minutes % 60);
        }

        public void NextDay(bool passedOut) {
            DailyUpdates();

            if (passedOut) {
                Hours = 9; 
                Minutes = 0;

                GameLoop.UIManager.AddMsg(new ColoredString("You passed out!", Color.Red, Color.Black));
            } else {
                Hours = 6;
                Minutes = 0; 

                GameLoop.UIManager.AddMsg(new ColoredString("You sleep through the night.", Color.Lime, Color.Black));
            }

            NetMsg newDay = new("newDay");
            newDay.Flag = passedOut;
            GameLoop.SendMessageIfNeeded(newDay, false, false);

            GameLoop.World.Player.UpdateHP();
            GameLoop.World.Player.Sleeping = false;
            GameLoop.World.Player.CurrentHP = passedOut ? GameLoop.World.Player.CurrentHP : GameLoop.World.Player.MaxHP;
            GameLoop.World.Player.CurrentStamina = passedOut ? Math.Max(GameLoop.World.Player.CurrentStamina, GameLoop.World.Player.MaxStamina / 2) : GameLoop.World.Player.MaxStamina;

            foreach (KeyValuePair<SteamId, Player> kv in GameLoop.World.otherPlayers) {
                kv.Value.CurrentHP = kv.Value.MaxHP;
                kv.Value.CurrentStamina = kv.Value.MaxStamina;
                kv.Value.Sleeping = false;
            } 

            GameLoop.UIManager.Photo.PopulateJobList();
            GameLoop.UIManager.AdventurerBoard.PopulateJobList();

            if (GameLoop.World.Player.NoonbreezeApt != null) {
                GameLoop.World.Player.NoonbreezeApt.DaysLeft--;

                if (GameLoop.World.Player.NoonbreezeApt.DaysLeft == 0) {
                    GameLoop.UIManager.AddMsg("This is your last day of rent, pay for another month!");
                }
            }

            GameLoop.World.Player.feed_funny1 = false;
            GameLoop.World.Player.feed_funny2 = false;
            GameLoop.World.Player.feed_funny3 = false;
            GameLoop.World.Player.feed_meticulous1 = false;
            GameLoop.World.Player.feed_meticulous2 = false;
            GameLoop.World.Player.feed_ocd = false;

            Map farm = Helper.ResolveMap(new Point3D("Overworld", -1, 0, 0));
            List<FarmAnimal> died = new();

            if (farm != null) {
                foreach (Entity ent in farm.Entities.Items) {
                    if (ent is FarmAnimal farmAnimal) {
                        if (!farmAnimal.FedToday) {
                            farmAnimal.Happiness -= 50;

                            if (farmAnimal.Happiness <= 0) {
                                farmAnimal.Happiness = 0;
                                if (!farmAnimal.Sick) {
                                    farmAnimal.Sick = true;
                                } else {
                                    if (GameLoop.rand.Next(2) == 1) {
                                        died.Add(farmAnimal);
                                    } 
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < died.Count; i++) {
                    farm.GetTile(died[i].RestSpot).AnimalBed.Animal = null;
                    farm.Entities.Remove(died[i]);
                    GameLoop.UIManager.Map.EntityRenderer.Remove(died[i]);
                    GameLoop.UIManager.AddMsg(died[i].Nickname + " passed away due to sickness.");
                }
            }


            if (GameLoop.World.Player.NoonbreezeApt != null) {
                Map apt = GameLoop.World.Player.NoonbreezeApt.map;
                List<FarmAnimal> aptDied = new();

                if (apt != null) {
                    foreach (Entity ent in apt.Entities.Items) {
                        if (ent is FarmAnimal farmAnimal) {
                            if (!farmAnimal.FedToday) {
                                farmAnimal.Happiness -= 50;

                                if (farmAnimal.Happiness <= 0) {
                                    farmAnimal.Happiness = 0;
                                    if (!farmAnimal.Sick) {
                                        farmAnimal.Sick = true;
                                    }
                                    else {
                                        if (GameLoop.rand.Next(2) == 1) {
                                            aptDied.Add(farmAnimal);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (int i = 0; i < aptDied.Count; i++) {
                        apt.GetTile(aptDied[i].RestSpot).AnimalBed.Animal = null;
                        apt.Entities.Remove(aptDied[i]);
                        GameLoop.UIManager.Map.EntityRenderer.Remove(aptDied[i]);
                        GameLoop.UIManager.AddMsg(aptDied[i].Nickname + " passed away due to sickness.");
                    }
                }
            }

            foreach (KeyValuePair<SteamId, Player> kv in GameLoop.World.otherPlayers) {
                if (kv.Value.NoonbreezeApt != null) {
                    Map apt = kv.Value.NoonbreezeApt.map;
                    List<FarmAnimal> aptDied = new();

                    if (apt != null) {
                        foreach (Entity ent in apt.Entities.Items) {
                            if (ent is FarmAnimal farmAnimal) {
                                if (!farmAnimal.FedToday) {
                                    farmAnimal.Happiness -= 50;

                                    if (farmAnimal.Happiness <= 0) {
                                        farmAnimal.Happiness = 0;
                                        if (!farmAnimal.Sick) {
                                            farmAnimal.Sick = true;
                                        }
                                        else {
                                            if (GameLoop.rand.Next(2) == 1) {
                                                aptDied.Add(farmAnimal);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < aptDied.Count; i++) {
                            apt.GetTile(aptDied[i].RestSpot).AnimalBed.Animal = null;
                            apt.Entities.Remove(aptDied[i]);
                            GameLoop.UIManager.Map.EntityRenderer.Remove(aptDied[i]);
                            GameLoop.UIManager.AddMsg(aptDied[i].Nickname + " passed away due to sickness.");
                        }
                    }
                }
            }



            if (GameLoop.World.Player.MapPos == new Point3D("Overworld", 1, 1, 1)) {
                if (GameLoop.World.Player.CheckRel("Ash") < 50 && GameLoop.World.Player.InnDays > 0) {
                    GameLoop.World.Player.InnDays--;
                    if (GameLoop.World.Player.InnDays == 1) {
                        GameLoop.UIManager.AddMsg(new ColoredString("You can stay in the inn one more night.", Color.Green, Color.Black));
                    } else if (GameLoop.World.Player.InnDays > 1) {
                        GameLoop.UIManager.AddMsg(new ColoredString("You can stay in the inn " + GameLoop.World.Player.InnDays + " more nights.", Color.Green, Color.Black));
                    }
                }
                else if (GameLoop.World.Player.CheckRel("Ash") >= 50) {
                    GameLoop.World.Player.MetNPCs["Ash"] -= 10;
                }
            }


            foreach (KeyValuePair<Point3D, Map> kv in GameLoop.World.maps) {
                for (int i = 0; i < kv.Value.Tiles.Length; i++) {
                    if (kv.Value.Tiles[i].SkillableTile != null) {
                        kv.Value.Tiles[i].Name = kv.Value.Tiles[i].SkillableTile.HarvestableName;
                    }
                }
            }



            GameLoop.World.SavePlayer();
        }


        public void TickTime() {
            Minutes++;
            if (Minutes >= 60) {
                Hours++;
                Minutes = 0;

                if (GameLoop.World.Player.MineLocation == "None") {
                    Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);
                    Point cellPos = GameLoop.World.Player.Position;
                    if (map != null) {
                        if (map.GetTile(cellPos).Name == "Bed") {
                            GameLoop.UIManager.AddMsg(new ColoredString("Press " + ((char)12) + " while standing on a bed to sleep.", Color.Yellow, Color.Black));
                        }
                    }
                }

                if (Hours == 24) {
                    GameLoop.UIManager.AddMsg(new ColoredString("You're getting a little tired.", Color.Yellow, Color.Black));
                }

                if (Hours == 25) {
                    GameLoop.UIManager.AddMsg(new ColoredString("You can't keep your eyes open much longer.", Color.Red, Color.Black));
                }

                if (Hours == 24) { 
                    Day++;

                    if ((Day > 28 && Month < 5) || (Day > 7 && Month == 5)) {
                        Day = 1;
                        Month++;

                        if (Month > 5) {
                            Month = 1;
                            Year++;
                        }
                    } 
                }
            }

            bool tickedDay = false;
            if (Hours >= 26) { // Time to pass out
                NextDay(true);
                tickedDay = true;
            }

            if (!tickedDay) {
                int sleepCount = 0;
                int totalPlayers = 1;
                if (GameLoop.World.Player.Sleeping)
                    sleepCount++;

                foreach (KeyValuePair<SteamId, Player> kv in GameLoop.World.otherPlayers) {
                    totalPlayers++;
                    if (kv.Value.Sleeping)
                        sleepCount++;
                }

                if (sleepCount == totalPlayers) {
                    if (Hours < 24) {
                        Day++;

                        if ((Day > 28 && Month < 5) || (Day > 7 && Month == 5)) {
                            Day = 1;
                            Month++;

                            if (Month > 5) {
                                Month = 1;
                                Year++;
                            }
                        }
                    }

                    NextDay(false);
                }
            }

            NetMsg timeUpdate = new("time", this.ToByteArray()); 
            GameLoop.SendMessageIfNeeded(timeUpdate, false, false);
        }


        public void DailyUpdates() {
            foreach (KeyValuePair<string, NPC> kv in GameLoop.World.npcLibrary) {
                kv.Value.ReceivedGiftToday = false;
            }

            if (!GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0)))
                GameLoop.World.LoadMapAt(new Point3D(-1, 0, 0));

            for (int i = 0; i < GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles.Length; i++) {
                Tile tile = GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles[i];

                if (tile.Plant != null) {
                    tile.Plant.DayUpdate();
                    tile.UpdateAppearance();
                    int x = i % GameLoop.MapWidth;
                    int y = i / GameLoop.MapWidth;

                    NetMsg updateTile = new("updateTile");
                    updateTile.SetFullPos(new Point(x, y), new Point3D(-1, 0, 0));
                    GameLoop.SendMessageIfNeeded(updateTile, false, false); 
                }
            }


            GameLoop.World.Player.killList.Clear();

            if (GameLoop.UIManager.Minigames.MineManager != null) {
                GameLoop.UIManager.Minigames.MineManager.MountainMine = new("Mountain");
                GameLoop.UIManager.Minigames.MineManager.LakeMine = new("Lake");
            }

            GameLoop.UIManager.Map.LoadMap(GameLoop.World.Player.MapPos);
        }
    }
}
