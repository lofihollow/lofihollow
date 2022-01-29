using LofiHollow.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Missions {
    [JsonObject(MemberSerialization.OptIn)]
    public class Response { 
        [JsonProperty]
        public string Text = "";
        [JsonProperty]
        public int LeadsToDialogue = 0;
        [JsonProperty]
        public int GoToStage = -10;
        [JsonProperty]
        public bool EndsDialogue = false;
        [JsonProperty]
        public string ItemGiven = "";

        [JsonProperty]
        public Dictionary<string, int> SkillLevelRequirements = new();
        [JsonProperty]
        public Dictionary<string, int> MissionRequirements = new();
        [JsonProperty]
        public Dictionary<string, int> RelationshipRequirements = new();
        [JsonProperty]
        public Dictionary<string, int> CurrencyRequirements = new();
        [JsonProperty]
        public Dictionary<string, int> ItemsRequired = new();
        

        public bool MeetsAllRequirements(Player act) {
            bool allMet = true;

            foreach (KeyValuePair<string, int> kv in SkillLevelRequirements) {
                if (act.Skills.ContainsKey(kv.Key)) {
                    if (kv.Value > act.Skills[kv.Key].Level) {
                        allMet = false;
                    }
                } else {
                    allMet = false;
                }
            }

            foreach (KeyValuePair<string, int> kv in MissionRequirements) {
                if (act.MissionLog.ContainsKey(kv.Key)) {
                    if (kv.Value > act.MissionLog[kv.Key].CurrentStage) {
                        allMet = false;
                    }
                } else {
                    allMet = false;
                }
            }

            foreach (KeyValuePair<string, int> kv in RelationshipRequirements) {
                if (act.MetNPCs.ContainsKey(kv.Key)) {
                    if (kv.Value > act.MetNPCs[kv.Key]) {
                        allMet = false;
                    }
                } else {
                    allMet = false;
                }
            }

            foreach (KeyValuePair<string, int> kv in CurrencyRequirements) {
                if (kv.Key == "Copper") {
                    if (act.CopperCoins < kv.Value) {
                        bool coinAsItem = false;
                        for (int i = 0; i < act.Inventory.Length; i++) {
                            if (act.Inventory[i].Name == "Copper Coin" && act.Inventory[i].ItemQuantity + act.CopperCoins >= kv.Value) {
                                coinAsItem = true;
                            } 
                        }

                        if (!coinAsItem)
                            allMet = false;
                    }
                }

                if (kv.Key == "Silver") {
                    bool coinAsItem = false;
                    for (int i = 0; i < act.Inventory.Length; i++) {
                        if (act.Inventory[i].Name == "Silver Coin" && act.Inventory[i].ItemQuantity + act.SilverCoins >= kv.Value) {
                            coinAsItem = true;
                        }
                    }

                    if (!coinAsItem)
                        allMet = false;
                }

                if (kv.Key == "Gold") {
                    bool coinAsItem = false;
                    for (int i = 0; i < act.Inventory.Length; i++) {
                        if (act.Inventory[i].Name == "Gold Coin" && act.Inventory[i].ItemQuantity + act.GoldCoins >= kv.Value) {
                            coinAsItem = true;
                        }
                    }

                    if (!coinAsItem)
                        allMet = false;
                }

                if (kv.Key == "Jade") {
                    bool coinAsItem = false;
                    for (int i = 0; i < act.Inventory.Length; i++) {
                        if (act.Inventory[i].Name == "Jade Coin" && act.Inventory[i].ItemQuantity + act.JadeCoins >= kv.Value) {
                            coinAsItem = true;
                        }
                    }

                    if (!coinAsItem)
                        allMet = false;
                }
            } 

            return allMet;
        }
    }
}
