using LofiHollow.Missions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Managers {
    public class MissionManager {
        public static void CheckHasItems() {
            foreach (KeyValuePair<string, Mission> kv in GameLoop.World.Player.MissionLog) {
                if (kv.Value.CurrentStage < kv.Value.Stages.Count) {
                    if (kv.Value.Stages[kv.Value.CurrentStage].CollectItemFullName != "") { 
                        for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                            if (GameLoop.World.Player.Inventory[i].FullName() == kv.Value.Stages[kv.Value.CurrentStage].CollectItemFullName) { 
                                if (GameLoop.World.Player.Inventory[i].Quality >= kv.Value.Stages[kv.Value.CurrentStage].CollectItemQuality) { 
                                    if (GameLoop.World.Player.Inventory[i].ItemQuantity >= kv.Value.Stages[kv.Value.CurrentStage].CollectItemQty) { 
                                        kv.Value.CurrentStage++; 
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void Increment(string type, string name, int amount) {
            foreach (KeyValuePair<string, Mission> kv in GameLoop.World.Player.MissionLog) {
                kv.Value.IncrementCounter(type, name, amount);
            }
        }
    }
}
