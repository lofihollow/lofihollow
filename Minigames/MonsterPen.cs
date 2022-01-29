using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames {
    [JsonObject(MemberSerialization.OptOut)]
    public class MonsterPen {
        public int PenNumber = 0;
        public MonsterPenMonster Monster = new();
        public List<Item> CurrentDrops = new();

        [JsonConstructor]
        public MonsterPen() { }


        public MonsterPen(int which) {
            PenNumber = which;
        }


        public void DailyUpdate() {
            Monster.Hunger += Monster.HungerSpeed;
            if (Monster.Hunger > 100)
                Monster.Hunger = 100;

            if (Monster.Happiness > 50) {
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
    }
}
