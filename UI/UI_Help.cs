using LofiHollow.Entities;
using LofiHollow.Managers;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Help : Lofi_UI{ 
        public string PreviousMenu = "";
        public string HelpMode = "None";
        public string[] Guide;
        public int GuideTopIndex = 0;

        public Dictionary<Rectangle, int> GuideHyperlinks = new();

        public UI_Help(int width, int height, string title) : base(width, height, title, "Help") {
            if (File.Exists("./data/guide.dat")) {
                Guide = File.ReadAllLines("./data/guide.dat"); 

                for (int i = 0; i < Guide.Length; i++) {
                    if (Guide[i].Contains("[")) {
                        int start = Guide[i].IndexOf("[") + 1;
                        int end = Guide[i].IndexOf("]", start);
                        string[] split = Guide[i].Substring(start, end - start).Split(":");
                        int x = Int32.Parse(split[1]);
                        int y = Int32.Parse(split[2]);
                        int rectW = Int32.Parse(split[3]);
                        int rectH = Int32.Parse(split[4]);
                        Rectangle rect = new Rectangle(x, y, rectW, rectH);
                        int line = Int32.Parse(split[5]);
                        GuideHyperlinks.Add(rect, line);
                        Guide[i] = Guide[i].Remove(start - 1, end - start + 2); 
                    }
                }
            }
        }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear(); 
             
            if (HelpMode == "Guide") {
                if (Guide != null) {
                    for (int i = GuideTopIndex; i < Guide.Length && i < GuideTopIndex + 40; i++) {
                        Con.Print(0, i - GuideTopIndex, Guide[i]);
                    }


                    foreach (KeyValuePair<Rectangle, int> kv in GuideHyperlinks) {
                        if (kv.Key.Y - GuideTopIndex >= 0 && kv.Key.Y - GuideTopIndex <= 40) { 
                            foreach (var pos in kv.Key.Positions()) {
                                Con.SetForeground(pos.X, pos.Y - GuideTopIndex, Color.Cyan);
                            } 
                        }
                    }
                }
            } else if (HelpMode == "Hotkeys") {
                Con.Print(0, 0, "Hotkeys");
                Con.Print(0, 2, "W - Move Up");
                Con.Print(0, 3, "S - Move Down");
                Con.Print(0, 4, "A - Move Left");
                Con.Print(0, 5, "D - Move Right");
                Con.Print(0, 6, "SHIFT - Hold to Run");

                Con.Print(0, 8, 12.AsString() + " - Move down stairs");
                Con.Print(0, 9, 12.AsString() + " - Sleep (while on bed)");
                Con.Print(0, 10, 11.AsString() + " - Move up stairs");

                Con.Print(0, 12, "I - Open Inventory");
                Con.Print(0, 13, "C - Open Crafting");
                Con.Print(0, 14, "K - Open Skills");
                Con.Print(0, 15, "Q - Open Quest Log");
                Con.Print(0, 16, "W - Move Up");

                Con.Print(0, 18, "` - Show Multiplayer Code (if hosting)");
                Con.Print(0, 19, "? - Show this menu!");
                Con.Print(0, 20, "F1 - Open up the Guide");
                Con.Print(0, 21, "F8 - Open up the Feedback Menu");
            }


        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.HasKeysPressed && !GameLoop.EitherShift()) {
                ToggleHelp("None");
            }

            if (Guide != null) {
                if (GameLoop.EitherShift()) {
                    if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0)
                        GuideTopIndex = Math.Clamp(GuideTopIndex + 10, 0, Guide.Length);
                    else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0)
                        GuideTopIndex = Math.Clamp(GuideTopIndex - 10, 0, Guide.Length);
                }
                else {
                    if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0)
                        GuideTopIndex = Math.Clamp(GuideTopIndex + 1, 0, Guide.Length);
                    else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0)
                        GuideTopIndex = Math.Clamp(GuideTopIndex - 1, 0, Guide.Length);
                }
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                Point scrolledPos = new Point(mousePos.X, mousePos.Y + GuideTopIndex);
                if (HelpMode == "Guide") {
                    foreach (KeyValuePair<Rectangle, int> kv in GuideHyperlinks) {
                        if (kv.Key.Contains(scrolledPos)) {
                            GuideTopIndex = kv.Value - 1;
                        }
                    }
                } 
            }

            if (GameHost.Instance.Mouse.RightClicked) {
                GuideTopIndex = 0;
            }
        }


        public void ToggleHelp(string mode) {
            if (Win.IsVisible) {
                GameLoop.UIManager.selectedMenu = PreviousMenu;
                Win.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;

            } else {
                Con.Clear();
                PreviousMenu = GameLoop.UIManager.selectedMenu;
                GameLoop.UIManager.selectedMenu = "Help";
                GuideTopIndex = 0;
                Win.IsVisible = true;
                Win.IsFocused = true;
                HelpMode = mode;
            }
        }
    }
}
