using LofiHollow.Entities;
using LofiHollow.Managers;
using LofiHollow.Missions;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_MissionLog {
        public SadConsole.Console MissionConsole;
        public Window MissionWindow;
        public int missionTopIndex = 0;
        public int CurrentChapter = 0;

        public UI_MissionLog(int width, int height, string title) {
            MissionWindow = new(width, height);
            MissionWindow.CanDrag = false;
            MissionWindow.Position = new(11, 6);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            MissionConsole = new(invConWidth, invConHeight);
            MissionConsole.Position = new(1, 1);
            MissionWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            MissionWindow.Children.Add(MissionConsole);
            GameLoop.UIManager.Children.Add(MissionWindow);

            MissionWindow.Show();
            MissionWindow.IsVisible = false;
        }


        public void RenderMissions() {
            Point mousePos = new MouseScreenObjectState(MissionConsole, GameHost.Instance.Mouse).CellPosition;

            MissionConsole.Clear();

            string CurrentChapterName = GameLoop.World.Chapters.Count > CurrentChapter && CurrentChapter >= 0 ? GameLoop.World.Chapters[CurrentChapter].Name : "ERROR";

            if (CurrentChapter > 0)
                MissionConsole.Print(1, 1, Helper.HoverColoredString(((char)11).ToString(), mousePos == new Point(1, 1)));
            if (CurrentChapter + 1 < GameLoop.World.Chapters.Count)
                MissionConsole.Print(67, 1, Helper.HoverColoredString(((char)12).ToString(), mousePos == new Point(67, 1)));
            MissionConsole.Print(3, 1, ("Chapter " + (CurrentChapter + 1) + ": " + CurrentChapterName).Align(HorizontalAlignment.Center, 64));
            MissionConsole.Print(69, 0, Helper.HoverColoredString("X", mousePos == new Point(69, 0)));
            MissionConsole.DrawLine(new Point(0, 2), new Point(69, 2), 196, Color.White);

            int y = 0;
            foreach (KeyValuePair<string, Mission> kv in GameLoop.World.Player.MissionLog) {
                if (kv.Value.Chapter == CurrentChapter + 1) {
                    Color missionStatus = Color.Yellow;

                    if (kv.Value.CurrentStage >= kv.Value.Stages.Count)
                        missionStatus = Color.Green;

                    MissionConsole.Print(1, 3 + (y), new ColoredString(kv.Value.Name, missionStatus, Color.Black)); 
                    y++;

                    for (int i = 0; i < kv.Value.Stages.Count; i++) {
                        if (i < kv.Value.CurrentStage) {
                            MissionConsole.Print(1, 3 + (y), ": " + kv.Value.Stages[i].Summary);
                            MissionConsole.SetDecorator(3, 3 + (y), kv.Value.Stages[i].Summary.Length, new CellDecorator(new Color(255, 255, 255, 200), '-', Mirror.None));
                        } else if (i == kv.Value.CurrentStage) {
                            string text = kv.Value.Stages[i].Summary;
                            if (kv.Value.Stages[i].CounterType != "")
                                text += " [" + kv.Value.Stages[i].CounterDone + " / " + kv.Value.Stages[i].CounterNeeded + "]";
                            MissionConsole.Print(1, 3 + (y), ": " + text);
                        } else {
                            MissionConsole.Print(1, 3 + (y), ": ??????????");
                        }

                        y++;
                    }
                    y++;
                }
            }
        }

        public void MissionInput() {
            Point mousePos = new MouseScreenObjectState(MissionConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape) || GameHost.Instance.Keyboard.IsKeyReleased(Key.Q)) {
                Toggle();
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos == new Point(69, 0)) {
                    Toggle();
                }

                if (mousePos == new Point(1, 1)) {
                    if (CurrentChapter > 0)
                        CurrentChapter--;
                }

                if (mousePos == new Point(67, 1)) {
                    if (CurrentChapter + 1 < GameLoop.World.Chapters.Count)
                        CurrentChapter++;
                }
            }
        }


        public void Toggle() {
            if (MissionWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                MissionWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
            } else {
                GameLoop.UIManager.selectedMenu = "MissionLog";
                MissionWindow.IsVisible = true;
                MissionWindow.IsFocused = true;
            }
        }
    }
}
