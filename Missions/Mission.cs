using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Missions {
    [JsonObject(MemberSerialization.OptOut)]
    public class Mission {
        public bool Visible = false;
        public int CurrentStage = 0;
        public string Name = "";
        public string Package = "";
        public int Chapter = 0; 

        public List<MissionStage> Stages = new();

        public void SelectChoice(Response rep) {
            if (rep.GoToStage == -1) {
                Stages[CurrentStage].CurrentDialogue = rep.LeadsToDialogue; 
            } else if (rep.GoToStage >= 0) {
                CurrentStage = rep.GoToStage;
            }
        }

        public void IncrementCounter(string type, string name, int amount) {
            if (CurrentStage < Stages.Count) {
                if (Stages[CurrentStage].CounterType != "" && Stages[CurrentStage].CounterName != "") {
                    if (type == Stages[CurrentStage].CounterType) {
                        if (name == Stages[CurrentStage].CounterName || name.Contains(Stages[CurrentStage].CounterName)) {
                            Stages[CurrentStage].CounterDone += amount;
                        }
                    }

                    GameLoop.UIManager.AddMsg("Counter " + Stages[CurrentStage].CounterDone);

                    if (Stages[CurrentStage].CounterDone >= Stages[CurrentStage].CounterNeeded) {
                        CurrentStage++;
                    }
                }
            }
        }

    }
}
