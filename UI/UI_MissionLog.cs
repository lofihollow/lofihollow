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
    public class UI_MissionLog : Lofi_UI { 
        public int missionTopIndex = 0;
        public int CurrentChapter = 0;

        public UI_MissionLog(int width, int height, string title) : base(width, height, title, "MissionLog") { }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            string CurrentChapterName = GameLoop.World.Chapters.Count > CurrentChapter && CurrentChapter >= 0 ? GameLoop.World.Chapters[CurrentChapter].Name : "ERROR";

            if (CurrentChapter > 0)
                Con.Print(1, 1, Helper.HoverColoredString(11.AsString(), mousePos == new Point(1, 1)));
            if (CurrentChapter + 1 < GameLoop.World.Chapters.Count)
                Con.Print(67, 1, Helper.HoverColoredString(12.AsString(), mousePos == new Point(67, 1)));
            Con.Print(3, 1, ("Chapter " + (CurrentChapter + 1) + ": " + CurrentChapterName).Align(HorizontalAlignment.Center, 64));
            Con.Print(69, 0, Helper.HoverColoredString("X", mousePos == new Point(69, 0)));
            Con.DrawLine(new Point(0, 2), new Point(69, 2), 196, Color.White);

            int y = 0;
            foreach (KeyValuePair<string, Mission> kv in GameLoop.World.Player.MissionLog) {
                if (kv.Value.Chapter == CurrentChapter + 1) {
                    Color missionStatus = Color.Yellow;

                    if (kv.Value.CurrentStage >= kv.Value.Stages.Count)
                        missionStatus = Color.Green;

                    Con.Print(1, 3 + (y), new ColoredString(kv.Value.Name, missionStatus, Color.Black)); 
                    y++;

                    for (int i = 0; i < kv.Value.Stages.Count; i++) {
                        if (i < kv.Value.CurrentStage) {
                            Con.Print(1, 3 + (y), ": " + kv.Value.Stages[i].Summary);
                            Con.SetDecorator(3, 3 + (y), kv.Value.Stages[i].Summary.Length, new CellDecorator(new Color(255, 255, 255, 200), '-', Mirror.None));
                        } else if (i == kv.Value.CurrentStage) {
                            string text = kv.Value.Stages[i].Summary;
                            if (kv.Value.Stages[i].CounterType != "")
                                text += " [" + kv.Value.Stages[i].CounterDone + " / " + kv.Value.Stages[i].CounterNeeded + "]";
                            Con.Print(1, 3 + (y), ": " + text);
                        } else {
                            Con.Print(1, 3 + (y), ": ??????????");
                        }

                        y++;
                    }
                    y++;
                }
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
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
    }
}
