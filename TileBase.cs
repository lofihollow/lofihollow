using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LofiHollow.EntityData;

namespace LofiHollow {
    [JsonObject(MemberSerialization.OptIn)]
    public class TileBase : ColoredGlyph {
        [JsonProperty]
        public int TileID = 0;
        [JsonProperty]
        public bool IsBlockingMove = false;
        [JsonProperty]
        public bool IsBlockingLOS = false;
        [JsonProperty]
        public bool SpawnsMonsters = false;

        [JsonProperty]
        public string Name = "Grass";

        [JsonProperty]
        public string MiscString = "";

        [JsonProperty]
        public int ForegroundR = 0;
        [JsonProperty]
        public int ForegroundG = 0;
        [JsonProperty]
        public int ForegroundB = 0;
        [JsonProperty]
        public int TileGlyph = 0;


        [JsonProperty]
        public Decorator Dec;

        [JsonProperty]
        public LockOwner Lock;

        [JsonProperty]
        public Container Container;

        [JsonProperty]
        public Plant Plant = null;

        [JsonProperty]
        public SkillableTile SkillableTile = null;

        public bool DeconstructFlag = false;

        [JsonConstructor]
        public TileBase() : base(Color.Black, Color.Transparent, 32) {
            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Glyph = TileGlyph;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) {
            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Glyph = TileGlyph; 
        }


        public TileBase(TileBase other) {
            IsBlockingMove = other.IsBlockingMove;
            IsBlockingLOS = other.IsBlockingLOS;
            Name = other.Name;
            TileID = other.TileID;
            SpawnsMonsters = other.SpawnsMonsters;

            ForegroundR = other.ForegroundR;
            ForegroundG = other.ForegroundG;
            ForegroundB = other.ForegroundB;
            TileGlyph = other.TileGlyph;
            SkillableTile = other.SkillableTile;

            MiscString = other.MiscString;

            Dec = other.Dec;
            Lock = other.Lock;
            Plant = other.Plant;
            Container = other.Container;

            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Glyph = TileGlyph;
        } 

        public TileBase(int index) {
            if (GameLoop.World.tileLibrary.ContainsKey(index)) {
                TileBase other = GameLoop.World.tileLibrary[index];
                IsBlockingMove = other.IsBlockingMove;
                IsBlockingLOS = other.IsBlockingLOS;
                Name = other.Name;
                TileID = other.TileID;
                SpawnsMonsters = other.SpawnsMonsters;

                ForegroundR = other.ForegroundR;
                ForegroundG = other.ForegroundG;
                ForegroundB = other.ForegroundB;
                TileGlyph = other.TileGlyph;
                SkillableTile = other.SkillableTile;

                MiscString = other.MiscString;

                Dec = other.Dec;
                Lock = other.Lock;
                Plant = other.Plant;
                Container = other.Container;

                Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
                Glyph = TileGlyph;
            }
        } 

        public void SetNewFG(Color fg, int glyph) {
            Foreground = fg;
            Glyph = glyph;
        }

        public void UpdateAppearance() {
            if (Lock != null) {
                if (Lock.Closed)
                    TileGlyph = Lock.ClosedGlyph;
                else
                    TileGlyph = Lock.OpenedGlyph;

                IsBlockingLOS = Lock.Closed;
                IsBlockingMove = Lock.Closed;
            }

            if (Plant != null && Plant.CurrentStage >= 0) { 
                ForegroundR = Plant.Stages[Plant.CurrentStage].ColorR;
                ForegroundG = Plant.Stages[Plant.CurrentStage].ColorG;
                ForegroundB = Plant.Stages[Plant.CurrentStage].ColorB;
                TileGlyph = Plant.Stages[Plant.CurrentStage].Glyph;

                if (Plant.Stages[Plant.CurrentStage].Dec != null) {
                    Dec = Plant.Stages[Plant.CurrentStage].Dec;
                }
            }

            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Glyph = TileGlyph;
        }

        public void Shade() {
            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB, 150);
        }

        public void Unshade() {
            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
        }

        public ColoredString AsColoredGlyph() {
            return new ColoredString(((char) TileGlyph).ToString(), Foreground, Background);
        }

        public CellDecorator GetDecorator() {
            CellDecorator dec = new(new Color(Dec.R, Dec.G, Dec.B, Dec.A), Dec.Glyph, Mirror.None);
            return dec;
        }
    }
}