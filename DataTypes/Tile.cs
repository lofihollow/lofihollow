using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LofiHollow.EntityData;
using LofiHollow.Entities;

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptIn)]
    public class Tile : ColoredGlyph {
        [JsonProperty]
        public string Name = "Grass";
        [JsonProperty]
        public string Package = "lh";
        [JsonProperty]
        public bool IsBlockingMove = false;
        [JsonProperty]
        public bool IsBlockingLOS = false; 

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
        public double LightBlocked = 0.0;
        [JsonProperty]
        public bool ExposedToSky = true;
        [JsonProperty]
        public Light EmitsLight;


        public int CurrentLight = 0;

        [JsonProperty]
        public Decorator Dec;

        [JsonProperty]
        public LockOwner Lock;

        [JsonProperty]
        public Container Container;

        [JsonProperty]
        public Plant Plant = null;

        [JsonProperty]
        public TeleportTile TeleportTile = null;


        [JsonProperty]
        public SkillableTile SkillableTile = null;

        public bool DeconstructFlag = false;

        [JsonConstructor]
        public Tile() : base(Color.Black, Color.Transparent, 32) {
            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Glyph = TileGlyph;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) {
            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Glyph = TileGlyph; 
        }


        public Tile(Tile other) {
            SetAll(other);
        }

        public void SetAll(Tile other) {
            IsBlockingMove = other.IsBlockingMove;
            IsBlockingLOS = other.IsBlockingLOS;
            Name = other.Name;
            Package = other.Package; 

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
            TeleportTile = other.TeleportTile;

            LightBlocked = other.LightBlocked;
            ExposedToSky = other.ExposedToSky;
            EmitsLight = other.EmitsLight;

            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Glyph = TileGlyph;
        } 

        public Tile(string name) {
            if (GameLoop.World.tileLibrary.ContainsKey(name)) {
                Tile other = GameLoop.World.tileLibrary[name];
                SetAll(other);
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

        public string FullName() {
            return Package + ":" + Name;
        }

        public void SetLight(double light) {
            CurrentLight = (int) (light * 255);
        }


        public void Shade() {
            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB, 150);
        }

        public void Unshade() {
            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
        }

        public ColoredString AsColoredGlyph() {
            return new ColoredString(TileGlyph.AsString(), Foreground, Background);
        }

        public CellDecorator GetDecorator() {
            CellDecorator dec = new(new Color(Dec.R, Dec.G, Dec.B, Dec.A), Dec.Glyph, Mirror.None);
            return dec;
        }
    }
}