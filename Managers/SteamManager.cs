using Steamworks;

namespace LofiHollow.Managers {
    public class SteamManager {

        public SteamManager() { 
            try {
                SteamClient.Init(832430);
            } catch (System.Exception e) {

            }
        }



        public void RunCallbacks() {
            if (SteamClient.IsValid)
                SteamClient.RunCallbacks();
        }

    }
}
