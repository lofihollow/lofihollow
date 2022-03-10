using LofiHollow.Managers; 
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using LofiHollow.EntityData;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using System;

namespace LofiHollow.UI {
    public class UI_Container : Lofi_UI { 
        public Container CurrentContainer; 
        public Point ContainerPosition;

        public UI_Container(int width, int height, string title) : base(width, height, title, "Container") { }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            Con.Clear();


            Con.Print(0, 0, CurrentContainer.Name.Align(HorizontalAlignment.Center, 36));
            Con.Print(0, 1, "".Align(HorizontalAlignment.Center, 36, '-'));
            Con.DrawLine(new Point(36, 0), new Point(36, 29), '|', Color.White, Color.Black);
            Con.Print(37, 0, "Your Backpack".Align(HorizontalAlignment.Center, 36));
            Con.Print(37, 1, "".Align(HorizontalAlignment.Center, 36, '-'));


            Con.DrawLine(new Point(42, 2), new Point(42, 29), '|', Color.White, Color.Black);
            Con.DrawLine(new Point(30, 2), new Point(30, 29), '|', Color.White, Color.Black);

            for (int i = 0; i < CurrentContainer.Items.Count; i++) {
                string qty = "";
                if (CurrentContainer.Items[i].ItemQuantity > 1)
                    qty = CurrentContainer.Items[i].ItemQuantity + "x ";
                Con.Print(0, i + 2, CurrentContainer.Items[i].AsColoredGlyph());

                if (CurrentContainer.Items[i].Dec != null)
                    Con.SetDecorator(0, i + 2, 1, CurrentContainer.Items[i].GetDecorator());
                else
                    Con.ClearDecorators(0, i + 2, 1);

                if (CurrentContainer.Items[i].ItemCat == "Soul") {
                    string name = CurrentContainer.Items[i].Name;
                    if (CurrentContainer.Items[i].SoulPhoto != null) {
                        name += " (" + CurrentContainer.Items[i].SoulPhoto.Name() + ")";
                    }

                    Con.Print(2, i + 2, Helper.HoverColoredString(qty + name, mousePos.X < 36 && mousePos.Y == i + 2));
                }
                else {
                    Con.Print(2, i + 2, Helper.HoverColoredString(qty + CurrentContainer.Items[i].Name, mousePos.X < 36 && mousePos.Y == i + 2));
                }

                Con.PrintClickable(35, i + 2, 12.AsString(), UI_Clicks, "MoveAllContainer," + (i));
                Con.PrintClickable(33, i + 2, 25.AsString(), UI_Clicks, "MoveHalfContainer," + (i));
                Con.PrintClickable(31, i + 2, 49.AsString(), UI_Clicks, "Move1Container," + (i));
                Con.Print(30, i + 2, "|");
            }

            for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                string qty = "";
                if (GameLoop.World.Player.Inventory[i].ItemQuantity > 1)
                    qty = GameLoop.World.Player.Inventory[i].ItemQuantity + "x ";
                Con.PrintClickable(37, i + 2, 11.AsString(), UI_Clicks, "MoveAllInventory," + (i));
                Con.PrintClickable(39, i + 2, 25.AsString(), UI_Clicks, "MoveHalfInventory," + (i));
                Con.PrintClickable(41, i + 2, 49.AsString(), UI_Clicks, "Move1Inventory," + (i));
                Con.Print(42, i + 2, "|");
                Con.Print(43, i + 2, GameLoop.World.Player.Inventory[i].AsColoredGlyph());

                if (GameLoop.World.Player.Inventory[i].Dec != null)
                    Con.SetDecorator(43, i + 2, 1, GameLoop.World.Player.Inventory[i].GetDecorator());
                else
                    Con.ClearDecorators(43, i + 2, 1);
                if (GameLoop.World.Player.Inventory[i].ItemCat == "Soul") {
                    string name = GameLoop.World.Player.Inventory[i].Name;
                    if (GameLoop.World.Player.Inventory[i].SoulPhoto != null) {
                        name += " (" + GameLoop.World.Player.Inventory[i].SoulPhoto.Name() + ")";
                    }

                    Con.Print(45, i + 2, Helper.HoverColoredString(qty + name, mousePos.X > 36 && mousePos.Y == i + 2));
                }
                else {
                    Con.Print(45, i + 2, Helper.HoverColoredString(qty + GameLoop.World.Player.Inventory[i].Name, mousePos.X > 36 && mousePos.Y == i + 2));
                }
            }

            Con.PrintClickable(72, 0, "X", UI_Clicks, "Close,0");
        }

        public override void Input() { 
            bool updateChest = false;

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            }

            foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                if (key.Character >= 'A' && key.Character <= 'z') { 
                    CurrentContainer.Name += key.Character;
                    updateChest = true;
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Space)) {
                CurrentContainer.Name += " ";
                updateChest = true;
            }

            if (CurrentContainer.Name.Length > 0) {
                if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Back)) {
                    CurrentContainer.Name = CurrentContainer.Name[0..^1];
                    updateChest = true;
                }
            }

            if (updateChest) {
                NetMsg updateContainer = new("updateChest", CurrentContainer.ToByteArray());
                updateContainer.SetFullPos(ContainerPosition, GameLoop.World.Player.MapPos);
                GameLoop.SendMessageIfNeeded(updateContainer, false, false);
            }
        }

        public override void UI_Clicks(string ID) {
            string[] split = ID.Split(",");
            int slot = Int32.Parse(split[1]);
            bool updateChest = false;

            if (split[0] == "MoveAllContainer") {
                int qty = 1;
                if (CurrentContainer.Items[slot].IsStackable)
                    qty = CurrentContainer.Items[slot].ItemQuantity;
                Item moved = CurrentContainer.Remove(slot, qty);
                if (moved != null) {
                    CommandManager.AddItemToInv(GameLoop.World.Player, moved);
                    updateChest = true;
                }
            } else if (split[0] == "MoveHalfContainer") {
                int qty = 1;
                if (CurrentContainer.Items[slot].IsStackable && CurrentContainer.Items[slot].ItemQuantity > 1)
                    qty = CurrentContainer.Items[slot].ItemQuantity / 2;

                Item moved = CurrentContainer.Remove(slot, qty);

                if (moved != null) {
                    CommandManager.AddItemToInv(GameLoop.World.Player, moved);
                    updateChest = true;
                }
            } else if (split[0] == "Move1Container") {
                Item moved = CurrentContainer.Remove(slot, 1);

                if (moved != null) {
                    CommandManager.AddItemToInv(GameLoop.World.Player, moved);
                    updateChest = true;
                }
            } else if (split[0] == "MoveAllInventory") {
                if (CurrentContainer.Add(GameLoop.World.Player.Inventory[slot], GameLoop.World.Player.Inventory[slot].ItemQuantity)) {
                    GameLoop.World.Player.Inventory[slot].ItemQuantity = 0;
                    updateChest = true;
                }
            } else if (split[0] == "MoveHalfInventory") {
                int half = 1;
                if (GameLoop.World.Player.Inventory[slot].ItemQuantity > 1)
                    half = GameLoop.World.Player.Inventory[slot].ItemQuantity / 2;

                if (CurrentContainer.Add(GameLoop.World.Player.Inventory[slot], half)) {
                    GameLoop.World.Player.Inventory[slot].ItemQuantity -= half;
                    updateChest = true;
                }
            } else if (split[0] == "Move1Inventory") {
                if (CurrentContainer.Add(GameLoop.World.Player.Inventory[slot], 1)) {
                    GameLoop.World.Player.Inventory[slot].ItemQuantity -= 1;
                    updateChest = true;
                }
            } else if (split[0] == "Close") {
                Toggle();
            }

            if (GameLoop.World.Player.Inventory[slot].ItemQuantity <= 0) {
                GameLoop.World.Player.Inventory[slot] = Item.Copy("lh:(EMPTY)");
            }

            if (updateChest) {
                NetMsg updateContainer = new("updateChest", CurrentContainer.ToByteArray());
                updateContainer.SetFullPos(ContainerPosition, GameLoop.World.Player.MapPos);
                GameLoop.SendMessageIfNeeded(updateContainer, false, false);
            }
        }

        public void SetupContainer(Container con, Point pos) {
            CurrentContainer = con; 
            ContainerPosition = pos;
            
            Toggle();
        } 
    }
}
