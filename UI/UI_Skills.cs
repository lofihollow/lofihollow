using LofiHollow.Managers;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using Steamworks.Data;
using Color = SadRogue.Primitives.Color;

namespace LofiHollow.UI {
    public class UI_Skills : InstantUI { 
        public string SkillView = "Overview";
        public string LeaderType = "Global";
        public string LeaderSkill = "None";
        public int LeaderTopIndex = 0;

        public bool NeedsToFetchScores = true;

        public Dictionary<string, HighscoreResult> LeaderboardScores = new();
        public Dictionary<string, List<LeaderboardSlot>> LeaderboardTop = new();
        public Dictionary<string, List<LeaderboardSlot>> LeaderboardFriends = new();

        public UI_Skills(int width, int height, string title) : base(width, height, title, "Skills") { }


        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            Con.Clear();

            Con.DrawLine(new Point(15, 0), new Point(15, Con.Height - 1), 179, Color.White, Color.Black);
            if (SkillView == "Overview") {
                Con.Print(16, 0, "Skill".Align(HorizontalAlignment.Center, 20) + "|" + "Level".Align(HorizontalAlignment.Center, 7) + "|" + "EXP".Align(HorizontalAlignment.Center, 12) + "|" + "Next".Align(HorizontalAlignment.Center, 12));
                Con.DrawLine(new Point(16, 1), new Point(Con.Width - 1, 1), 196, Color.White, Color.Black);
                int index = 0;

                foreach (KeyValuePair<string, Skill> kv in GameLoop.World.Player.Skills) {
                    string name = kv.Key;
                    int level = kv.Value.Level;
                    int exp = kv.Value.TotalExp;
                    int next = (kv.Value.ExpToLevel() - kv.Value.Experience);

                    Con.Print(16, 2 + (index * 2), Helper.HoverColoredString(name.Align(HorizontalAlignment.Center, 20) + "|" + level.ToString().Align(HorizontalAlignment.Center, 7) + "|" + exp.ToString().Align(HorizontalAlignment.Center, 12) + "|" + next.ToString().Align(HorizontalAlignment.Center, 12), mousePos.Y == 2 + (index * 2)));
                    Con.Print(16, 3 + (index * 2), new ColoredString(" ".Align(HorizontalAlignment.Center, 20) + "|" + " ".Align(HorizontalAlignment.Center, 7) + "|" + " ".Align(HorizontalAlignment.Center, 12) + "|" + " ".Align(HorizontalAlignment.Center, 12), Color.White, Color.Black));
                    index++;
                }
            }
            else if (SkillView == "Leaderboards") {
                Con.Print(16, 0, "Global");
                Con.DrawLine(new Point(16, 1), new Point(40, 1), 196, Color.White);
                int y = 0;

                foreach (KeyValuePair<string, Skill> kv in GameLoop.World.skillLibrary) {
                    Con.PrintClickable(16, 2 + (y * 2), kv.Key, UI_Clicks, "Global," + kv.Key);
                    Con.DrawLine(new Point(16, 3 + (y * 2)), new Point(40, 3 + (y * 2)), '-', Color.White);
                    y++;
                }
                Con.DrawLine(new Point(40, 2), new Point(40, 39), 179, Color.White);



                Con.Print(41, 0, "Friends");
                Con.DrawLine(new Point(41, 1), new Point(69, 1), 196, Color.White);
                y = 0;

                foreach (KeyValuePair<string, Skill> kv in GameLoop.World.skillLibrary) {
                    Con.PrintClickable(41, 2 + (y * 2), kv.Key, UI_Clicks, "Friends," + kv.Key);
                    Con.DrawLine(new Point(41, 3 + (y * 2)), new Point(69, 3 + (y * 2)), '-', Color.White);
                    y++;
                }
            }
            else if (SkillView == "SpecificLeaderboard") {
                Con.Print(16, 0, "Rank".Align(HorizontalAlignment.Center, 5) + "|" + "Name".Align(HorizontalAlignment.Center, 20) + "|" + "Exp Earned".Align(HorizontalAlignment.Center, 29) + "|");
                Con.DrawLine(new Point(16, 1), new Point(69, 1), 196, Color.White);

                if (LeaderType == "Global") {
                    if (LeaderboardTop.ContainsKey(LeaderSkill)) {
                        for (int i = LeaderTopIndex; i < LeaderboardTop[LeaderSkill].Count && i < LeaderTopIndex + 38; i++) {
                            int slot = i - LeaderTopIndex;
                            LeaderboardSlot entry = LeaderboardTop[LeaderSkill][i];
                            string Rank = entry.Rank.ToString().Align(HorizontalAlignment.Center, 5);
                            string Name = entry.Name.Align(HorizontalAlignment.Center, 20);
                            string ExpEarned = entry.Score.ToString("N0").Align(HorizontalAlignment.Center, 29);
                            Con.Print(16, 2 + (slot * 2), Rank + "|" + Name + "|" + ExpEarned);
                            Con.DrawLine(new Point(16, 3 + (slot * 2)), new Point(69, 3 + (slot * 2)), '-', Color.White);
                        }
                    }
                } else {
                    if (LeaderboardFriends.ContainsKey(LeaderSkill)) {
                        for (int i = LeaderTopIndex; i < LeaderboardFriends[LeaderSkill].Count && i < LeaderTopIndex + 38; i++) {
                            int slot = i - LeaderTopIndex;
                            LeaderboardSlot entry = LeaderboardFriends[LeaderSkill][i];
                            string Rank = entry.Rank.ToString().Align(HorizontalAlignment.Center, 5);
                            string Name = entry.Name.Align(HorizontalAlignment.Center, 20);
                            string ExpEarned = entry.Score.ToString("N0").Align(HorizontalAlignment.Center, 29);
                            Con.Print(16, 2 + (slot * 2), Rank + "|" + Name + "|" + ExpEarned); 
                            Con.DrawLine(new Point(16, 3 + (slot * 2)), new Point(69, 3 + (slot * 2)), '-', Color.White);
                        }
                    }
                }

                Con.DrawLine(new Point(21, 1), new Point(21, 39), '|', Color.White);
                Con.DrawLine(new Point(42, 1), new Point(42, 39), '|', Color.White); 
            }

            Con.PrintClickable(0, 1, "Overview", UI_Clicks, "Overview");
            Con.PrintClickable(0, 3, "Leaderboards", UI_Clicks, "Leaderboards");


        }

        public override void UI_Clicks(string ID) {
            if (ID == "Leaderboards") {
                SkillView = "Leaderboards";
                LeaderTopIndex = 0;
                if (NeedsToFetchScores) {
                    SubmitAllSkills();
                    NeedsToFetchScores = false;
                } 
                Win.Title = "[SKILLS]";
            } 
            else if (ID == "Overview") {
                SkillView = "Overview"; 
                Win.Title = "[SKILLS]";
            }
            else if (ID == "BackToSelect") {
                SkillView = "Leaderboards";
                LeaderType = "Global";
                LeaderSkill = "None";
                LeaderTopIndex = 0;
                Win.Title = "[SKILLS]";
            }
            else {
                string[] split = ID.Split(",");

                SkillView = "SpecificLeaderboard";
                LeaderTopIndex = 0;
                LeaderType = split[0];
                LeaderSkill = split[1];
                Win.Title = "[" + LeaderType.ToUpper() + " LEADERBOARD - " + LeaderSkill.ToUpper() + "]";
            }
        }


        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            if (SkillView == "SpecificLeaderboard") {
                int rankMax = LeaderType == "Global" ? LeaderboardTop.Count : LeaderboardFriends.Count;

                if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                    if (LeaderTopIndex + 1 < rankMax - 19)
                        LeaderTopIndex++;
                }
                else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                    if (LeaderTopIndex > 0)
                        LeaderTopIndex--;
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape) || GameHost.Instance.Keyboard.IsKeyReleased(Key.K)) {
                GameLoop.UIManager.ToggleUI("Skills");
                SkillView = "Overview";
                LeaderTopIndex = 0;
                NeedsToFetchScores = true;
                Con.Clear();
            }
        } 


        public async void SubmitAllSkills() {
            LeaderboardScores.Clear();
            LeaderboardTop.Clear();
            LeaderboardFriends.Clear();

            foreach (KeyValuePair<string, Skill> kv in GameLoop.World.Player.Skills) {
                if (GameLoop.SteamManager.SkillLeaderboards.ContainsKey(kv.Key)) {
                    if (!LeaderboardScores.ContainsKey(kv.Key))
                        LeaderboardScores.Add(kv.Key, GameLoop.SteamManager.PostHighscore(kv.Key, kv.Value.TotalExp + kv.Value.Experience, true));
                    else
                        LeaderboardScores[kv.Key] = GameLoop.SteamManager.PostHighscore(kv.Key, kv.Value.TotalExp + kv.Value.Experience, true);

                    var global = await GameLoop.SteamManager.SkillLeaderboards[kv.Key].GetScoresAsync(100);

                    if (global != null) {
                        List<LeaderboardSlot> SkillTop = new(); 

                        for (int i = 0; i < global.Length; i++) {
                            LeaderboardEntry entry = global[i];
                            LeaderboardSlot newSlot = new();
                            newSlot.Rank = entry.GlobalRank;
                            newSlot.Score = entry.Score;
                            newSlot.Name = entry.User.Name;
                            SkillTop.Add(newSlot);
                        }

                        if (!LeaderboardTop.ContainsKey(kv.Key))
                            LeaderboardTop.Add(kv.Key, SkillTop);
                        else
                            LeaderboardTop[kv.Key] = SkillTop;
                    }
                     


                    var friends = await GameLoop.SteamManager.SkillLeaderboards[kv.Key].GetScoresFromFriendsAsync();

                    if (friends != null) {
                        List<LeaderboardSlot> SkillTop = new();

                        for (int i = 0; i < friends.Length; i++) {
                            LeaderboardEntry entry = friends[i];
                            LeaderboardSlot newSlot = new();
                            newSlot.Rank = entry.GlobalRank;
                            newSlot.Score = entry.Score;
                            newSlot.Name = entry.User.Name;
                            SkillTop.Add(newSlot);
                        }

                        if (!LeaderboardFriends.ContainsKey(kv.Key))
                            LeaderboardFriends.Add(kv.Key, SkillTop);
                        else
                            LeaderboardFriends[kv.Key] = SkillTop;
                    }
                }
            }
        }
    }
}
