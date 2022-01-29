using LofiHollow.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Missions {
    [JsonObject(MemberSerialization.OptOut)]
    public class MissionStage {
        public int CurrentDialogue = 0;

        public Dictionary<int, MissionDialogue> Dialogue = new();
        public Dictionary<string, int> SkillLevelRequirements = new();
        public Dictionary<string, int> MissionRequirements = new();
        public Dictionary<string, int> RelationshipRequirements = new();

        public string NPC = "";
        public string Summary = "";

        public string CounterType = "";
        public string CounterName = "";
        public int CounterDone = 0;
        public int CounterNeeded = 0;

        public string CollectItemFullName = "";
        public int CollectItemQty = 1;
        public int CollectItemQuality = 0;

        public bool CheckVisible(Player act) {
            bool anyReqMet = false;

            foreach (KeyValuePair<string, int> kv in SkillLevelRequirements) {
                if (act.Skills.ContainsKey(kv.Key)) {
                    if (kv.Value <= act.Skills[kv.Key].Level) {
                        anyReqMet = true;
                    }
                }
            }

            foreach (KeyValuePair<string, int> kv in MissionRequirements) {
                if (act.MissionLog.ContainsKey(kv.Key)) {
                    if (kv.Value <= act.MissionLog[kv.Key].CurrentStage) {
                        anyReqMet = true;
                    }
                }
            }

            foreach (KeyValuePair<string, int> kv in RelationshipRequirements) {
                if (act.MetNPCs.ContainsKey(kv.Key)) {
                    if (act.MetNPCs[kv.Key] >= kv.Value) {
                        anyReqMet = true;
                    }
                } 
            }

            return anyReqMet;
        }

        public bool AllRequirementsMet(Player act) {
            bool allMet = true;

            foreach (KeyValuePair<string, int> kv in SkillLevelRequirements) {
                if (act.Skills.ContainsKey(kv.Key)) {
                    if (kv.Value > act.Skills[kv.Key].Level) {
                        allMet = false;
                    }
                }
            }

            foreach (KeyValuePair<string, int> kv in MissionRequirements) {
                if (act.MissionLog.ContainsKey(kv.Key)) {
                    if (kv.Value > act.MissionLog[kv.Key].CurrentStage) {
                        allMet = false;
                    }
                }
            }

            foreach (KeyValuePair<string, int> kv in RelationshipRequirements) {
                if (act.MetNPCs.ContainsKey(kv.Key)) {
                    if (kv.Value > act.MetNPCs[kv.Key]) {
                        allMet = false;
                    }
                }
            }

            return allMet;
        }
    }
}
