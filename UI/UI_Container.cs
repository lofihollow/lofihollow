using LofiHollow.Entities;
using LofiHollow.Managers;
using Newtonsoft.Json;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using Key = SadConsole.Input.Keys;

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

                ContainerConsole.Print(2, i + 2, Helper.HoverColoredString(qty + CurrentContainer.Items[i].Name, mousePos.X < 36 && mousePos.Y == i+2));

                ContainerConsole.Print(35, i + 2, Helper.HoverColoredString(((char)12).ToString(), mousePos == new Point(35, i + 2)));
                ContainerConsole.Print(33, i + 2, Helper.HoverColoredString(((char)25).ToString(), mousePos == new Point(33, i + 2)));
                ContainerConsole.Print(31, i + 2, Helper.HoverColoredString(((char)49).ToString(), mousePos == new Point(31, i + 2)));
                ContainerConsole.Print(30, i + 2, "|");
            }

            for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                string qty = "";
                if (GameLoop.World.Player.Inventory[i].ItemQuantity > 1)
                    qty = GameLoop.World.Player.Inventory[i].ItemQuantity + "x ";
                ContainerConsole.Print(37, i + 2, Helper.HoverColoredString(((char)11).ToString(), mousePos == new Point(37, i + 2)));
                ContainerConsole.Print(39, i + 2, Helper.HoverColoredString(((char)25).ToString(), mousePos == new Point(39, i + 2)));
                ContainerConsole.Print(41, i + 2, Helper.HoverColoredString(((char)49).ToString(), mousePos == new Point(41, i + 2)));
                ContainerConsole.Print(42, i + 2, "|");
                ContainerConsole.Print(43, i + 2, GameLoop.World.Player.Inventory[i].AsColoredGlyph());

                if (GameLoop.World.Player.Inventory[i].Dec != null)
                    ContainerConsole.SetDecorator(43, i + 2, 1, GameLoop.World.Player.Inventory[i].GetDecorator());
                else
                    ContainerConsole.ClearDecorators(43, i + 2, 1);

                ContainerConsole.Print(45, i + 2, Helper.HoverColoredString(qty + GameLoop.World.Player.Inventory[i].Name, mousePos.X > 36 && mousePos.Y == i+2));
            }

            ContainerConsole.Print(72, 0, Helper.HoverColoredString("X", mousePos == new Point(72, 0)));
        }

        public void ContainerInput() {
            Point mousePos = new MouseScreenObjectState(ContainerConsole, GameHost.Instance.Mouse).CellPosition;
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

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos == new Point(72, 0)) {
                    ToggleContainer();
                    return;
                }

                if (mousePos.X > 36) {
                    if (mousePos.Y - 2 >= 0 && mousePos.Y - 2 <= GameLoop.World.Player.Inventory.Length) {
                        int slot = mousePos.Y - 2;
                        if (GameLoop.World.Player.Inventory[slot].Name != "(EMPTY)") {
                            if (mousePos == new Point(37, slot + 2)) { // Move all from inventory to container
                                if (CurrentContainer.Add(GameLoop.World.Player.Inventory[slot], GameLoop.World.Player.Inventory[slot].ItemQuantity)) {
                                    GameLoop.World.Player.Inventory[slot].ItemQuantity = 0;
                                    updateChest = true;
                                }
                            }

                            if (mousePos == new Point(39, slot + 2)) { // Move half from inventory to container
                                int half = 1;
                                if (GameLoop.World.Player.Inventory[slot].ItemQuantity > 1)
                                    half = GameLoop.World.Player.Inventory[slot].ItemQuantity / 2;

                                if (CurrentContainer.Add(GameLoop.World.Player.Inventory[slot], half)) {
                                    GameLoop.World.Player.Inventory[slot].ItemQuantity -= half;
                                    updateChest = true;
                                }
                            }

                            if (mousePos == new Point(41, slot + 2)) { // Move one from inventory to container 
                                if (CurrentContainer.Add(GameLoop.World.Player.Inventory[slot], 1)) {
                                    GameLoop.World.Player.Inventory[slot].ItemQuantity -= 1;
                                    updateChest = true;
                                }
                            }

                            if (GameLoop.World.Player.Inventory[slot].ItemQuantity <= 0) {
                                GameLoop.World.Player.Inventory[slot] = new("lh:(EMPTY)");
                            }
                        }
                    }
                } else {
                    if (mousePos.Y - 2 >= 0 && mousePos.Y - 2 <= CurrentContainer.Items.Count) {
                        int slot = mousePos.Y - 2;
                        if (mousePos == new Point(35, slot + 2)) { // Move all from inventory to container
                            int qty = 1;
                            if (CurrentContainer.Items[slot].IsStackable)
                                qty = CurrentContainer.Items[slot].ItemQuantity;
                            Item moved = CurrentContainer.Remove(slot, qty);
                            if (moved != null) {
                                CommandManager.AddItemToInv(GameLoop.World.Player, moved);
                                updateChest = true;
                            }
                        }

                        if (mousePos == new Point(33, slot + 2)) { // Move half from inventory to container
                            int qty = 1;
                            if (CurrentContainer.Items[slot].IsStackable && CurrentContainer.Items[slot].ItemQuantity > 1)
                                qty = CurrentContainer.Items[slot].ItemQuantity / 2;

                            Item moved = CurrentContainer.Remove(slot, qty);

                            if (moved != null) {
                                CommandManager.AddItemToInv(GameLoop.World.Player, moved);
                                updateChest = true;
                            }
                        }

                        if (mousePos == new Point(31, slot + 2)) { // Move one from inventory to container 
                            Item moved = CurrentContainer.Remove(slot, 1);

                            if (moved != null) {
                                CommandManager.AddItemToInv(GameLoop.World.Player, moved);
                                updateChest = true;
                            }
                        }
                    }
                }


                
            }

            if (updateChest) {
                string json =  JsonConvert.SerializeObject(CurrentContainer, Formatting.Indented);
                GameLoop.SendMessageIfNeeded(new string[] { "updateChest", ContainerPosition.X.ToString(), ContainerPosition.Y.ToString(), GameLoop.World.Player.MapPos.ToString(), json }, false, false);
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
