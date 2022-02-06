using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LofiHollow.EntityData;
using System.Collections.Generic;
using LofiHollow.Minigames;

namespace LofiHollow {
    [JsonObject(MemberSerialization.OptIn)]
    public class Item {
        [JsonProperty]
        public string Name = "";
        [JsonProperty]
        public string Package = "";
        [JsonProperty]
        public int SubID = 0;
        [JsonProperty]
        public int ItemQuantity = 0;
        [JsonProperty]
        public bool IsStackable = false;
        [JsonProperty]
        public string ShortDesc = "";
        [JsonProperty]
        public string Description = "";
        [JsonProperty]
        public int AverageValue = 0;
        [JsonProperty]
        public float Weight = 0.0f;
        [JsonProperty]
        public int Durability = -1;
        [JsonProperty]
        public int MaxDurability = -1;
        [JsonProperty]
        public int Quality = 1;
        [JsonProperty]
        public int ItemTier = -1;
        [JsonProperty]
        public string ItemCat = "";
         
        [JsonProperty]
        public int EquipSlot = -1;
        // -1: Not equippable
        // 0: Main hand
        // 1: Off hand
        // 2: Helmet
        // 3: Torso
        // 4: Legs
        // 5: Hands
        // 6: Feet
        // 7: Amulet
        // 8: Ring
        // 9: Cape

        [JsonProperty]
        public Dictionary<string, object> Properties = new();

        [JsonProperty]
        public Decorator Dec;


        [JsonProperty]
        public int ForegroundR = 0;
        [JsonProperty]
        public int ForegroundG = 0;
        [JsonProperty]
        public int ForegroundB = 0;
        [JsonProperty]
        public int ItemGlyph = 0;


        [JsonConstructor]
        public Item() { }


        public Item(string name, int quantity = 1) {
            if (GameLoop.World != null && GameLoop.World.itemLibrary != null && GameLoop.World.itemLibrary.ContainsKey(name)) {
                Item temp = GameLoop.World.itemLibrary[name];

                Name = temp.Name;
                Package = temp.Package;
                SubID = temp.SubID;
                ItemCat = temp.ItemCat;
                EquipSlot = temp.EquipSlot;
                IsStackable = temp.IsStackable; 
                Description = temp.Description;
                ShortDesc = temp.ShortDesc;

                Weight = temp.Weight;
                AverageValue = temp.AverageValue;

                Durability = temp.Durability;
                MaxDurability = temp.MaxDurability;
                ItemTier = temp.ItemTier;
                 
                Quality = temp.Quality;

                ForegroundR = temp.ForegroundR; 
                ForegroundG = temp.ForegroundG; 
                ForegroundB = temp.ForegroundB;
                ItemGlyph = temp.ItemGlyph; 
                Dec = temp.Dec;

                foreach(KeyValuePair<string, object> kv in temp.Properties) {
                    Properties.Add(kv.Key, kv.Value);
                }

                if (name == "lh:(EMPTY)") {
                    ItemQuantity = 0;
                    Quality = 0;
                } else {
                    if (Quality == 0)
                        Quality = 1;
                    ItemQuantity = quantity;
                }
            }
        }

        public Item(Item temp) { 
            Name = temp.Name;
            Package = temp.Package;
            SubID = temp.SubID;
            ItemCat = temp.ItemCat;
            EquipSlot = temp.EquipSlot;
            IsStackable = temp.IsStackable;
            Description = temp.Description;
            ShortDesc = temp.ShortDesc;

            Weight = temp.Weight;
            AverageValue = temp.AverageValue;
            Durability = temp.Durability;
            MaxDurability = temp.MaxDurability;

            ForegroundR = temp.ForegroundR;
            ForegroundG = temp.ForegroundG;
            ForegroundB = temp.ForegroundB;
            ItemGlyph = temp.ItemGlyph;
            ItemTier = temp.ItemTier;
             
            Quality = temp.Quality;

            Dec = temp.Dec;

            foreach (KeyValuePair<string, object> kv in temp.Properties) {
                Properties.Add(kv.Key, kv.Value);
            }


            if (Name == "(EMPTY)") {
                ItemQuantity = 0;
                Quality = 0;
            } else {
                if (Quality == 0)
                    Quality = 1;
                ItemQuantity = 1;
            }
        }

        public ColoredString AsColoredGlyph() {
            ColoredString output = new(((char)ItemGlyph).ToString(), new Color(ForegroundR, ForegroundG, ForegroundB), Color.Transparent);
            return output;
        }

        public CellDecorator GetDecorator() {
            CellDecorator dec = new(new Color(Dec.R, Dec.G, Dec.B, Dec.A), Dec.Glyph, Mirror.None);
            return dec;
        }

        public bool StacksWith(Item other, bool JustIdentical = false) {
            if (FullName() == other.FullName() && SubID == other.SubID && Quality == other.Quality && IsStackable)
                return true;
            if (Name == other.Name && Package == other.Package && SubID == other.SubID && Quality == other.Quality && JustIdentical)
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
                    qual = ((char) 172).ToString();
                    col = Color.White;
                    break;
                default:
                    return new ColoredString(""); 
            }

            return new ColoredString(qual, col, Color.Black);
        }
    }
}
