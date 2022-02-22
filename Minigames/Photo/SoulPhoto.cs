using LofiHollow.DataTypes;
using LofiHollow.Entities;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Photo {
    [JsonObject(MemberSerialization.OptOut)]
    public class SoulPhoto {
        public Monster Contained1; 
        public Monster Contained2;
        public string MonsterName;
        public int Level = 1;
        public int Experience = 0;
        public int TotalExperience = 0;
        public Point Position;

        [JsonConstructor]
        public SoulPhoto() { }

        public SoulPhoto(Monster mon, int level) {
            Contained1 = mon;
            MonsterName = mon.Name;

            for (int i = 0; i < level; i++) {
                TotalExperience += ExpToLevel();
                Level++;
            }
        }
        public int ExpToLevel() {
            double exp = 0.25 * Math.Floor(Level - 1.0 + 300.0 * (Math.Pow(2.0, (Level - 1.0) / 7.0)));
            return (int)Math.Floor(exp);
        }

        public string Name() {
            if (Contained2 != null)
                return Contained1.Name + "-" + Contained2.Name;
            return Contained1.Name;
        }

        public DecoratedString GetAppearance() {
            if (Contained1 != null && Contained2 != null) {
                CellDecorator[] decs = new CellDecorator[1];
                decs[0] = Contained2.AsDecorator();
                DecoratedString str = new(Contained1.GetAppearance(), decs);

                return str;
            } else {
                return new(Contained1.GetAppearance());
            }
        }
    }
}
