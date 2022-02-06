using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LofiHollow.EntityData;
using ProtoBuf;

namespace LofiHollow {
    [ProtoContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class Tile : ColoredGlyph {
        [ProtoMember(1)]
        public string Name = "Grass";
        [ProtoMember(2)]
        public string Package = "lh";
        [ProtoMember(3)]
        public bool IsBlockingMove = false;
        [ProtoMember(4)]
        public bool IsBlockingLOS = false;

        [ProtoMember(5)]
        public string MiscString = "";

        [ProtoMember(6)]
        public int ForegroundR = 0;
        [ProtoMember(7)]
        public int ForegroundG = 0;
        [ProtoMember(8)]
        public int ForegroundB = 0;
        [ProtoMember(9)]
        public int TileGlyph = 0;


        [ProtoMember(10)]
        public double LightBlocked = 0.0;
        [ProtoMember(11)]
        public bool ExposedToSky = false;
        [ProtoMember(12)]
        public Light EmitsLight;

        [JsonIgnore]
        public int CurrentLight = 0;

        [ProtoMember(13)]
        public Decorator Dec;

        [ProtoMember(14)]
        public LockOwner Lock;

        [ProtoMember(15)]
        public Container Container;

        [ProtoMember(16)]
        public Plant Plant = null;

        [ProtoMember(17)]
        public SkillableTile SkillableTile = null;

        [JsonIgnore]
        public bool DeconstructFlag = false;

        [JsonConstructor]
        public Tile() : base(Color.Black, Color.Transparent, 32) {
            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Glyph = TileGlyph;
            UpdateAppearance();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) {
            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Glyph = TileGlyph; 
        }


        public Tile(Tile other) {
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

            LightBlocked = other.LightBlocked;
            ExposedToSky = other.ExposedToSky;
            EmitsLight = other.EmitsLight;

            Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Glyph = TileGlyph;
        } 

        public Tile(string name) {
            if (GameLoop.World.tileLibrary.ContainsKey(name)) {
                Tile other = GameLoop.World.tileLibrary[name];
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

                LightBlocked = other.LightBlocked;
                ExposedToSky = other.ExposedToSky;
                EmitsLight = other.EmitsLight;

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
            return new ColoredString(((char) TileGlyph).ToString(), Foreground, Background);
        }

        public CellDecorator GetDecorator() {
            CellDecorator dec = new(new Color(Dec.R, Dec.G, Dec.B, Dec.A), Dec.Glyph, Mirror.None);
            return dec;
        }
    }
}