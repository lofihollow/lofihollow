using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using ProtoBuf;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Managers {
    [ProtoContract]
    public class TimeManager {
        [ProtoMember(1)]
        public int Hours = 9;
        [ProtoMember(2)]
        public int Minutes = 0;
        [ProtoMember(3)]
        public int Month = 1;
        [ProtoMember(4)]
        public int Day = 1;
        [ProtoMember(5)]
        public int Year = 1;
        [ProtoMember(6)]
        public bool AM = true;

        public string GetSeason() {
            if (Month == 1) { return "Spring"; } else if (Month == 2) { return "Summer"; } else if (Month == 3) { return "Fall"; } else if (Month == 4) { return "Winter"; } else { return "Holiday"; }
        }

        public bool IsItThisDay(int month, int day) { return (Month == month && Day == day); }

        public int GetCurrentTime() {
            int total = Minutes + (Hours * 60);

            if (!AM && Hours != 12 || (AM && Hours == 12) || (AM && Hours == 1) || (AM && Hours == 2))
                total += (12 * 60);
            if ((AM && Hours == 1) || (AM && Hours == 2))
                total += (12 * 60);

            return total;
        }

        public static string MinutesToTime(int minutes) {
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

            NetMsg msg = new("newDay", passedOut.ToByteArray());
            GameLoop.SendMessageIfNeeded(msg, false, false);

            GameLoop.World.Player.player.Sleeping = false;
            GameLoop.World.Player.player.CurrentHP = GameLoop.World.Player.player.MaxHP;
            GameLoop.World.Player.player.CurrentStamina = GameLoop.World.Player.player.MaxStamina;

            foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
                kv.Value.player.CurrentHP = kv.Value.player.MaxHP;
                kv.Value.player.CurrentStamina = kv.Value.player.MaxStamina;
                kv.Value.player.Sleeping = false;
            }

            GameLoop.UIManager.Photo.PopulateJobList();

            GameLoop.World.SavePlayer();
        }


        public void TickTime() {
            Minutes++;
            if (Minutes >= 60) {
                Hours++;
                Minutes = 0;

                if (GameLoop.World.Player.player.MineLocation == "None") {
                    if (GameLoop.World.maps[GameLoop.World.Player.player.MapPos].GetTile(GameLoop.World.Player.player.Position).Name == "Bed") {
                        GameLoop.UIManager.AddMsg(new ColoredString("Press " + ((char)12) + " while standing on a bed to sleep.", Color.Yellow, Color.Black));
                    }
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
                if (GameLoop.World.Player.player.Sleeping)
                    sleepCount++;

                foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
                    totalPlayers++;
                    if (kv.Value.player.Sleeping)
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

            NetMsg timeUpdate = new("time", this.ToByteArray());
            GameLoop.SendMessageIfNeeded(timeUpdate, false, false);
        }


        public void DailyUpdates() {
            foreach (KeyValuePair<string, NPCWrapper> kv in GameLoop.World.npcLibrary) {
                kv.Value.npc.ReceivedGiftToday = false;
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

                    NetMsg tileUpdate = new("updateTile", tile.ToByteArray());
                    tileUpdate.X = x;
                    tileUpdate.Y = y;
                    tileUpdate.mX = -1;
                    tileUpdate.mY = 0;
                    tileUpdate.mZ = 0;


                    GameLoop.SendMessageIfNeeded(tileUpdate, false, false);
                }
            }


            GameLoop.World.Player.player.MapsClearedToday = 0;
            GameLoop.World.Player.player.killList.Clear();

            foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
                kv.Value.player.MapsClearedToday = 0;
            }

            GameLoop.UIManager.Minigames.MineManager.MountainMine = new("Mountain");
            GameLoop.UIManager.Minigames.MineManager.LakeMine = new("Lake");

            GameLoop.UIManager.Map.LoadMap(GameLoop.World.Player.player.MapPos);


            GameLoop.UIManager.Minigames.MonsterPenManager.DailyUpdate();
        }
    }
}
