using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Construction {
        public SadConsole.Console ConstructionConsole;
        public Window ConstructionWindow;
        public int SelectedConstructible = -1; 

        public UI_Construction(int width, int height, string title) {
            ConstructionWindow = new(width, height);
            ConstructionWindow.CanDrag = false;
            ConstructionWindow.Position = new(11, 6);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            ConstructionConsole = new(invConWidth, invConHeight);
            ConstructionConsole.Position = new(1, 1);
            ConstructionWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            ConstructionWindow.Children.Add(ConstructionConsole);
            GameLoop.UIManager.Children.Add(ConstructionWindow);

            ConstructionWindow.Show();
            ConstructionWindow.IsVisible = false;
        }


        public void RenderConstruction() {
            Point mousePos = new MouseScreenObjectState(ConstructionConsole, GameHost.Instance.Mouse).CellPosition;

            ConstructionConsole.Clear();

            ConstructionConsole.Print(50, 0, "Deconstruct Mode -" + (char) 12);
            ConstructionConsole.Print(69, 0, Helper.HoverColoredString("X", mousePos == new Point(69, 0)));

            ConstructionConsole.Print(33, 1, "Block");
            ConstructionConsole.Print(39, 1, "Block");
            ConstructionConsole.Print(0, 2, "Construction Name".Align(HorizontalAlignment.Center, 27, ' ') + "|" + " Lv " + "|" + "Mov".Align(HorizontalAlignment.Center, 5, ' ') + "|" + "LOS".Align(HorizontalAlignment.Center, 5, ' ') + "|" + "Can Build".Align(HorizontalAlignment.Center, 11, ' '));
            ConstructionConsole.Print(0, 3, "".Align(HorizontalAlignment.Center, 70, (char) 196));
            for (int i = 0; i < GameLoop.World.constructibles.Count; i++) {
                bool canBuild = CheckValidConstruction(GameLoop.World.constructibles[i]);

                ColoredString line = GameLoop.World.constructibles[i].Appearance();
                line += new ColoredString(" ", Color.White, Color.Black);
                line += Helper.HoverColoredString(GameLoop.World.constructibles[i].Name.Align(HorizontalAlignment.Center, 25, ' '), mousePos.Y == i + 4);
                line += new ColoredString("|", Color.White, Color.Black); 
                line += new ColoredString(("" + GameLoop.World.constructibles[i].RequiredLevel).Align(HorizontalAlignment.Center, 4, ' '), Color.White, Color.Black);
                line += new ColoredString("|", Color.White, Color.Black); 
                if (GameLoop.World.constructibles[i].BlocksMove)
                     line += new ColoredString(((char)4).ToString().Align(HorizontalAlignment.Center, 5, ' '), Color.Lime, Color.Black);
                else
                    line += new ColoredString("x".Align(HorizontalAlignment.Center, 5, ' '), Color.Red, Color.Black);
                line += new ColoredString("|", Color.White, Color.Black);
                if (GameLoop.World.constructibles[i].BlocksLOS)
                    line += new ColoredString(((char)4).ToString().Align(HorizontalAlignment.Center, 5, ' '), Color.Lime, Color.Black);
                else
                    line += new ColoredString("x".Align(HorizontalAlignment.Center, 5, ' '), Color.Red, Color.Black);
                line += new ColoredString("|", Color.White, Color.Black);
                if (canBuild)
                    line += new ColoredString(((char)4).ToString().Align(HorizontalAlignment.Center, 11, ' '), Color.Lime, Color.Black);
                else 
                    line += new ColoredString("x".Align(HorizontalAlignment.Center, 11, ' '), Color.Red, Color.Black);

                ConstructionConsole.Print(0, i + 4, line);
            }
        }

        public void ConstructionInput() {
            Point mousePos = new MouseScreenObjectState(ConstructionConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                ToggleConstruction();
            }


            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos == new Point(69, 0)) {
                    SelectedConstructible = -1;
                    ToggleConstruction();
                }

                if (mousePos.Y - 4 >= 0 && mousePos.Y - 4 < GameLoop.World.constructibles.Count) {
                    SelectedConstructible = mousePos.Y - 4;
                    ToggleConstruction();
                }
            }
        }


        public static bool CheckValidConstruction(Constructible con) {
            if (con.RequiredLevel > GameLoop.World.Player.Skills["Construction"].Level)
                return false;

            for (int i = 0; i < con.MaterialsNeeded.Count; i++) {
                int quantity = 0;

                for (int j = 0; j < GameLoop.World.Player.Inventory.Length; j++) {
                    if (con.MaterialsNeeded[i].Name.Contains("Nails")) {
                        if (GameLoop.World.Player.Inventory[j].Name.Contains("Nails") && GameLoop.World.Player.Inventory[j].SubID >= con.MaterialsNeeded[i].SubID) {
                            quantity += GameLoop.World.Player.Inventory[j].ItemQuantity;
                        }
                    } else {
                        if (GameLoop.World.Player.Inventory[j].Name == con.MaterialsNeeded[i].Name && GameLoop.World.Player.Inventory[j].SubID == con.MaterialsNeeded[i].SubID) {
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




        public void ToggleConstruction() {
            if (ConstructionWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                ConstructionWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
            } else {
                GameLoop.UIManager.selectedMenu = "Construction";
                ConstructionWindow.IsVisible = true;
                ConstructionWindow.IsFocused = true;
            }
        }
    }
}
