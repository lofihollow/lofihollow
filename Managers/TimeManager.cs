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
    [JsonObject(MemberSerialization.OptOut)]
    public class TimeManager {
        public int Hours = 9;
        public int Minutes = 0;
        public int Month = 1;
        public int Day = 1;
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
                GameLoop.UIManager.AddMsg(new ColoredString("You passed out!", Color.Red, Color.Black));
            } else {  
                GameLoop.UIManager.AddMsg(new ColoredString("You go to sleep and wake up feeling refreshed.", Color.Lime, Color.Black));
            }


            Hours = GameLoop.World.Player.WakeupHour;
            Minutes = -1;
            Day++;
            TickTime();


            Helper.ErasePlayerData("cozyCotDays");

            GameLoop.World.Player.CurrentHP = passedOut ? GameLoop.World.Player.CurrentHP : GameLoop.World.Player.MaxHP;
            GameLoop.World.Player.CurrentStamina = passedOut ? Math.Max(GameLoop.World.Player.CurrentStamina, GameLoop.World.Player.MaxStamina / 2) : GameLoop.World.Player.MaxStamina;
               

            GameLoop.World.SavePlayer();
        }


        public void TickTime() {
            Minutes++;

            foreach (var kv in GameLoop.World.npcLibrary) {
                kv.Value.Update(false);
            }

            GameLoop.UIManager.Nav.MovedMaps();


            if (Minutes >= 60) {
                Hours++;
                Minutes = 0;
                 

                if (Hours == GameLoop.World.Player.BlackoutHour - 2 && GameLoop.World.Player.BlackoutsOn) {
                    GameLoop.UIManager.AddMsg(new ColoredString("You're getting a little tired.", Color.Yellow, Color.Black));
                }

                if (Hours == GameLoop.World.Player.BlackoutHour - 1 && GameLoop.World.Player.BlackoutsOn) {
                    GameLoop.UIManager.AddMsg(new ColoredString("You can't keep your eyes open much longer.", Color.Red, Color.Black));
                }

                if (Hours == 24) { 
                    Day++;
                    Hours = 0;
                    DailyUpdates();

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
        }


        public void DailyUpdates() { 
        }
    }
}
