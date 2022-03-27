using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class TurnAction {
        public int Speed = 0;
        public CombatParticipant Owner;
        public CombatParticipant Target;

        public string Action;
        public Move UsingMove; 
    }
}
