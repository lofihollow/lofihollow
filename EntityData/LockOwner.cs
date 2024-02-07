using Newtonsoft.Json; 

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptOut)]
    public class LockOwner { 
        public bool AlwaysLocked = false; // whether or not the tile is locked always (true) or only locked outside the times below (false)
        public string Owner = ""; // which NPC owns the tile, if any
        public int RelationshipUnlock = 0; // how much relationship with the owner unlocks the tile at any time
        public int MissionUnlock = -1; // which completed mission, if any, results in the lock being open always
        public int UnlockTime = 550; // The day time expressed as minutes that the lock is open
        public int LockTime = 600; // the day time expressed as minutes that the lock locks again 
        public string UnlockKeyName = ""; 

        

        public bool CanOpen() {
            if (Owner == "" && MissionUnlock == -1 && UnlockKeyName == "")
                return true;

            if (GameLoop.World.Player.CheckRel(Owner) >= RelationshipUnlock && RelationshipUnlock != 0)
                return true; 

            if (GameLoop.World.Player.Clock.GetCurrentTime() > UnlockTime && GameLoop.World.Player.Clock.GetCurrentTime() < LockTime)
                return true;

            if (UnlockKeyName != "")
                for (int i = 0; i < GameLoop.World.Player.Inventory.Count; i++)
                    if (GameLoop.World.Player.Inventory[i].FullName() == UnlockKeyName)
                        return true; 

            // Do some code to see if the player has completed the mission needed to pass this lock
            return false;
        } 
    }
}
