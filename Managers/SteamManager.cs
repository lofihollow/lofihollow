using LofiHollow.DataTypes;
using Steamworks;
using System.Collections.Generic;
using System.IO;

namespace LofiHollow.Managers {
    public class SteamManager {
        public bool Initialized = false;

        protected CallResult<LeaderboardFindResult_t> BlacksmithingFind = new CallResult<LeaderboardFindResult_t>();
        protected CallResult<LeaderboardScoreUploaded_t> m_uploadResult = new CallResult<LeaderboardScoreUploaded_t>();
        protected CallResult<LeaderboardScoresDownloaded_t> m_blacksmithDownload = new CallResult<LeaderboardScoresDownloaded_t>();

        public SteamLeaderboard_t BlacksmithingLeaderboard;

        public HighscoreResult MostRecentResult;
        public List<LeaderboardSlot> GlobalLeader = new();

        public void Start() {
            Initialized = SteamAPI.Init();

            if (Initialized) {
                SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard("Blacksmithing");
                BlacksmithingFind.Set(hSteamAPICall, FindBlacksmithingLB);
            }
        }
        

        private void FindBlacksmithingLB(LeaderboardFindResult_t pCallback, bool failure) {
            if (pCallback.m_bLeaderboardFound == 0) {
                // Leaderboard couldn't be found
                return;
            } else {
                BlacksmithingLeaderboard = pCallback.m_hSteamLeaderboard;
            }
        }

        private void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure) {
            MostRecentResult = new(pCallback.m_bSuccess, pCallback.m_nGlobalRankNew, pCallback.m_nGlobalRankPrevious, pCallback.m_nScore, pCallback.m_bScoreChanged); 
        }

        private void OnLeaderboardDownloadResult(LeaderboardScoresDownloaded_t pCallback, bool failure) {
            GlobalLeader.Clear();

            for (int i = 0; i < pCallback.m_cEntryCount; i++) {
                SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out LeaderboardEntry_t entry, null, 0);
                LeaderboardSlot newSlot = new();
                newSlot.Rank = entry.m_nGlobalRank;
                newSlot.Score = entry.m_nScore;
                newSlot.Name = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
                GlobalLeader.Add(newSlot);
            }
        }

        public void Update() {
            if (Initialized) {
                SteamAPI.RunCallbacks();
            }
        }


        public bool FullGame() {
            if (Initialized) {
                var hasLicense = SteamUser.UserHasLicenseForApp(SteamUser.GetSteamID(), new AppId_t(1906540));
                if (hasLicense == EUserHasLicenseForAppResult.k_EUserHasLicenseResultHasLicense) {
                    return true;
                }
            }

            return false;
        }

        public void CreateWorkshopItem() {
            if (Initialized) {
                SteamAPICall_t CreateItem = SteamUGC.CreateItem(new AppId_t(1906540), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
            }
        }

        public HighscoreResult PostHighscore(string leaderboardName, int score) {
            if (Initialized) {
                if (leaderboardName == "Blacksmithing") {
                    SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(BlacksmithingLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, null, 0);
                    m_uploadResult.Set(hSteamAPICall, OnLeaderboardUploadResult);

                    SteamAPICall_t hDownloadCall = SteamUserStats.DownloadLeaderboardEntries(BlacksmithingLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, 10);
                    m_blacksmithDownload.Set(hDownloadCall, OnLeaderboardDownloadResult);
                }
            }

            return null;
        }

        public bool UnlockAchievement(string ID) {
            if (Initialized) {
                SteamUserStats.SetAchievement(ID);
                SteamUserStats.StoreStats();

                SteamUserStats.GetAchievement(ID, out bool achieved);

                return achieved;
            }

            return false;
        }

        public void CountSocials() {
            if (Initialized) {
                int maxed = 0;
                int minimum = 0;

                foreach (KeyValuePair<string, int> kv in GameLoop.World.Player.MetNPCs) {
                    if (kv.Value == 100) {
                        maxed++;
                        if (GameLoop.World.npcLibrary.ContainsKey(kv.Key)) {
                            if (GameLoop.World.npcLibrary[kv.Key].Shop != null) {
                                SteamUserStats.GetAchievement("BEST_PRICE", out bool best_price);
                                if (!best_price) {
                                    UnlockAchievement("BEST_PRICE");
                                }
                            }
                        }
                    }

                    if (kv.Value == -100) {
                        minimum++;
                        if (GameLoop.World.npcLibrary.ContainsKey(kv.Key)) {
                            if (GameLoop.World.npcLibrary[kv.Key].Shop != null) {
                                SteamUserStats.GetAchievement("WORST_PRICE", out bool worst_price);
                                if (!worst_price) {
                                    UnlockAchievement("WORST_PRICE");
                                }
                            }
                        }
                    }
                }

                SteamUserStats.GetAchievement("ONE_WHOLE_FRIEND", out bool one_friend);
                if (maxed >= 1 && !one_friend) {
                    UnlockAchievement("ONE_WHOLE_FRIEND");
                }

                SteamUserStats.GetAchievement("WELL_LIKED", out bool well_liked);
                if (maxed >= 5 && !well_liked) {
                    UnlockAchievement("WELL_LIKED");
                }

                SteamUserStats.GetAchievement("MANY_FRIENDS", out bool many_friends);
                if (maxed >= 10 && !many_friends) {
                    UnlockAchievement("MANY_FRIENDS");
                }

                SteamUserStats.GetAchievement("POPULAR", out bool popular);
                if (maxed >= 20 && !popular) {
                    UnlockAchievement("POPULAR");
                }

                SteamUserStats.GetAchievement("NEMESIS", out bool nemesis);
                if (minimum >= 1 && !nemesis) {
                    UnlockAchievement("NEMESIS");
                }

                SteamUserStats.GetAchievement("NEMESES", out bool nemeses);
                if (minimum >= 5 && !nemeses) {
                    UnlockAchievement("NEMESES");
                }

                SteamUserStats.GetAchievement("UNPOPULAR", out bool unpopular);
                if (minimum >= 10 && !unpopular) {
                    UnlockAchievement("UNPOPULAR");
                }

                SteamUserStats.GetAchievement("HATED_IN_THE_NATION", out bool hated_nation);
                if (minimum >= 20 && !hated_nation) {
                    UnlockAchievement("HATED_IN_THE_NATION");
                }
            }
        }
    }
}
