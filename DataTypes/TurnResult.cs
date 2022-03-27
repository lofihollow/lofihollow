using Newtonsoft.Json;
using SadConsole; 

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptOut)]
    public class TurnResult {
        public ColoredString Damage;
        public ColoredString OwnStatus;
        public ColoredString TargetStatus;
        public bool Hit;

        [JsonConstructor]
        public TurnResult() {

        }


        public TurnResult(ColoredString dam, ColoredString ownStat, ColoredString targetStat, bool hit) {
            Damage = dam;
            OwnStatus = ownStat;
            TargetStatus = targetStat;
            Hit = hit;
        }
    }
}
