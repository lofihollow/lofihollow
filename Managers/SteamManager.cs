using Steamworks;

namespace LofiHollow.Managers {
    public class SteamManager {
        public bool SteamInitialized;

        public SteamManager() { 
            SteamInitialized = SteamAPI.Init();
        }



        public void RunCallbacks() {
            if (!SteamInitialized)
                return;

            SteamAPI.RunCallbacks();
        }


        public void Workshop() {
            SteamUGC.AddItemToFavorites(AppId_t.Invalid, PublishedFileId_t.Invalid); 
        }

    }
}
