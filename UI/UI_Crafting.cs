using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using LofiHollow.EntityData;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Crafting : Lofi_UI { 
        public int craftTopIndex = 0;
        public int selectedIndex = 0;
        public int craftingQuantity = 1;
        public int MinimumQuality = 0;
        public string CurrentSkill = "Crafting";
        public string StationTool = "None";
        public int StationTier = 0;
        public List<CraftingRecipe> CurrentSkillRecipes = new();
        public List<CraftingRecipe> CurrentCraftable = new();

        public UI_Crafting(int width, int height, string title) : base(width, height, title, "Crafting") { }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            Con.Print(69, 0, Helper.HoverColoredString("X", mousePos == new Point(69, 0)));

            string Header = "Name".Align(HorizontalAlignment.Center, 44) + "|";
            Header += "Lv".Align(HorizontalAlignment.Center, 4) + "|";
            Header += "XP per".Align(HorizontalAlignment.Center, 8) + "|";
            Header += "Craftable".Align(HorizontalAlignment.Center, 11);
            Con.Print(0, 1, Header);
            Con.DrawLine(new Point(0, 2), new Point(70, 2), 196, Color.White, Color.Black);


            for (int i = 0; i < CurrentSkillRecipes.Count && i < 11; i++) {
                CraftingRecipe current = CurrentSkillRecipes[i + craftTopIndex];
                string Entry;

                if (current.FinishedQty > 1) 
                    Entry = (current.FinishedQty + "x " + current.Name).Align(HorizontalAlignment.Center, 44) + "|";
                else
                    Entry = current.Name.Align(HorizontalAlignment.Center, 44) + "|";
                Entry += current.RequiredLevel.ToString().Align(HorizontalAlignment.Center, 4) + "|";
                Entry += current.ExpGranted.ToString().Align(HorizontalAlignment.Center, 8) + "|";

                bool craftable = CurrentCraftable.Contains(CurrentSkillRecipes[i]);
                ColoredString check = new ColoredString(4.AsString(), Color.Lime, Color.Black);

                if (!craftable)
                    check = new ColoredString("x", Color.Red, Color.Black);


                Con.Print(0, 3 + (2 * i), Helper.HoverColoredString(Entry, mousePos.Y - 3 == 2 * i));
                Con.Print(64, 3 + (2 * i), check);
                Con.Print(0, 4 + (2 * i), "".Align(HorizontalAlignment.Center, 70, '-'));
            }


            Con.DrawLine(new Point(0, 25), new Point(70, 25), 196, Color.White, Color.Black);

            if (CurrentSkillRecipes.Count > selectedIndex) {
                CraftingRecipe current = CurrentSkillRecipes[selectedIndex];
                Item finished = Item.Copy(current.FinishedID);
                int QualityCap = (int)Math.Floor((GameLoop.World.Player.Skills[CurrentSkill].Level + 1f) / 10f) + 1;

                int canCraft = current.ActorCanCraft(GameLoop.World.Player, CurrentSkill, craftingQuantity, MinimumQuality);


                Con.Print(0, 26, finished.AsColoredGlyph());
                if (finished.Dec != null)
                    Con.AddDecorator(0, 26, 1, finished.GetDecorator());
                Con.Print(2, 26, (current.FinishedQty * craftingQuantity) + "x " + finished.Name);
                Con.Print(0, 27, new ColoredString("Value: ", Color.White, Color.Black) + Helper.ConvertCoppers(finished.AverageValue * current.FinishedQty));
                Con.Print(0, 28, "Desc: " + finished.Description);
                Con.Print(25, 26, new ColoredString("Max. " + CurrentSkill + " Quality: ") + Helper.LetterGrade(QualityCap));
                Con.Print(25, 27, new ColoredString("Best Ingredient Quality: ") + Helper.LetterGrade(canCraft));
                Con.Print(25, 28, new ColoredString("Min. Ingredient Quality: -"));
                Con.Print(52, 28, MinimumQuality == 0 ? new ColoredString("Any") : Helper.LetterGrade(MinimumQuality, 3));
                Con.Print(56, 28, "+");
                Con.DrawLine(new Point(0, 29), new Point(70, 29), '-', Color.White, Color.Black);
                Con.Print(62, 26, new ColoredString("CRAFT", canCraft > -1 ? Color.Lime : Color.Red, Color.Black));
                Con.Print(62, 27, "-");
                Con.Print(63, 27, craftingQuantity.ToString().Align(HorizontalAlignment.Center, 3));
                Con.Print(66, 27, "+");

                Con.Print(0, 30, "Required Tools: ");
                for (int i = 0; i < current.RequiredTools.Count; i++) {
                    bool hasTool = current.RequiredTools[i].ActorHasTool(GameLoop.World.Player);
                    ColoredString check = new ColoredString(4.AsString(), Color.Lime, Color.Black);
                    
                    if (!hasTool)
                        check = new ColoredString("x", Color.Red, Color.Black);

                    Con.Print(1, 31 + i, new ColoredString(current.RequiredTools[i].Property + " [" + current.RequiredTools[i].Tier + "]: ") + check);
                }

                Con.DrawLine(new Point(24, 30), new Point(24, 40), '|', Color.White, Color.Black);
                Con.Print(25, 30, "Specific Materials: ");
                for (int i = 0; i < current.SpecificMaterials.Count; i++) {
                    bool hasTool = current.SpecificMaterials[i].ActorHasComponent(GameLoop.World.Player, craftingQuantity, MinimumQuality) != -1;
                    ColoredString check = new ColoredString(4.AsString(), Color.Lime, Color.Black);

                    if (!hasTool)
                        check = new ColoredString("x", Color.Red, Color.Black);

                    Item specific = Item.Copy(current.SpecificMaterials[i].ID);

                    Con.Print(26, 31 + i, new ColoredString((current.SpecificMaterials[i].ItemQuantity * craftingQuantity) + "x " + specific.Name + ": ") + check);
                }

                Con.DrawLine(new Point(49, 30), new Point(49, 40), '|', Color.White, Color.Black);
                Con.Print(50, 30, "Flexible Materials: ");
                for (int i = 0; i < current.GenericMaterials.Count; i++) {
                    int Qual = current.GenericMaterials[i].ActorHasComponent(GameLoop.World.Player, craftingQuantity, MinimumQuality);
                    ColoredString check = new ColoredString("x", Color.Red, Color.Black);

                    if (Qual == 0)
                        check = new ColoredString(4.AsString(), Color.Lime, Color.Black);
                    else if (Qual > 0)
                        check = Helper.LetterGrade(Qual);


                    Con.Print(51, 31 + i, new ColoredString((current.GenericMaterials[i].Quantity * craftingQuantity) + "x " + current.GenericMaterials[i].Property + " [" + current.GenericMaterials[i].Tier + "]: ") + check);
                }
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                if (craftTopIndex + 1 < CurrentSkillRecipes.Count - 11)
                    craftTopIndex++;
            } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                if (craftTopIndex > 0)
                    craftTopIndex--;
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos == new Point(69, 0)) {
                    Toggle();
                }

                if (mousePos.Y - 3 < 25 && mousePos.Y - 3 >= 0) {
                    int itemSlot = -1;
                    if ((float)((mousePos.Y - 3) / 2f) == (mousePos.Y - 3) / 2) {
                        itemSlot = (mousePos.Y - 3) / 2;

                        if (itemSlot >= 0)
                            itemSlot += craftTopIndex;

                        if (itemSlot < CurrentSkillRecipes.Count) {
                            selectedIndex = itemSlot;
                        }
                    }

                    if (itemSlot >= 0)
                        itemSlot += craftTopIndex; 
                }

                if (mousePos.Y == 26 && (mousePos.X >= 62 && mousePos.X <= 66)) {
                    CurrentSkillRecipes[selectedIndex].Craft(GameLoop.World.Player, craftingQuantity, MinimumQuality);

                    RefreshList();
                }

                if (mousePos == new Point(56, 28)) {
                    if (MinimumQuality < 11) {
                        MinimumQuality++;
                        RefreshList();
                    }
                }

                if (mousePos == new Point(50, 28)) {
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
                Toggle();
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

            Win.Title = skill.Align(HorizontalAlignment.Center, 70, (char) 196);

            selectedIndex = 0;
            craftTopIndex = 0;
            craftingQuantity = 1;

            Toggle();
        } 
    }
}
