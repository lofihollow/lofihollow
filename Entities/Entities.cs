using System;
using Newtonsoft.Json;
using SadRogue.Primitives;

namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Entity : SadConsole.Entities.Entity, GoRogue.IHasID { 
        public uint ID { get; private set; } // stores the entity's unique identification number

        [JsonProperty]
        public Point3D MapPos = new Point3D(0, 0, 0);

        protected Entity(Color foreground, Color background, int glyph, int width = 1, int height = 1) : base(foreground, background, glyph, 0) {
            Appearance.Foreground = foreground;
            Appearance.Background = background;
            Appearance.Glyph = glyph;

            // Create a new unique identifier for this entity
            ID = Map.IDGenerator.UseID();
        } 
    }
}
