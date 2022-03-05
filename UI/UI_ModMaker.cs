using SadRogue.Primitives;
using SadConsole;
using SadConsole.UI;
using SadConsole.Input;
using System.IO;
using System;
using LofiHollow.Managers;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using Newtonsoft.Json;
using LofiHollow.EntityData;
using LofiHollow.Entities;
using LofiHollow.Missions;
using LofiHollow.Entities.NPC;
using LofiHollow.Minigames.Archaeology;
using LofiHollow.Minigames.Picross;

namespace LofiHollow.UI {
    public class UI_ModMaker {
        public string ModMenuSelect = "List";
        public string ModSubSelect = "None";
        public bool CreatingMod = false;
        public int ModItemIndex = 0;
        public int CurrentGlyphIndex = 0;
        public int SelectedField = 0;
        public Mod CurrentMod;
        public ControlsConsole ModConsole;
         
        public int PaintR = 127;
        public int PaintG = 127;
        public int PaintB = 127;

        public bool HostMode = false;

        public UI_ModMaker() {
            ModConsole = new(90, 50);
            ModConsole.Position = new(5, 5); 
            ModConsole.IsVisible = false;
        }

        public void ModMakerClicks(string ID) {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.MainMenu.MenuConsole, GameHost.Instance.Mouse).CellPosition;

            if (ID == "BackToMainMenu") {
                GameLoop.UIManager.selectedMenu = "MainMenu";
                GameLoop.UIManager.MainMenu.MenuConsole.IsVisible = true;
                ModConsole.IsVisible = false;
            }

            else if (ID == "CreateNewMod") {
                ModMenuSelect = "Create";
                ModSubSelect = "Overview";
                CurrentMod = new();
                CreatingMod = true;
            }

            else if (ID == "BackToOverview") {
                ModMenuSelect = "Overview";
                ModSubSelect = "Overview";
                SaveMod();
                CreatingMod = false;
            }

            else if (ID == "SaveMod") { SaveMod(); }

            else if (ID == "Go Constructibles") {
                ModSubSelect = "Constructible";
                ModItemIndex = 0;
                SelectedField = 0;
                if (CurrentMod.ModConstructs.Count == 0)
                    CurrentMod.ModConstructs.Add(new());
            }

            else if (ID == "Go Recipes") {
                ModSubSelect = "Recipe";
                ModItemIndex = 0;
                SelectedField = 0;
                if (CurrentMod.ModRecipes.Count == 0)
                    CurrentMod.ModRecipes.Add(new());
            }

            else if (ID == "Go Items") {
                ModSubSelect = "Item";
                ModItemIndex = 0;
                SelectedField = 0;
                if (CurrentMod.ModItems.Count == 0)
                    CurrentMod.ModItems.Add(new());
            }

            else if (ID == "Go Missions") {
                ModSubSelect = "Mission";
                ModItemIndex = 0;
                SelectedField = 0;
                if (CurrentMod.ModMissions.Count == 0)
                    CurrentMod.ModMissions.Add(new());
            }

            else if (ID == "Go Monsters") {
                ModSubSelect = "Monster";
                ModItemIndex = 0;
                SelectedField = 0;
                if (CurrentMod.ModMonsters.Count == 0)
                    CurrentMod.ModMonsters.Add(new());
            }

            else if (ID == "Go NPCs") {
                ModSubSelect = "NPC";
                ModItemIndex = 0;
                SelectedField = 0;
                if (CurrentMod.ModNPCs.Count == 0)
                    CurrentMod.ModNPCs.Add(new());
            }

            else if (ID == "Go Skills") {
                ModSubSelect = "Skill";
                ModItemIndex = 0;
                SelectedField = 0;
                if (CurrentMod.ModSkills.Count == 0)
                    CurrentMod.ModSkills.Add(new());
            }

            else if (ID == "Go Artifacts") {
                ModSubSelect = "Artifact";
                ModItemIndex = 0;
                SelectedField = 0;
                if (CurrentMod.ModArtifacts.Count == 0)
                    CurrentMod.ModArtifacts.Add(new());
            }

            else if (ID == "Go Picross") {
                ModSubSelect = "Picross";
                ModItemIndex = 0;
                SelectedField = 0;
                if (CurrentMod.ModPicross.Count == 0)
                    CurrentMod.ModPicross.Add(new());
            }

            else if (ID == "BackToList") {
                ModMenuSelect = "List";
                SaveMod();
                CreatingMod = false;
                CurrentMod = null;
            }

            else if (ID == "ToCreateEdit") {
                ModMenuSelect = "Create";
                CreatingMod = true;
            }

            else if (ID == "ToggleMod") {
                CurrentMod.Enabled = !CurrentMod.Enabled;
                SaveMod();
            }

            else if (ID == "OverviewConstructs") { ModSubSelect = "Constructible"; }
            else if (ID == "OverviewRecipes") { ModSubSelect = "Recipe"; }
            else if (ID == "OverviewItems") { ModSubSelect = "Item"; }
            else if (ID == "OverviewMissions") { ModSubSelect = "Mission"; }
            else if (ID == "OverviewMonsters") { ModSubSelect = "Monster"; }
            else if (ID == "OverviewNPCs") { ModSubSelect = "NPC"; }
            else if (ID == "OverviewSkills") { ModSubSelect = "Skill"; }
            else if (ID == "OverviewArtifacts") { ModSubSelect = "Artifact"; }
            else if (ID == "OverviewPicross") { ModSubSelect = "Picross"; }

            else if (ID == "PreviousItem") { if (ModItemIndex > 0) ModItemIndex--; }
            else if (ID == "NextItem") {
                if (ModSubSelect == "Constructible") {
                    if (CurrentMod.ModConstructs.Count < ModItemIndex + 2)
                        CurrentMod.ModConstructs.Add(new());
                    ModItemIndex++;
                }
                else if (ModSubSelect == "Recipe") {
                    if (CurrentMod.ModRecipes.Count < ModItemIndex + 2)
                        CurrentMod.ModRecipes.Add(new());
                    ModItemIndex++;
                }

                else if (ModSubSelect == "Item") {
                    if (CurrentMod.ModItems.Count < ModItemIndex + 2)
                        CurrentMod.ModItems.Add(new());
                    ModItemIndex++;
                }

                else if (ModSubSelect == "Mission") {
                    if (CurrentMod.ModMissions.Count < ModItemIndex + 2)
                        CurrentMod.ModMissions.Add(new());
                    ModItemIndex++;
                }

                else if (ModSubSelect == "Monster") {
                    if (CurrentMod.ModMonsters.Count < ModItemIndex + 2)
                        CurrentMod.ModMonsters.Add(new());
                    ModItemIndex++;
                }

                else if (ModSubSelect == "NPC") {
                    if (CurrentMod.ModNPCs.Count < ModItemIndex + 2)
                        CurrentMod.ModNPCs.Add(new());
                    ModItemIndex++;
                }

                else if (ModSubSelect == "Skill") {
                    if (CurrentMod.ModSkills.Count < ModItemIndex + 2)
                        CurrentMod.ModSkills.Add(new());
                    ModItemIndex++;
                }

                else if (ModSubSelect == "Artifact") {
                    if (CurrentMod.ModArtifacts.Count < ModItemIndex + 2)
                        CurrentMod.ModArtifacts.Add(new());
                    ModItemIndex++;
                }

                else if (ModSubSelect == "Picross") {
                    if (CurrentMod.ModPicross.Count < ModItemIndex + 2)
                        CurrentMod.ModPicross.Add(new());
                    ModItemIndex++;
                }
            }

            else if (ID == "PackMod") { PackMod(); }
            else if (ID == "UnpackMod") { UnpackMod(); }

            else if (ID == "PicrossEasy") {
                CurrentMod.ModPicross[ModItemIndex].Width = 5;
                CurrentMod.ModPicross[ModItemIndex].Height = 5;
                CurrentMod.ModPicross[ModItemIndex].Difficulty = "Easy";
                CurrentMod.ModPicross[ModItemIndex].Grid = new PicrossTile[CurrentMod.ModPicross[ModItemIndex].Width * CurrentMod.ModPicross[ModItemIndex].Height];
            
                for (int i = 0; i < CurrentMod.ModPicross[ModItemIndex].Grid.Length; i++) {
                    CurrentMod.ModPicross[ModItemIndex].Grid[i] = new PicrossTile();
                }
            }

            else if (ID == "PicrossMedium") {
                CurrentMod.ModPicross[ModItemIndex].Width = 10;
                CurrentMod.ModPicross[ModItemIndex].Height = 10;
                CurrentMod.ModPicross[ModItemIndex].Difficulty = "Medium";
                CurrentMod.ModPicross[ModItemIndex].Grid = new PicrossTile[CurrentMod.ModPicross[ModItemIndex].Width * CurrentMod.ModPicross[ModItemIndex].Height];

                for (int i = 0; i < CurrentMod.ModPicross[ModItemIndex].Grid.Length; i++) {
                    CurrentMod.ModPicross[ModItemIndex].Grid[i] = new PicrossTile();
                }
            }

            else if (ID == "PicrossHard") {
                CurrentMod.ModPicross[ModItemIndex].Width = 15;
                CurrentMod.ModPicross[ModItemIndex].Height = 15;
                CurrentMod.ModPicross[ModItemIndex].Difficulty = "Hard";
                CurrentMod.ModPicross[ModItemIndex].Grid = new PicrossTile[CurrentMod.ModPicross[ModItemIndex].Width * CurrentMod.ModPicross[ModItemIndex].Height];

                for (int i = 0; i < CurrentMod.ModPicross[ModItemIndex].Grid.Length; i++) {
                    CurrentMod.ModPicross[ModItemIndex].Grid[i] = new PicrossTile();
                }
            }

            else if (ID == "Upload") {
                SaveMod();
                string[] tags = new string[1];
                tags[0] = "Mod";
               // GameLoop.Workshop.SaveToWorkshop("./mods/" + CurrentMod.Name + ".dat.gz", CurrentMod.ToByteArray(), CurrentMod.Name, "Lofi Hollow Core (Example Mod)", tags);
            }

            else {
                string[] splits = ID.Split(",");

                if (splits[0] == "Load") {
                    int y = 3;
                    foreach (var modPath in Directory.GetFiles("./mods/")) {
                        if (y == Int32.Parse(splits[1])) { // Load when we find the right one 
                            CurrentMod = Helper.DeserializeFromFileCompressed<Mod>(modPath);
                            ModMenuSelect = "Overview";
                            CreatingMod = false;
                            break;
                        }
                        else { // Otherwise keep counting
                            y++;
                        }
                    }
                }
                else if (splits[0] == "Toggle") {
                    int y = 3;
                    foreach (var modPath in Directory.GetFiles("./mods/")) {
                        if (y == Int32.Parse(splits[1])) { // Load when we find the right one 
                            CurrentMod = Helper.DeserializeFromFileCompressed<Mod>(modPath);
                            CurrentMod.Enabled = !CurrentMod.Enabled;
                            SaveMod();
                            break;
                        }
                        else { // Otherwise keep counting
                            y++;
                        }
                    }
                }
            }
        }

        public void DrawMod() {
            ModConsole.Clear();
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.MainMenu.MenuConsole, GameHost.Instance.Mouse).CellPosition;

            ModConsole.DrawBox(new Rectangle(0, 0, 90, 50), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.White, Color.Black)));

            if (ModMenuSelect == "Create") {
                ModConsole.PrintClickable(1, 1, "[BACK]", ModMakerClicks, "BackToOverview");
                ModConsole.PrintClickable(10, 1, "[SAVE]", ModMakerClicks, "SaveMod");

                ModConsole.Print(1, 3, "Name: " + CurrentMod.Name, SelectedField == 0 ? Color.Yellow : Color.White);

                ModConsole.Print(1, 5, "Package: " + CurrentMod.Package, SelectedField == 1 ? Color.Yellow : Color.White);

                ModConsole.PrintClickable(1, 9, "Constructibles", ModMakerClicks, "Go Constructibles");
                ModConsole.PrintClickable(1, 11, "Crafting Recipes", ModMakerClicks, "Go Recipes");
                ModConsole.PrintClickable(1, 13, "Fish", ModMakerClicks, "Go Fish");
                ModConsole.PrintClickable(1, 15, "Items", ModMakerClicks, "Go Items");
                ModConsole.PrintClickable(1, 17, "Missions", ModMakerClicks, "Go Missions");
                ModConsole.PrintClickable(1, 19, "Monsters", ModMakerClicks, "Go Monsters");
                ModConsole.PrintClickable(1, 21, "NPCs", ModMakerClicks, "Go NPCs");
                ModConsole.PrintClickable(1, 23, "Skills", ModMakerClicks, "Go Skills");
                ModConsole.PrintClickable(1, 25, "Artifacts", ModMakerClicks, "Go Artifacts");
                ModConsole.PrintClickable(1, 27, "Picross", ModMakerClicks, "Go Picross");

                if (ModSubSelect == "Overview") {
                    ModConsole.Print(21, 1, "Listen.");
                    ModConsole.Print(21, 2, "I tried to make a whole built-in mod thing, I really did.");
                    ModConsole.Print(21, 3, "But there's so much data attached to everything that ");
                    ModConsole.Print(21, 4, "it's just not feasible to have all of it packed in here.");
                    ModConsole.Print(21, 6, "So instead, this menu is mostly documentation for how to");
                    ModConsole.Print(21, 7, "define each type of data, and then tools to pack it up.");
                }


                if (ModSubSelect == "Constructible") {

                }


                if (ModSubSelect == "Recipe") {

                }

                if (ModSubSelect == "Fish") {

                }

                if (ModSubSelect == "Item") {
                    ModConsole.Print(21, 1, "Item");
                }

                if (ModSubSelect == "Mission") {
                    ModConsole.Print(21, 1, "Mission");
                }

                if (ModSubSelect == "Monster") {

                }

                if (ModSubSelect == "NPC") {
                    ModConsole.Print(21, 1, "NPC");
                }

                if (ModSubSelect == "Skill") {
                    ModConsole.Print(21, 1, "Name: " + CurrentMod.ModSkills[ModItemIndex].Name, SelectedField == 2 ? Color.Yellow : Color.White);

                }

                if (ModSubSelect == "Artifact") {
                    ModConsole.Print(22, 1, "You'll have to edit a lot of this data externally too.");
                    ModConsole.Print(22, 3, "Name: " + CurrentMod.ModArtifacts[ModItemIndex].Name, SelectedField == 2 ? Color.Yellow : Color.White);
                    ModConsole.Print(22, 5, "Glyph: " + CurrentGlyphIndex.AsString(), new Color(PaintR, PaintG, PaintB));
                    ModConsole.Print(22, 7, "ForeR: " + PaintR);
                    ModConsole.Print(22, 9, "ForeG: " + PaintG);
                    ModConsole.Print(22, 11, "ForeB: " + PaintB);

                    ModConsole.DrawLine(new Point(49, 5), new Point(49, 34), 179, Color.White);
                    ModConsole.DrawLine(new Point(80, 5), new Point(80, 34), 179, Color.White);
                    ModConsole.DrawLine(new Point(50, 4), new Point(79, 4), 196, Color.White);
                    ModConsole.DrawLine(new Point(50, 35), new Point(79, 35), 196, Color.White);


                    for (int x = 0; x < 30; x++) {
                        for (int y = 0; y < 30; y++) {
                            ModConsole.Print(50 + x, 5 + y, CurrentMod.ModArtifacts[ModItemIndex].Tiles[x + (y * 30)].GetAppearance());
                        }
                    }

                    for (int x = 0; x < 16; x++) {
                        for (int y = 0; y < 31; y++) {
                            int index = x + (y * 16);
                            Color col = index == CurrentGlyphIndex ? Color.Lime : mousePos == new Point(22 + x, 15 + y) ? Color.Yellow : Color.White;
                            ModConsole.PrintClickable(22 + x, 15 + y, new ColoredString(index.AsString(), col, Color.Black), SetGlyph, index.ToString());
                        }
                    }
                }

                if (ModSubSelect == "Picross") {
                    ModConsole.Print(22, 1, "Clues are generated automatically during play.");
                    ModConsole.Print(22, 3, "Name: " + CurrentMod.ModPicross[ModItemIndex].Name, SelectedField == 2 ? Color.Yellow : Color.White);
                    ModConsole.Print(22, 5, "Difficulty: ");

                    string diff = CurrentMod.ModPicross[ModItemIndex].Difficulty;
                    ModConsole.PrintClickable(34, 5, new ColoredString("E", diff == "Easy" ? Color.Lime : Color.White, Color.Black), ModMakerClicks, "PicrossEasy");
                    ModConsole.PrintClickable(36, 5, new ColoredString("M", diff == "Medium" ? Color.Yellow : Color.White, Color.Black), ModMakerClicks, "PicrossMedium");
                    ModConsole.PrintClickable(38, 5, new ColoredString("H", diff == "Hard" ? Color.Red : Color.White, Color.Black), ModMakerClicks, "PicrossHard");

                    ModConsole.Print(22, 7, "ForeR: " + PaintR);
                    ModConsole.Print(22, 9, "ForeG: " + PaintG);
                    ModConsole.Print(22, 11, "ForeB: " + PaintB);

                    ModConsole.DrawLine(new Point(49, 5), new Point(49, 5 + CurrentMod.ModPicross[ModItemIndex].Height - 1), 179, Color.White);
                    ModConsole.DrawLine(new Point(50 + CurrentMod.ModPicross[ModItemIndex].Width, 5), new Point(50 + CurrentMod.ModPicross[ModItemIndex].Width, 5 + CurrentMod.ModPicross[ModItemIndex].Height - 1), 179, Color.White);
                    ModConsole.DrawLine(new Point(50, 4), new Point(50 + CurrentMod.ModPicross[ModItemIndex].Width - 1, 4), 196, Color.White);
                    ModConsole.DrawLine(new Point(50, 5 + CurrentMod.ModPicross[ModItemIndex].Height), new Point(50 + CurrentMod.ModPicross[ModItemIndex].Width - 1, 5 + CurrentMod.ModPicross[ModItemIndex].Height), 196, Color.White);

                    for (int x = 0; x < CurrentMod.ModPicross[ModItemIndex].Width; x++) {
                        for (int y = 0; y < CurrentMod.ModPicross[ModItemIndex].Height; y++) {
                            ModConsole.Print(50 + x, 5 + y, CurrentMod.ModPicross[ModItemIndex].Grid[x + (y * CurrentMod.ModPicross[ModItemIndex].Width)].GetAppearance(true));
                        }
                    }
                }


                ModConsole.PrintClickable(1, 48, "[PACK]", ModMakerClicks, "PackMod");
                ModConsole.PrintClickable(11, 48, "[UNPACK]", ModMakerClicks, "UnpackMod");

              //  ModConsole.PrintClickable(1, 46, "[UPLOAD TO WORKSHOP]", ModMakerClicks, "Upload");


                ModConsole.PrintClickable(21, 48, "Previous", ModMakerClicks, "PreviousItem");
                ModConsole.PrintClickable(85, 48, "Next", ModMakerClicks, "NextItem");
            }

            if (ModMenuSelect == "List") {
                ModConsole.PrintClickable(1, 1, "[BACK]", ModMakerClicks, "BackToMainMenu");
                ModConsole.PrintClickable(10, 1, "[NEW]", ModMakerClicks, "CreateNewMod");

                if (!Directory.Exists("./mods/"))
                    Directory.CreateDirectory("./mods/");

                ModConsole.Print(21, 1, "Mod Name".Align(HorizontalAlignment.Center, 20) + "|" + "Enabled".Align(HorizontalAlignment.Center, 9));
                ModConsole.DrawLine(new Point(21, 2), new Point(88, 2), 196, Color.White, Color.Black);

                int y = 3;

                foreach (var modPath in Directory.GetFiles("./mods/")) {
                    Mod mod = Helper.DeserializeFromFileCompressed<Mod>(modPath);

                    ColoredString modEntry = new("", Color.White, Color.Black);

                    ColoredString check = new ColoredString(4.AsString().Align(HorizontalAlignment.Center, 9), Color.Lime, Color.Black);

                    if (!mod.Enabled)
                        check = new ColoredString("x".Align(HorizontalAlignment.Center, 9), Color.Red, Color.Black);

                    modEntry += (" ".Align(HorizontalAlignment.Center, 20));
                    modEntry += "|";
                    modEntry += check;

                    ModConsole.Print(21, y, modEntry);

                    ModConsole.PrintClickable(21, y, mod.Name.Align(HorizontalAlignment.Center, 20), ModMakerClicks, "Load," + y);

                    y++;
                }
            }

            if (ModMenuSelect == "Overview") {
                ModConsole.PrintClickable(1, 1, "[BACK]", ModMakerClicks, "BackToList");
                ModConsole.PrintClickable(10, 1, "[EDIT]", ModMakerClicks, "ToCreateEdit");

                if (CurrentMod != null) {
                    ModConsole.Print(1, 3, CurrentMod.Package + ":" + CurrentMod.Name);
                    if (CurrentMod.ModConstructs.Count > 0)
                        ModConsole.PrintClickable(1, 6, "Constructibles", ModMakerClicks, "OverviewConstructs");
                    else
                        ModConsole.Print(1, 6, new ColoredString("Constructibles", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModRecipes.Count > 0)
                        ModConsole.PrintClickable(1, 8, "Recipes", ModMakerClicks, "OverviewRecipes");
                    else
                        ModConsole.Print(1, 8, new ColoredString("Crafting Recipes", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModItems.Count > 0)
                        ModConsole.PrintClickable(1, 12, "Items", ModMakerClicks, "OverviewItems");
                    else
                        ModConsole.Print(1, 12, new ColoredString("Items", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModMissions.Count > 0)
                        ModConsole.PrintClickable(1, 14, "Missions", ModMakerClicks, "OverviewMissions");
                    else
                        ModConsole.Print(1, 14, new ColoredString("Missions", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModMonsters.Count > 0)
                        ModConsole.PrintClickable(1, 16, "Monsters", ModMakerClicks, "OverviewMonsters");
                    else
                        ModConsole.Print(1, 16, new ColoredString("Monsters", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModNPCs.Count > 0)
                        ModConsole.PrintClickable(1, 18, "NPCs", ModMakerClicks, "OverviewNPCs");
                    else
                        ModConsole.Print(1, 18, new ColoredString("NPCs", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModSkills.Count > 0)
                        ModConsole.PrintClickable(1, 20, "Skills", ModMakerClicks, "OverviewSkills");
                    else
                        ModConsole.Print(1, 20, new ColoredString("Skills", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModArtifacts.Count > 0)
                        ModConsole.PrintClickable(1, 22, "Artifacts", ModMakerClicks, "OverviewArtifacts");
                    else
                        ModConsole.Print(1, 22, new ColoredString("Artifacts", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModPicross.Count > 0)
                        ModConsole.PrintClickable(1, 24, "Picross", ModMakerClicks, "OverviewPicross");
                    else
                        ModConsole.Print(1, 24, new ColoredString("Picross", Color.DarkSlateGray, Color.Black));

                    if (ModSubSelect == "None") {
                        ModConsole.Print(21, 1, "Mod   Name: " + CurrentMod.Name);
                        ModConsole.Print(21, 3, "Mod Prefix: " + CurrentMod.Package);

                        ColoredString check = new ColoredString(4.AsString(), Color.Lime, Color.Black);
                        if (!CurrentMod.Enabled)
                            check = new ColoredString("x", Color.Red, Color.Black);
                        ModConsole.Print(21, 5, "Enabled: ");
                        ModConsole.PrintClickable(30, 5, check, ModMakerClicks, "ToggleMod");

                        ModConsole.Print(21, 8, "Constructibles Added: " + CurrentMod.ModConstructs.Count);
                        ModConsole.Print(21, 10, "Crafting Recipes Added: " + CurrentMod.ModRecipes.Count);
                        ModConsole.Print(21, 14, "Items Added: " + CurrentMod.ModItems.Count);
                        ModConsole.Print(21, 16, "Missions Added: " + CurrentMod.ModMissions.Count);
                        ModConsole.Print(21, 18, "Monsters Added: " + CurrentMod.ModMonsters.Count);
                        ModConsole.Print(21, 20, "NPCs Added: " + CurrentMod.ModNPCs.Count);
                        ModConsole.Print(21, 22, "Skills Added: " + CurrentMod.ModSkills.Count);
                        ModConsole.Print(21, 24, "Artifacts Added: " + CurrentMod.ModArtifacts.Count);
                        ModConsole.Print(21, 24, "Picross Puzzles Added: " + CurrentMod.ModPicross.Count);
                    }
                }
            }


            ModConsole.DrawLine(new Point(20, 1), new Point(20, 48), 179, Color.White, Color.Black);
        }


        public void SetGlyph(string ID) {
            int index = Int32.Parse(ID);
            CurrentGlyphIndex = index;
        }

        public void Input() {
            Point mousePos = new MouseScreenObjectState(ModConsole, GameHost.Instance.Mouse).CellPosition; 
             
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
                }

                if (ModMenuSelect == "Create" && ModSubSelect == "Artifact" && CurrentMod != null) {  
                    if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                        if (mousePos.Y == 5)
                            CurrentGlyphIndex++;
                        else if (mousePos.Y == 7)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintR += 10;
                            else
                                PaintR++;
                        else if (mousePos.Y == 9)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintG += 10;
                            else
                                PaintG++;
                        else if (mousePos.Y == 11)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintB += 10;
                            else
                                PaintB++;
                    }
                    else if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                        if (mousePos.Y == 5)
                            CurrentGlyphIndex--;
                        else if (mousePos.Y == 7)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintR -= 10;
                            else
                                PaintR--;
                        else if (mousePos.Y == 9)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintG -= 10;
                            else
                                PaintG--;
                        else if (mousePos.Y == 11)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintB -= 10;
                            else
                                PaintB--;
                    }

                    PaintR = System.Math.Clamp(PaintR, 0, 255);
                    PaintG = System.Math.Clamp(PaintG, 0, 255);
                    PaintB = System.Math.Clamp(PaintB, 0, 255);

                    if (GameHost.Instance.Mouse.LeftButtonDown) {
                        if (mousePos.Y == 3 && mousePos.X > 20 && mousePos.X < 40)
                            SelectedField = 2;


                        Point offset = mousePos - new Point(50, 5);

                        if (offset.X >= 0 && offset.Y >= 0 && offset.X <= 29 && offset.Y <= 29) {
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeR = PaintR;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeG = PaintG;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeB = PaintB;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].Glyph = CurrentGlyphIndex;
                        } 
                    }

                    if (GameHost.Instance.Mouse.RightButtonDown) {
                        Point offset = mousePos - new Point(50, 5);

                        if (offset.X >= 0 && offset.Y >= 0 && offset.X <= 29 && offset.Y <= 29) {
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeR = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeG = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeB = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].Glyph = 32;
                        }
                    }

                    if (GameHost.Instance.Mouse.RightClicked) {
                        Point offset = mousePos - new Point(50, 5);

                        if (offset.X >= 0 && offset.Y >= 0 && offset.X <= 29 && offset.Y <= 29) {
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeR = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeG = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeB = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].Glyph = 32;
                        }
                    }
                }

                if (ModMenuSelect == "Create" && ModSubSelect == "Picross" && CurrentMod != null) {
                    if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) { 
                        if (mousePos.Y == 7)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintR += 10;
                            else
                                PaintR++;
                        else if (mousePos.Y == 9)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintG += 10;
                            else
                                PaintG++;
                        else if (mousePos.Y == 11)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintB += 10;
                            else
                                PaintB++;
                    }
                    else if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                        if (mousePos.Y == 5)
                            CurrentGlyphIndex--;
                        else if (mousePos.Y == 7)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintR -= 10;
                            else
                                PaintR--;
                        else if (mousePos.Y == 9)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintG -= 10;
                            else
                                PaintG--;
                        else if (mousePos.Y == 11)
                            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                                PaintB -= 10;
                            else
                                PaintB--;
                    }

                    PaintR = System.Math.Clamp(PaintR, 0, 255);
                    PaintG = System.Math.Clamp(PaintG, 0, 255);
                    PaintB = System.Math.Clamp(PaintB, 0, 255);

                    if (GameHost.Instance.Mouse.LeftButtonDown) {
                        if (mousePos.Y == 3 && mousePos.X > 20 && mousePos.X < 40)
                            SelectedField = 2;


                        Point offset = mousePos - new Point(50, 5);

                        PicrossPuzzle current = CurrentMod.ModPicross[ModItemIndex];

                        if (offset.X >= 0 && offset.Y >= 0 && offset.X < current.Width && offset.Y < current.Height) {
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].R = PaintR;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].G = PaintG;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].B = PaintB;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].PartOfSolution = true;
                        }
                    }

                    if (GameHost.Instance.Mouse.RightButtonDown) {
                        Point offset = mousePos - new Point(50, 5); 
                        PicrossPuzzle current = CurrentMod.ModPicross[ModItemIndex]; 
                        if (offset.X >= 0 && offset.Y >= 0 && offset.X < current.Width && offset.Y < current.Height) {
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].R = 0;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].G = 0;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].B = 0;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].PartOfSolution = false;
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
                string path = "./mods/" + CurrentMod.Name + ".dat.gz";
                Helper.SerializeToFileCompressed(CurrentMod, path); 
            }
        }

        public void UnpackMod() {
            if (CurrentMod != null) {
                Directory.CreateDirectory("./sandbox/constructibles/");
                for (int i = 0; i < CurrentMod.ModConstructs.Count; i++) {
                    string path = "./sandbox/constructibles/" + CurrentMod.ModConstructs[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModConstructs[i], path);
                }

                Directory.CreateDirectory("./sandbox/items/");
                for (int i = 0; i < CurrentMod.ModItems.Count; i++) {
                    string path = "./sandbox/items/" + CurrentMod.ModItems[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModItems[i], path);
                }

                Directory.CreateDirectory("./sandbox/maps/");
                for (int i = 0; i < CurrentMod.ModMaps.Count; i++) {
                    string path = "./sandbox/maps/" + CurrentMod.ModMaps[i].MapPos + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModMaps[i], path);
                }

                Directory.CreateDirectory("./sandbox/missions/");
                for (int i = 0; i < CurrentMod.ModMissions.Count; i++) {
                    string path = "./sandbox/missions/" + CurrentMod.ModMissions[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModMissions[i], path);
                }

                Directory.CreateDirectory("./sandbox/monsters/");
                for (int i = 0; i < CurrentMod.ModMonsters.Count; i++) {
                    string path = "./sandbox/monsters/" + CurrentMod.ModMonsters[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModMonsters[i], path);
                }

                Directory.CreateDirectory("./sandbox/npcs/");
                for (int i = 0; i < CurrentMod.ModNPCs.Count; i++) {
                    string path = "./sandbox/npcs/" + CurrentMod.ModNPCs[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModNPCs[i], path);
                }

                Directory.CreateDirectory("./sandbox/recipes/");
                for (int i = 0; i < CurrentMod.ModRecipes.Count; i++) {
                    string path = "./sandbox/recipes/" + CurrentMod.ModRecipes[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModRecipes[i], path);
                }

                Directory.CreateDirectory("./sandbox/artifacts/");
                for (int i = 0; i < CurrentMod.ModArtifacts.Count; i++) {
                    string path = "./sandbox/artifacts/" + CurrentMod.ModArtifacts[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModArtifacts[i], path);
                }

                Directory.CreateDirectory("./sandbox/picross/");
                for (int i = 0; i < CurrentMod.ModPicross.Count; i++) {
                    string path = "./sandbox/picross/" + CurrentMod.ModPicross[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModPicross[i], path);
                }

                Directory.CreateDirectory("./sandbox/scripts/");
                for (int i = 0; i < CurrentMod.ModScripts.Count; i++) {
                    string path = "./sandbox/scripts/" + CurrentMod.ModScripts[i].Name + ".lua"; 
                    using StreamWriter output = new StreamWriter(path);
                    string scriptString = CurrentMod.ModScripts[i].Script;
                    output.WriteLine(scriptString);
                    output.Close();
                }

                Directory.CreateDirectory("./sandbox/skills/");
                for (int i = 0; i < CurrentMod.ModSkills.Count; i++) {
                    string path = "./sandbox/skills/" + CurrentMod.ModSkills[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModSkills[i], path);
                }

                Directory.CreateDirectory("./sandbox/tiles/");
                for (int i = 0; i < CurrentMod.ModTiles.Count; i++) {
                    string path = "./sandbox/tiles/" + CurrentMod.ModTiles[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModTiles[i], path);
                }
            }
        }

        public void PackMod() {
            Mod load = new();

            if (Directory.Exists("./sandbox/constructibles/")) {
                string[] files = Directory.GetFiles("./sandbox/constructibles/"); 
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<Constructible>(json);
                    load.ModConstructs.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/items/")) {
                string[] files = Directory.GetFiles("./sandbox/items/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<Item>(json);
                    load.ModItems.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/maps/")) {
                string[] files = Directory.GetFiles("./sandbox/maps/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<ModMap>(json);
                    load.ModMaps.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/missions/")) {
                string[] files = Directory.GetFiles("./sandbox/missions/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<Mission>(json);
                    load.ModMissions.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/monsters/")) {
                string[] files = Directory.GetFiles("./sandbox/monsters/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<Monster>(json);
                    load.ModMonsters.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/npcs/")) {
                string[] files = Directory.GetFiles("./sandbox/npcs/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<NPC>(json);
                    load.ModNPCs.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/recipes/")) {
                string[] files = Directory.GetFiles("./sandbox/recipes/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<CraftingRecipe>(json);
                    load.ModRecipes.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/artifacts/")) {
                string[] files = Directory.GetFiles("./sandbox/artifacts/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<ArchArtifact>(json);
                    load.ModArtifacts.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/picross/")) {
                string[] files = Directory.GetFiles("./sandbox/picross/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<PicrossPuzzle>(json);
                    load.ModPicross.Add(item);
                }
            }


            if (Directory.Exists("./sandbox/scripts/")) {
                string[] files = Directory.GetFiles("./sandbox/scripts/");
                foreach (string fileName in files) {
                    string script = File.ReadAllText(fileName);
                    string name = fileName.Split("/")[^1][0..^4];
                    ModScript ob = new();
                    ob.Script = script;
                    ob.Name = name;
                    load.ModScripts.Add(ob);
                }
            }

            if (Directory.Exists("./sandbox/skills/")) {
                string[] files = Directory.GetFiles("./sandbox/skills/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<Skill>(json);
                    load.ModSkills.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/tiles/")) {
                string[] files = Directory.GetFiles("./sandbox/tiles/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<Tile>(json);
                    load.ModTiles.Add(item);
                }
            }

            CurrentMod = load;
        }

        public void RunInput(string add, bool backspace) {
            if (CurrentMod != null) {
                if (backspace) {
                    if (SelectedField == 0)
                        RemoveOneCharacter(ref CurrentMod.Name);
                    if (SelectedField == 1)
                        RemoveOneCharacter(ref CurrentMod.Package);
                    if (SelectedField == 2 && ModSubSelect == "Artifact")
                        RemoveOneCharacter(ref CurrentMod.ModArtifacts[ModItemIndex].Name);
                    if (SelectedField == 2 && ModSubSelect == "Picross")
                        RemoveOneCharacter(ref CurrentMod.ModPicross[ModItemIndex].Name);

                } else {
                    if (SelectedField == 0)
                        CurrentMod.Name += add;
                    if (SelectedField == 1)
                        CurrentMod.Package += add;
                    if (SelectedField == 2 && ModSubSelect == "Artifact")
                        CurrentMod.ModArtifacts[ModItemIndex].Name += add;
                    if (SelectedField == 2 && ModSubSelect == "Picross")
                        CurrentMod.ModPicross[ModItemIndex].Name += add;
                }
            }
        }
    }
}
