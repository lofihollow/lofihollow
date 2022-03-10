using LofiHollow.DataTypes;
using LofiHollow.Managers;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Inventory : Lofi_UI { 
        public int invMoveIndex = -1;

        public UI_Inventory(int width, int height, string title) : base(width, height, title, "Inventory") { }
         
        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();
            Con.Print((Con.Width / 2) - 4, 0, "BACKPACK");

            for (int i = 0; i < 27; i++) {
                if (i < GameLoop.World.Player.Inventory.Length) {
                    Item item = GameLoop.World.Player.Inventory[i];

                    ColoredString LetterGrade = new("");
                    if (item.Quality > 0)
                        LetterGrade = new ColoredString(" [") + item.LetterGrade() + new ColoredString("]");


                    Con.Print(0, i + 1, item.AsColoredGlyph());
                    if (item.Dec != null) {
                        Con.SetDecorator(0, i+1, 1, new CellDecorator(new Color(item.Dec.R, item.Dec.G, item.Dec.B), item.Dec.Glyph, Mirror.None));
                    }
                    if (!item.IsStackable || (item.IsStackable && item.ItemQuantity == 1))
                        if (item.ItemCat == "Soul") {
                            string name = item.Name;
                            if (item.SoulPhoto != null) {
                                name += " (" + item.SoulPhoto.Name() + ")";
                            }

                            Con.Print(2, i + 1, new ColoredString(name, invMoveIndex == i ? Color.Yellow : Color.White, Color.Black));
                        }
                        else {
                            Con.Print(2, i + 1, new ColoredString(item.Name, invMoveIndex == i ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black) + LetterGrade);
                        }
                    else
                        Con.Print(2, i + 1, new ColoredString(("(" + item.ItemQuantity + ") " + item.Name), invMoveIndex == i ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black) + LetterGrade);

                    ColoredString Options = new("MOVE", (mousePos.Y == i + 1 && mousePos.X < 33) ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black);

                    Options += new ColoredString(" | ", Color.White, Color.Black);

                    if (item.EquipSlot != -1) {
                        Options += new ColoredString("EQUIP", (mousePos.Y == i + 1 && mousePos.X > 33 && mousePos.X < 41) ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black);
                    } else if (item.ItemCat == "Consumable") {
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
                Toggle();
            }

            if (mousePos.X >= 0 && mousePos.X <= 70 && mousePos.Y >= 0 && mousePos.Y <= 40) {
                int slot = mousePos.Y - 1;
                if (slot >= 0 && slot < GameLoop.World.Player.Inventory.Length) {
                    int x = mousePos.X;
                    if (GameHost.Instance.Mouse.LeftClicked) {
                        if (x < 34) {
                            if (invMoveIndex == -1)
                                invMoveIndex = slot;
                            else {
                                Item tempID = GameLoop.World.Player.Inventory[invMoveIndex];
                                GameLoop.World.Player.Inventory[invMoveIndex] = GameLoop.World.Player.Inventory[slot];
                                GameLoop.World.Player.Inventory[slot] = tempID;
                                invMoveIndex = -1;
                            }
                        } else if (x > 34 && x < 42) {
                            Item item = GameLoop.World.Player.Inventory[slot];
                            if (GameLoop.World.Player.Inventory[slot].EquipSlot != -1) {
                                CommandManager.EquipItem(GameLoop.World.Player, slot, GameLoop.World.Player.Inventory[slot]);
                            } else if (item.ItemCat == "Consumable") {
                                string[] itemResult = CommandManager.UseItem(GameLoop.World.Player, item).Split("|"); ;

                                if (itemResult[0] != "f") {
                                    if (item.IsStackable && item.ItemQuantity > 1) {
                                        item.ItemQuantity -= 1;
                                    } else {
                                        GameLoop.World.Player.Inventory[slot] = Item.Copy("lh:(EMPTY)");
                                    }
                                    GameLoop.UIManager.AddMsg(new ColoredString("Used the " + item.Name + ".", Color.AliceBlue, Color.Black));
                                    GameLoop.UIManager.AddMsg(new ColoredString(itemResult[1], Color.AliceBlue, Color.Black));
                                } else {
                                    GameLoop.UIManager.AddMsg(new ColoredString("Tried to use the " + item.Name + ".", Color.AliceBlue, Color.Black));
                                    GameLoop.UIManager.AddMsg(new ColoredString(itemResult[1], Color.AliceBlue, Color.Black));
                                }
                            }
                        } else if (x > 42) {
                            if (slot < GameLoop.World.Player.Inventory.Length && slot >= 0) {
                                CommandManager.DropItem(GameLoop.World.Player, slot);
                            }
                        }
                    }
                }
            }
        } 
    }
}
