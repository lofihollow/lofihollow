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
    public class UI_Help {
        public SadConsole.Console HelpConsole;
        public Window HelpWindow;
        public string PreviousMenu = "";
        public string HelpMode = "None";
        public string[] Guide;
        public int GuideTopIndex = 0;

        public Dictionary<Rectangle, int> GuideHyperlinks = new();

        public UI_Help(int width, int height, string title) {
            HelpWindow = new(width, height);
            HelpWindow.CanDrag = false;
            HelpWindow.Position = new(11, 6);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            HelpConsole = new(invConWidth, invConHeight);
            HelpConsole.Position = new(1, 1);
            HelpWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            HelpWindow.Children.Add(HelpConsole);
            GameLoop.UIManager.Children.Add(HelpWindow);

            HelpWindow.Show();
            HelpWindow.IsVisible = false;

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


        public void RenderHelp() {
            Point mousePos = new MouseScreenObjectState(HelpConsole, GameHost.Instance.Mouse).CellPosition;

            HelpConsole.Clear(); 
             
            if (HelpMode == "Guide") {
                if (Guide != null) {
                    for (int i = GuideTopIndex; i < Guide.Length && i < GuideTopIndex + 40; i++) {
                        HelpConsole.Print(0, i - GuideTopIndex, Guide[i]);
                    }


                    foreach (KeyValuePair<Rectangle, int> kv in GuideHyperlinks) {
                        if (kv.Key.Y - GuideTopIndex >= 0 && kv.Key.Y - GuideTopIndex <= 40) { 
                            foreach (var pos in kv.Key.Positions()) {
                                HelpConsole.SetForeground(pos.X, pos.Y - GuideTopIndex, Color.Cyan);
                            } 
                        }
                    }
                }
            } else if (HelpMode == "Hotkeys") {
                HelpConsole.Print(0, 0, "Hotkeys");
            }


        }

        public void HelpInput() {
            Point mousePos = new MouseScreenObjectState(HelpConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                ToggleHelp("None");
            }

            if (Guide != null) {
                if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                    if (GuideTopIndex + 1 < Guide.Length)
                        GuideTopIndex++;
                } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                    if (GuideTopIndex > 0)
                        GuideTopIndex--;
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
            if (HelpWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = PreviousMenu;
                HelpWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;

            } else {
                HelpConsole.Clear();
                PreviousMenu = GameLoop.UIManager.selectedMenu;
                GameLoop.UIManager.selectedMenu = "Help";
                GuideTopIndex = 0;
                HelpWindow.IsVisible = true;
                HelpWindow.IsFocused = true;
                HelpMode = mode;
            }
        }
    }
}
