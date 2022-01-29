using LofiHollow.EntityData;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Mining {
    [JsonObject(MemberSerialization.OptIn)]
    public class MineTile : ColoredGlyph {
        [JsonProperty]
        public int MineTileID = 0;
        [JsonProperty]
        public string Name = "";
        [JsonProperty]
        public bool Spawns = false;
        [JsonProperty]
        public int TileGlyph = 32;
        [JsonProperty]
        public int ForeR = 0;
        [JsonProperty]
        public int ForeG = 0;
        [JsonProperty]
        public int ForeB = 0;
        [JsonProperty]
        public Decorator Dec;

        [JsonProperty]
        public string OutputID = "";

        [JsonProperty]
        public int RequiredTier = 0;
        [JsonProperty]
        public int TileHP = 0;
        [JsonProperty]
        public int Chance = 0;
        [JsonProperty]
        public int MaxPerMap = 0;
        [JsonProperty]
        public int MinDepth = 0;
        [JsonProperty]
        public int MaxDepth = 0;
        [JsonProperty]
        public int GrantedExp = 0;

        [JsonProperty]
        public bool BlocksMove = false;
        [JsonProperty]
        public bool BlocksLOS = false;

        public bool Visible = false;
        public int ForeA = 255;

        public void Shade() { ForeA = 150; }
        public void Unshade() { ForeA = 255; }

        public void UpdateAppearance() {
            if (Name != "Air") {
                Foreground = new Color(ForeR, ForeG, ForeB, ForeA);
                Background = Color.Black;
            } else {
                Background = new Color(ForeR, ForeG, ForeB, ForeA);
                Foreground = Color.Black;
            }

            Glyph = TileGlyph;
        }

        [JsonConstructor]
        public MineTile() { }

        public MineTile(MineTile other) {
            Name = other.Name;
            TileGlyph = other.TileGlyph;
            ForeR = other.ForeR;
            ForeG = other.ForeG;
            ForeB = other.ForeB;
            ForeA = other.ForeA;

            Dec = other.Dec;
            RequiredTier = other.RequiredTier;
            TileHP = other.TileHP;

            OutputID = other.OutputID;
            Chance = other.Chance;
            MaxPerMap = other.MaxPerMap;
            MinDepth = other.MinDepth;
            MaxDepth = other.MaxDepth;
            GrantedExp = other.GrantedExp;

            BlocksMove = other.BlocksMove;
            BlocksLOS = other.BlocksLOS;
        }

        public MineTile(int index) {
            MineTile other = GameLoop.World.mineTileLibrary[index];

            Name = other.Name;
            TileGlyph = other.TileGlyph;

            ForeR = other.ForeR;
            ForeG = other.ForeG;
            ForeB = other.ForeB;
            ForeA = other.ForeA; 

            Dec = other.Dec;
            RequiredTier = other.RequiredTier;
            TileHP = other.TileHP;
            OutputID = other.OutputID;
            Chance = other.Chance;
            MaxPerMap = other.MaxPerMap;
            MinDepth = other.MinDepth;
            MaxDepth = other.MaxDepth;
            GrantedExp = other.GrantedExp;
            BlocksMove = other.BlocksMove;
            BlocksLOS = other.BlocksLOS;
        }

        public bool Damage(int hitTier) {
            if (hitTier >= RequiredTier) {
                TileHP -= hitTier;
                return true;
            }

            return false;
        }

        public ColoredString GetAppearance() {
            UpdateAppearance();
            return new ColoredString(((char)Glyph).ToString(), Foreground, Background);
        }

        public CellDecorator Decorator() {
            return new(new Color(Dec.R, Dec.G, Dec.G, Dec.A), Dec.Glyph, Mirror.None); 
        }
    }
}
