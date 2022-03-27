using SadRogue.Primitives;
using SadConsole;
using SadConsole.UI;
using SadConsole.Input;
using System.IO;
using System; 
using LofiHollow.Managers;
using Key = SadConsole.Input.Keys;
using Newtonsoft.Json; 
using LofiHollow.DataTypes;

namespace LofiHollow.UI {
    public class UI_MainMenu {
        public SadConsole.Console MenuBackdrop;
        public SadConsole.UI.ControlsConsole NameConsole;
        public SadConsole.UI.Controls.TextBox NameBox;
        public Window MainMenuWindow;
        public SadRex.Image MenuImage;

        public SadConsole.Console MenuConsole;

        public UI_ModMaker ModMaker;

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

            MenuBackdrop = new SadConsole.Console(GameLoop.GameWidth, GameLoop.GameHeight, cells);
            MainMenuWindow.Children.Add(MenuBackdrop);

            MenuBackdrop.Position = new Point(0, 0);
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

            ModMaker = new UI_ModMaker();
            MainMenuWindow.Children.Add(ModMaker.ModConsole);
        }

        private void NameChanged(object sender, EventArgs e) {
            GameLoop.World.Player.Name = NameBox.Text;
        }


        public void RemakeMenu() {
            MenuBackdrop.Clear();

            for (int i = 0; i < MenuImage.Layers[0].Cells.Count; i++) {
                var cell = MenuImage.Layers[0].Cells[i];
                Color convertedFG = new(cell.Foreground.R, cell.Foreground.G, cell.Foreground.B);

                MenuBackdrop.SetCellAppearance(i % GameLoop.GameWidth, i / GameLoop.GameWidth, new ColoredGlyph(Color.Transparent, convertedFG, MenuImage.Layers[0].Cells[i].Character));
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
                    MenuBackdrop.SetDecorator(leftEdge + 1, topEdge + i, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                }
                // MainMenuConsole.SetDecorator(28, 7, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None)); 



                // O
                MenuBackdrop.SetDecorator(leftEdge + 3, topEdge + 2, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 3, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 5, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 3, topEdge + 4, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                // F
                MenuBackdrop.SetDecorator(leftEdge + 8, topEdge + 1, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 7, topEdge + 2, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 7, topEdge + 3, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 7, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                // I
                MenuBackdrop.SetDecorator(leftEdge + 10, topEdge + 1, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 10, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 10, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));



                // H
                MenuBackdrop.SetDecorator(leftEdge + 14, topEdge, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 14, topEdge + 1, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 14, topEdge + 2, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 14, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 14, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 16, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 16, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                // O
                MenuBackdrop.SetDecorator(leftEdge + 18, topEdge + 2, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 18, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 20, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 18, topEdge + 4, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                // LL
                for (int i = 0; i < 5; i++) {
                    MenuBackdrop.SetDecorator(leftEdge + 22, topEdge + i, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                    MenuBackdrop.SetDecorator(leftEdge + 24, topEdge + i, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                }

                // O
                MenuBackdrop.SetDecorator(leftEdge + 26, topEdge + 2, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 26, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 28, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 26, topEdge + 4, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                // W
                MenuBackdrop.SetDecorator(leftEdge + 30, topEdge + 2, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 34, topEdge + 2, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 30, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 32, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 34, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 30, topEdge + 4, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 33, topEdge + 4, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

                MenuConsole.DrawBox(new Rectangle(0, 0, 20, 20), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.Black, Color.Black)));
                MenuConsole.PrintClickable(5, 2, " New Game", MenuClicks, "GoToCharCreate");
                MenuConsole.PrintClickable(5, 4, "Load Game", MenuClicks, "GoToLoad");
                MenuConsole.PrintClickable(5, 6, "  Mods", MenuClicks, "GoToMods");
                MenuConsole.PrintClickable(5, 18, "  Quit ", MenuClicks, "ExitGame");
            } 
            else if (GameLoop.UIManager.selectedMenu == "CharCreation") {
                MenuConsole.DrawBox(new Rectangle(0, 0, 50, 50), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.Black, Color.Black)));

                int CreateX = 1;
                int CreateY = 1;

                MenuConsole.DrawLine(new Point(CreateX + 13, CreateY), new Point(CreateX + 13, CreateY + 47), (char)179, Color.White, Color.Black);
                // Attribute window

                string forR = GameLoop.World.Player.ForegroundR.ToString();
                string forG = GameLoop.World.Player.ForegroundG.ToString();
                string forB = GameLoop.World.Player.ForegroundB.ToString();

                MenuConsole.Print(CreateX + 1, CreateY + 1, new ColoredString("R: ", Color.White, Color.Black));
                MenuConsole.PrintClickable(CreateX + 6, CreateY + 1, "-", MenuClicks, "ForeRdown");
                MenuConsole.Print(CreateX + 8, CreateY + 1, new ColoredString(forR, Color.White, Color.Black));
                MenuConsole.PrintClickable(CreateX + 12, CreateY + 1, "+", MenuClicks, "ForeRup");

                MenuConsole.Print(CreateX + 1, CreateY + 2, new ColoredString("G: ", Color.White, Color.Black));
                MenuConsole.PrintClickable(CreateX + 6, CreateY + 2, "-", MenuClicks, "ForeGdown");
                MenuConsole.Print(CreateX + 8, CreateY + 2, new ColoredString(forG, Color.White, Color.Black));
                MenuConsole.PrintClickable(CreateX + 12, CreateY + 2, "+", MenuClicks, "ForeGup");

                MenuConsole.Print(CreateX + 1, CreateY + 3, new ColoredString("B: ", Color.White, Color.Black));
                MenuConsole.PrintClickable(CreateX + 6, CreateY + 3, "-", MenuClicks, "ForeBdown");
                MenuConsole.Print(CreateX + 8, CreateY + 3, new ColoredString(forB, Color.White, Color.Black));
                MenuConsole.PrintClickable(CreateX + 12, CreateY + 3, "+", MenuClicks, "ForeBup");

                GameLoop.World.Player.UpdateAppearance();
                MenuConsole.Print(CreateX + 1, CreateY + 5, new ColoredString("Current: ", Color.White, Color.Black) + GameLoop.World.Player.GetAppearance());
                MenuConsole.SetBackground(CreateX + 10, CreateY + 5, Color.Black);


                MenuConsole.Print(CreateX + 15, CreateY, new ColoredString("Deaths", Color.White, Color.Black));

                MenuConsole.PrintClickable(CreateX + 15, CreateY + 1, new ColoredString(236.AsString(), GameLoop.World.Player.LivesRemaining == -1 ? Color.Yellow : Color.White, Color.Black), MenuClicks, "LivesInfinite");
                MenuConsole.PrintClickable(CreateX + 17, CreateY + 1, new ColoredString("3", GameLoop.World.Player.LivesRemaining == 3 ? Color.Yellow : Color.White, Color.Black), MenuClicks, "Lives3");
                MenuConsole.PrintClickable(CreateX + 19, CreateY + 1, new ColoredString("1".ToString(), GameLoop.World.Player.LivesRemaining == 1 ? Color.Yellow : Color.White, Color.Black), MenuClicks, "Lives1");

                MenuConsole.Print(CreateX + 25, CreateY, new ColoredString("Item Drops On Death", Color.White, Color.Black));
                MenuConsole.PrintClickable(CreateX + 25, CreateY + 1, new ColoredString("Nothing", GameLoop.World.Player.DropsOnDeath == -1 ? Color.Yellow : Color.White, Color.Black), MenuClicks, "DropsNone");
                MenuConsole.PrintClickable(CreateX + 25, CreateY + 2, new ColoredString("Only Gold", GameLoop.World.Player.DropsOnDeath == 0 ? Color.Yellow : Color.White, Color.Black), MenuClicks, "DropsGold");
                MenuConsole.PrintClickable(CreateX + 25, CreateY + 3, new ColoredString("Gold and Items".ToString(), GameLoop.World.Player.DropsOnDeath == 1 ? Color.Yellow : Color.White, Color.Black), MenuClicks, "DropsAll");

                string playerEle = GameLoop.World.Player.ElementalAlignment;

                MenuConsole.Print(CreateX + 15, CreateY + 5, new ColoredString("Elemental Affinity: ", Color.White, Color.Black));

                MenuConsole.Print(CreateX + 15, CreateY + 7, new ColoredString("Type  | Weak to | Resists", Color.DarkSlateBlue, Color.Black));
                MenuConsole.Print(CreateX + 15, CreateY + 8, new ColoredString("------------------------", Color.DarkSlateBlue, Color.Black));
                MenuConsole.PrintClickable(CreateX + 15, CreateY + 9, new ColoredString("Wood", playerEle == "Wood" ? Color.Green : Color.DarkSlateGray, Color.Black), MenuClicks, "AffinityWood");
                MenuConsole.PrintClickable(CreateX + 15, CreateY + 10, new ColoredString("Fire", playerEle == "Fire" ? Color.Firebrick : Color.DarkSlateGray, Color.Black), MenuClicks, "AffinityFire");
                MenuConsole.PrintClickable(CreateX + 15, CreateY + 11, new ColoredString("Earth", playerEle == "Earth" ? new Color(111, 66, 33) : Color.DarkSlateGray, Color.Black), MenuClicks, "AffinityEarth");
                MenuConsole.PrintClickable(CreateX + 15, CreateY + 12, new ColoredString("Metal", playerEle == "Metal" ? Color.Gray : Color.DarkSlateGray, Color.Black), MenuClicks, "AffinityMetal");
                MenuConsole.PrintClickable(CreateX + 15, CreateY + 13, new ColoredString("Water", playerEle == "Water" ? Color.Cyan : Color.DarkSlateGray, Color.Black), MenuClicks, "AffinityWater");


                MenuConsole.Print(CreateX + 21, CreateY + 9, new ColoredString( "|  Metal  | Water", playerEle == "Wood" ? Color.Green : Color.DarkSlateGray, Color.Black)); 
                MenuConsole.Print(CreateX + 21, CreateY + 10, new ColoredString("|  Water  | Wood", playerEle == "Fire" ? Color.Firebrick : Color.DarkSlateGray, Color.Black)); 
                MenuConsole.Print(CreateX + 21, CreateY + 11, new ColoredString("|  Wood   | Fire", playerEle == "Earth" ? new Color(111, 66, 33) : Color.DarkSlateGray, Color.Black)); 
                MenuConsole.Print(CreateX + 21, CreateY + 12, new ColoredString("|  Fire   | Earth", playerEle == "Metal" ? Color.Gray : Color.DarkSlateGray, Color.Black));
                MenuConsole.Print(CreateX + 21, CreateY + 13, new ColoredString("|  Earth  | Metal", playerEle == "Water" ? Color.Cyan : Color.DarkSlateGray, Color.Black));

                MenuConsole.Print(CreateX + 1, CreateY + 11, new ColoredString("Name:", Color.White, Color.Black));

                MenuConsole.PrintClickable(8, MenuConsole.Height - 2, "BACK", MenuClicks, "SoftBack");
                MenuConsole.PrintClickable(2, MenuConsole.Height - 2, "DONE", MenuClicks, "FinishCharCreation");
            } 
            else if (GameLoop.UIManager.selectedMenu == "LoadFile") {
                int fileSize = 5;
                if (Names != null && Names.Length > 0) {
                    fileSize = Names.Length + 2;
                    if (fileSize < 20)
                        fileSize = 20;

                    MenuConsole.DrawBox(new Rectangle(0, 0, 20, fileSize), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.Black, Color.Black)));
                    MenuConsole.Print(1, 1, new ColoredString("Select a Save File", Color.White, Color.Black));
                    MenuConsole.DrawLine(new Point(1, 2), new Point(18, 2), (char)196, Color.White, Color.Black);

                    for (int i = 0; i < Names.Length; i++) {
                        MenuConsole.PrintClickable(1, 3 + i, Names[i].Align(HorizontalAlignment.Center, 18), LoadFileClick, Names[i]);
                    }
                }

                MenuConsole.PrintClickable(1, 1 + fileSize - 3, "[BACK]".Align(HorizontalAlignment.Center, 18), MenuClicks, "SoftBack");
             } 
            else if (GameLoop.UIManager.selectedMenu == "ConnectOrHost") {
                MenuConsole.DrawBox(new Rectangle(0, 0, 20, 10), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.Black, Color.Black)));
                MenuConsole.PrintClickable(1, 1, "Singleplayer", MenuClicks, "StartSingleplayer");
                MenuConsole.PrintClickable(1, 2, "Host and Play", MenuClicks, "StartMultiplayer");

                string buff = LobbyCode;

                for (int i = buff.Length; i < 6; i++) {
                    buff += "-";
                }

                MenuConsole.Print(1, 4, new ColoredString(buff, Color.White, Color.Black));
                MenuConsole.PrintClickable(8, 4, "[JOIN]", MenuClicks, "JoinLobby");


                MenuConsole.Print(1, 6, new ColoredString(joinError, Color.White, Color.Black));


                MenuConsole.PrintClickable(1, 8, "[BACK]".Align(HorizontalAlignment.Center, 18), MenuClicks, "SoftBack");
            } else if (GameLoop.UIManager.selectedMenu == "ModMenu") {
                ModMaker.DrawMod();
            }
        }

        public void LoadFileClick(string ID) {
            GameLoop.World.LoadAllMods();
            GameLoop.World.LoadPlayer(ID);
            GameLoop.UIManager.selectedMenu = "ConnectOrHost";
            GameLoop.UIManager.Map.UpdateVision();
        }

        public void MenuClicks(string ID) {
            if (ID == "BackToMenu") {
                RemakeMenu();
                MainMenuWindow.Children.Clear();
                MenuConsole.Clear();
                MenuConsole = new SadConsole.Console(20, 20);
                MainMenuWindow.Children.Add(MenuConsole);
                MenuConsole.Position = new Point(40, 20);
                NameConsole.IsVisible = false;
                GameLoop.UIManager.selectedMenu = "MainMenu";
            }

            else if (ID == "SoftBack") {
                RemakeMenu();
                GameLoop.UIManager.selectedMenu = "MainMenu"; 
                NameConsole.IsVisible = false; 
                MenuConsole.Position = new Point(40, 20);
                MenuConsole.Clear();
            }

            else if (ID == "ExitGame") {
                System.Environment.Exit(0);
            }

            else if (ID == "GoToMods") {
                GameLoop.UIManager.selectedMenu = "ModMenu";
                ModMaker.ModConsole.IsVisible = true;
                MenuConsole.IsVisible = false; 
                MenuConsole.Clear();
                ModMaker.FetchMods();
            }

            else if (ID == "GoToLoad") {
                GameLoop.UIManager.selectedMenu = "LoadFile";
                if (Directory.Exists("./saves/")) {
                    Names = Directory.GetDirectories("./saves/");

                    for (int i = 0; i < Names.Length; i++) {
                        string[] split = Names[i].Split("/");
                        Names[i] = split[^1];
                    }
                }
            }

            else if (ID == "GoToCharCreate") {
                GameLoop.UIManager.selectedMenu = "CharCreation";
                MenuConsole = new SadConsole.Console(50, 50);
                MainMenuWindow.Children.Add(MenuConsole);
                MenuConsole.Children.Add(NameConsole);
                MenuConsole.Position = new Point(25, 5);
                NameConsole.IsVisible = true; 
            }

            else if (ID == "ForeRdown") {
                if (GameLoop.EitherShift())
                    GameLoop.World.Player.ForegroundR = Math.Clamp(GameLoop.World.Player.ForegroundR - 10, 0, 255);
                else
                    GameLoop.World.Player.ForegroundR = Math.Clamp(GameLoop.World.Player.ForegroundR - 1, 0, 255);
            }

            else if (ID == "ForeRup") {
                if (GameLoop.EitherShift())
                    GameLoop.World.Player.ForegroundR = Math.Clamp(GameLoop.World.Player.ForegroundR + 10, 0, 255);
                else
                    GameLoop.World.Player.ForegroundR = Math.Clamp(GameLoop.World.Player.ForegroundR + 1, 0, 255);
            }

            else if (ID == "ForeGdown") {
                if (GameLoop.EitherShift())
                    GameLoop.World.Player.ForegroundG = Math.Clamp(GameLoop.World.Player.ForegroundG - 10, 0, 255);
                else
                    GameLoop.World.Player.ForegroundG = Math.Clamp(GameLoop.World.Player.ForegroundG - 1, 0, 255);
            }

            else if (ID == "ForeGup") {
                if (GameLoop.EitherShift())
                    GameLoop.World.Player.ForegroundG = Math.Clamp(GameLoop.World.Player.ForegroundG + 10, 0, 255);
                else
                    GameLoop.World.Player.ForegroundG = Math.Clamp(GameLoop.World.Player.ForegroundG + 1, 0, 255);
            }

            else if (ID == "ForeBdown") {
                if (GameLoop.EitherShift())
                    GameLoop.World.Player.ForegroundB = Math.Clamp(GameLoop.World.Player.ForegroundB - 10, 0, 255);
                else
                    GameLoop.World.Player.ForegroundB = Math.Clamp(GameLoop.World.Player.ForegroundB - 1, 0, 255);
            }

            else if (ID == "ForeBup") {
                if (GameLoop.EitherShift())
                    GameLoop.World.Player.ForegroundB = Math.Clamp(GameLoop.World.Player.ForegroundB + 10, 0, 255);
                else
                    GameLoop.World.Player.ForegroundB = Math.Clamp(GameLoop.World.Player.ForegroundB + 1, 0, 255);
            }

            else if (ID == "LivesInfinite") {
                GameLoop.World.Player.LivesRemaining = -1;
            }

            else if (ID == "Lives3") {
                GameLoop.World.Player.LivesRemaining = 3;
            }

            else if (ID == "Lives1") {
                GameLoop.World.Player.LivesRemaining = 1;
            }

            else if (ID == "DropsNone") {
                GameLoop.World.Player.DropsOnDeath = -1;
            }

            else if (ID == "DropsGold") {
                GameLoop.World.Player.DropsOnDeath = 0;
            }

            else if (ID == "DropsAll") {
                GameLoop.World.Player.DropsOnDeath = 1;
            }

            else if (ID == "AffinityFire") { GameLoop.World.Player.ElementalAlignment = "Fire"; }
            else if (ID == "AffinityWater") { GameLoop.World.Player.ElementalAlignment = "Water"; }
            else if (ID == "AffinityEarth") { GameLoop.World.Player.ElementalAlignment = "Earth"; }
            else if (ID == "AffinityMetal") { GameLoop.World.Player.ElementalAlignment = "Metal"; }
            else if (ID == "AffinityWood") { GameLoop.World.Player.ElementalAlignment = "Wood"; }

            else if (ID == "FinishCharCreation") {
                if (NameBox.EditingText != "" && NameBox.Text != "") {
                    GameLoop.UIManager.selectedMenu = "ConnectOrHost";

                    RemakeMenu();
                    MenuConsole.Clear();
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
            }

            else if (ID == "StartSingleplayer") {
                MainMenuWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapWindow.IsVisible = true; 
                GameLoop.UIManager.Sidebar.SidebarWindow.IsVisible = true;
                GameLoop.UIManager.selectedMenu = "None";
            }

            else if (ID == "StartMultiplayer") {
                GameLoop.NetworkManager = new NetworkManager();
                GameLoop.NetworkManager.CreateSteamLobby();
            }

            else if (ID == "JoinLobby") {
                if (LobbyCode.Length == 6) {
                    GameLoop.NetworkManager = new NetworkManager();
                    GameLoop.NetworkManager.JoinSteamLobby(LobbyCode);
                }
                else {
                    joinError = "Enter lobby code first";
                }
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

            if (GameLoop.UIManager.selectedMenu == "ModMenu") {
                ModMaker.Input();
            }  
        }
    }
}
