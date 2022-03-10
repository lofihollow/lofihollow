using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LofiHollow.EntityData;
using System.Collections.Generic;
using LofiHollow.Minigames;
using LofiHollow.Minigames.Photo;
using LofiHollow.Minigames.Archaeology;

namespace LofiHollow.DataTypes {
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
        public string ItemSkill = "";
         
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
        // 10: Monster Soul

        [JsonProperty]
        public Decorator Dec;

        [JsonProperty]
        public Plant Plant;
        [JsonProperty]
        public Photo Photo;
        [JsonProperty]
        public SoulPhoto SoulPhoto;
        [JsonProperty]
        public List<ToolData> Tool;
        [JsonProperty]
        public List<CraftComponent> Craft;
        [JsonProperty]
        public Heal Heal;
        [JsonProperty]
        public Equipment Stats;
        [JsonProperty]
        public ArchArtifact Artifact;
        [JsonProperty]
        public SpawnAnimal SpawnAnimal;

        [JsonProperty]
        public int ForegroundR = 0;
        [JsonProperty]
        public int ForegroundG = 0;
        [JsonProperty]
        public int ForegroundB = 0;
        [JsonProperty]
        public int ItemGlyph = 0;

        [JsonProperty]
        public string LeftClickScript = "";
        [JsonProperty]
        public string RightClickScript = "";
        [JsonProperty]
        public string LeftReleaseScript = ""; 

        [JsonConstructor]
        public Item() { }


        public static Item Copy(string name, int quantity = 1) {
            if (GameLoop.World != null && GameLoop.World.itemLibrary != null && GameLoop.World.itemLibrary.ContainsKey(name)) {
                Item temp = Helper.Clone(GameLoop.World.itemLibrary[name]); 

                if (temp.Name == "(EMPTY)") {
                    temp.ItemQuantity = 0;
                    temp.Quality = 0;
                } else {
                    if (temp.Quality == 0)
                        temp.Quality = 1;
                    temp.ItemQuantity = quantity;
                }

                return temp;
            }

            return null;
        }

        public static Item Copy(Item other, int newQuantity = 1) {
            Item other2 = Helper.Clone(other);
            if (other2.Name == "lh:(EMPTY)") {
                other2.ItemQuantity = 0;
                other2.Quality = 0;
            }
            else {
                if (other2.Quality == 0)
                    other2.Quality = 1;
                other2.ItemQuantity = newQuantity;
            }

            return other2;
        } 

        public ColoredString AsColoredGlyph() {
            ColoredString output = new(ItemGlyph.AsString(), new Color(ForegroundR, ForegroundG, ForegroundB), Color.Transparent);
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
