using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames {
    [JsonObject(MemberSerialization.OptOut)]
    public class MonsterPenMonster {
        public string Name = "(EMPTY)";
        public string Species = "(EMPTY)";
        public int Glyph = 0;
        public int ForeR = 0;
        public int ForeG = 0;
        public int ForeB = 0;

        public int Health = 0;
        public int Relationship = 0;
        public int Happiness = 0;
        public int Hunger = 0;

        public int HungerSpeed = 20; // Amount of hunger gained per day
        public int HappinessDecay = 5; // Amount of happiness lost per day

        public string RecurringDrop = "";
        public bool DropHasQuality = false;
        public int MaxDropQuality = 0;
        public int DropMinQty = 1;
        public int DropMaxQty = 1;

        public string Eats = "Anything";


        public bool CanEat(string target) {
            if (target.Contains(Eats) || Eats == "Anything")
                return true;
            return false;
        } 

        public void Feed(MonsterPenFood food) {
            if (CanEat(food.Type)) {
                Hunger -= food.Satiety;
                if (Hunger < 0)
                    Hunger = 0;
            }
        }

        public Item GetDrop() {
            Item drop = new(RecurringDrop);

            if (DropMinQty != DropMaxQty) {
                int dropRange = DropMaxQty - DropMinQty;
                drop.ItemQuantity = GameLoop.rand.Next(dropRange) + DropMinQty;
            }
            
            if (DropHasQuality) {
                int RelCap = (int)Math.Floor((Relationship + 1f) / 10f) + 1;
                int QualityCap = Math.Min(RelCap, MaxDropQuality);
                int ActualQuality = GameLoop.rand.Next(QualityCap) + 1;
                drop.Quality = ActualQuality;
            }

            return drop;
        }



        public ColoredString Appearance() {
            return new ColoredString(((char)Glyph).ToString(), new Color(ForeR, ForeG, ForeB), Color.Black);
        }
    }
}
