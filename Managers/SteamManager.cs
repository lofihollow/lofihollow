using LofiHollow.DataTypes;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LofiHollow.Managers {
    public class SteamManager {  
        public Leaderboard BlacksmithingLeaderboard;
        public Leaderboard MailSortLB;
        public Leaderboard FruitCatchLB;
        public Dictionary<string, Leaderboard> SkillLeaderboards = new();

        public HighscoreResult MostRecentResult;
        public List<LeaderboardSlot> GlobalLeader = new();

        public List<ulong> ModsEnabled = new();

        public async void Start() {
            try {
                SteamClient.Init(1906540, true);

                var bsl = await SteamUserStats.FindLeaderboardAsync("Blacksmithing");

                if (bsl.HasValue)
                    BlacksmithingLeaderboard = bsl.Value;

                var mslb = await SteamUserStats.FindOrCreateLeaderboardAsync("MailSort", LeaderboardSort.Descending, LeaderboardDisplay.Numeric);

                if (mslb.HasValue)
                    MailSortLB = mslb.Value;

                var flcl = await SteamUserStats.FindOrCreateLeaderboardAsync("00-WeeklyFruitCatch", LeaderboardSort.Descending, LeaderboardDisplay.Numeric);

                if (flcl.HasValue)
                    FruitCatchLB = flcl.Value;

                if (File.Exists("./modlist.dat")) {
                    foreach (var line in File.ReadAllLines("./modlist.dat")) {
                        ModsEnabled.Add(ulong.Parse(line));
                    }
                } else {
                    File.Create("./modlist.dat");
                }

            }
            catch {

            }
        }

        public void SaveModList() {
            if (!File.Exists("./modlist.dat")) {
                File.Create("./modlist.dat");
            }

            List<string> allMods = new();

            foreach (var line in ModsEnabled) {
                allMods.Add(line.ToString());
            } 

            File.WriteAllLines("./modlist.dat", allMods.ToArray()); 
        }

        public async void PullSkillBoards() {
            if (SkillLeaderboards.Count == 0) {
                foreach (KeyValuePair<string, Skill> kv in GameLoop.World.skillLibrary) {
                    string name = kv.Key + " Experience Earned";
                    var skillLB = await SteamUserStats.FindOrCreateLeaderboardAsync(name, LeaderboardSort.Descending, LeaderboardDisplay.Numeric);

                    if (skillLB.HasValue)
                        SkillLeaderboards.Add(kv.Key, skillLB.Value);
                }
            }
        }

        public void Update() {
            if (SteamClient.IsValid) {
                SteamClient.RunCallbacks(); 
            }
        } 

        private async Task<LeaderboardUpdate?> PostAsync(string leaderboardName, int score) {
            if (SteamClient.IsValid) {
                if (leaderboardName == "Blacksmithing") {
                    return await BlacksmithingLeaderboard.SubmitScoreAsync(score);
                }
                else if (leaderboardName == "00-WeeklyFruitCatch") {
                    return await FruitCatchLB.SubmitScoreAsync(score);
                }
                else if (leaderboardName == "MailSort") {
                    return await MailSortLB.SubmitScoreAsync(score);
                }
            } 
            return null;
        }

        private async Task<LeaderboardUpdate?> SkillPostAsync(string skillName, int score) {
            if (SteamClient.IsValid) { 
                if (SkillLeaderboards.ContainsKey(skillName)) {  
                    return await SkillLeaderboards[skillName].SubmitScoreAsync(score);
                }
                return null;
            }
            return null;
        }

        public HighscoreResult PostHighscore(string leaderboardName, int score, bool skillExp = false) {
            if (!skillExp) {
                var result = PostAsync(leaderboardName, score).Result;

                if (result.HasValue) {
                    HighscoreResult output = new(true, result.Value.NewGlobalRank, result.Value.OldGlobalRank, result.Value.Score, result.Value.Changed);
                    return output;
                }
            } else {
                var result = SkillPostAsync(leaderboardName, score).Result;

                if (result.HasValue) {
                    HighscoreResult output = new(true, result.Value.NewGlobalRank, result.Value.OldGlobalRank, result.Value.Score, result.Value.Changed);
                    return output;
                }
            }

            return null;
        }

        public bool UnlockAchievement(string ID) {
            bool newUnlock = false;
            if (SteamClient.IsValid) {
                foreach(var achieve in SteamUserStats.Achievements) {
                    if (achieve.Identifier == ID) {
                        if (!achieve.State) {
                            achieve.Trigger();
                            newUnlock = true;
                        }
                    }
                } 

                SteamUserStats.StoreStats();
            }

            return newUnlock;
        }

        public void MoneyAchieves() {
            if (SteamClient.IsValid) {
                int MoneyHeld = GameLoop.World.Player.CopperWealth();

                if (GameLoop.World.Player.SilverCoins >= 1) {
                    UnlockAchievement("COIN_1SILVER");
                }

                if (GameLoop.World.Player.SilverCoins >= 3) {
                    UnlockAchievement("COIN_3SILVER");
                }

                if (GameLoop.World.Player.GoldCoins >= 1) {
                    UnlockAchievement("COIN_1GOLD");
                }

                if (GameLoop.World.Player.JadeCoins >= 1) {
                    UnlockAchievement("COIN_1JADE");
                }
            }
        }

        public void CountSocials() {
            if (SteamClient.IsValid) {
                int maxed = 0;
                int minimum = 0;

                foreach (KeyValuePair<string, int> kv in GameLoop.World.Player.MetNPCs) {
                    if (kv.Value == 100) {
                        maxed++;
                        if (GameLoop.World.npcLibrary.ContainsKey(kv.Key)) {
                            if (GameLoop.World.npcLibrary[kv.Key].Shop != null) { 
                                UnlockAchievement("BEST_PRICE"); 
                            }
                        }
                    }

                    if (kv.Value == -100) {
                        minimum++;
                        if (GameLoop.World.npcLibrary.ContainsKey(kv.Key)) {
                            if (GameLoop.World.npcLibrary[kv.Key].Shop != null) { 
                                UnlockAchievement("WORST_PRICE"); 
                            }
                        }
                    }
                }
                 
                if (maxed >= 1) {
                    UnlockAchievement("ONE_WHOLE_FRIEND");
                }

                if (maxed >= 5) {
                    UnlockAchievement("WELL_LIKED");
                }

                if (maxed >= 10) {
                    UnlockAchievement("MANY_FRIENDS");
                }

                if (maxed >= 20) {
                    UnlockAchievement("POPULAR");
                }

                if (minimum >= 1) {
                    UnlockAchievement("NEMESIS");
                }

                if (minimum >= 5) {
                    UnlockAchievement("NEMESES");
                }

                if (minimum >= 10) {
                    UnlockAchievement("UNPOPULAR");
                }

                if (minimum >= 20) {
                    UnlockAchievement("HATED_IN_THE_NATION");
                }
            }
        }
    }
}
