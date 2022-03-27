using LofiHollow.Entities;
using LofiHollow.EntityData;
using LofiHollow.Managers;
using LofiHollow.Minigames;
using LofiHollow.Minigames.Photo; 
using SadConsole;
using SadConsole.Input; 
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_AdventurerBoard : Lofi_UI {
        public List<KillJob> DailyJobs = new();
        public bool ShowingBoard = false;

        public UI_AdventurerBoard(int width, int height, string title) : base(width, height, title, "AdventurerBoard") { }
         
          
        public void PopulateJobList() {
            DailyJobs.Clear();

            for (int i = 0; i < 12; i++) {
                int index = GameLoop.rand.Next(GameLoop.World.monsterLibrary.Count);
                Monster mon = Monster.Clone(GameLoop.World.monsterLibrary.ElementAt(index).Value);
                if (mon.Level <= GameLoop.World.Player.GetSkillLevel("Bounty Hunting")) {
                    string targetName = mon.Species;
                    ColoredString app = mon.GetAppearance();
                    int level = mon.Level;
                    int number = GameLoop.rand.Next(20) + 1;
                    int rewardAmount = (int)(Math.Ceiling((level * number) / 2f));

                    KillJob newJob = new(app, targetName, number, rewardAmount);
                    newJob.Level = level;

                    DailyJobs.Add(newJob);
                }
            }
        }

        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            Con.Clear();

            if (GameLoop.World.Player.CurrentKillTask != "")
                Con.Print(0, 0, "Current: " + GameLoop.World.Player.CurrentKillTask + ", " + GameLoop.World.Player.KillTaskProgress + "/" + GameLoop.World.Player.KillTaskGoal);
            else
                Con.Print(0, 0, "Current: None");

            if (GameLoop.World.Player.CurrentKillTask != "") {

                bool complete = false;
                if (GameLoop.World.Player.CurrentKillTask != "" && GameLoop.World.Player.KillTaskProgress == GameLoop.World.Player.KillTaskGoal)
                    complete = true;

                if (complete)
                    Con.PrintClickable(0, 1, new ColoredString("[DONE]", complete ? Color.Lime : Color.Red, Color.Black), UI_Clicks, "complete");
                else
                    Con.PrintClickable(0, 1, new ColoredString("[ABANDON]", complete ? Color.Lime : Color.Red, Color.Black), UI_Clicks, "abandon");
            }

            Con.Print(0, 3, ("Target".Align(HorizontalAlignment.Center, 24) + "|" + "Amount".Align(HorizontalAlignment.Center, 6) + "|" + "Level".Align(HorizontalAlignment.Center, 5) + "|" + "Reward".Align(HorizontalAlignment.Center, 6) + "|"), Color.DarkSlateBlue);
            Con.DrawLine(new Point(0, 4), new Point(49, 4), 196, Color.DarkSlateBlue);
            for (int i = 0; i < DailyJobs.Count; i++) {
                Con.Print(0, 5 + (i * 2), DailyJobs[i].Appearance +
                    new ColoredString((" " + DailyJobs[i].Target).Align(HorizontalAlignment.Left, 23)
                    + "|" + DailyJobs[i].Amount.ToString().Align(HorizontalAlignment.Center, 6)
                    + "|" + DailyJobs[i].Level.ToString().Align(HorizontalAlignment.Center, 5)
                    + "|" + DailyJobs[i].Reward.ToString().Align(HorizontalAlignment.Center, 6) + "|"));

               
                if (GameLoop.World.Player.CurrentKillTask == "")
                    Con.PrintClickable(45, 5 + (i * 2), new ColoredString("TAKE", Color.Lime, Color.Black), UI_Clicks, "take," + i);

                Con.DrawLine(new Point(0, 6 + (i * 2)), new Point(49, 6 + (i * 2)), '-', Color.DarkSlateBlue);
            }

            Con.DrawLine(new Point(24, 5), new Point(24, 30), '|', Color.DarkSlateBlue);
            Con.DrawLine(new Point(31, 5), new Point(31, 30), '|', Color.DarkSlateBlue);
            Con.DrawLine(new Point(37, 5), new Point(37, 30), '|', Color.DarkSlateBlue);
            Con.DrawLine(new Point(44, 5), new Point(44, 30), '|', Color.DarkSlateBlue);


            Con.PrintClickable(48, 0, "X", UI_Clicks, "close");
        }

        public override void UI_Clicks(string ID) {
            if (ID == "close") {
                Toggle();
            }
            else if (ID == "complete") {
                if (GameLoop.World.Player.CurrentKillTask != "") {
                    if (GameLoop.World.Player.KillTaskProgress >= GameLoop.World.Player.KillTaskGoal) {
                        GameLoop.World.Player.CopperCoins += GameLoop.World.Player.KillTaskReward;
                        GameLoop.World.Player.CurrentKillTask = "";
                        GameLoop.World.Player.KillTaskProgress = 0; 
                    }
                }
            }
            else if (ID == "abandon") {
                if (GameLoop.World.Player.CurrentKillTask != "") {  
                    GameLoop.World.Player.CurrentKillTask = "";
                    GameLoop.World.Player.KillTaskProgress = 0; 
                }
            }
            else {
                string[] split = ID.Split(",");
                int slot = Int32.Parse(split[1]);

                 if (split[0] == "take") {
                    if (GameLoop.World.Player.CurrentKillTask == "") {
                        KillJob job = DailyJobs[slot];
                        GameLoop.World.Player.CurrentKillTask = job.Target;
                        GameLoop.World.Player.KillTaskProgress = 0;
                        GameLoop.World.Player.KillTaskGoal = job.Amount;
                        GameLoop.World.Player.KillTaskReward = job.Reward;
                        GameLoop.World.Player.KillTaskMonLevel = job.Level;
                        DailyJobs.RemoveAt(slot);
                    }
                }
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            } 
        }
    }
}
