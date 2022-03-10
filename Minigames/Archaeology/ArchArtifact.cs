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

        public int BaseExp = 0;
        public int RequiredLevel = 0;
        public string TurnIntoItem = "";
        public bool KeepArtifact = false;

        public bool AlreadyClean = false;

        public ArchTile[] Tiles;


        [JsonConstructor]
        public ArchArtifact() { 
            Tiles = new ArchTile[30 * 30]; 

            for (int i = 0; i < Tiles.Length; i++) {
                Tiles[i] = new ArchTile();
            }
        }

        public ArchArtifact(ArchArtifact other) {
            Tiles = new ArchTile[30 * 30];

            for (int i = 0; i < Tiles.Length; i++) {
                Tiles[i] = new(other.Tiles[i]);
            }

            Name = other.Name;
            Package = other.Package;
            UnidentifiedName = other.UnidentifiedName;
            Location = other.Location;
            BaseValue = other.BaseValue;
            BaseExp = other.BaseExp;
            Weight = other.Weight;
            RequiredLevel = other.RequiredLevel;
            TurnIntoItem = other.TurnIntoItem;
            KeepArtifact = other.KeepArtifact;
            AlreadyClean = other.AlreadyClean;
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
            if (!AlreadyClean) {
                AlreadyClean = true;
                
                if (GameLoop.World.Player.Skills.ContainsKey("Archaeology")) {
                    int granted = BaseExp * Quality(); 
                    GameLoop.UIManager.AddMsg("You got " + granted + " experience for cleaning the " + Name + "!");
                    GameLoop.World.Player.Skills["Archaeology"].GrantExp(granted); 
                }


                if (TurnIntoItem == "") {
                    item.Name = Name;
                    item.Package = Package;
                    item.AverageValue = BaseValue;
                    item.Quality = Quality();
                    item.Weight = Weight;
                }
                else {
                    item = Item.Copy(TurnIntoItem);
                    if (KeepArtifact)
                        item.Artifact = this;
                }

                for (int i = 0; i < Tiles.Length; i++) {
                    Tiles[i].Clean();
                }
            }
        }

        public void Dirtify(bool StoneToo = false) {
            for (int i = 0; i < Tiles.Length; i++) {
                Tiles[i].DirtPercent = GameLoop.rand.Next(5) + 6;

                if (StoneToo)
                    Tiles[i].StonePercent = GameLoop.rand.Next(5) + 6;
            }
        }

        public void Weather(int maxCondition) {
            for (int i = 0; i < Tiles.Length; i++) {
                Tiles[i].Condition = GameLoop.rand.Next(maxCondition);
            }
        }

        public void BrushSpot(int x, int y, int pow) {
            int index = x + (y * 30);
            
            if (index >= 0 && index < Tiles.Length) {
                Tiles[index].Brush(pow); 
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
