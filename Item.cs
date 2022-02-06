using SadRogue.Primitives;
using SadConsole;
using LofiHollow.EntityData;
using System.Collections.Generic;
using LofiHollow.Minigames;
using ProtoBuf;
using LofiHollow.Minigames.Photo;
using Newtonsoft.Json;

namespace LofiHollow {
    [ProtoContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class Item {
        [ProtoMember(1)]
        public string Name = "";
        [ProtoMember(2)]
        public string Package = "";
        [ProtoMember(3)]
        public int SubID = 0;
        [ProtoMember(4)]
        public int ItemQuantity = 0;
        [ProtoMember(5)]
        public bool IsStackable = false;
        [ProtoMember(6)]
        public string ShortDesc = "";
        [ProtoMember(7)]
        public string Description = "";
        [ProtoMember(8)]
        public int AverageValue = 0;
        [ProtoMember(9)]
        public float Weight = 0.0f;
        [ProtoMember(10)]
        public int Durability = -1;
        [ProtoMember(11)]
        public int MaxDurability = -1;
        [ProtoMember(12)]
        public int Quality = 1;
        [ProtoMember(13)]
        public int ItemTier = -1;
        [ProtoMember(14)]
        public string ItemCat = "";

        [ProtoMember(15)]
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

        [ProtoMember(16)]
        public Decorator Dec;


        [ProtoMember(17)]
        public int ForegroundR = 0;
        [ProtoMember(18)]
        public int ForegroundG = 0;
        [ProtoMember(19)]
        public int ForegroundB = 0;
        [ProtoMember(20)]
        public int ItemGlyph = 0;

        [ProtoMember(21)]
        public Plant Plant;
        [ProtoMember(22)]
        public List<ToolData> Tool;
        [ProtoMember(23)]
        public List<CraftComponent> Craft;
        [ProtoMember(24)]
        public Equipment Stats;
        [ProtoMember(25)]
        public Photo Photo;
        [ProtoMember(26)]
        public Egg Egg;
        [ProtoMember(27)]
        public MonsterPenFood MonsterFood;
        [ProtoMember(28)]
        public Heal Heal;


        [JsonIgnore]
        public Point Position;
        [JsonIgnore]
        public Point3D MapPos;


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

                Plant = temp.Plant;
                Tool = temp.Tool;
                Craft = temp.Craft;
                Stats = temp.Stats;
                Photo = temp.Photo;
                Egg = temp.Egg;
                MonsterFood = temp.MonsterFood;
                Heal = temp.Heal;

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

            Plant = temp.Plant;
            Tool = temp.Tool;
            Craft = temp.Craft;
            Stats = temp.Stats;
            Photo = temp.Photo;
            Egg = temp.Egg;
            MonsterFood = temp.MonsterFood;
            Heal = temp.Heal;

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

            switch (Quality) {
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
                    qual = ((char)172).ToString();
                    col = Color.White;
                    break;
                default:
                    return new ColoredString("");
            }

            return new ColoredString(qual, col, Color.Black);
        }
    }
}
