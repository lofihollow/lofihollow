using LofiHollow.DataTypes;
using LofiHollow.Managers;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Inventory : InstantUI { 
        public int invMoveIndex = -1;

        public UI_Inventory(int width, int height, string title) : base(width, height, title, "Inventory") { }
         
        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();
            Con.Print((Con.Width / 2) - 4, 0, "BACKPACK");

            for (int i = 0; i < 27; i++) {
                if (i < GameLoop.World.Player.Inventory.Count) {
                    Item item = GameLoop.World.Player.Inventory[i];

                    ColoredString LetterGrade = new("");
                    if (item.Quality > 0)
                        LetterGrade = new ColoredString(" [") + item.LetterGrade() + new ColoredString("]");
                     
                    if (!item.IsStackable || (item.IsStackable && item.Quantity == 1)) 
                        Con.Print(2, i + 1, new ColoredString(item.Name, invMoveIndex == i ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black) + LetterGrade);
                    else
                        Con.Print(2, i + 1, new ColoredString(("(" + item.Quantity + ") " + item.Name), invMoveIndex == i ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black) + LetterGrade);

                    ColoredString Options = new("MOVE", (mousePos.Y == i + 1 && mousePos.X < 33) ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black);

                    Options += new ColoredString(" | ", Color.White, Color.Black);

                    if (item.EquipSlot != -1) {
                        Options += new ColoredString("EQUIP", (mousePos.Y == i + 1 && mousePos.X > 33 && mousePos.X < 41) ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black);
                    } else if (item.Consumable) {
                        Options += new ColoredString(" USE ", (mousePos.Y == i + 1 && mousePos.X > 33 && mousePos.X < 41) ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black);
                    } else {
                        Options += new ColoredString("     ", item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black);
                    }

                    Options += new ColoredString(" | ", Color.White, Color.Black);
                    Options += new ColoredString("DROP", (mousePos.Y == i + 1 && mousePos.X > 41) ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black);

                    Con.Print(28, i + 1, Options);
                } else {
                    Con.Print(2, i + 1, new ColoredString("[LOCKED]", Color.DarkSlateGray, Color.Black));
                }
            }

        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.I) || GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                GameLoop.UIManager.ToggleUI("Inventory");
            } 
        } 
    }
}
