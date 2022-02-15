using Steamworks;
using System.Collections.Generic;

namespace LofiHollow.Managers {
    public class SteamManager {
        public bool Initialized = false;
        public void Start() {
            Initialized = SteamAPI.Init();
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
