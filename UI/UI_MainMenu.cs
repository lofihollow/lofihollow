using SadRogue.Primitives;
using SadConsole;
using SadConsole.UI;
using SadConsole.Input;
using System.IO;
using System;
using System.Collections.Generic;
using LofiHollow.Managers;
using Key = SadConsole.Input.Keys;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LofiHollow.UI {
    public class UI_MainMenu {
        public SadConsole.Console MenuBackdrop; 
        public SadConsole.UI.ControlsConsole NameConsole; 
        public SadConsole.UI.Controls.TextBox NameBox;
        public Window MainMenuWindow;
        public SadRex.Image MenuImage;
        
        public SadConsole.Console MenuConsole;


        public ControlsConsole ModConsole;
         
        public string LobbyCode = "";
        public string[] Names;
        public string joinError = "";

        public string ModMenuSelect = "List";
        public string ModSubSelect = "None";
        public bool CreatingMod = false;
        public int ModItemIndex = 0;
        public int CurrentGlyphIndex = 0;
        public int SelectedField = 0;
        public Mod CurrentMod;

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

            ModConsole = new(90, 50);
            ModConsole.Position = new(5, 5);
            MainMenuWindow.Children.Add(ModConsole);
            ModConsole.IsVisible = false;
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
            ModConsole.Clear();
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
                MenuConsole.Print(5, 2, Helper.HoverColoredString(" New Game", mousePos.Y == 2));
                MenuConsole.Print(5, 4, Helper.HoverColoredString("Load Game", mousePos.Y == 4));
                MenuConsole.Print(5, 6, Helper.HoverColoredString("  Mods", mousePos.Y == 6));
                MenuConsole.Print(5, 18, Helper.HoverColoredString("  Quit ", mousePos.Y == 18));
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
                        MenuConsole.Print(1, 3 + i, Helper.HoverColoredString(Names[i].Align(HorizontalAlignment.Center, 18), mousePos.Y == 3 + i));
                    }
                }

                MenuConsole.Print(1, 1 + fileSize - 3, Helper.HoverColoredString("[BACK]".Align(HorizontalAlignment.Center, 18), mousePos.Y == (1 + fileSize - 3)));
            } 
            else if (GameLoop.UIManager.selectedMenu == "ConnectOrHost") {
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
            else if (GameLoop.UIManager.selectedMenu == "ModMenu") {
                mousePos = new MouseScreenObjectState(ModConsole, GameHost.Instance.Mouse).CellPosition;
                 
                ModConsole.DrawBox(new Rectangle(0, 0, 90, 50), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.White, Color.Black)));
                
                if (ModMenuSelect == "Create") { 
                    ModConsole.Print(1, 1, Helper.HoverColoredString("[BACK]".Align(HorizontalAlignment.Center, 9), mousePos.Y == 1 && mousePos.X < 10));
                    ModConsole.Print(10, 1, Helper.HoverColoredString("[SAVE]".Align(HorizontalAlignment.Center, 9), mousePos.Y == 1 && mousePos.X > 10 && mousePos.X < 20));

                    ModConsole.Print(1, 3, "Name: " + CurrentMod.Name, SelectedField == 0 ? Color.Yellow : Color.White);

                    ModConsole.Print(1, 5, "Package: " + CurrentMod.Package, SelectedField == 1 ? Color.Yellow : Color.White);

                    if (CurrentMod.ModConstructs.Count > 0)
                        ModConsole.Print(1, 9, Helper.HoverColoredString("Constructibles", mousePos.Y == 9 && mousePos.X < 20));
                    else
                        ModConsole.Print(1, 9, new ColoredString("Add Constructible", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModRecipes.Count > 0)
                        ModConsole.Print(1, 11, Helper.HoverColoredString("Crafting Recipes", mousePos.Y == 11 && mousePos.X < 20));
                    else
                        ModConsole.Print(1, 11, new ColoredString("Add Recipe", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModFish.Count > 0)
                        ModConsole.Print(1, 13, Helper.HoverColoredString("Fish", mousePos.Y == 13 && mousePos.X < 20));
                    else
                        ModConsole.Print(1, 13, new ColoredString("Add Fish", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModItems.Count > 0)
                        ModConsole.Print(1, 15, Helper.HoverColoredString("Items", mousePos.Y == 15 && mousePos.X < 20));
                    else
                        ModConsole.Print(1, 15, new ColoredString("Add Item", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModMissions.Count > 0)
                        ModConsole.Print(1, 17, Helper.HoverColoredString("Missions", mousePos.Y == 17 && mousePos.X < 20));
                    else
                        ModConsole.Print(1, 17, new ColoredString("Add Mission", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModMonsters.Count > 0)
                        ModConsole.Print(1, 19, Helper.HoverColoredString("Monsters", mousePos.Y == 19 && mousePos.X < 20));
                    else
                        ModConsole.Print(1, 19, new ColoredString("Add Monster", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModNPCs.Count > 0)
                        ModConsole.Print(1, 21, Helper.HoverColoredString("NPCs", mousePos.Y == 21 && mousePos.X < 20));
                    else
                        ModConsole.Print(1, 21, new ColoredString("Add NPC", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModSkills.Count > 0)
                        ModConsole.Print(1, 23, Helper.HoverColoredString("Skills", mousePos.Y == 23 && mousePos.X < 20));
                    else
                        ModConsole.Print(1, 23, new ColoredString("Add Skill", Color.DarkSlateGray, Color.Black));



                    if (ModSubSelect == "Constructible") {
                        ModConsole.Print(21, 1, "Name: " + CurrentMod.ModConstructs[ModItemIndex].Name, SelectedField == 2 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 3, "Material Desc: " + CurrentMod.ModConstructs[ModItemIndex].Materials, SelectedField == 3 ? Color.Yellow : Color.White);
                        ModConsole.Print(58, 1, "Misc String: " + CurrentMod.ModConstructs[ModItemIndex].SpecialProps, SelectedField == 4 ? Color.Yellow : Color.White);

                        ModConsole.Print(21, 5, "Appearance: " + CurrentMod.ModConstructs[ModItemIndex].Appearance());
                         
                        ModConsole.Print(21, 7, "Main Red: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].ForegroundR, 3) + "+");
                        ModConsole.Print(21, 8, "Main Grn: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].ForegroundG, 3) + "+");
                        ModConsole.Print(21, 9, "Main Blu: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].ForegroundB, 3) + "+");
                        ModConsole.Print(21, 10, "   Glyph: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].Glyph, 3) + "+");

                        ModConsole.Print(21, 13, "Decorator: ");
                        if (CurrentMod.ModConstructs[ModItemIndex].Dec == null) {
                            ModConsole.Print(21, 14, "[Add Decorator]", Color.DarkSlateGray);
                        } else {
                            ModConsole.SetDecorator(33, 5, 1, CurrentMod.ModConstructs[ModItemIndex].Dec.GetDec());
                            ModConsole.Print(21, 14, ":   Red: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].Dec.R, 3) + "+");
                            ModConsole.Print(21, 15, ":   Grn: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].Dec.G, 3) + "+");
                            ModConsole.Print(21, 16, ":   Blu: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].Dec.B, 3) + "+");
                            ModConsole.Print(21, 17, ": Alpha: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].Dec.A, 3) + "+");
                            ModConsole.Print(21, 18,": Glyph: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].Dec.Glyph, 3) + "+");
                            ModConsole.Print(21, 19, "[Delete Decorator]", mousePos.Y == 19 && mousePos.X > 20 && mousePos.Y < 40 ? Color.Yellow : Color.DarkSlateGray);
                        }

                        ModConsole.Print(21, 22, "Level to Build: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].RequiredLevel, 2) + "+");
                        ModConsole.Print(21, 23, "Exp Granted: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].ExpGranted, 7) + "+");

                        ColoredString moveblockcheck = new(Helper.Center((char)4, 3), Color.Lime, Color.Black); 
                        if (!CurrentMod.ModConstructs[ModItemIndex].BlocksMove)
                            moveblockcheck = new(Helper.Center("x", 3), Color.Red, Color.Black);

                        ColoredString visblockcheck = new(Helper.Center((char) 4, 3), Color.Lime, Color.Black);
                        if (!CurrentMod.ModConstructs[ModItemIndex].BlocksLOS)
                            visblockcheck = new(Helper.Center("x", 3), Color.Red, Color.Black);

                        ModConsole.Print(21, 25, "Blocks Movement: " + moveblockcheck);
                        ModConsole.Print(21, 26, "  Blocks Vision: " + visblockcheck);

                        ColoredString containerCheck = new(Helper.Center((char)4, 3), Color.Lime, Color.Black);
                        if (CurrentMod.ModConstructs[ModItemIndex].Container == null)
                            containerCheck = new(Helper.Center("x", 3), Color.Red, Color.Black);
                        ModConsole.Print(21, 28, "Container: " + containerCheck);

                        if (CurrentMod.ModConstructs[ModItemIndex].Container != null) {
                            ModConsole.Print(21, 29, "Capacity: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].Container.Capacity, 2) + "+");
                        }

                        ModConsole.Print(21, 31, "Construction Materials: " + Helper.HoverColoredString("[ADD]", mousePos.Y == 31 && mousePos.X >= 45 && mousePos.X <= 49) + " " + Helper.HoverColoredString("[DEL]", mousePos.Y == 31 && mousePos.X >= 51 && mousePos.X <= 55));
                        int y = 32;

                        for (int i = 0; i < CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count; i++) {
                            ModConsole.Print(21, y+i, ": Name: " + CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[i].Name, SelectedField == 5 + i? Color.Yellow : Color.White);
                            ModConsole.Print(51, y+i, "Quantity: -" + Helper.Center(CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[i].ItemQuantity, 3) + "+");

                        }
                    }

                    if (GameHost.Instance.Mouse.LeftButtonDown) {
                        ModConsole.Print(1, 24, mousePos.X + ", " + mousePos.Y);
                    }

                    if (ModSubSelect == "Recipe") {
                        ModConsole.Print(21, 1, "Name: " + CurrentMod.ModRecipes[ModItemIndex].Name, SelectedField == 2 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 2, "Finished Item ID: " + CurrentMod.ModRecipes[ModItemIndex].FinishedID, SelectedField == 3 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 3, "Finished Item Quantity: -" + Helper.Center(CurrentMod.ModRecipes[ModItemIndex].FinishedQty, 3) + "+");
                        ModConsole.Print(21, 5, "Skill: " + CurrentMod.ModRecipes[ModItemIndex].Skill, SelectedField == 4 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 6, "Required Level: -" + Helper.Center(CurrentMod.ModRecipes[ModItemIndex].RequiredLevel, 2) + "+");
                        ModConsole.Print(21, 7, "Exp Granted: -" + Helper.Center(CurrentMod.ModRecipes[ModItemIndex].ExpGranted, 5) + "+");

                        ColoredString qualityCheck = new(Helper.Center((char)4, 3), Color.Lime, Color.Black);
                        if (!CurrentMod.ModRecipes[ModItemIndex].HasQuality)
                            qualityCheck = new(Helper.Center("x", 3), Color.Red, Color.Black);

                        ColoredString weightCheck = new(Helper.Center((char)4, 3), Color.Lime, Color.Black);
                        if (!CurrentMod.ModRecipes[ModItemIndex].WeightBasedOutput)
                            weightCheck = new(Helper.Center("x", 3), Color.Red, Color.Black);

                        ModConsole.Print(21, 9, "Item has Quality: " + qualityCheck);
                        ModConsole.Print(21, 10, "Weight Based Output: " + weightCheck);


                        ModConsole.Print(21, 14, "Required Tools: " + Helper.HoverColoredString("[ADD]", mousePos.Y == 14 && mousePos.X >= 37 && mousePos.X <= 41) + " " + Helper.HoverColoredString("[DEL]", mousePos.Y == 14 && mousePos.X >= 43 && mousePos.X <= 47));
                        for (int i = 0; i < CurrentMod.ModRecipes[ModItemIndex].RequiredTools.Count; i++) {
                            ModConsole.Print(21, 15 + i, ": Property: " + CurrentMod.ModRecipes[ModItemIndex].RequiredTools[i].Property, SelectedField == 5 + i ? Color.Yellow : Color.White);
                            ModConsole.Print(55, 15 + i, " Tier: -" + Helper.Center(CurrentMod.ModRecipes[ModItemIndex].RequiredTools[i].Tier, 2) + "+");
                        }

                        ModConsole.Print(21, 22, "Generic Materials: " + Helper.HoverColoredString("[ADD]", mousePos.Y == 22 && mousePos.X >= 40 && mousePos.X <= 44) + " " + Helper.HoverColoredString("[DEL]", mousePos.Y == 22 && mousePos.X >= 46 && mousePos.X <= 50));
                        for (int i = 0; i < CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count; i++) {
                            ModConsole.Print(21, 23 + i, ": Property: " + CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[i].Property, SelectedField == 10 + i ? Color.Yellow : Color.White);
                            ModConsole.Print(55, 23 + i, " Qty: -" + Helper.Center(CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[i].Quantity, 3) + "+    Tier: -" + Helper.Center(CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[i].Tier, 2) + "+");
                        }

                        ModConsole.Print(21, 30, "Specific Materials: " + Helper.HoverColoredString("[ADD]", mousePos.Y == 30 && mousePos.X >= 41 && mousePos.X <= 45) + " " + Helper.HoverColoredString("[DEL]", mousePos.Y == 30 && mousePos.X >= 47 && mousePos.X <= 51));
                        for (int i = 0; i < CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials.Count; i++) {
                            ModConsole.Print(21, 31 + i, ": Name: " + CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[i].Name, SelectedField == 15 + i ? Color.Yellow : Color.White);
                            ModConsole.Print(55, 31 + i, " Qty: -" + Helper.Center(CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[i].ItemQuantity, 3) + "+");

                        }
                    }

                    if (ModSubSelect == "Fish") {
                        ModConsole.Print(21, 1, "Name: " + CurrentMod.ModFish[ModItemIndex].Name, SelectedField == 2 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 3, "Seasons: " + CurrentMod.ModFish[ModItemIndex].Season, SelectedField == 3 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 5, "Locations: " + CurrentMod.ModFish[ModItemIndex].CatchLocation, SelectedField == 4 ? Color.Yellow : Color.White);

                        ModConsole.Print(21, 11, "Earliest Time: - " + Helper.Center(TimeManager.MinutesToTime(CurrentMod.ModFish[ModItemIndex].EarliestTime), 5, '0') + " +");
                        ModConsole.Print(21, 12, "  Latest Time: - " + Helper.Center(TimeManager.MinutesToTime(CurrentMod.ModFish[ModItemIndex].LatestTime), 5, '0') + " +");

                        ModConsole.Print(21, 14, "Fish Item ID: " + CurrentMod.ModFish[ModItemIndex].FishItemID, SelectedField == 5 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 15, "Fish Fillet ID: " + CurrentMod.ModFish[ModItemIndex].FilletID, SelectedField == 6 ? Color.Yellow : Color.White);

                        ModConsole.Print(21, 17, "Appearance: " + CurrentMod.ModFish[ModItemIndex].GetAppearance());
                        ModConsole.Print(21, 18, "  Red: -" + Helper.Center(CurrentMod.ModFish[ModItemIndex].colR, 5) + "+");
                        ModConsole.Print(21, 19, "  Grn: -" + Helper.Center(CurrentMod.ModFish[ModItemIndex].colG, 5) + "+");
                        ModConsole.Print(21, 20, "  Blu: -" + Helper.Center(CurrentMod.ModFish[ModItemIndex].colB, 5) + "+");
                        ModConsole.Print(21, 21, "Alpha: -" + Helper.Center(CurrentMod.ModFish[ModItemIndex].colA, 5) + "+");
                        ModConsole.Print(21, 22, "Glyph: -" + Helper.Center(CurrentMod.ModFish[ModItemIndex].glyph, 5) + "+");

                        ModConsole.Print(21, 24, "Required Level: -" + Helper.Center(CurrentMod.ModFish[ModItemIndex].RequiredLevel, 4) + "+");
                        ModConsole.Print(21, 25, "Exp Granted: -" + Helper.Center(CurrentMod.ModFish[ModItemIndex].GrantedExp, 5) + "+");

                        ModConsole.Print(21, 27, "Strength: -" + Helper.Center(CurrentMod.ModFish[ModItemIndex].Strength, 4) + "+");
                        ModConsole.Print(21, 28, "Fight Chance: -" + Helper.Center(CurrentMod.ModFish[ModItemIndex].FightChance, 4) + "+");
                        ModConsole.Print(21, 29, "Fight Length: -" + Helper.Center(CurrentMod.ModFish[ModItemIndex].FightLength, 4) + "+");

                        ModConsole.Print(21, 31, "Max Quality: -" + Helper.LetterGrade(CurrentMod.ModFish[ModItemIndex].MaxQuality, 5) + new ColoredString("+"));
                    }

                    if (ModSubSelect == "Item") {
                        ModConsole.Print(21, 1, "Item");
                    }

                    if (ModSubSelect == "Mission") {
                        ModConsole.Print(21, 1, "Mission");
                    }

                    if (ModSubSelect == "Monster") {
                        ModConsole.Print(21, 1, "Name: " + CurrentMod.ModMonsters[ModItemIndex].Name, SelectedField == 2 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 3, "Combat Type: " + CurrentMod.ModMonsters[ModItemIndex].CombatType, SelectedField == 3 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 5, "Damage Type: " + CurrentMod.ModMonsters[ModItemIndex].DamageType, SelectedField == 4 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 7, "   Weakness: " + CurrentMod.ModMonsters[ModItemIndex].SpecificWeakness, SelectedField == 5 ? Color.Yellow : Color.White);
                        ModConsole.Print(21, 9, "   Location: " + CurrentMod.ModMonsters[ModItemIndex].SpawnLocation, SelectedField == 6 ? Color.Yellow : Color.White);

                        ModConsole.Print(50, 1, "Appearance: " + CurrentMod.ModMonsters[ModItemIndex].GetAppearance());
                        ModConsole.Print(50, 3, "  Red: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].ForegroundR, 5) + "+");
                        ModConsole.Print(50, 4, "  Grn: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].ForegroundG, 5) + "+");
                        ModConsole.Print(50, 5, "  Blu: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].ForegroundB, 5) + "+");
                        ModConsole.Print(50, 6, "Alpha: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].ForegroundA, 5) + "+");
                        ModConsole.Print(50, 7, "Glyph: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].ActorGlyph, 5) + "+");

                        ModConsole.Print(21, 11, "Constitution: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].MonConstitution, 4) + "+");
                        ModConsole.Print(21, 12, "      Attack: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].MonAttack, 4) + "+");
                        ModConsole.Print(21, 13, "    Strength: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].MonStrength, 4) + "+");
                        ModConsole.Print(21, 14, "     Defense: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].MonDefense, 4) + "+");
                        ModConsole.Print(21, 15, "       Magic: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].MonMagic, 4) + "+");
                        ModConsole.Print(21, 16, "      Ranged: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].MonRanged, 4) + "+");

                        ColoredString eggCheck = new(Helper.Center((char)4, 3), Color.Lime, Color.Black);
                        if (!CurrentMod.ModMonsters[ModItemIndex].CanDropEgg)
                            eggCheck = new(Helper.Center("x", 3), Color.Red, Color.Black);

                        ColoredString aggroCheck = new(Helper.Center((char)4, 3), Color.Lime, Color.Black);
                        if (!CurrentMod.ModMonsters[ModItemIndex].AlwaysAggro)
                            aggroCheck = new(Helper.Center("x", 3), Color.Red, Color.Black);

                        ModConsole.Print(50, 9, "Always Aggro:" + aggroCheck);
                        ModConsole.Print(50, 11, "Drops Egg:" + eggCheck);

                        if (CurrentMod.ModMonsters[ModItemIndex].EggData != null) {
                            ModConsole.Print(50, 11, "Hatches Into:" + CurrentMod.ModMonsters[ModItemIndex].EggData.HatchesInto);
                            ModConsole.Print(50, 11, "Days to Hatch: -" + Helper.Center(CurrentMod.ModMonsters[ModItemIndex].EggData.DaysToHatch, 4) + "+");
                        }

                        ModConsole.Print(21, 20, "Drops: " + Helper.HoverColoredString("[ADD]", mousePos.Y == 20 && mousePos.X >= 28 && mousePos.X <= 32) + " " + Helper.HoverColoredString("[DEL]", mousePos.Y == 20 && mousePos.X >= 34 && mousePos.X <= 38));
                        for (int i = 0; i < CurrentMod.ModMonsters[ModItemIndex].DropTable.Count; i++) {
                            ModConsole.Print(21, 21 + i, ": Drop: " + CurrentMod.ModMonsters[ModItemIndex].DropTable[i], SelectedField == 7 + i ? Color.Yellow : Color.White);
                        }
                        ModConsole.Print(21, 32, "Drop format: itemID;drop chance (1 in X);quantity");
                        ModConsole.Print(21, 34, "Drop example: lh:Copper Coin;2;5");
                        ModConsole.Print(21, 35, "       50% chance of dropping 5 copper coins.");
                        ModConsole.DrawLine(new Point(48, 1), new Point(48, 20), 179, Color.White);
                    }

                    if (ModSubSelect == "NPC") {
                        ModConsole.Print(21, 1, "NPC");
                    }

                    if (ModSubSelect == "Skill") {
                        ModConsole.Print(21, 1, "Name: " + CurrentMod.ModSkills[ModItemIndex].Name, SelectedField == 2 ? Color.Yellow : Color.White);

                    }


                    for (int i = 0; i < 400; i++) {
                        ModConsole.Print(1 + (i % 20), 29 + (i / 20), ((char)i).ToString(), CurrentGlyphIndex == i ? Color.Green : Color.Gray);
                    }


                    ModConsole.Print(21, 48, Helper.HoverColoredString("Previous", mousePos.Y == 48 && mousePos.X < 30));
                    ModConsole.Print(85, 48, Helper.HoverColoredString("Next", mousePos.Y == 48 && mousePos.X > 84));
                }

                if (ModMenuSelect == "List") {
                    ModConsole.Print(1, 1, Helper.HoverColoredString("[BACK]".Align(HorizontalAlignment.Center, 9), mousePos.Y == 1 && mousePos.X < 10));
                    ModConsole.Print(11, 1, Helper.HoverColoredString("[NEW]".Align(HorizontalAlignment.Center, 9), mousePos.Y == 1 && mousePos.X > 10 && mousePos.X < 20));

                    if (!Directory.Exists("./mods/"))
                        Directory.CreateDirectory("./mods/");

                    ModConsole.Print(21, 1, "Mod Name".Align(HorizontalAlignment.Center, 20) + "|" + "Enabled".Align(HorizontalAlignment.Center, 9));
                    ModConsole.DrawLine(new Point(21, 2), new Point(88, 2), 196, Color.White, Color.Black);

                    int y = 3;

                    foreach (var modPath in Directory.GetFiles("./mods/")) {
                        string json = File.ReadAllText(modPath); 
                        Mod mod = JsonConvert.DeserializeObject<Mod>(json);

                        ColoredString modEntry = new("", Color.White, Color.Black);

                        ColoredString check = new ColoredString(((char)4).ToString().Align(HorizontalAlignment.Center, 9), Color.Lime, Color.Black);

                        if (!mod.Enabled)
                            check = new ColoredString("x".Align(HorizontalAlignment.Center, 9), Color.Red, Color.Black);

                        modEntry += Helper.HoverColoredString(mod.Name.Align(HorizontalAlignment.Center, 20), mousePos.Y == y && mousePos.X > 20);
                        modEntry += "|";
                        modEntry += check;

                        ModConsole.Print(21, y, modEntry);


                        y++;
                    }
                }

                if (ModMenuSelect == "Overview") {
                    ModConsole.Print(1, 1, Helper.HoverColoredString("[BACK]".Align(HorizontalAlignment.Center, 9), mousePos.Y == 1 && mousePos.X < 10));
                    ModConsole.Print(10, 1, Helper.HoverColoredString("[EDIT]".Align(HorizontalAlignment.Center, 9), mousePos.Y == 1 && mousePos.X > 10 && mousePos.X < 20));

                    if (CurrentMod != null) {
                        ModConsole.Print(1, 3, CurrentMod.Package + ":" + CurrentMod.Name);
                        if (CurrentMod.ModConstructs.Count > 0)
                            ModConsole.Print(1, 6, Helper.HoverColoredString("Constructibles", mousePos.Y == 6));
                        else
                            ModConsole.Print(1, 6, new ColoredString("Constructibles", Color.DarkSlateGray, Color.Black));

                        if (CurrentMod.ModRecipes.Count > 0)
                            ModConsole.Print(1, 8, Helper.HoverColoredString("Crafting Recipes", mousePos.Y == 8));
                        else
                            ModConsole.Print(1, 8, new ColoredString("Crafting Recipes", Color.DarkSlateGray, Color.Black));

                        if (CurrentMod.ModFish.Count > 0)
                            ModConsole.Print(1, 10, Helper.HoverColoredString("Fish", mousePos.Y == 10));
                        else
                            ModConsole.Print(1, 10, new ColoredString("Fish", Color.DarkSlateGray, Color.Black));

                        if (CurrentMod.ModItems.Count > 0)
                            ModConsole.Print(1, 12, Helper.HoverColoredString("Items", mousePos.Y == 12));
                        else
                            ModConsole.Print(1, 12, new ColoredString("Items", Color.DarkSlateGray, Color.Black));

                        if (CurrentMod.ModMissions.Count > 0)
                            ModConsole.Print(1, 14, Helper.HoverColoredString("Missions", mousePos.Y == 14));
                        else
                            ModConsole.Print(1, 14, new ColoredString("Missions", Color.DarkSlateGray, Color.Black));

                        if (CurrentMod.ModMonsters.Count > 0)
                            ModConsole.Print(1, 16, Helper.HoverColoredString("Monsters", mousePos.Y == 16));
                        else
                            ModConsole.Print(1, 16, new ColoredString("Monsters", Color.DarkSlateGray, Color.Black));

                        if (CurrentMod.ModNPCs.Count > 0)
                            ModConsole.Print(1, 18, Helper.HoverColoredString("NPCs", mousePos.Y == 18));
                        else
                            ModConsole.Print(1, 18, new ColoredString("NPCs", Color.DarkSlateGray, Color.Black));

                        if (CurrentMod.ModSkills.Count > 0)
                            ModConsole.Print(1, 20, Helper.HoverColoredString("Skills", mousePos.Y == 20));
                        else
                            ModConsole.Print(1, 20, new ColoredString("Skills", Color.DarkSlateGray, Color.Black));


                        if (ModSubSelect == "None") {
                            ModConsole.Print(21, 1, "Mod   Name: " + CurrentMod.Name);
                            ModConsole.Print(21, 3, "Mod Prefix: " + CurrentMod.Package);

                            ColoredString check = new ColoredString(((char)4).ToString(), Color.Lime, Color.Black);
                            if (!CurrentMod.Enabled)
                                check = new ColoredString("x", Color.Red, Color.Black);
                            ModConsole.Print(21, 5, "Enabled: " + check);

                            ModConsole.Print(21, 8, "Constructibles Added: " + CurrentMod.ModConstructs.Count);
                            ModConsole.Print(21, 10, "Crafting Recipes Added: " + CurrentMod.ModRecipes.Count);
                            ModConsole.Print(21, 12, "Fish Added: " + CurrentMod.ModFish.Count);
                            ModConsole.Print(21, 14, "Items Added: " + CurrentMod.ModItems.Count);
                            ModConsole.Print(21, 16, "Missions Added: " + CurrentMod.ModMissions.Count);
                            ModConsole.Print(21, 18, "Monsters Added: " + CurrentMod.ModMonsters.Count);
                            ModConsole.Print(21, 20, "NPCs Added: " + CurrentMod.ModNPCs.Count);
                            ModConsole.Print(21, 22, "Skills Added: " + CurrentMod.ModSkills.Count);
                        }
                    }
                }


                ModConsole.DrawLine(new Point(20, 1), new Point(20, 48), 179, Color.White, Color.Black);
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
                    if (mousePos.Y == 2) { // Make a new character
                        GameLoop.UIManager.selectedMenu = "CharCreation";
                        MenuConsole = new SadConsole.Console(50, 50);
                        MainMenuWindow.Children.Add(MenuConsole);
                        MenuConsole.Children.Add(NameConsole);
                        MenuConsole.Position = new Point(25, 5);
                        NameConsole.IsVisible = true;
                    } else if (mousePos.Y == 4) { // Go to file select
                        GameLoop.UIManager.selectedMenu = "LoadFile";
                        if (Directory.Exists("./saves/")) {
                            Names = Directory.GetDirectories("./saves/");

                            for (int i = 0; i < Names.Length; i++) {
                                string[] split = Names[i].Split("/");
                                Names[i] = split[^1];
                            }
                        }
                    } else if (mousePos.Y == 6) { // Go to mod tools
                        GameLoop.UIManager.selectedMenu = "ModMenu";
                        ModConsole.IsVisible = true;
                        MenuConsole.IsVisible = false;
                    } else if (mousePos.Y == 18) { // Quit the game
                        System.Environment.Exit(0);
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
                } 
                else if (GameLoop.UIManager.selectedMenu == "LoadFile") {
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
                else if (GameLoop.UIManager.selectedMenu == "ModMenu") {
                    mousePos = new MouseScreenObjectState(ModConsole, GameHost.Instance.Mouse).CellPosition;

                    if (ModMenuSelect == "List") {
                        if (mousePos.Y == 1 && mousePos.X < 10) {
                            GameLoop.UIManager.selectedMenu = "MainMenu";
                            MenuConsole.IsVisible = true;
                            ModConsole.IsVisible = false;
                        }

                        if (mousePos.Y == 1 && mousePos.X > 10 && mousePos.X < 20) {
                            ModMenuSelect = "Create";
                            CurrentMod = new();
                            CreatingMod = true;
                        }

                        if (mousePos.Y >= 3 && mousePos.X > 20 && mousePos.X < 40) {
                            int y = 3;
                            foreach (var modPath in Directory.GetFiles("./mods/")) {
                                if (y == mousePos.Y) { // Load when we find the right one
                                    string json = File.ReadAllText(modPath);
                                    CurrentMod = JsonConvert.DeserializeObject<Mod>(json);
                                    ModMenuSelect = "Overview";
                                    CreatingMod = false;
                                    break;
                                } else { // Otherwise keep counting
                                    y++;
                                }
                            }
                        }

                        if (mousePos.Y >= 3 && mousePos.X > 40) {
                            int y = 3;
                            foreach (var modPath in Directory.GetFiles("./mods/")) {
                                if (y == mousePos.Y) { // Load when we find the right one
                                    string json = File.ReadAllText(modPath);
                                    CurrentMod = JsonConvert.DeserializeObject<Mod>(json);
                                    CurrentMod.Enabled = !CurrentMod.Enabled;
                                    SaveMod();
                                    break;
                                } else { // Otherwise keep counting
                                    y++;
                                }
                            }
                        }
                    } 
                    else if (ModMenuSelect == "Create") {
                        if (mousePos.Y == 1 && mousePos.X < 10) { // Back to mod list
                            ModMenuSelect = "List";
                            SaveMod();
                            CreatingMod = false;
                            CurrentMod = null;
                        }

                        if (mousePos.Y == 1 && mousePos.X > 10 && mousePos.X < 20) { // Save mod
                            SaveMod(); 
                        }

                        if (mousePos.X < 20) {
                            // Go to constructible sub-menu
                            if (mousePos.Y == 9) {
                                ModSubSelect = "Constructible";
                                ModItemIndex = 0;
                                SelectedField = 0;
                                if (CurrentMod.ModConstructs.Count == 0)
                                    CurrentMod.ModConstructs.Add(new());
                            }

                            // Go to recipe sub-menu
                            if (mousePos.Y == 11) {
                                ModSubSelect = "Recipe";
                                ModItemIndex = 0;
                                SelectedField = 0;
                                if (CurrentMod.ModRecipes.Count == 0)
                                    CurrentMod.ModRecipes.Add(new());
                            }

                            // Go to fish sub-menu
                            if (mousePos.Y == 13) {
                                ModSubSelect = "Fish";
                                ModItemIndex = 0;
                                SelectedField = 0;
                                if (CurrentMod.ModFish.Count == 0)
                                    CurrentMod.ModFish.Add(new());
                            }

                            // Go to item sub-menu
                            if (mousePos.Y == 15) {
                                ModSubSelect = "Item";
                                ModItemIndex = 0;
                                SelectedField = 0;
                                if (CurrentMod.ModItems.Count == 0)
                                    CurrentMod.ModItems.Add(new());
                            }

                            // Go to mission sub-menu
                            if (mousePos.Y == 17) {
                                ModSubSelect = "Mission";
                                ModItemIndex = 0;
                                SelectedField = 0;
                                if (CurrentMod.ModMissions.Count == 0)
                                    CurrentMod.ModMissions.Add(new());
                            }

                            // Go to monster sub-menu
                            if (mousePos.Y == 19) {
                                ModSubSelect = "Monster";
                                ModItemIndex = 0;
                                SelectedField = 0;
                                if (CurrentMod.ModMonsters.Count == 0)
                                    CurrentMod.ModMonsters.Add(new());
                            }

                            // Go to NPC sub-menu
                            if (mousePos.Y == 21) {
                                ModSubSelect = "NPC";
                                ModItemIndex = 0;
                                SelectedField = 0;
                                if (CurrentMod.ModNPCs.Count == 0)
                                    CurrentMod.ModNPCs.Add(new());
                            }

                            // Go to skill sub-menu
                            if (mousePos.Y == 23) {
                                ModSubSelect = "Skill";
                                ModItemIndex = 0;
                                SelectedField = 0;
                                if (CurrentMod.ModSkills.Count == 0)
                                    CurrentMod.ModSkills.Add(new());
                            }
                        }
                    } 
                    else if (ModMenuSelect == "Overview") {
                        if (mousePos.Y == 1 && mousePos.X < 10) { // Back to mod list
                            ModMenuSelect = "List";
                            SaveMod();
                            CreatingMod = false;
                            CurrentMod = null;
                        }

                        if (mousePos.Y == 1 && mousePos.X > 10) { // Edit mod
                            ModMenuSelect = "Create"; 
                            CreatingMod = true;
                        }

                        if (mousePos.Y == 5 && mousePos.X > 20) { // Toggle mod enabled status
                            CurrentMod.Enabled = !CurrentMod.Enabled;
                            SaveMod(); 
                        }

                        if (CurrentMod != null) {
                            if (mousePos.X < 20) {
                                if (mousePos.Y == 6 && CurrentMod.ModConstructs.Count > 0)
                                    ModSubSelect = "Constructibles";
                                if (mousePos.Y == 8 && CurrentMod.ModRecipes.Count > 0)
                                    ModSubSelect = "Crafting Recipes";
                                if (mousePos.Y == 10 && CurrentMod.ModFish.Count > 0)
                                    ModSubSelect = "Fish";
                                if (mousePos.Y == 12 && CurrentMod.ModItems.Count > 0)
                                    ModSubSelect = "Items";
                                if (mousePos.Y == 14 && CurrentMod.ModMissions.Count > 0)
                                    ModSubSelect = "Missions";
                                if (mousePos.Y == 16 && CurrentMod.ModMonsters.Count > 0)
                                    ModSubSelect = "Monsters";
                                if (mousePos.Y == 18 && CurrentMod.ModNPCs.Count > 0)
                                    ModSubSelect = "NPCs";
                                if (mousePos.Y == 20 && CurrentMod.ModSkills.Count > 0)
                                    ModSubSelect = "Skills";
                            }
                        }
                    }
                }
            }

            if (GameLoop.UIManager.selectedMenu == "ModMenu") {
                mousePos = new MouseScreenObjectState(ModConsole, GameHost.Instance.Mouse).CellPosition;
                if (CurrentMod != null && ModMenuSelect == "Create") {
                    foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                        if ((key.Character >= 'A' && key.Character <= 'z') || (key.Character >= '0' && key.Character <= '9'
                            || key.Character == ';' || key.Character == ':' || key.Character == '|')) {
                            RunInput(key.Character.ToString(), false);
                        }
                    }

                    if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                        RunInput(" ", false);
                    }

                    if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Back)) {
                        RunInput("", true);
                    }

                    
                    if (GameHost.Instance.Mouse.LeftClicked) {
                        if (mousePos.Y == 3 && mousePos.X < 20)
                            SelectedField = 0;
                        if (mousePos.Y == 5 && mousePos.X < 20)
                            SelectedField = 1;

                        if (ModSubSelect == "Constructible") {
                            if (mousePos.Y == 1 && mousePos.X > 20 && mousePos.X < 57)
                                SelectedField = 2;
                            if (mousePos.Y == 3 && mousePos.X > 20 && mousePos.X < 57)
                                SelectedField = 3;

                            if (mousePos.Y == 1 && mousePos.X > 57)
                                SelectedField = 4;

                            if (mousePos.Y == 32 && mousePos.X > 20 && mousePos.X < 49 && CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count > 0)
                                SelectedField = 5;
                            if (mousePos.Y == 33 && mousePos.X > 20 && mousePos.X < 49 && CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count > 1)
                                SelectedField = 6;
                            if (mousePos.Y == 34 && mousePos.X > 20 && mousePos.X < 49 && CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count > 2)
                                SelectedField = 7;
                            if (mousePos.Y == 35 && mousePos.X > 20 && mousePos.X < 49 && CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count > 3)
                                SelectedField = 8;
                            if (mousePos.Y == 36 && mousePos.X > 20 && mousePos.X < 49 && CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count > 4)
                                SelectedField = 9; 

                            if (mousePos.Y == 14 && mousePos.X >= 22 && mousePos.Y <= 35 && CurrentMod.ModConstructs[ModItemIndex].Dec == null) {
                                CurrentMod.ModConstructs[ModItemIndex].Dec = new();
                            }

                            if (mousePos.Y == 19 && mousePos.X >= 20 && mousePos.Y <= 38 && CurrentMod.ModConstructs[ModItemIndex].Dec != null) {
                                CurrentMod.ModConstructs[ModItemIndex].Dec = null;
                            }

                            if (mousePos.Y == 25 && mousePos.X > 20 && mousePos.X < 35)
                                CurrentMod.ModConstructs[ModItemIndex].BlocksMove = !CurrentMod.ModConstructs[ModItemIndex].BlocksMove;
                            if (mousePos.Y == 26 && mousePos.X > 20 && mousePos.X < 35)
                                CurrentMod.ModConstructs[ModItemIndex].BlocksLOS = !CurrentMod.ModConstructs[ModItemIndex].BlocksLOS;

                            bool JustMadeCon = false;
                            if (mousePos.Y == 28 && mousePos.X > 20 && mousePos.X < 35 && CurrentMod.ModConstructs[ModItemIndex].Container == null) {
                                CurrentMod.ModConstructs[ModItemIndex].Container = new();
                                JustMadeCon = true;
                            }
                            if (mousePos.Y == 28 && mousePos.X > 20 && mousePos.X < 35 && CurrentMod.ModConstructs[ModItemIndex].Container != null && !JustMadeCon)
                                CurrentMod.ModConstructs[ModItemIndex].Container = null;

                            if (mousePos.Y == 31 && mousePos.X >= 45 && mousePos.X <= 49)
                                if (CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count < 5)
                                    CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Add(new());
                            if (mousePos.Y == 31 && mousePos.X >= 51 && mousePos.X <= 55)
                                if (CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count > 0)
                                    CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.RemoveAt(CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count - 1);
                        
                            if (mousePos.Y == 48 && mousePos.X > 20 && mousePos.X < 30 && ModItemIndex > 0) { 
                                ModItemIndex--;
                            }

                            if (mousePos.Y == 48 && mousePos.X > 85) { 
                                if (ModItemIndex + 1 >= CurrentMod.ModConstructs.Count) {
                                    CurrentMod.ModConstructs.Add(new());
                                }
                                ModItemIndex++;
                            }
                        }
                    
                        if (ModSubSelect == "Recipe") {
                            if (mousePos.Y == 1 && mousePos.X > 20 && mousePos.X < 40)
                                SelectedField = 2;
                            if (mousePos.Y == 2 && mousePos.X > 20 && mousePos.X < 40)
                                SelectedField = 3;
                            if (mousePos.Y == 5 && mousePos.X > 20 && mousePos.X < 40)
                                SelectedField = 4;

                            if (mousePos.X > 20 && mousePos.X < 49) {
                                if (mousePos.Y >= 15 && mousePos.Y <= 19) {
                                    SelectedField = mousePos.Y - 10;
                                }
                            } 

                            if (mousePos.X > 20 && mousePos.X < 49) {
                                if (mousePos.Y >= 23 && mousePos.Y <= 27) {
                                    SelectedField = mousePos.Y - 13;
                                }
                            } 

                            if (mousePos.X > 20 && mousePos.X < 49) {
                                if (mousePos.Y >= 31 && mousePos.Y <= 35) {
                                    SelectedField = mousePos.Y - 16;
                                }
                            }


                            if (mousePos.Y == 14 && mousePos.X >= 37 && mousePos.X <= 41 && CurrentMod.ModRecipes[ModItemIndex].RequiredTools.Count < 5)
                                CurrentMod.ModRecipes[ModItemIndex].RequiredTools.Add(new());
                            if (mousePos.Y == 14 && mousePos.X >= 43 && mousePos.X <= 47 && CurrentMod.ModRecipes[ModItemIndex].RequiredTools.Count > 0) {
                                CurrentMod.ModRecipes[ModItemIndex].RequiredTools.RemoveAt(CurrentMod.ModRecipes[ModItemIndex].RequiredTools.Count - 1);
                                SelectedField = 0;
                            }

                            if (mousePos.Y == 22 && mousePos.X >= 40 && mousePos.X <= 44 && CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count < 5)
                                CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Add(new());
                            if (mousePos.Y == 22 && mousePos.X >= 46 && mousePos.X <= 50 && CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count > 0) {
                                CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.RemoveAt(CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count - 1);
                                SelectedField = 0;
                            }

                            if (mousePos.Y == 30 && mousePos.X >= 41 && mousePos.X <= 45 && CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials.Count < 5)
                                CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials.Add(new());
                            if (mousePos.Y == 30 && mousePos.X >= 47 && mousePos.X <= 51 && CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials.Count > 0) {
                                CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials.RemoveAt(CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials.Count - 1);
                                SelectedField = 0;
                            }

                            if (mousePos.Y == 9 && mousePos.X > 20)
                                CurrentMod.ModRecipes[ModItemIndex].HasQuality = !CurrentMod.ModRecipes[ModItemIndex].HasQuality;

                            if (mousePos.Y == 10 && mousePos.X > 20)
                                CurrentMod.ModRecipes[ModItemIndex].WeightBasedOutput = !CurrentMod.ModRecipes[ModItemIndex].WeightBasedOutput;

                           
                            if (mousePos.Y == 48 && mousePos.X > 20 && mousePos.X < 30 && ModItemIndex > 0) {
                                ModItemIndex--;
                            }

                            if (mousePos.Y == 48 && mousePos.X > 85) {
                                if (ModItemIndex + 1 >= CurrentMod.ModRecipes.Count) {
                                    CurrentMod.ModRecipes.Add(new());
                                }
                                ModItemIndex++;
                            }
                        }

                        // Fish inputs
                        if (ModSubSelect == "Fish") {
                            if (mousePos.Y == 1 && mousePos.X > 20)
                                SelectedField = 2;
                            if (mousePos.Y == 3 && mousePos.X > 20)
                                SelectedField = 3;
                            if (mousePos.Y == 5 && mousePos.X > 20)
                                SelectedField = 4;
                            if (mousePos.Y == 14 && mousePos.X > 20)
                                SelectedField = 5;
                            if (mousePos.Y == 15 && mousePos.X > 20)
                                SelectedField = 6;

                            if (mousePos.Y == 48 && mousePos.X > 20 && mousePos.X < 30 && ModItemIndex > 0) {
                                ModItemIndex--;
                            }

                            if (mousePos.Y == 48 && mousePos.X > 85) {
                                if (ModItemIndex + 1 >= CurrentMod.ModFish.Count) {
                                    CurrentMod.ModFish.Add(new());
                                }
                                ModItemIndex++;
                            }
                        }

                        // Monster inputs
                        if (ModSubSelect == "Monster") {
                            if (mousePos.Y == 1 && mousePos.X > 20 && mousePos.X < 49)
                                SelectedField = 2;
                            if (mousePos.Y == 3 && mousePos.X > 20 && mousePos.X < 49)
                                SelectedField = 3;
                            if (mousePos.Y == 5 && mousePos.X > 20 && mousePos.X < 49)
                                SelectedField = 4;
                            if (mousePos.Y == 7 && mousePos.X > 20 && mousePos.X < 49)
                                SelectedField = 5;
                            if (mousePos.Y == 9 && mousePos.X > 20 && mousePos.X < 49)
                                SelectedField = 6;

                            if (mousePos.Y == 11 && mousePos.X > 49) {
                                CurrentMod.ModMonsters[ModItemIndex].CanDropEgg = !CurrentMod.ModMonsters[ModItemIndex].CanDropEgg;
                                if (CurrentMod.ModMonsters[ModItemIndex].CanDropEgg) {
                                    CurrentMod.ModMonsters[ModItemIndex].EggData = new();
                                } else {
                                    CurrentMod.ModMonsters[ModItemIndex].EggData = null;
                                }
                            }
                            if (mousePos.Y == 9 && mousePos.X > 49)
                                CurrentMod.ModMonsters[ModItemIndex].AlwaysAggro = !CurrentMod.ModMonsters[ModItemIndex].AlwaysAggro;

                            if (mousePos.Y == 48 && mousePos.X > 20 && mousePos.X < 30 && ModItemIndex > 0) {
                                ModItemIndex--;
                            }

                            if (mousePos.Y == 48 && mousePos.X > 85) {
                                if (ModItemIndex + 1 >= CurrentMod.ModMonsters.Count) {
                                    CurrentMod.ModMonsters.Add(new());
                                }
                                ModItemIndex++;
                            }
                        }

                        if (ModSubSelect == "Skill") {
                            if (mousePos.Y == 1 && mousePos.X > 20 && mousePos.X < 40)
                                SelectedField = 2;

                            if (mousePos.Y == 48 && mousePos.X > 20 && mousePos.X < 30 && ModItemIndex > 0) {
                                ModItemIndex--;
                            }

                            if (mousePos.Y == 48 && mousePos.X > 85) {
                                if (ModItemIndex + 1 >= CurrentMod.ModSkills.Count) {
                                    CurrentMod.ModSkills.Add(new());
                                }
                                ModItemIndex++;
                            }
                        }
                    }

                    if (GameHost.Instance.Mouse.LeftButtonDown) {
                        // Constructible inputs
                        if (ModSubSelect == "Constructible") {
                            if (mousePos == new Point(31, 7))
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundR, 1);
                            if (mousePos == new Point(31, 8))
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundG, 1);
                            if (mousePos == new Point(31, 9))
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundB, 1);
                            if (mousePos == new Point(31, 10)) {
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Glyph, 1);
                                CurrentGlyphIndex = CurrentMod.ModConstructs[ModItemIndex].Glyph;
                            }

                            if (mousePos == new Point(35, 7) && CurrentMod.ModConstructs[ModItemIndex].ForegroundR < 255)
                                AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundR, 1, 255);
                            if (mousePos == new Point(35, 8) && CurrentMod.ModConstructs[ModItemIndex].ForegroundG < 255)
                                AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundG, 1, 255);
                            if (mousePos == new Point(35, 9) && CurrentMod.ModConstructs[ModItemIndex].ForegroundB < 255)
                                AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundB, 1, 255);
                            if (mousePos == new Point(35, 10)) {
                                CurrentMod.ModConstructs[ModItemIndex].Glyph++;
                                CurrentGlyphIndex = CurrentMod.ModConstructs[ModItemIndex].Glyph;
                            }

                            if (CurrentMod.ModConstructs[ModItemIndex].Dec != null) {
                                if (mousePos == new Point(30, 14))
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Dec.R, 1);
                                if (mousePos == new Point(30, 15))
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Dec.G, 1);
                                if (mousePos == new Point(30, 16))
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Dec.B, 1);
                                if (mousePos == new Point(30, 17))
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Dec.A, 1);
                                if (mousePos == new Point(30, 18)) {
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Dec.Glyph, 1);
                                    CurrentGlyphIndex = CurrentMod.ModConstructs[ModItemIndex].Dec.Glyph;
                                }

                                if (mousePos == new Point(34, 14))
                                    AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].Dec.R, 1, 255);
                                if (mousePos == new Point(34, 15))
                                    AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].Dec.G, 1, 255);
                                if (mousePos == new Point(34, 16))
                                    AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].Dec.B, 1, 255);
                                if (mousePos == new Point(34, 17))
                                    AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].Dec.A, 1, 255);
                                if (mousePos == new Point(34, 18)) {
                                    CurrentMod.ModConstructs[ModItemIndex].Dec.Glyph++;
                                    CurrentGlyphIndex = CurrentMod.ModConstructs[ModItemIndex].Dec.Glyph;
                                }
                            }

                            if (mousePos == new Point(37, 22))
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].RequiredLevel, 1);
                            if (mousePos == new Point(40, 22))
                                AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].RequiredLevel, 1, 99);
                            if (mousePos == new Point(34, 23))
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].ExpGranted, 1);
                            if (mousePos == new Point(42, 23))
                                CurrentMod.ModConstructs[ModItemIndex].ExpGranted++;

                            if (CurrentMod.ModConstructs[ModItemIndex].Container != null) {
                                if (mousePos == new Point(31, 29))
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Container.Capacity, 1);
                                if (mousePos == new Point(34, 29))
                                    AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].Container.Capacity, 1, 27);
                            }

                            if (mousePos.X == 61) {
                                if (mousePos.Y >= 32 && mousePos.Y <= 36) {
                                    int slot = mousePos.Y - 32;
                                    if (CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count > slot)
                                        SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[slot].ItemQuantity, 1);
                                }
                            }

                            if (mousePos.X == 65) {
                                if (mousePos.Y >= 32 && mousePos.Y <= 36) {
                                    int slot = mousePos.Y - 32;
                                    if (CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count > slot)
                                        CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[slot].ItemQuantity++;
                                }
                            } 
                        }
                    
                    
                        // Recipe inputs
                        if (ModSubSelect == "Recipe") {
                            if (mousePos == new Point(37, 6))
                                SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].RequiredLevel, 1);
                            if (mousePos == new Point(40, 6))
                                AddWithCap(ref CurrentMod.ModRecipes[ModItemIndex].RequiredLevel, 1, 99);

                            if (mousePos == new Point(34, 7))
                                SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].ExpGranted, 1);
                            if (mousePos == new Point(40, 7))
                                CurrentMod.ModRecipes[ModItemIndex].ExpGranted++;

                            if (mousePos == new Point(45, 3))
                                SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].FinishedQty, 1);
                            if (mousePos == new Point(49, 3))
                                CurrentMod.ModRecipes[ModItemIndex].FinishedQty++;

                            if (mousePos.X == 62 && mousePos.Y >= 15 && mousePos.Y <= 19) {
                                int slot = mousePos.Y - 15;
                                if (CurrentMod.ModRecipes[ModItemIndex].RequiredTools.Count > slot)
                                    SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].RequiredTools[slot].Tier, 1);
                            }

                            if (mousePos.X == 65 && mousePos.Y >= 15 && mousePos.Y <= 19) {
                                int slot = mousePos.Y - 15;
                                if (CurrentMod.ModRecipes[ModItemIndex].RequiredTools.Count > slot)
                                    AddWithCap(ref CurrentMod.ModRecipes[ModItemIndex].RequiredTools[slot].Tier, 1, 99);
                            }

                            if (mousePos.X == 61 && mousePos.Y >= 23 && mousePos.Y <= 27) {
                                int slot = mousePos.Y - 23;
                                if (CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count > slot)
                                    SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[slot].Quantity, 1);
                            }

                            if (mousePos.X == 65 && mousePos.Y >= 23 && mousePos.Y <= 27) {
                                int slot = mousePos.Y - 23;
                                if (CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count > slot)
                                    AddWithCap(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[slot].Quantity, 1, 99);
                            }

                            if (mousePos.X == 76 && mousePos.Y >= 23 && mousePos.Y <= 27) {
                                int slot = mousePos.Y - 23;
                                if (CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count > slot)
                                    SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[slot].Tier, 1);
                            }

                            if (mousePos.X == 79 && mousePos.Y >= 23 && mousePos.Y <= 27) {
                                int slot = mousePos.Y - 23;
                                if (CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count > slot)
                                    AddWithCap(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[slot].Tier, 1, 99);
                            }

                            if (mousePos.X == 61 && mousePos.Y >= 31 && mousePos.Y <= 35) {
                                int slot = mousePos.Y - 31;
                                if (CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials.Count > slot)
                                    SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[slot].ItemQuantity, 1);
                            }

                            if (mousePos.X == 65 && mousePos.Y >= 31 && mousePos.Y <= 35) {
                                int slot = mousePos.Y - 31;
                                if (CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials.Count > slot)
                                    AddWithCap(ref CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[slot].ItemQuantity, 1, 99);
                            }
                        }

                        // Fish inputs
                        if (ModSubSelect == "Fish") {

                            if (mousePos.X == 36) {
                                if (mousePos.Y == 11)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].EarliestTime, 1, 360);
                                if (mousePos.Y == 12)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].LatestTime, 1, 360);
                            }

                            if (mousePos.X == 44) {
                                if (mousePos.Y == 11)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].EarliestTime, 1, 1560);
                                if (mousePos.Y == 12)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].LatestTime, 1, 1560);
                            }

                            if (mousePos.X == 28) {
                                if (mousePos.Y == 18)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].colR, 1);
                                if (mousePos.Y == 19)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].colG, 1);
                                if (mousePos.Y == 20)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].colB, 1);
                                if (mousePos.Y == 21)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].colA, 1);
                                if (mousePos.Y == 22)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].glyph, 1);
                            }

                            if (mousePos.X == 34) {
                                if (mousePos.Y == 18)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].colR, 1, 255);
                                if (mousePos.Y == 19)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].colG, 1, 255);
                                if (mousePos.Y == 20)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].colB, 1, 255);
                                if (mousePos.Y == 21)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].colA, 1, 255);
                                if (mousePos.Y == 22)
                                    CurrentMod.ModFish[ModItemIndex].glyph++;
                            }

                            if (mousePos == new Point(37, 24))
                                SubMinZero(ref CurrentMod.ModFish[ModItemIndex].RequiredLevel, 1);
                            if (mousePos == new Point(42, 24))
                                AddWithCap(ref CurrentMod.ModFish[ModItemIndex].RequiredLevel, 1, 99);

                            if (mousePos == new Point(34, 25))
                                SubMinZero(ref CurrentMod.ModFish[ModItemIndex].GrantedExp, 1);
                            if (mousePos == new Point(40, 25))
                                CurrentMod.ModFish[ModItemIndex].GrantedExp++;

                            if (mousePos == new Point(31, 27))
                                SubMinZero(ref CurrentMod.ModFish[ModItemIndex].Strength, 1);
                            if (mousePos == new Point(36, 27))
                                AddWithCap(ref CurrentMod.ModFish[ModItemIndex].Strength, 1, 99);

                            if (mousePos.X == 35) {
                                if (mousePos.Y == 28)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].FightChance, 1);
                                if (mousePos.Y == 29)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].FightLength, 1);
                            }

                            if (mousePos.X == 40) {
                                if (mousePos.Y == 28)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].FightChance, 1, 99);
                                if (mousePos.Y == 29)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].FightLength, 1, 10);
                            }

                            if (mousePos == new Point(34, 31))
                                SubMinZero(ref CurrentMod.ModFish[ModItemIndex].MaxQuality, 1, 1);
                            if (mousePos == new Point(40, 31))
                                AddWithCap(ref CurrentMod.ModFish[ModItemIndex].MaxQuality, 1, 11);
                        }
                    
                        // Monster inputs
                        if (ModSubSelect == "Monster") {
                            if (mousePos.X == 57) {
                                if (mousePos.Y == 3)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundR, 1);
                                if (mousePos.Y == 4)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundG, 1);
                                if (mousePos.Y == 5)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundB, 1);
                                if (mousePos.Y == 6)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundA, 1);
                                if (mousePos.Y == 7)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].ActorGlyph, 1);
                            }

                            if (mousePos.X == 63) {
                                if (mousePos.Y == 3)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundR, 1, 255);
                                if (mousePos.Y == 4)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundG, 1, 255);
                                if (mousePos.Y == 5)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundB, 1, 255);
                                if (mousePos.Y == 6)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundA, 1, 255);
                                if (mousePos.Y == 7)
                                    CurrentMod.ModMonsters[ModItemIndex].ActorGlyph++;
                            }


                            if (mousePos.X == 35) {
                                if (mousePos.Y == 11)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonConstitution, 1);
                                if (mousePos.Y == 12)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonAttack, 1);
                                if (mousePos.Y == 13)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonStrength, 1);
                                if (mousePos.Y == 14)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonDefense, 1);
                                if (mousePos.Y == 15)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonMagic, 1);
                                if (mousePos.Y == 16)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonRanged, 1);
                            }

                            if (mousePos.X == 40) {
                                if (mousePos.Y == 11)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonConstitution, 1, 255);
                                if (mousePos.Y == 12)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonAttack, 1, 255);
                                if (mousePos.Y == 13)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonStrength, 1, 255);
                                if (mousePos.Y == 14)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonDefense, 1, 255);
                                if (mousePos.Y == 15)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonMagic, 1, 255);
                                if (mousePos.Y == 16)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonRanged, 1, 255); 
                            }
                        }
                    }

                    if (GameHost.Instance.Mouse.RightClicked) { 
                        // Constructible inputs
                        if (ModSubSelect == "Constructible") {
                            if (mousePos == new Point(31, 7))
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundR, 1);
                            if (mousePos == new Point(31, 8))
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundG, 1);
                            if (mousePos == new Point(31, 9))
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundB, 1);
                            if (mousePos == new Point(31, 10)) {
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Glyph, 1);
                                CurrentGlyphIndex = CurrentMod.ModConstructs[ModItemIndex].Glyph;
                            }

                            if (mousePos == new Point(35, 7) && CurrentMod.ModConstructs[ModItemIndex].ForegroundR < 255)
                                AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundR, 1, 255);
                            if (mousePos == new Point(35, 8) && CurrentMod.ModConstructs[ModItemIndex].ForegroundG < 255)
                                AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundG, 1, 255);
                            if (mousePos == new Point(35, 9) && CurrentMod.ModConstructs[ModItemIndex].ForegroundB < 255)
                                AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].ForegroundB, 1, 255);
                            if (mousePos == new Point(35, 10)) {
                                CurrentMod.ModConstructs[ModItemIndex].Glyph++;
                                CurrentGlyphIndex = CurrentMod.ModConstructs[ModItemIndex].Glyph;
                            }

                            if (CurrentMod.ModConstructs[ModItemIndex].Dec != null) {
                                if (mousePos == new Point(30, 14))
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Dec.R, 1);
                                if (mousePos == new Point(30, 15))
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Dec.G, 1);
                                if (mousePos == new Point(30, 16))
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Dec.B, 1);
                                if (mousePos == new Point(30, 17))
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Dec.A, 1);
                                if (mousePos == new Point(30, 18)) {
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Dec.Glyph, 1);
                                    CurrentGlyphIndex = CurrentMod.ModConstructs[ModItemIndex].Dec.Glyph;
                                }

                                if (mousePos == new Point(34, 14))
                                    AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].Dec.R, 1, 255);
                                if (mousePos == new Point(34, 15))
                                    AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].Dec.G, 1, 255);
                                if (mousePos == new Point(34, 16))
                                    AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].Dec.B, 1, 255);
                                if (mousePos == new Point(34, 17))
                                    AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].Dec.A, 1, 255);
                                if (mousePos == new Point(34, 18)) {
                                    CurrentMod.ModConstructs[ModItemIndex].Dec.Glyph++;
                                    CurrentGlyphIndex = CurrentMod.ModConstructs[ModItemIndex].Dec.Glyph;
                                }
                            }

                            if (mousePos == new Point(37, 22))
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].RequiredLevel, 1);
                            if (mousePos == new Point(40, 22))
                                AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].RequiredLevel, 1, 99);
                            if (mousePos == new Point(34, 23))
                                SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].ExpGranted, 1);
                            if (mousePos == new Point(42, 23))
                                CurrentMod.ModConstructs[ModItemIndex].ExpGranted++;

                            if (CurrentMod.ModConstructs[ModItemIndex].Container != null) {
                                if (mousePos == new Point(31, 29))
                                    SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].Container.Capacity, 1);
                                if (mousePos == new Point(34, 29))
                                    AddWithCap(ref CurrentMod.ModConstructs[ModItemIndex].Container.Capacity, 1, 27);
                            }

                            if (mousePos.X == 61) {
                                if (mousePos.Y >= 32 && mousePos.Y <= 36) {
                                    int slot = mousePos.Y - 32;
                                    if (CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count > slot)
                                        SubMinZero(ref CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[slot].ItemQuantity, 1);
                                }
                            }

                            if (mousePos.X == 65) {
                                if (mousePos.Y >= 32 && mousePos.Y <= 36) {
                                    int slot = mousePos.Y - 32;
                                    if (CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded.Count > slot) 
                                        CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[slot].ItemQuantity++;
                                }
                            }
                        }

                        // Recipe inputs
                        if (ModSubSelect == "Recipe") {
                            if (mousePos == new Point(37, 6))
                                SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].RequiredLevel, 1);
                            if (mousePos == new Point(40, 6))
                                AddWithCap(ref CurrentMod.ModRecipes[ModItemIndex].RequiredLevel, 1, 99);

                            if (mousePos == new Point(34, 7))
                                SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].ExpGranted, 1);
                            if (mousePos == new Point(40, 7))
                                CurrentMod.ModRecipes[ModItemIndex].ExpGranted++;

                            if (mousePos == new Point(45, 3))
                                SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].FinishedQty, 1);
                            if (mousePos == new Point(49, 3))
                                CurrentMod.ModRecipes[ModItemIndex].FinishedQty++;

                            if (mousePos.X == 62 && mousePos.Y >= 15 && mousePos.Y <= 19) {
                                int slot = mousePos.Y - 15;
                                if (CurrentMod.ModRecipes[ModItemIndex].RequiredTools.Count > slot)
                                    SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].RequiredTools[slot].Tier, 1);
                            }

                            if (mousePos.X == 65 && mousePos.Y >= 15 && mousePos.Y <= 19) {
                                int slot = mousePos.Y - 15;
                                if (CurrentMod.ModRecipes[ModItemIndex].RequiredTools.Count > slot)
                                    AddWithCap(ref CurrentMod.ModRecipes[ModItemIndex].RequiredTools[slot].Tier, 1, 99);
                            }

                            if (mousePos.X == 61 && mousePos.Y >= 23 && mousePos.Y <= 27) {
                                int slot = mousePos.Y - 23;
                                if (CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count > slot)
                                    SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[slot].Quantity, 1);
                            }

                            if (mousePos.X == 65 && mousePos.Y >= 23 && mousePos.Y <= 27) {
                                int slot = mousePos.Y - 23;
                                if (CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count > slot)
                                    AddWithCap(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[slot].Quantity, 1, 99);
                            }

                            if (mousePos.X == 76 && mousePos.Y >= 23 && mousePos.Y <= 27) {
                                int slot = mousePos.Y - 23;
                                if (CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count > slot)
                                    SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[slot].Tier, 1);
                            }

                            if (mousePos.X == 79 && mousePos.Y >= 23 && mousePos.Y <= 27) {
                                int slot = mousePos.Y - 23;
                                if (CurrentMod.ModRecipes[ModItemIndex].GenericMaterials.Count > slot)
                                    AddWithCap(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[slot].Tier, 1, 99);
                            }


                            if (mousePos.X == 61 && mousePos.Y >= 31 && mousePos.Y <= 35) {
                                int slot = mousePos.Y - 31;
                                if (CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials.Count > slot)
                                    SubMinZero(ref CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[slot].ItemQuantity, 1);
                            }

                            if (mousePos.X == 65 && mousePos.Y >= 31 && mousePos.Y <= 35) {
                                int slot = mousePos.Y - 31;
                                if (CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials.Count > slot)
                                    AddWithCap(ref CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[slot].ItemQuantity, 1, 99);
                            }
                        }


                        // Fish inputs
                        if (ModSubSelect == "Fish") {

                            if (mousePos.X == 36) {
                                if (mousePos.Y == 11)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].EarliestTime, 1, 360);
                                if (mousePos.Y == 12)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].LatestTime, 1, 360);
                            }

                            if (mousePos.X == 44) {
                                if (mousePos.Y == 11)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].EarliestTime, 1, 1560);
                                if (mousePos.Y == 12)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].LatestTime, 1, 1560);
                            }

                            if (mousePos.X == 28) {
                                if (mousePos.Y == 18)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].colR, 1);
                                if (mousePos.Y == 19)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].colG, 1);
                                if (mousePos.Y == 20)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].colB, 1);
                                if (mousePos.Y == 21)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].colA, 1);
                                if (mousePos.Y == 22)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].glyph, 1);
                            }

                            if (mousePos.X == 34) {
                                if (mousePos.Y == 18)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].colR, 1, 255);
                                if (mousePos.Y == 19)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].colG, 1, 255);
                                if (mousePos.Y == 20)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].colB, 1, 255);
                                if (mousePos.Y == 21)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].colA, 1, 255);
                                if (mousePos.Y == 22)
                                    CurrentMod.ModFish[ModItemIndex].glyph++;
                            }

                            if (mousePos == new Point(37, 24))
                                SubMinZero(ref CurrentMod.ModFish[ModItemIndex].RequiredLevel, 1);
                            if (mousePos == new Point(42, 24))
                                AddWithCap(ref CurrentMod.ModFish[ModItemIndex].RequiredLevel, 1, 99);

                            if (mousePos == new Point(34, 25))
                                SubMinZero(ref CurrentMod.ModFish[ModItemIndex].GrantedExp, 1);
                            if (mousePos == new Point(40, 25))
                                CurrentMod.ModFish[ModItemIndex].GrantedExp++;

                            if (mousePos == new Point(31, 27))
                                SubMinZero(ref CurrentMod.ModFish[ModItemIndex].Strength, 1);
                            if (mousePos == new Point(36, 27))
                                AddWithCap(ref CurrentMod.ModFish[ModItemIndex].Strength, 1, 99);

                            if (mousePos.X == 35) {
                                if (mousePos.Y == 28)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].FightChance, 1);
                                if (mousePos.Y == 29)
                                    SubMinZero(ref CurrentMod.ModFish[ModItemIndex].FightLength, 1);
                            }

                            if (mousePos.X == 40) {
                                if (mousePos.Y == 28)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].FightChance, 1, 99);
                                if (mousePos.Y == 29)
                                    AddWithCap(ref CurrentMod.ModFish[ModItemIndex].FightLength, 1, 10);
                            }

                            if (mousePos == new Point(34, 31))
                                SubMinZero(ref CurrentMod.ModFish[ModItemIndex].MaxQuality, 1, 1);
                            if (mousePos == new Point(40, 31))
                                AddWithCap(ref CurrentMod.ModFish[ModItemIndex].MaxQuality, 1, 11);
                        }

                        // Monster inputs
                        if (ModSubSelect == "Monster") {
                            if (mousePos.X == 57) {
                                if (mousePos.Y == 3)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundR, 1);
                                if (mousePos.Y == 4)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundG, 1);
                                if (mousePos.Y == 5)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundB, 1);
                                if (mousePos.Y == 6)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundA, 1);
                                if (mousePos.Y == 7)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].ActorGlyph, 1);
                            }

                            if (mousePos.X == 63) {
                                if (mousePos.Y == 3)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundR, 1, 255);
                                if (mousePos.Y == 4)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundG, 1, 255);
                                if (mousePos.Y == 5)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundB, 1, 255);
                                if (mousePos.Y == 6)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].ForegroundA, 1, 255);
                                if (mousePos.Y == 7)
                                    CurrentMod.ModMonsters[ModItemIndex].ActorGlyph++;
                            }


                            if (mousePos.X == 35) {
                                if (mousePos.Y == 11)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonConstitution, 1);
                                if (mousePos.Y == 12)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonAttack, 1);
                                if (mousePos.Y == 13)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonStrength, 1);
                                if (mousePos.Y == 14)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonDefense, 1);
                                if (mousePos.Y == 15)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonMagic, 1);
                                if (mousePos.Y == 16)
                                    SubMinZero(ref CurrentMod.ModMonsters[ModItemIndex].MonRanged, 1);
                            }

                            if (mousePos.X == 40) {
                                if (mousePos.Y == 11)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonConstitution, 1, 255);
                                if (mousePos.Y == 12)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonAttack, 1, 255);
                                if (mousePos.Y == 13)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonStrength, 1, 255);
                                if (mousePos.Y == 14)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonDefense, 1, 255);
                                if (mousePos.Y == 15)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonMagic, 1, 255);
                                if (mousePos.Y == 16)
                                    AddWithCap(ref CurrentMod.ModMonsters[ModItemIndex].MonRanged, 1, 255);
                            }
                        }
                    }
                }
            }
        }

        public void RemoveOneCharacter(ref string input) {
            if (input.Length > 0)
                input = input[0..^1];
        }

        public void AddWithCap(ref int input, int add, int cap) {
            input += add;
            if (input > cap)
                input = cap;
        }

        public void SubMinZero(ref int input, int sub, int min = 0) {
            input -= sub;
            if (input < min)
                input = min;
        }

        public void SaveMod() {
            if (CurrentMod != null && CurrentMod.Name != "" && CurrentMod.Package != "") {
                for (int i = 0; i < CurrentMod.ModFish.Count; i++) {
                    CurrentMod.ModFish[i].PackageID = CurrentMod.Package;
                }

                for (int i = 0; i < CurrentMod.ModMonsters.Count; i++) {
                    CurrentMod.ModMonsters[i].Package = CurrentMod.Package;
                }



                string path = "./mods/" + CurrentMod.Name + ".dat";
                using StreamWriter output = new StreamWriter(path);
                string jsonString = JsonConvert.SerializeObject(CurrentMod, Formatting.Indented);
                output.WriteLine(jsonString);
                output.Close();
            }
        }


        public void RunInput(string add, bool backspace) {
            if (CurrentMod != null) {
                if (backspace) {
                    if (SelectedField == 0)
                        RemoveOneCharacter(ref CurrentMod.Name);
                    if (SelectedField == 1)
                        RemoveOneCharacter(ref CurrentMod.Package);

                    if (ModSubSelect == "Constructible") {
                        if (SelectedField == 2)
                            RemoveOneCharacter(ref CurrentMod.ModConstructs[ModItemIndex].Name);
                        if (SelectedField == 3)
                            RemoveOneCharacter(ref CurrentMod.ModConstructs[ModItemIndex].Materials);
                        if (SelectedField == 4)
                            RemoveOneCharacter(ref CurrentMod.ModConstructs[ModItemIndex].SpecialProps);
                        if (SelectedField == 5)
                            RemoveOneCharacter(ref CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[0].Name);
                        if (SelectedField == 6)
                            RemoveOneCharacter(ref CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[1].Name);
                        if (SelectedField == 7)
                            RemoveOneCharacter(ref CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[2].Name);
                        if (SelectedField == 8)
                            RemoveOneCharacter(ref CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[3].Name);
                        if (SelectedField == 9)
                            RemoveOneCharacter(ref CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[4].Name);
                    }

                    if (ModSubSelect == "Recipe") {
                        if (SelectedField == 2)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].Name);
                        if (SelectedField == 3)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].FinishedID);
                        if (SelectedField == 4)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].Skill);
                        if (SelectedField == 5)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].RequiredTools[0].Property);
                        if (SelectedField == 6)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].RequiredTools[1].Property);
                        if (SelectedField == 7)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].RequiredTools[2].Property);
                        if (SelectedField == 8)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].RequiredTools[3].Property);
                        if (SelectedField == 9)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].RequiredTools[4].Property);
                        if (SelectedField == 10)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[0].Property);
                        if (SelectedField == 11)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[1].Property);
                        if (SelectedField == 12)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[2].Property);
                        if (SelectedField == 13)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[3].Property);
                        if (SelectedField == 14)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[4].Property);
                        if (SelectedField == 15)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[0].Name);
                        if (SelectedField == 16)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[1].Name);
                        if (SelectedField == 17)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[2].Name);
                        if (SelectedField == 18)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[3].Name);
                        if (SelectedField == 19)
                            RemoveOneCharacter(ref CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[4].Name);
                    }

                    if (ModSubSelect == "Fish") {
                        if (SelectedField == 2)
                            RemoveOneCharacter(ref CurrentMod.ModFish[ModItemIndex].Name);
                        if (SelectedField == 3)
                            RemoveOneCharacter(ref CurrentMod.ModFish[ModItemIndex].Season);
                        if (SelectedField == 4)
                            RemoveOneCharacter(ref CurrentMod.ModFish[ModItemIndex].CatchLocation);
                        if (SelectedField == 5)
                            RemoveOneCharacter(ref CurrentMod.ModFish[ModItemIndex].FishItemID);
                        if (SelectedField == 6)
                            RemoveOneCharacter(ref CurrentMod.ModFish[ModItemIndex].FilletID);
                    }

                    if (ModSubSelect == "Monster") {
                        if (SelectedField == 2)
                            RemoveOneCharacter(ref CurrentMod.ModMonsters[ModItemIndex].Name);
                        if (SelectedField == 3)
                            RemoveOneCharacter(ref CurrentMod.ModMonsters[ModItemIndex].CombatType);
                        if (SelectedField == 4)
                            RemoveOneCharacter(ref CurrentMod.ModMonsters[ModItemIndex].DamageType);
                        if (SelectedField == 5)
                            RemoveOneCharacter(ref CurrentMod.ModMonsters[ModItemIndex].SpecificWeakness);
                        if (SelectedField == 6)
                            RemoveOneCharacter(ref CurrentMod.ModMonsters[ModItemIndex].SpawnLocation);
                        if (SelectedField == 7 && CurrentMod.ModMonsters[ModItemIndex].DropTable[0].Length > 0)
                            CurrentMod.ModMonsters[ModItemIndex].DropTable[0] = CurrentMod.ModMonsters[ModItemIndex].DropTable[0][0..^1];
                        if (SelectedField == 8 && CurrentMod.ModMonsters[ModItemIndex].DropTable[1].Length > 0)
                            CurrentMod.ModMonsters[ModItemIndex].DropTable[1] = CurrentMod.ModMonsters[ModItemIndex].DropTable[1][0..^1];
                        if (SelectedField == 9 && CurrentMod.ModMonsters[ModItemIndex].DropTable[2].Length > 0)
                            CurrentMod.ModMonsters[ModItemIndex].DropTable[2] = CurrentMod.ModMonsters[ModItemIndex].DropTable[2][0..^1];
                        if (SelectedField == 10 && CurrentMod.ModMonsters[ModItemIndex].DropTable[3].Length > 0)
                            CurrentMod.ModMonsters[ModItemIndex].DropTable[3] = CurrentMod.ModMonsters[ModItemIndex].DropTable[3][0..^1];
                        if (SelectedField == 11 && CurrentMod.ModMonsters[ModItemIndex].DropTable[4].Length > 0)
                            CurrentMod.ModMonsters[ModItemIndex].DropTable[4] = CurrentMod.ModMonsters[ModItemIndex].DropTable[4][0..^1];
                    }

                    if (ModSubSelect == "Skill") {
                        if (SelectedField == 2)
                            RemoveOneCharacter(ref CurrentMod.ModSkills[ModItemIndex].Name);
                    } 
                } else {
                    if (SelectedField == 0)
                        CurrentMod.Name += add;
                    if (SelectedField == 1)
                        CurrentMod.Package += add;

                    if (ModSubSelect == "Constructible") {
                        if (SelectedField == 2)
                            CurrentMod.ModConstructs[ModItemIndex].Name += add;
                        if (SelectedField == 3)
                            CurrentMod.ModConstructs[ModItemIndex].Materials += add;
                        if (SelectedField == 4)
                            CurrentMod.ModConstructs[ModItemIndex].SpecialProps += add;
                        if (SelectedField == 5)
                            CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[0].Name += add;
                        if (SelectedField == 6)
                            CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[1].Name += add;
                        if (SelectedField == 7)
                            CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[2].Name += add;
                        if (SelectedField == 8)
                            CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[3].Name += add;
                        if (SelectedField == 9)
                            CurrentMod.ModConstructs[ModItemIndex].MaterialsNeeded[4].Name += add;
                    }

                    if (ModSubSelect == "Recipe") {
                        if (SelectedField == 2)
                            CurrentMod.ModRecipes[ModItemIndex].Name += add;
                        if (SelectedField == 3)
                            CurrentMod.ModRecipes[ModItemIndex].FinishedID += add;
                        if (SelectedField == 4)
                            CurrentMod.ModRecipes[ModItemIndex].Skill += add;
                        if (SelectedField == 5)
                            CurrentMod.ModRecipes[ModItemIndex].RequiredTools[0].Property += add;
                        if (SelectedField == 6)
                            CurrentMod.ModRecipes[ModItemIndex].RequiredTools[1].Property += add;
                        if (SelectedField == 7)
                            CurrentMod.ModRecipes[ModItemIndex].RequiredTools[2].Property += add;
                        if (SelectedField == 8)
                            CurrentMod.ModRecipes[ModItemIndex].RequiredTools[3].Property += add;
                        if (SelectedField == 9)
                            CurrentMod.ModRecipes[ModItemIndex].RequiredTools[4].Property += add;
                        if (SelectedField == 10)
                            CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[0].Property += add;
                        if (SelectedField == 11)
                            CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[1].Property += add;
                        if (SelectedField == 12)
                            CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[2].Property += add;
                        if (SelectedField == 13)
                            CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[3].Property += add;
                        if (SelectedField == 14)
                            CurrentMod.ModRecipes[ModItemIndex].GenericMaterials[4].Property += add;
                        if (SelectedField == 15)
                            CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[0].Name += add;
                        if (SelectedField == 16)
                            CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[1].Name += add;
                        if (SelectedField == 17)
                            CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[2].Name += add;
                        if (SelectedField == 18)
                            CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[3].Name += add;
                        if (SelectedField == 19)
                            CurrentMod.ModRecipes[ModItemIndex].SpecificMaterials[4].Name += add;
                    }

                    if (ModSubSelect == "Fish") {
                        if (SelectedField == 2)
                            CurrentMod.ModFish[ModItemIndex].Name += add;
                        if (SelectedField == 3)
                            CurrentMod.ModFish[ModItemIndex].Season += add;
                        if (SelectedField == 4)
                            CurrentMod.ModFish[ModItemIndex].CatchLocation += add;
                        if (SelectedField == 5)
                            CurrentMod.ModFish[ModItemIndex].FishItemID += add;
                        if (SelectedField == 6)
                            CurrentMod.ModFish[ModItemIndex].FilletID += add;
                    }


                    if (ModSubSelect == "Monster") {
                        if (SelectedField == 2)
                            CurrentMod.ModMonsters[ModItemIndex].Name += add;
                        if (SelectedField == 3)
                            CurrentMod.ModMonsters[ModItemIndex].CombatType += add;
                        if (SelectedField == 4)
                            CurrentMod.ModMonsters[ModItemIndex].DamageType += add;
                        if (SelectedField == 5)
                            CurrentMod.ModMonsters[ModItemIndex].SpecificWeakness += add;
                        if (SelectedField == 6)
                            CurrentMod.ModMonsters[ModItemIndex].SpawnLocation += add;
                        if (SelectedField == 7)
                            CurrentMod.ModMonsters[ModItemIndex].DropTable[0] += add;
                        if (SelectedField == 8)
                            CurrentMod.ModMonsters[ModItemIndex].DropTable[1] += add;
                        if (SelectedField == 9)
                            CurrentMod.ModMonsters[ModItemIndex].DropTable[2] += add;
                        if (SelectedField == 10)
                            CurrentMod.ModMonsters[ModItemIndex].DropTable[3] += add;
                        if (SelectedField == 11)
                            CurrentMod.ModMonsters[ModItemIndex].DropTable[4] += add;
                    }

                    if (ModSubSelect == "Skill") {
                        if (SelectedField == 2)
                            CurrentMod.ModSkills[ModItemIndex].Name += add;
                    }

                    /* 
                     * */
                }
            }
        }
    }
}
