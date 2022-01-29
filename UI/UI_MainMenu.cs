using SadRogue.Primitives;
using SadConsole;
using SadConsole.UI;
using SadConsole.Input;
using System.IO;
using System;
using System.Collections.Generic;
using LofiHollow.Managers;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_MainMenu {
        public SadConsole.Console MainMenuConsole;
        public SadConsole.UI.ControlsConsole NameConsole;
        public SadConsole.UI.Controls.TextBox NameBox;
        public Window MainMenuWindow;
        public SadRex.Image MenuImage;
        
        public SadConsole.Console MenuConsole;
         
        public string LobbyCode = "";
        public string[] Names;
        public string joinError = "";

        

        public UI_MainMenu() {
            MainMenuWindow = new(GameLoop.GameWidth, GameLoop.GameHeight);
            MainMenuWindow.CanDrag = false;
            MainMenuWindow.Position = new(0, 0);

            int menuConWidth = GameLoop.GameWidth;

            Stream menuXP = new FileStream("./data/trees.xp", FileMode.Open);
            MenuImage = SadRex.Image.Load(menuXP);


            ColoredGlyph[] cells = new ColoredGlyph[100 * 60];

            for (int i = 0; i < MenuImage.Layers[0].Cells.Count; i++) {
                var cell = MenuImage.Layers[0].Cells[i];
                Color convertedFG = new(cell.Foreground.R, cell.Foreground.G, cell.Foreground.B);
                Color convertedBG = new(cell.Background.R, cell.Background.G, cell.Background.B);

                cells[i] = new ColoredGlyph(Color.Transparent, convertedFG, MenuImage.Layers[0].Cells[i].Character);
            }

            MainMenuConsole = new SadConsole.Console(GameLoop.GameWidth, GameLoop.GameHeight, cells);
            MainMenuWindow.Children.Add(MainMenuConsole);

            MainMenuConsole.Position = new Point(0, 0);
            MainMenuWindow.Title = "".Align(HorizontalAlignment.Center, menuConWidth, (char)196);

            GameLoop.UIManager.Children.Add(MainMenuWindow);

            MainMenuWindow.Show();
            MainMenuWindow.IsVisible = true;

            MenuConsole = new(20, 20);
            MenuConsole.Position = new(40, 20);
            MainMenuWindow.Children.Add(MenuConsole);

            NameConsole = new ControlsConsole(13, 1);
            NameBox = new SadConsole.UI.Controls.TextBox(13);
            NameConsole.Controls.Add(NameBox);
            NameConsole.Position = new Point(1, 13);

            MenuConsole.Children.Add(NameConsole);
            NameConsole.IsVisible = false;
            NameBox.TextChanged += NameChanged; 
        }

        private void NameChanged(object sender, EventArgs e) {
            GameLoop.World.Player.Name = NameBox.Text;
        }
         

        public void RemakeMenu() {
            MenuConsole.Clear();

            for (int i = 0; i < MenuImage.Layers[0].Cells.Count; i++) {
                var cell = MenuImage.Layers[0].Cells[i];
                Color convertedFG = new(cell.Foreground.R, cell.Foreground.G, cell.Foreground.B);

                MainMenuConsole.SetCellAppearance(i % GameLoop.GameWidth, i / GameLoop.GameWidth, new ColoredGlyph(Color.Transparent, convertedFG, MenuImage.Layers[0].Cells[i].Character));
            }
        }


        public void RenderMainMenu() {
            MenuConsole.Clear();
            Point mousePos = new MouseScreenObjectState(MenuConsole, GameHost.Instance.Mouse).CellPosition;

            if (GameLoop.UIManager.selectedMenu == "MainMenu") {
                int leftEdge = 32;
                int topEdge = 10;

                // L
                for (int i = 0; i < 5; i++) {
                    MainMenuConsole.SetDecorator(leftEdge + 1, topEdge + i, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                }
                // MainMenuConsole.SetDecorator(28, 7, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None)); 



                // O
                MainMenuConsole.SetDecorator(leftEdge + 3, topEdge + 2, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 3, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 5, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 3, topEdge + 4, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                // F
                MainMenuConsole.SetDecorator(leftEdge + 8, topEdge + 1, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 7, topEdge + 2, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 7, topEdge + 3, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 7, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                // I
                MainMenuConsole.SetDecorator(leftEdge + 10, topEdge + 1, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 10, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 10, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));



                // H
                MainMenuConsole.SetDecorator(leftEdge + 14, topEdge, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 14, topEdge + 1, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 14, topEdge + 2, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 14, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 14, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 16, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 16, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                // O
                MainMenuConsole.SetDecorator(leftEdge + 18, topEdge + 2, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 18, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 20, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 18, topEdge + 4, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                // LL
                for (int i = 0; i < 5; i++) {
                    MainMenuConsole.SetDecorator(leftEdge + 22, topEdge + i, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                    MainMenuConsole.SetDecorator(leftEdge + 24, topEdge + i, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                }

                // O
                MainMenuConsole.SetDecorator(leftEdge + 26, topEdge + 2, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 26, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 28, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 26, topEdge + 4, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                // W
                MainMenuConsole.SetDecorator(leftEdge + 30, topEdge + 2, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 34, topEdge + 2, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 30, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 32, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 34, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 30, topEdge + 4, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MainMenuConsole.SetDecorator(leftEdge + 33, topEdge + 4, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));



                MenuConsole.DrawBox(new Rectangle(0, 0, 20, 20), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.Black, Color.Black)));
                MenuConsole.Print(6, 2, Helper.HoverColoredString("New Game", mousePos.Y == 2));
                MenuConsole.Print(5, 3, Helper.HoverColoredString("Load Game", mousePos.Y == 3));
            } else if (GameLoop.UIManager.selectedMenu == "CharCreation") {
                MenuConsole.DrawBox(new Rectangle(0, 0, 50, 50), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.Black, Color.Black)));

                int CreateX = 1;
                int CreateY = 1;

                MenuConsole.DrawLine(new Point(CreateX + 13, CreateY), new Point(CreateX + 13, CreateY + 47), (char)179, Color.White, Color.Black);
                // Attribute window

                string forR = GameLoop.World.Player.ForegroundR.ToString();
                string forG = GameLoop.World.Player.ForegroundG.ToString();
                string forB = GameLoop.World.Player.ForegroundB.ToString();

                MenuConsole.Print(CreateX + 1, CreateY + 1, new ColoredString("R: ", Color.White, Color.Black));
                MenuConsole.Print(CreateX + 6, CreateY + 1, Helper.HoverColoredString("-", mousePos == new Point(CreateX + 6, CreateY + 1)));
                MenuConsole.Print(CreateX + 8, CreateY + 1, new ColoredString(forR, Color.White, Color.Black));
                MenuConsole.Print(CreateX + 12, CreateY + 1, Helper.HoverColoredString("+", mousePos == new Point(CreateX + 11, CreateY + 1)));

                MenuConsole.Print(CreateX + 1, CreateY + 2, new ColoredString("G: ", Color.White, Color.Black));
                MenuConsole.Print(CreateX + 6, CreateY + 2, Helper.HoverColoredString("-", mousePos == new Point(CreateX + 6, CreateY + 2)));
                MenuConsole.Print(CreateX + 8, CreateY + 2, new ColoredString(forG, Color.White, Color.Black));
                MenuConsole.Print(CreateX + 12, CreateY + 2, Helper.HoverColoredString("+", mousePos == new Point(CreateX + 11, CreateY + 2)));

                MenuConsole.Print(CreateX + 1, CreateY + 3, new ColoredString("B: ", Color.White, Color.Black));
                MenuConsole.Print(CreateX + 6, CreateY + 3, Helper.HoverColoredString("-", mousePos == new Point(CreateX + 6, CreateY + 3)));
                MenuConsole.Print(CreateX + 8, CreateY + 3, new ColoredString(forB, Color.White, Color.Black));
                MenuConsole.Print(CreateX + 12, CreateY + 3, Helper.HoverColoredString("+", mousePos == new Point(CreateX + 11, CreateY + 3)));

                GameLoop.World.Player.UpdateAppearance();
                MenuConsole.Print(CreateX + 1, CreateY + 5, new ColoredString("Current: ", Color.White, Color.Black) + GameLoop.World.Player.GetAppearance());
                MenuConsole.SetBackground(CreateX + 10, CreateY + 5, Color.Black);


                MenuConsole.Print(CreateX + 15, CreateY, new ColoredString("Deaths", Color.White, Color.Black));

                MenuConsole.Print(CreateX + 15, CreateY + 1, new ColoredString(((char)236).ToString(), GameLoop.World.Player.LivesRemaining == -1 ? Color.Yellow : Color.White, Color.Black));
                MenuConsole.Print(CreateX + 17, CreateY + 1, new ColoredString("3", GameLoop.World.Player.LivesRemaining == 3 ? Color.Yellow : Color.White, Color.Black));
                MenuConsole.Print(CreateX + 19, CreateY + 1, new ColoredString("1".ToString(), GameLoop.World.Player.LivesRemaining == 1 ? Color.Yellow : Color.White, Color.Black));

                MenuConsole.Print(CreateX + 25, CreateY, new ColoredString("Item Drops On Death", Color.White, Color.Black));
                MenuConsole.Print(CreateX + 25, CreateY + 1, new ColoredString("Nothing", GameLoop.World.Player.DropsOnDeath == -1 ? Color.Yellow : Color.White, Color.Black));
                MenuConsole.Print(CreateX + 25, CreateY + 2, new ColoredString("Only Gold", GameLoop.World.Player.DropsOnDeath == 0 ? Color.Yellow : Color.White, Color.Black));
                MenuConsole.Print(CreateX + 25, CreateY + 3, new ColoredString("Gold and Items".ToString(), GameLoop.World.Player.DropsOnDeath == 1 ? Color.Yellow : Color.White, Color.Black));


                MenuConsole.Print(CreateX + 1, CreateY + 11, new ColoredString("Name:", Color.White, Color.Black));


                MenuConsole.Print(2, MenuConsole.Height - 2, Helper.HoverColoredString("DONE", (mousePos.Y == MenuConsole.Height - 2 && mousePos.X <= 6 && mousePos.X >= 2)));
            } else if (GameLoop.UIManager.selectedMenu == "LoadFile") {
                int fileSize = 5;
                if (Names != null && Names.Length > 0) {
                    fileSize = Names.Length + 2;
                    if (fileSize < 20)
                        fileSize = 20;

                    MenuConsole.DrawBox(new Rectangle(0, 0, 20, fileSize), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.Black, Color.Black)));
                    MenuConsole.Print(1, 1, new ColoredString("Select a Save File", Color.White, Color.Black));
                    MenuConsole.DrawLine(new Point(1, 2), new Point(18, 2), (char)196, Color.White, Color.Black);

                    for (int i = 0; i < Names.Length; i++) {
                        MenuConsole.Print(1, 3 + i, Helper.HoverColoredString(Names[i].Align(HorizontalAlignment.Center, 18), mousePos.Y == 3 + i));
                    }
                }

                MenuConsole.Print(1, 1 + fileSize - 3, Helper.HoverColoredString("[BACK]".Align(HorizontalAlignment.Center, 18), mousePos.Y == (1 + fileSize - 3)));
            } else if (GameLoop.UIManager.selectedMenu == "ConnectOrHost") {
                MenuConsole.DrawBox(new Rectangle(0, 0, 20, 10), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.Black, Color.Black)));
                MenuConsole.Print(1, 1, Helper.HoverColoredString("Singleplayer", mousePos.Y == 1));
                MenuConsole.Print(1, 2, Helper.HoverColoredString("Host and Play", mousePos.Y == 2));

                string buff = LobbyCode;

                for (int i = buff.Length; i < 6; i++) {
                    buff += "-";
                }

                MenuConsole.Print(1, 4, new ColoredString(buff, Color.White, Color.Black));
                MenuConsole.Print(8, 4, Helper.HoverColoredString("[JOIN]", mousePos.Y == 4));


                MenuConsole.Print(1, 7, new ColoredString(joinError, Color.White, Color.Black));
            }
        }

        public void CaptureMainMenuClicks() {
            Point mousePos = new MouseScreenObjectState(MenuConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameLoop.UIManager.selectedMenu == "ConnectOrHost") {
                if (LobbyCode.Length < 6) {
                    foreach (var key in GameHost.Instance.Keyboard.KeysReleased) {
                        if (key.Character >= 'A' && key.Character <= 'z') {
                            LobbyCode += key.Character;
                        }
                    }

                    if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Space)) {
                        LobbyCode += " ";
                    } 
                }

                if (LobbyCode.Length > 0) {
                    if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Back)) {
                        LobbyCode = LobbyCode[0..^1];
                    }
                }
            }

            if (GameHost.Instance.Mouse.LeftButtonDown) {
                if (GameLoop.UIManager.selectedMenu == "CharCreation") {
                    int CreateX = 1;
                    int CreateY = 1;
                    if (mousePos == new Point(CreateX + 6, CreateY + 1))
                        if (GameLoop.World.Player.ForegroundR > 0)
                            GameLoop.World.Player.ForegroundR--;

                    if (mousePos == new Point(CreateX + 12, CreateY + 1))
                        if (GameLoop.World.Player.ForegroundR < 255)
                            GameLoop.World.Player.ForegroundR++;

                    if (mousePos == new Point(CreateX + 6, CreateY + 2))
                        if (GameLoop.World.Player.ForegroundG > 0)
                            GameLoop.World.Player.ForegroundG--;

                    if (mousePos == new Point(CreateX + 12, CreateY + 2))
                        if (GameLoop.World.Player.ForegroundG < 255)
                            GameLoop.World.Player.ForegroundG++;

                    if (mousePos == new Point(CreateX + 6, CreateY + 3))
                        if (GameLoop.World.Player.ForegroundB > 0)
                            GameLoop.World.Player.ForegroundB--;

                    if (mousePos == new Point(CreateX + 12, CreateY + 3))
                        if (GameLoop.World.Player.ForegroundB < 255)
                            GameLoop.World.Player.ForegroundB++;
                }
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (GameLoop.UIManager.selectedMenu == "MainMenu") {
                    if (mousePos.Y == 2) {
                        GameLoop.UIManager.selectedMenu = "CharCreation";
                        MenuConsole = new SadConsole.Console(50, 50);
                        MainMenuWindow.Children.Add(MenuConsole);
                        MenuConsole.Children.Add(NameConsole);
                        MenuConsole.Position = new Point(25, 5);
                        NameConsole.IsVisible = true;
                    } else if (mousePos.Y == 3) {
                        GameLoop.UIManager.selectedMenu = "LoadFile";
                        if (Directory.Exists("./saves/")) {
                            Names = Directory.GetDirectories("./saves/");

                            for (int i = 0; i < Names.Length; i++) {
                                string[] split = Names[i].Split("/");
                                Names[i] = split[^1];
                            }
                        }
                    }
                }
                else if (GameLoop.UIManager.selectedMenu == "CharCreation") {
                    if (mousePos.Y == MenuConsole.Height - 2 && mousePos.X <= 6 && mousePos.X >= 2 && NameBox.EditingText != "") {
                        GameLoop.UIManager.selectedMenu = "ConnectOrHost";

                        RemakeMenu();
                        MenuConsole = new SadConsole.Console(20, 20);
                        MainMenuWindow.Children.Add(MenuConsole); 
                        MenuConsole.Position = new Point(40, 20);
                        NameConsole.IsVisible = false; 
                       
                        

                        if (GameLoop.World.Player.Name != NameBox.EditingText)
                            GameLoop.World.Player.Name = NameBox.EditingText;

                        GameLoop.World.Player.SizeMod = 0;
                         
                        GameLoop.World.FreshStart();
                        GameLoop.UIManager.Map.UpdateVision();
                    }

                    int CreateX = 1;
                    int CreateY = 1;

                    if (mousePos == new Point(CreateX + 15, CreateY + 1))
                        GameLoop.World.Player.LivesRemaining = -1;
                    if (mousePos == new Point(CreateX + 17, CreateY + 1))
                        GameLoop.World.Player.LivesRemaining = 3;
                    if (mousePos == new Point(CreateX + 19, CreateY + 1))
                        GameLoop.World.Player.LivesRemaining = 1;

                    if (mousePos.X >= CreateX + 25 && mousePos.Y == CreateY + 1)
                        GameLoop.World.Player.DropsOnDeath = -1;
                    if (mousePos.X >= CreateX + 25 && mousePos.Y == CreateY + 2)
                        GameLoop.World.Player.DropsOnDeath = 0;
                    if (mousePos.X >= CreateX + 25 && mousePos.Y == CreateY + 3)
                        GameLoop.World.Player.DropsOnDeath = 1; 
                } else if (GameLoop.UIManager.selectedMenu == "LoadFile") {
                    int fileSize = 5;
                    if (Names != null && Names.Length > 0) {
                        fileSize = Names.Length + 2;
                        if (fileSize < 20)
                            fileSize = 20;
                    }

                    if (mousePos.Y == 1 + fileSize - 3) {
                        RemakeMenu();
                        GameLoop.UIManager.selectedMenu = "MainMenu";
                    } else {
                        int fileSlot = mousePos.Y - 3;
                        if (Names != null && Names.Length > fileSlot && fileSlot >= 0) {
                            GameLoop.World.LoadPlayer(Names[fileSlot]);
                            GameLoop.UIManager.selectedMenu = "ConnectOrHost";
                            GameLoop.UIManager.Map.UpdateVision(); 
                        }
                    }
                } 
                else if (GameLoop.UIManager.selectedMenu == "ConnectOrHost") {
                    if (mousePos.Y == 1) {
                        MainMenuWindow.IsVisible = false;
                        GameLoop.UIManager.Map.MapWindow.IsVisible = true;
                        GameLoop.UIManager.Map.MessageLog.IsVisible = true;
                        GameLoop.UIManager.Sidebar.BattleLog.IsVisible = true;
                        GameLoop.UIManager.Sidebar.SidebarWindow.IsVisible = true;
                        GameLoop.UIManager.selectedMenu = "None";

                        // Singleplayer
                    } else if (mousePos.Y == 2) {
                        // Host Immediately
                        GameLoop.NetworkManager = new NetworkManager(true); 
                        GameLoop.NetworkManager.CreateLobby();
                    } else if (mousePos.Y == 4 && mousePos.X >= 8 && mousePos.X <= 13) {
                        // Join game

                        if (LobbyCode.Length == 6) {
                            GameLoop.NetworkManager = new NetworkManager(false);
                            GameLoop.NetworkManager.SearchLobbiesAndJoin(LobbyCode);
                        } else {
                            joinError = "Enter lobby code first";
                        }
                    }
                }
            }
        } 
    }
}
