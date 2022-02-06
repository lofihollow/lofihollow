using LofiHollow.Managers;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Skills {
        public Window SkillWindow;
        public SadConsole.Console SkillConsole;

        public string SkillView = "Overview";

        public UI_Skills(int width, int height, string title) { 
            SkillWindow = new(width, height);
            SkillWindow.CanDrag = false;
            SkillWindow.Position = new(11, 6);

            int skillConsoleWidth = width - 2;
            int skillConsoleHeight = height - 2;

            SkillConsole = new(skillConsoleWidth, skillConsoleHeight);

            SkillConsole.Position = new(1, 1);
            SkillWindow.Title = title.Align(HorizontalAlignment.Center, skillConsoleWidth, (char)196);


            SkillWindow.Children.Add(SkillConsole);
            GameLoop.UIManager.Children.Add(SkillWindow);

            SkillWindow.Show();

            SkillWindow.IsVisible = false;
        }


        public void RenderSkills() {
            Point mousePos = new MouseScreenObjectState(SkillConsole, GameHost.Instance.Mouse).CellPosition;
            SkillConsole.Clear();

            SkillConsole.DrawLine(new Point(15, 0), new Point(15, SkillConsole.Height - 1), 179, Color.White, Color.Black);
            if (SkillView == "Overview") {
                SkillConsole.Print(16, 0, "Skill".Align(HorizontalAlignment.Center, 20) + "|" + "Level".Align(HorizontalAlignment.Center, 7) + "|" + "EXP".Align(HorizontalAlignment.Center, 12) + "|" + "Next".Align(HorizontalAlignment.Center, 12));
                SkillConsole.DrawLine(new Point(16, 1), new Point(SkillConsole.Width - 1, 1), 196, Color.White, Color.Black);
                int index = 0;

                foreach (KeyValuePair<string, Skill> kv in GameLoop.World.Player.Skills) {
                    string name = kv.Key;
                    int level = kv.Value.Level;
                    int exp = kv.Value.TotalExp;
                    int next = (kv.Value.ExpToLevel() - kv.Value.Experience);

                    SkillConsole.Print(16, 2 + (index * 2), Helper.HoverColoredString(name.Align(HorizontalAlignment.Center, 20) + "|" + level.ToString().Align(HorizontalAlignment.Center, 7) + "|" + exp.ToString().Align(HorizontalAlignment.Center, 12) + "|" + next.ToString().Align(HorizontalAlignment.Center, 12), mousePos.Y == 2 + (index * 2)));
                    SkillConsole.Print(16, 3 + (index * 2), new ColoredString(" ".Align(HorizontalAlignment.Center, 20) + "|" + " ".Align(HorizontalAlignment.Center, 7) + "|" + " ".Align(HorizontalAlignment.Center, 12) + "|" + " ".Align(HorizontalAlignment.Center, 12), Color.White, Color.Black));
                    index++;
                }
            }


        }


        public void SkillInput() {
            Point mousePos = new MouseScreenObjectState(SkillConsole, GameHost.Instance.Mouse).CellPosition;
            if (mousePos.X >= 0 && mousePos.X <= SkillConsole.Width && mousePos.Y >= 0 && mousePos.Y <= SkillConsole.Height) {
                if (GameHost.Instance.Mouse.LeftClicked) {
                    int slot = mousePos.Y;
                    if (slot >= 0 && slot <= 15)
                        CommandManager.UnequipItem(GameLoop.World.Player, slot);
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.K)) {
                Toggle();
            }
        }


        public void Toggle() {
            if (SkillWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                SkillWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
                SkillView = "Overview";
            } else {
                GameLoop.UIManager.selectedMenu = "Skills";
                SkillWindow.IsVisible = true;
                SkillWindow.IsFocused = true;
            }
        }
    }
}
