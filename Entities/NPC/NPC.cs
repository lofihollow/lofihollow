using Newtonsoft.Json;
using SadRogue.Primitives; 
using System.Collections.Generic;
using System.Linq;
using LofiHollow.DataTypes;

namespace LofiHollow.Entities.NPC {
    [JsonObject(MemberSerialization.OptOut)]
    public class NPC  {
        public string Name;
        public string Package;
        public NPCAi AI; 
        public string Occupation = "";
        public bool Animal = false;
         
        public string CourierGuild = "Noonbreeze";
        public string NavLocation = "";
          
        public int BirthMonth = 1;
        public int BirthDay = 2;  

        public int DispR = 255;
        public int DispG = 255;
        public int DispB = 255;
         
         
        public NPC() { 
        }


        public NPC(NPC other) {
            AI = Helper.Clone(other.AI); 
        }
         

        public void Update(bool newSchedule) {
            if (newSchedule || (AI != null && AI.Current == null)) {
                string season = GameLoop.World.Player.Clock.GetSeason();

                if (GameLoop.World.Player.Clock.IsItThisDay(BirthMonth, BirthDay))
                    season = "Birthday";


                AI.SetSchedule(season, "Sunny");
            }

            List<KeyValuePair<string, Location>> locs = GameLoop.World.atlas.ToList();
            NavLocation = locs[GameLoop.rand.Next(locs.Count - 1)].Key;

            // TODO: Rewrite NPC movement code
        } 

        public bool IsBirthday() {
            return GameLoop.World.Player.Clock.IsItThisDay(BirthMonth, BirthDay);
        } 

        public string FullName() {
            return Package + ":" + Name;
        }

        public Color GetDispColor() {
            return new Color(DispR, DispG, DispB);
        }
    }
}
