using LofiHollow.Managers;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;

namespace LofiHollow.UI {
    public class UI_Skills : Lofi_UI { 
        public string SkillView = "Overview";

        public UI_Skills(int width, int height, string title) : base(width, height, title, "Skills") { }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            Con.Clear();

            Con.DrawLine(new Point(15, 0), new Point(15, Con.Height - 1), 179, Color.White, Color.Black);
            if (SkillView == "Overview") {
                Con.Print(16, 0, "Skill".Align(HorizontalAlignment.Center, 20) + "|" + "Level".Align(HorizontalAlignment.Center, 7) + "|" + "EXP".Align(HorizontalAlignment.Center, 12) + "|" + "Next".Align(HorizontalAlignment.Center, 12));
                Con.DrawLine(new Point(16, 1), new Point(Con.Width - 1, 1), 196, Color.White, Color.Black);
                int index = 0;

                foreach (KeyValuePair<string, Skill> kv in GameLoop.World.Player.Skills) {
                    string name = kv.Key;
                    int level = kv.Value.Level;
                    int exp = kv.Value.TotalExp;
                    int next = (kv.Value.ExpToLevel() - kv.Value.Experience);

                    Con.Print(16, 2 + (index * 2), Helper.HoverColoredString(name.Align(HorizontalAlignment.Center, 20) + "|" + level.ToString().Align(HorizontalAlignment.Center, 7) + "|" + exp.ToString().Align(HorizontalAlignment.Center, 12) + "|" + next.ToString().Align(HorizontalAlignment.Center, 12), mousePos.Y == 2 + (index * 2)));
                    Con.Print(16, 3 + (index * 2), new ColoredString(" ".Align(HorizontalAlignment.Center, 20) + "|" + " ".Align(HorizontalAlignment.Center, 7) + "|" + " ".Align(HorizontalAlignment.Center, 12) + "|" + " ".Align(HorizontalAlignment.Center, 12), Color.White, Color.Black));
                    index++;
                }
            }


        }


        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (mousePos.X >= 0 && mousePos.X <= Con.Width && mousePos.Y >= 0 && mousePos.Y <= Con.Height) {
                if (GameHost.Instance.Mouse.LeftClicked) {
                    int slot = mousePos.Y;
                    if (slot >= 0 && slot <= 15)
                        CommandManager.UnequipItem(GameLoop.World.Player, slot);
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape) || GameHost.Instance.Keyboard.IsKeyReleased(Key.K)) {
                Toggle();
            }
        } 
    }
}
