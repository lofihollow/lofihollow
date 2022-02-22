using LofiHollow.DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Archaeology {
    public class ArchArtifact {
        public string Name = "";
        public string Package = "";

        public string UnidentifiedName = "Fossil";

        public string Location = "Any";
        public int BaseValue = 20;
        public int Weight = 1;

        public ArchTile[] Tiles;


        [JsonConstructor]
        public ArchArtifact() { 
            Tiles = new ArchTile[30 * 30]; 

            for (int i = 0; i < Tiles.Length; i++) {
                Tiles[i] = new ArchTile();
            }
        } 

        public string FullName() {
            return Package + ":" + Name;
        }



        public void SetStats(Item item) {
            item.Name = UnidentifiedName;
            item.AverageValue = 5;
            item.Quality = -1;
            item.Weight = Weight;
        }


        public void TransformToCleaned(Item item) {
            item.Name = Name;
            item.Package = Package;
            item.AverageValue = BaseValue;
            item.Quality = Quality();
            item.Weight = Weight;

            for (int i = 0; i < Tiles.Length; i++) {
                Tiles[i].Clean();
            }
        }

        public bool FullyCleaned() { 
            for (int i = 0; i < Tiles.Length; i++) {
                if (Tiles[i].Vital()) {
                    if (Tiles[i].DirtPercent > 0 || Tiles[i].StonePercent > 0)
                        return false;
                }
            }

            return true;
        }


        public int Quality() {
            int RunningTotal = 0;
            int TotalTiles = 0;

            for (int i = 0; i < Tiles.Count(); i++) {
                if (Tiles[i].Vital()) {
                    RunningTotal += Tiles[i].Condition;
                    TotalTiles++;
                }
            }

            return (int)Math.Floor((double) RunningTotal / (double) TotalTiles);
        }
    }
}
