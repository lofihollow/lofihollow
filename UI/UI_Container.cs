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
    public class UI_Container {
        public SadConsole.Console ContainerConsole;
        public Window ContainerWindow;
        public Container CurrentContainer; 
        public Point ContainerPosition;

        public UI_Container(int width, int height, string title) {
            ContainerWindow = new(width, height);
            ContainerWindow.CanDrag = false;
            ContainerWindow.Position = new Point((GameLoop.GameWidth - width) / 2, (GameLoop.GameHeight - height) / 2);

            int containerConWidth = width - 2;
            int containerConHeight = height - 2;

            ContainerConsole = new(containerConWidth, containerConHeight);
            ContainerConsole.Position = new(1, 1);
            ContainerWindow.Title = title.Align(HorizontalAlignment.Center, containerConWidth, (char)196);


            ContainerWindow.Children.Add(ContainerConsole);
            GameLoop.UIManager.Children.Add(ContainerWindow);

            ContainerWindow.Show();
            ContainerWindow.IsVisible = false;
        }


        public void RenderContainer() {
            Point mousePos = new MouseScreenObjectState(ContainerConsole, GameHost.Instance.Mouse).CellPosition;
            ContainerConsole.Clear();

            
            ContainerConsole.Print(0, 0, CurrentContainer.Name.Align(HorizontalAlignment.Center, 36));
            ContainerConsole.Print(0, 1, "".Align(HorizontalAlignment.Center, 36, '-'));
            ContainerConsole.DrawLine(new Point(36, 0), new Point(36, 29), '|', Color.White, Color.Black);
            ContainerConsole.Print(37, 0, "Your Backpack".Align(HorizontalAlignment.Center, 36));
            ContainerConsole.Print(37, 1, "".Align(HorizontalAlignment.Center, 36, '-')); 


            ContainerConsole.DrawLine(new Point(42, 2), new Point(42, 29), '|', Color.White, Color.Black);
            ContainerConsole.DrawLine(new Point(30, 2), new Point(30, 29), '|', Color.White, Color.Black);

            for (int i = 0; i < CurrentContainer.Items.Count; i++) {
                string qty = "";
                if (CurrentContainer.Items[i].ItemQuantity > 1)
                    qty = CurrentContainer.Items[i].ItemQuantity + "x ";
                ContainerConsole.Print(0, i + 2, CurrentContainer.Items[i].AsColoredGlyph());

                if (CurrentContainer.Items[i].Dec != null)
                    ContainerConsole.SetDecorator(0, i + 2, 1, CurrentContainer.Items[i].GetDecorator());
                else
                    ContainerConsole.ClearDecorators(0, i + 2, 1);

                if (CurrentContainer.Items[i].ItemCat == "Soul") {
                    string name = CurrentContainer.Items[i].Name;
                    if (CurrentContainer.Items[i].SoulPhoto != null) {
                        name += " (" + CurrentContainer.Items[i].SoulPhoto.Name() + ")";
                    }

                    ContainerConsole.Print(2, i + 2, Helper.HoverColoredString(qty + name, mousePos.X < 36 && mousePos.Y == i + 2));
                }
                else {
                    ContainerConsole.Print(2, i + 2, Helper.HoverColoredString(qty + CurrentContainer.Items[i].Name, mousePos.X < 36 && mousePos.Y == i + 2));
                }

                ContainerConsole.PrintClickable(35, i + 2, 12.AsString(), ContainerClicks, "MoveAllContainer," + (i));
                ContainerConsole.PrintClickable(33, i + 2, 25.AsString(), ContainerClicks, "MoveHalfContainer," + (i));
                ContainerConsole.PrintClickable(31, i + 2, 49.AsString(), ContainerClicks, "Move1Container," + (i)); 
                ContainerConsole.Print(30, i + 2, "|");
            }

            for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                string qty = "";
                if (GameLoop.World.Player.Inventory[i].ItemQuantity > 1)
                    qty = GameLoop.World.Player.Inventory[i].ItemQuantity + "x ";
                ContainerConsole.PrintClickable(37, i + 2, 11.AsString(), ContainerClicks, "MoveAllInventory," + (i));
                ContainerConsole.PrintClickable(39, i + 2, 25.AsString(), ContainerClicks, "MoveHalfInventory," + (i));
                ContainerConsole.PrintClickable(41, i + 2, 49.AsString(), ContainerClicks, "Move1Inventory," + (i)); 
                ContainerConsole.Print(42, i + 2, "|");
                ContainerConsole.Print(43, i + 2, GameLoop.World.Player.Inventory[i].AsColoredGlyph());

                if (GameLoop.World.Player.Inventory[i].Dec != null)
                    ContainerConsole.SetDecorator(43, i + 2, 1, GameLoop.World.Player.Inventory[i].GetDecorator());
                else
                    ContainerConsole.ClearDecorators(43, i + 2, 1);
                if (GameLoop.World.Player.Inventory[i].ItemCat == "Soul") {
                    string name = GameLoop.World.Player.Inventory[i].Name;
                    if (GameLoop.World.Player.Inventory[i].SoulPhoto != null) {
                        name += " (" + GameLoop.World.Player.Inventory[i].SoulPhoto.Name() + ")";
                    }

                    ContainerConsole.Print(45, i + 2, Helper.HoverColoredString(qty + name, mousePos.X > 36 && mousePos.Y == i + 2));
                }
                else {
                    ContainerConsole.Print(45, i + 2, Helper.HoverColoredString(qty + GameLoop.World.Player.Inventory[i].Name, mousePos.X > 36 && mousePos.Y == i + 2));
                }
            }

            ContainerConsole.PrintClickable(72, 0, "X", ContainerClicks, "Close,0");
        }

        public void ContainerInput() { 
            bool updateChest = false;

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                ToggleContainer();
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

        public void ContainerClicks(string ID) {
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
                ToggleContainer();
            }

            if (GameLoop.World.Player.Inventory[slot].ItemQuantity <= 0) {
                GameLoop.World.Player.Inventory[slot] = new("lh:(EMPTY)");
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
            
            ToggleContainer();
        }


        public void ToggleContainer() {
            if (ContainerWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                ContainerWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
            } else {
                GameLoop.UIManager.selectedMenu = "Container";
                ContainerWindow.IsVisible = true;
                ContainerWindow.IsFocused = true;
            }
        }
    }
}
