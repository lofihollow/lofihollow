using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LofiHollow.EntityData;
using System.Collections.Generic;
using LofiHollow.Minigames; 
using LofiHollow.Minigames.Archaeology;

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptOut)]
    public class Item { 
        public string Name = ""; 
        public string Package = "";  
        public int Quantity = 0; 
        public bool IsStackable = false;  
        public string Description = ""; 
        public int AverageValue = 0; 
        public float Weight = 0.0f;  
        public int Quality = 1;
        public int Tier = 0; // Usually used for level required to equip
        public string Category = ""; 
        public string RelatedSkill = "";
        public string MiscString = "";
        public int MiscInt = 0;
         
        public bool Consumable = false;
          
        public int EquipSlot = -1;
        // -1: Not equippable
        // 0: Wielded
        // 1: Helmet
        // 2: Torso
        // 3: Legs
        // 4: Hands
        // 5: Feet
        // 6: Ring
        // 7: Amulet
        // 8: Back (backpack / cape)

           
        public Plant Plant;    
        public ArchArtifact Artifact;

        [JsonConstructor]
        public Item() { }


        public static Item Copy(string name, int quantity = 1) {
            if (GameLoop.World != null && GameLoop.World.itemLibrary != null && GameLoop.World.itemLibrary.ContainsKey(name)) {
                Item temp = Helper.Clone(GameLoop.World.itemLibrary[name]); 

                if (temp.Name == "(EMPTY)") {
                    temp.Quantity = 0;
                    temp.Quality = 0;
                } else {
                    if (temp.Quality == 0)
                        temp.Quality = 1;
                    temp.Quantity = quantity;
                }

                return temp;
            }

            return null;
        }

        public static Item Copy(Item other, int newQuantity = 1) {
            Item other2 = Helper.Clone(other);
            if (other2.Name == "lh:(EMPTY)") {
                other2.Quantity = 0;
                other2.Quality = 0;
            }
            else {
                if (other2.Quality == 0)
                    other2.Quality = 1;
                other2.Quantity = newQuantity;
            }

            return other2;
        }  

        public bool StacksWith(Item other) {
            if (FullName() == other.FullName() && MiscString == other.MiscString && MiscInt == other.MiscInt && Quality == other.Quality && IsStackable)
                return true; 
            return false;
        }

        public string FullName() {
            return Package + ":" + Name;
        }

        public ColoredString LetterGrade() {
            string qual = "";
            Color col = Color.Black;
             
            switch(Quality) {
                case 1:
                    qual = "F";
                    col = Color.Red;
                    break;
                case 2:
                    qual = "E";
                    col = Color.OrangeRed;
                    break;
                case 3:
                    qual = "D";
                    col = Color.Orange;
                    break;
                case 4:
                    qual = "C";
                    col = Color.Yellow;
                    break;
                case 5:
                    qual = "B";
                    col = Color.LimeGreen;
                    break;
                case 6:
                    qual = "A";
                    col = Color.DodgerBlue;
                    break;
                case 7:
                    qual = "A+";
                    col = Color.Cyan;
                    break;
                case 8:
                    qual = "S";
                    col = Color.HotPink;
                    break;
                case 9:
                    qual = "S+";
                    col = Color.MediumPurple;
                    break;
                case 10:
                    qual = "S++";
                    col = Color.BlueViolet;
                    break;
                case 11:
                    qual = 172.AsString();
                    col = Color.White;
                    break;
                default:
                    return new ColoredString(""); 
            }

            return new ColoredString(qual, col, Color.Black);
        }
    }
}
