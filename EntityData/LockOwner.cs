using ProtoBuf;

namespace LofiHollow.EntityData {
    [ProtoContract]
    public class LockOwner {
        [ProtoMember(1)]
        public bool AlwaysLocked = false; // whether or not the tile is locked always (true) or only locked outside the times below (false)
        [ProtoMember(2)]
        public string Owner = ""; // which NPC owns the tile, if any
        [ProtoMember(4)]
        public int RelationshipUnlock = 0; // how much relationship with the owner unlocks the tile at any time
        [ProtoMember(5)]
        public int MissionUnlock = -1; // which completed mission, if any, results in the lock being open always
        [ProtoMember(6)]
        public int UnlockTime = 360; // The day time expressed as minutes that the lock is open
        [ProtoMember(7)]
        public int LockTime = 1440; // the day time expressed as minutes that the lock locks again
        [ProtoMember(8)]
        public int ClosedGlyph = 32;
        [ProtoMember(9)]
        public int OpenedGlyph = 32;
        [ProtoMember(10)]
        public bool Closed = true;
        [ProtoMember(11)]
        public string UnlockKeyName = "";
        [ProtoMember(12)]
        public bool ClosedBlocksLOS = true;
        [ProtoMember(13)]
        public bool OpenBlocksMove = false;

        public bool CanOpen() {
            if (Owner == "" && MissionUnlock == -1 && UnlockKeyName == "")
                return true;

            if (GameLoop.World.Player.player.MetNPCs.ContainsKey(Owner) && GameLoop.World.Player.player.MetNPCs[Owner] >= RelationshipUnlock && RelationshipUnlock != 0)
                return true;


            if (GameLoop.World.Player.player.Clock.GetCurrentTime() > UnlockTime && GameLoop.World.Player.player.Clock.GetCurrentTime() < LockTime)
                return true;

            if (UnlockKeyName != "")
                for (int i = 0; i < GameLoop.World.Player.player.Inventory.Length; i++)
                    if (GameLoop.World.Player.player.Inventory[i].Package + ":" + GameLoop.World.Player.player.Inventory[i].Name == UnlockKeyName)
                        return true;

            // Do some code to see if the player has completed the mission needed to pass this lock
            return false;
        }
    }
}
