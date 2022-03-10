using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LofiHollow.EntityData;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Construction : Lofi_UI { 
        public int SelectedConstructible = -1; 

        public UI_Construction(int width, int height, string title) : base(width, height, title, "Construction") { }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            Con.Print(50, 0, "Deconstruct Mode -" + (char) 12);
            Con.Print(69, 0, Helper.HoverColoredString("X", mousePos == new Point(69, 0)));

            Con.Print(33, 1, "Block");
            Con.Print(39, 1, "Block");
            Con.Print(0, 2, "Construction Name".Align(HorizontalAlignment.Center, 27, ' ') + "|" + " Lv " + "|" + "Mov".Align(HorizontalAlignment.Center, 5, ' ') + "|" + "LOS".Align(HorizontalAlignment.Center, 5, ' ') + "|" + "Can Build".Align(HorizontalAlignment.Center, 11, ' '));
            Con.Print(0, 3, "".Align(HorizontalAlignment.Center, 70, (char) 196));
            for (int i = 0; i < GameLoop.World.constructibles.Count; i++) {
                bool canBuild = CheckValidConstruction(GameLoop.World.constructibles[i]);

                ColoredString line = GameLoop.World.constructibles[i].Appearance();
                line += new ColoredString(" ", Color.White, Color.Black);
                line += Helper.HoverColoredString(GameLoop.World.constructibles[i].Name.Align(HorizontalAlignment.Center, 25, ' '), mousePos.Y == i + 4);
                line += new ColoredString("|", Color.White, Color.Black); 
                line += new ColoredString(("" + GameLoop.World.constructibles[i].RequiredLevel).Align(HorizontalAlignment.Center, 4, ' '), Color.White, Color.Black);
                line += new ColoredString("|", Color.White, Color.Black); 
                if (GameLoop.World.constructibles[i].BlocksMove)
                     line += new ColoredString(4.AsString().Align(HorizontalAlignment.Center, 5, ' '), Color.Lime, Color.Black);
                else
                    line += new ColoredString("x".Align(HorizontalAlignment.Center, 5, ' '), Color.Red, Color.Black);
                line += new ColoredString("|", Color.White, Color.Black);
                if (GameLoop.World.constructibles[i].BlocksLOS)
                    line += new ColoredString(4.AsString().Align(HorizontalAlignment.Center, 5, ' '), Color.Lime, Color.Black);
                else
                    line += new ColoredString("x".Align(HorizontalAlignment.Center, 5, ' '), Color.Red, Color.Black);
                line += new ColoredString("|", Color.White, Color.Black);
                if (canBuild)
                    line += new ColoredString(4.AsString().Align(HorizontalAlignment.Center, 11, ' '), Color.Lime, Color.Black);
                else 
                    line += new ColoredString("x".Align(HorizontalAlignment.Center, 11, ' '), Color.Red, Color.Black);

                Con.Print(0, i + 4, line);
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            }


            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos == new Point(69, 0)) {
                    SelectedConstructible = -1;
                    Toggle();
                }

                if (mousePos.Y - 4 >= 0 && mousePos.Y - 4 < GameLoop.World.constructibles.Count) {
                    SelectedConstructible = mousePos.Y - 4;
                    Toggle();
                }
            }
        }


        public static bool CheckValidConstruction(Constructible con) {
            if (con.RequiredLevel > GameLoop.World.Player.Skills["Construction"].Level)
                return false;

            for (int i = 0; i < con.MaterialsNeeded.Count; i++) {
                int quantity = 0;

                for (int j = 0; j < GameLoop.World.Player.Inventory.Length; j++) {
                    if (con.MaterialsNeeded[i].ID.Contains("Nails")) {
                        if (GameLoop.World.Player.Inventory[j].Name.Contains("Nails")) {
                            quantity += GameLoop.World.Player.Inventory[j].ItemQuantity;
                        }
                    } else {
                        if (GameLoop.World.Player.Inventory[j].FullName() == con.MaterialsNeeded[i].ID) {
                            quantity += GameLoop.World.Player.Inventory[j].ItemQuantity;
                        }
                    }
                }

                if (quantity < con.MaterialsNeeded[i].ItemQuantity) {
                    return false;
                }
            }

            return true;
        } 
    }
}
