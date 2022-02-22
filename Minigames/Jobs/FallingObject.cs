using SadRogue.Primitives; 

namespace LofiHollow.Minigames.Jobs {
    public class FallingObject {
        public double LastTimeMoved = 0;
        public int FallingSpeed = 0;
        public int X = 0;
        public int Y = 0;
        public string Name = "";

        public int Glyph = 235;
        public Color Col = Color.Lime;

        public FallingObject() {
            X = GameLoop.rand.Next(50);
            Y = 5;

            string[] objects = { "Apple", "Orange", "Lime", "Spider" };

            Name = objects[GameLoop.rand.Next(objects.Length)];

            if (Name == "Apple")
                Col = Color.Red;
            else if (Name == "Orange")
                Col = Color.Orange;
            else if (Name == "Lime")
                Col = Color.Lime;
            else if (Name == "Spider") {
                Col = Color.White;
                Glyph = 42;
            }

            FallingSpeed = (GameLoop.rand.Next(10) + 1) + 20;
        }
    }
}
