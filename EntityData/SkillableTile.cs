using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.EntityData {
    //Harvest Tool category, Required level, Item given, Depleted tile name, time to restore, Exp given each harvest, Exp given on depletion
    [JsonObject(MemberSerialization.OptIn)]
    public class SkillableTile {
        [JsonProperty]
        public string HarvestableName; // The name of the tile when it can be harvested
        [JsonProperty]
        public string DepletedName; // The name of the tile once it has been depleted
        [JsonProperty]
        public int HarvestTool; // ItemCategory number that must be matched for harvesting
        [JsonProperty]
        public string RequiredSkill; // Skill associated with this tile
        [JsonProperty]
        public int RequiredLevel; // Skill level required to harvest this tile
        [JsonProperty]
        public string ItemGiven; // Item ID of item on regular harvest
        [JsonProperty]
        public string DepletedItem; // Item ID of item on depleting harvest, if different than normal
        [JsonProperty]
        public string RestoreTime; // "Hour", "Day", "Week", "Month", or "Year". 1 of the chosen increment must past before the tile is restored
        [JsonProperty]
        public int ExpOnHarvest; // EXP given to the player in the associated skill when the tile is harvested
        [JsonProperty]
        public int ExpOnDeplete; // EXP given to the player in the associated skill when the tile is depleted
        [JsonProperty]
        public string HarvestMessage; // Message put into the players log when they harvest the resource
        [JsonProperty]
        public string DepleteMessage; // Message put into the players log when they deplete the resource
    }
}
