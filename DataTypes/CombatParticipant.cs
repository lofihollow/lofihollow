using LofiHollow.Entities;
using Newtonsoft.Json;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class CombatParticipant {
        public Actor Participant;
        public bool Enemy = false;
        public SteamId playerID;

        public CombatParticipant(SteamId id) {
            if (id.IsValid)
                playerID = id;
        }

        public CombatParticipant(MonsterWrapper mon) {
            Participant = mon;
        }

        [JsonConstructor]
        public CombatParticipant() {

        }

        public Actor GetActor() {
            if (playerID.IsValid) {
                if (GameLoop.World.otherPlayers.ContainsKey(playerID)) {
                    return GameLoop.World.otherPlayers[playerID];
                }

                if (playerID == SteamClient.SteamId) {
                    return GameLoop.World.Player;
                }
            } else {
                return Participant;
            }

            return null;
        }
    }
}
