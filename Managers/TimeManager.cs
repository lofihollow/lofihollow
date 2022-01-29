using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool AM = true;

        public string GetSeason() {
            if (Month == 1) { return "Spring"; } 
            else if (Month == 2) { return "Summer"; }
            else if (Month == 3) { return "Fall"; } 
            else if (Month == 4) { return "Winter"; } 
            else { return "Holiday"; }
        }

        public bool IsItThisDay(int month, int day) { return (Month == month && Day == day); }

        public int GetCurrentTime() {
            int total = Minutes + (Hours * 60);

            if (!AM)
                total += (12 * 60);

            return total;
        }

        public string MinutesToTime(int minutes) {
            return (minutes / 60) + ":" + (minutes % 60);
        }

        public void NextDay(bool passedOut) {
            DailyUpdates();

            if (passedOut) {
                Hours = 9;
                AM = true;
                Minutes = 0;

                GameLoop.UIManager.AddMsg(new ColoredString("You passed out!", Color.Red, Color.Black));
            } else {
                Hours = 6;
                Minutes = 0;
                AM = true;

                GameLoop.UIManager.AddMsg(new ColoredString("You sleep through the night.", Color.Lime, Color.Black));
            }

            GameLoop.SendMessageIfNeeded(new string[] { "newDay", passedOut.ToString() }, false, false);

            GameLoop.World.Player.Sleeping = false;
            GameLoop.World.Player.CurrentHP = GameLoop.World.Player.MaxHP;
            GameLoop.World.Player.CurrentStamina = GameLoop.World.Player.MaxStamina;

            foreach (KeyValuePair<long, Player> kv in GameLoop.World.otherPlayers) {
                kv.Value.CurrentHP = kv.Value.MaxHP;
                kv.Value.CurrentStamina = kv.Value.MaxStamina;
                kv.Value.Sleeping = false;
            }

            GameLoop.World.SavePlayer();
        }


        public void TickTime() {
            Minutes++;
            if (Minutes >= 60) {
                Hours++;
                Minutes = 0;

                if (GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(GameLoop.World.Player.Position).Name == "Bed") {
                    GameLoop.UIManager.AddMsg(new ColoredString("Press " + ((char) 12) + " while standing on a bed to sleep.", Color.Yellow, Color.Black));
                }

                if (Hours == 12) {
                    AM = !AM;
                    if (AM) {
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
                if (Hours > 12) {
                    Hours = 1;
                }
            }

            bool tickedDay = false;
            if (Hours == 2 && AM) { // Time to pass out
                NextDay(true);
                tickedDay = true;
            }

            if (!tickedDay) {
                int sleepCount = 0;
                int totalPlayers = 1;
                if (GameLoop.World.Player.Sleeping)
                    sleepCount++;

                foreach (KeyValuePair<long, Player> kv in GameLoop.World.otherPlayers) {
                    totalPlayers++;
                    if (kv.Value.Sleeping)
                        sleepCount++;
                }

                if (sleepCount == totalPlayers) {
                    if (!((Hours == 12 || Hours == 1 || Hours == 2) && AM)) {
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

            string time = JsonConvert.SerializeObject(this, Formatting.Indented);
            GameLoop.SendMessageIfNeeded(new string[] { "time", time.ToString() }, false, false);
        }


        public void DailyUpdates() {
            foreach (KeyValuePair<int, NPC> kv in GameLoop.World.npcLibrary) {
                kv.Value.ReceivedGiftToday = false;
            }

            if (!GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0)))
                GameLoop.World.LoadMapAt(new Point3D(-1, 0, 0));

            for (int i = 0; i < GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles.Length; i++) {
                TileBase tile = GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles[i];

                if (tile.Plant != null) {
                    tile.Plant.DayUpdate();
                    tile.UpdateAppearance();
                    int x = i % GameLoop.MapWidth;
                    int y = i / GameLoop.MapWidth;

                    GameLoop.SendMessageIfNeeded(new string[] { "updateTile", x.ToString(), y.ToString(), "-1;0;0" }, false, false); 
                }
            }


            GameLoop.World.Player.MapsClearedToday = 0;
            GameLoop.World.Player.killList.Clear();

            foreach (KeyValuePair<long, Player> kv in GameLoop.World.otherPlayers) {
                kv.Value.MapsClearedToday = 0;
            }

            GameLoop.UIManager.Minigames.MineManager.MountainMine = new("Mountain");
            GameLoop.UIManager.Minigames.MineManager.LakeMine = new("Lake");
               
            GameLoop.UIManager.Map.LoadMap(GameLoop.World.Player.MapPos);
        }
    }
}
