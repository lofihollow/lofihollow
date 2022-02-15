using Newtonsoft.Json; 
using System.Collections.Generic; 
using LofiHollow.DataTypes;

namespace LofiHollow.Minigames {
    [JsonObject(MemberSerialization.OptOut)]
    public class MonsterPen {
        public int PenNumber = 0;
        public MonsterPenMonster Monster = new();
        public Egg Egg = null;
        public List<Item> CurrentDrops = new();

        [JsonConstructor]
        public MonsterPen() { }


        public MonsterPen(int which) {
            PenNumber = which;
        }


        public void DailyUpdate() {
            if (Egg != null) {
                Egg.DaysToHatch -= 1;
                if (Egg.DaysToHatch == -1) {
                    Monster = Egg.HatchesInto;
                    Egg = null;
                }
            }


            if (Monster.Hunger == 100)
                Monster.Health -= 10;
            else {
                Monster.Health += 1;
            }
            

            Monster.Hunger += Monster.HungerSpeed;
            if (Monster.Hunger > 100)
                Monster.Hunger = 100;

            if (Monster.Happiness > 50) {
                if (Monster.RecurringDrop != "") {
                    Item drop = Monster.GetDrop();

                    bool AlreadyInList = false;
                    for (int i = 0; i < CurrentDrops.Count; i++) {
                        if (drop.StacksWith(CurrentDrops[i])) {
                            CurrentDrops[i].ItemQuantity += drop.ItemQuantity;
                            AlreadyInList = true;
                        }
                    }

                    if (!AlreadyInList)
                        CurrentDrops.Add(drop);
                }
            }

            if (Monster.Hunger >= 25)
                Monster.Happiness -= Monster.HappinessDecay;

            if (Monster.Hunger >= 50)
                Monster.Happiness -= Monster.HappinessDecay;

            if (Monster.Hunger >= 75)
                Monster.Happiness -= Monster.HappinessDecay;

            if (Monster.Hunger == 100) {
                Monster.Happiness -= Monster.HappinessDecay;
            }

            Monster.AgeInDays++;
            Monster.DaysInStage++;
            if (Monster.CurrentStage < Monster.Stages.Count) {
                if (Monster.DaysInStage > Monster.Stages[Monster.CurrentStage].StageLengthInDays) {
                    Monster.CurrentStage++;
                    Monster.DaysInStage = 0;
                }
            } else {
                // Death check
            }
        }
    }
}
