using LofiHollow.Entities;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Jobs {
    public class Drink : Entity {
        public int DrinkVelocity = 0;
        public string DrinkName = "";

        public Drink() : base(Color.White, Color.Black, 340) {

        }
    }
}
