using LofiHollow.Entities;
using LofiHollow.Managers;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Crafting {
        public SadConsole.Console CraftingConsole;
        public Window CraftingWindow;
        public int craftTopIndex = 0;
        public int selectedIndex = 0;
        public int craftingQuantity = 1;
        public int MinimumQuality = 0;
        public string CurrentSkill = "Crafting";
        public string StationTool = "None";
        public int StationTier = 0;
        public List<CraftingRecipe> CurrentSkillRecipes = new();
        public List<CraftingRecipe> CurrentCraftable = new();

        public UI_Crafting(int width, int height, string title) {
            CraftingWindow = new(width, height);
            CraftingWindow.CanDrag = false;
            CraftingWindow.Position = new(0, 0);

            int craftConWidth = width - 2;
            int craftConHeight = height - 2;

            CraftingConsole = new(craftConWidth, craftConHeight);
            CraftingConsole.Position = new(1, 1);
            CraftingWindow.Title = title.Align(HorizontalAlignment.Center, craftConWidth, (char)196);


            CraftingWindow.Children.Add(CraftingConsole);
            GameLoop.UIManager.Children.Add(CraftingWindow);

            CraftingWindow.Show();
            CraftingWindow.IsVisible = false;
        }


        public void RenderCrafting() {
            Point mousePos = new MouseScreenObjectState(CraftingConsole, GameHost.Instance.Mouse).CellPosition;

            CraftingConsole.Clear();

            CraftingConsole.Print(69, 0, Helper.HoverColoredString("X", mousePos == new Point(69, 0)));

            string Header = "Name".Align(HorizontalAlignment.Center, 44) + "|";
            Header += "Lv".Align(HorizontalAlignment.Center, 4) + "|";
            Header += "XP per".Align(HorizontalAlignment.Center, 8) + "|";
            Header += "Craftable".Align(HorizontalAlignment.Center, 11);
            CraftingConsole.Print(0, 1, Header);
            CraftingConsole.DrawLine(new Point(0, 2), new Point(70, 2), 196, Color.White, Color.Black);


            for (int i = 0; i < CurrentSkillRecipes.Count; i++) {
                CraftingRecipe current = CurrentSkillRecipes[i];
                string Entry;

                if (current.FinishedQty > 1) 
                    Entry = (current.FinishedQty + "x " + current.Name).Align(HorizontalAlignment.Center, 44) + "|";
                else
                    Entry = current.Name.Align(HorizontalAlignment.Center, 44) + "|";
                Entry += current.RequiredLevel.ToString().Align(HorizontalAlignment.Center, 4) + "|";
                Entry += current.ExpGranted.ToString().Align(HorizontalAlignment.Center, 8) + "|";

                bool craftable = CurrentCraftable.Contains(CurrentSkillRecipes[i]);
                ColoredString check = new ColoredString(((char)4).ToString(), Color.Lime, Color.Black);

                if (!craftable)
                    check = new ColoredString("x", Color.Red, Color.Black);


                CraftingConsole.Print(0, 3 + (2 * i), Helper.HoverColoredString(Entry, mousePos.Y - 3 == 2 * i));
                CraftingConsole.Print(64, 3 + (2 * i), check);
                CraftingConsole.Print(0, 4 + (2 * i), "".Align(HorizontalAlignment.Center, 70, '-'));
            }


            CraftingConsole.DrawLine(new Point(0, 25), new Point(70, 25), 196, Color.White, Color.Black);

            if (CurrentSkillRecipes.Count > selectedIndex) {
                CraftingRecipe current = CurrentSkillRecipes[selectedIndex];
                Item finished = new(current.FinishedID);
                int QualityCap = (int)Math.Floor((GameLoop.World.Player.Skills[CurrentSkill].Level + 1f) / 10f) + 1;

                int canCraft = current.ActorCanCraft(GameLoop.World.Player, CurrentSkill, craftingQuantity, MinimumQuality);


                CraftingConsole.Print(0, 26, finished.AsColoredGlyph());
                CraftingConsole.Print(2, 26, (current.FinishedQty * craftingQuantity) + "x " + finished.Name);
                CraftingConsole.Print(0, 27, new ColoredString("Value: ", Color.White, Color.Black) + Helper.ConvertCoppers(finished.AverageValue * current.FinishedQty));
                CraftingConsole.Print(0, 28, "Desc: " + finished.Description);
                CraftingConsole.Print(20, 26, new ColoredString("Max. " + CurrentSkill + " Quality: ") + Helper.LetterGrade(QualityCap));
                CraftingConsole.Print(20, 27, new ColoredString("Best Ingredient Quality: ") + Helper.LetterGrade(canCraft));
                CraftingConsole.Print(20, 28, new ColoredString("Min. Ingredient Quality: -"));
                CraftingConsole.Print(47, 28, MinimumQuality == 0 ? new ColoredString("Any") : Helper.LetterGrade(MinimumQuality, 3));
                CraftingConsole.Print(51, 28, "+");
                CraftingConsole.DrawLine(new Point(0, 29), new Point(70, 29), '-', Color.White, Color.Black);
                CraftingConsole.Print(62, 26, new ColoredString("CRAFT", canCraft > -1 ? Color.Lime : Color.Red, Color.Black));
                CraftingConsole.Print(62, 27, "-");
                CraftingConsole.Print(63, 27, craftingQuantity.ToString().Align(HorizontalAlignment.Center, 3));
                CraftingConsole.Print(66, 27, "+");

                CraftingConsole.Print(0, 30, "Required Tools: ");
                for (int i = 0; i < current.RequiredTools.Count; i++) {
                    bool hasTool = current.RequiredTools[i].ActorHasTool(GameLoop.World.Player);
                    ColoredString check = new ColoredString(((char)4).ToString(), Color.Lime, Color.Black);
                    
                    if (!hasTool)
                        check = new ColoredString("x", Color.Red, Color.Black);

                    CraftingConsole.Print(1, 31 + i, new ColoredString(current.RequiredTools[i].Property + " [" + current.RequiredTools[i].Tier + "]: ") + check);
                }

                CraftingConsole.DrawLine(new Point(24, 30), new Point(24, 40), '|', Color.White, Color.Black);
                CraftingConsole.Print(25, 30, "Specific Materials: ");
                for (int i = 0; i < current.SpecificMaterials.Count; i++) {
                    bool hasTool = current.SpecificMaterials[i].ActorHasComponent(GameLoop.World.Player, craftingQuantity, MinimumQuality) != -1;
                    ColoredString check = new ColoredString(((char)4).ToString(), Color.Lime, Color.Black);

                    if (!hasTool)
                        check = new ColoredString("x", Color.Red, Color.Black);

                    Item specific = new(current.SpecificMaterials[i].Name);

                    CraftingConsole.Print(26, 31 + i, new ColoredString((current.SpecificMaterials[i].ItemQuantity * craftingQuantity) + "x " + specific.Name + ": ") + check);
                }

                CraftingConsole.DrawLine(new Point(49, 30), new Point(49, 40), '|', Color.White, Color.Black);
                CraftingConsole.Print(50, 30, "Flexible Materials: ");
                for (int i = 0; i < current.GenericMaterials.Count; i++) {
                    int Qual = current.GenericMaterials[i].ActorHasComponent(GameLoop.World.Player, craftingQuantity, MinimumQuality);
                    ColoredString check = new ColoredString("x", Color.Red, Color.Black);

                    if (Qual == 0)
                        check = new ColoredString(((char)4).ToString(), Color.Lime, Color.Black);
                    else if (Qual > 0)
                        check = Helper.LetterGrade(Qual);


                    CraftingConsole.Print(51, 31 + i, new ColoredString((current.GenericMaterials[i].Quantity * craftingQuantity) + "x " + current.GenericMaterials[i].Property + " [" + current.GenericMaterials[i].Tier + "]: ") + check);
                }
            }
        }

        public void CraftingInput() {
            Point mousePos = new MouseScreenObjectState(CraftingConsole, GameHost.Instance.Mouse).CellPosition;

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos == new Point(69, 0)) {
                    ToggleCraft();
                }

                if (mousePos.Y - 3 < 25 && mousePos.Y - 3 >= 0) {
                    if (CurrentSkillRecipes.Count > (mousePos.Y + craftTopIndex - 3) / 2) {
                        if ((float)((mousePos.Y + craftTopIndex - 3) / 2f) == (mousePos.Y + craftTopIndex - 3) / 2) {
                            selectedIndex = (mousePos.Y + craftTopIndex - 3) / 2;
                        }
                    } 
                }

                if (mousePos.Y == 26 && (mousePos.X >= 62 && mousePos.X <= 66)) {
                    CurrentSkillRecipes[selectedIndex].Craft(GameLoop.World.Player, craftingQuantity, MinimumQuality);

                    RefreshList();
                }

                if (mousePos == new Point(51, 28)) {
                    if (MinimumQuality < 11) {
                        MinimumQuality++;
                        RefreshList();
                    }
                }

                if (mousePos == new Point(45, 28)) {
                    if (MinimumQuality > 0) {
                        MinimumQuality--;
                        RefreshList();
                    }
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(Key.RightShift)) {
                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (mousePos == new Point(62, 27)) {
                        if (craftingQuantity > 10)
                            craftingQuantity -= 10;
                    }

                    if (mousePos == new Point(66, 27)) {
                        craftingQuantity += 10;
                    }
                }
            } else {
                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (mousePos == new Point(62, 27)) {
                        if (craftingQuantity > 1)
                            craftingQuantity--;
                    }

                    if (mousePos == new Point(66, 27)) {
                        craftingQuantity++;
                    }
                }
            }


            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                ToggleCraft();
            }
        }

        public void RefreshList() {
            CurrentSkillRecipes.Clear();
            CurrentCraftable.Clear();

            for (int i = 0; i < GameLoop.World.recipeLibrary.Count; i++) {
                if (GameLoop.World.recipeLibrary[i].Skill == CurrentSkill)
                    CurrentSkillRecipes.Add(GameLoop.World.recipeLibrary[i]);
                if (GameLoop.World.recipeLibrary[i].ActorCanCraft(GameLoop.World.Player, CurrentSkill, 1, MinimumQuality) > -1)
                    CurrentCraftable.Add(GameLoop.World.recipeLibrary[i]);
            }
        }

        public void SetupCrafting(string skill, string tool, int tier) {
            CurrentSkill = skill;
            StationTool = tool;
            StationTier = tier;
            RefreshList();

            CraftingWindow.Title = skill.Align(HorizontalAlignment.Center, 70, (char) 196);

            selectedIndex = 0;
            craftTopIndex = 0;
            craftingQuantity = 1;

            ToggleCraft();
        }



        public void ToggleCraft() {
            if (CraftingWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                CraftingWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
            } else {
                GameLoop.UIManager.selectedMenu = "Crafting";
                CraftingWindow.IsVisible = true;
                CraftingWindow.IsFocused = true;
            }
        }
    }
}
