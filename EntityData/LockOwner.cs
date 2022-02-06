using Newtonsoft.Json; 

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptIn)]
    public class LockOwner {
        [JsonProperty]
        public bool AlwaysLocked = false; // whether or not the tile is locked always (true) or only locked outside the times below (false)
        [JsonProperty]
        public string Owner = ""; // which NPC owns the tile, if any
        [JsonProperty]
        public int OwnerID = -1;
        [JsonProperty]
        public int RelationshipUnlock = 0; // how much relationship with the owner unlocks the tile at any time
        [JsonProperty]
        public int MissionUnlock = -1; // which completed mission, if any, results in the lock being open always
        [JsonProperty]
        public int UnlockTime = 550; // The day time expressed as minutes that the lock is open
        [JsonProperty]
        public int LockTime = 600; // the day time expressed as minutes that the lock locks again
        [JsonProperty]
        public int ClosedGlyph = 32;
        [JsonProperty]
        public int OpenedGlyph = 32;
        [JsonProperty]
        public bool Closed = true;
        [JsonProperty]
        public string UnlockKeyName = "";
        [JsonProperty]
        public bool ClosedBlocksLOS = true;
        [JsonProperty]
        public bool OpenBlocksMove = false;

        public void UpdateOwner() { 
            if (OwnerID == -1)
                Owner = "";
            else {
                if (GameLoop.World.npcLibrary.ContainsKey(OwnerID)) {
                    Owner = GameLoop.World.npcLibrary[OwnerID].Name;
                }
            }
        }
        

        public bool CanOpen() {
            if (Owner == "" && MissionUnlock == -1 && UnlockKeyName == "")
                return true;

            if (GameLoop.World.Player.MetNPCs.ContainsKey(Owner) && GameLoop.World.Player.MetNPCs[Owner] >= RelationshipUnlock && RelationshipUnlock != 0)
                return true;


            if (GameLoop.World.Player.Clock.GetCurrentTime() > UnlockTime && GameLoop.World.Player.Clock.GetCurrentTime() < LockTime)
                return true;

            if (UnlockKeyName != "")
                for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++)
                    if (GameLoop.World.Player.Inventory[i].Package + ":" + GameLoop.World.Player.Inventory[i].Name == UnlockKeyName)
                        return true; 

            // Do some code to see if the player has completed the mission needed to pass this lock
            return false;
        } 
    }
}
