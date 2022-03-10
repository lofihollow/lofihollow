using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class StackEm : Minigame {
        public int Score = 30;

        public int Timer = 61;
        public double LastTimeTick = 0;
        public bool DisplayingScore = false;

        public int blockX = 10;
        public double Speed = 100;
        public double LastMoveTime = 0;
        public bool MovingLeft = false;
        public int highestStack = 0;
        public bool Dropped = false;

        public Dictionary<Point, bool> PlayGrid = new();


        public StackEm() {
            PlayGrid.Clear();
            for (int x = 0; x < 20; x++) {
                for (int y = 0; y < 13; y++) {
                    PlayGrid.Add(new Point(x, y), false);
                }
            }
        }

        public void Reset() {
            Score = 30;
            Timer = 61;
            DisplayingScore = false;
            Speed = 100;
            blockX = 10;
            LastTimeTick = 0;
            highestStack = 0;
            Dropped = false;

            PlayGrid.Clear();
            for (int x = 0; x < 20; x++) {
                for (int y = 0; y < 13; y++) {
                    PlayGrid.Add(new Point(x, y), false);
                }
            }
        }

        public void DropBlock() {
            for (int y = 0; y < 13; y++) { 
                if (PlayGrid.ContainsKey(new Point(blockX, y)) && (y != 12 || (y == 12 && PlayGrid[new Point(blockX, y)]))) {
                    if (PlayGrid[new Point(blockX, y)]) {
                        PlayGrid[new Point(blockX, y - 1)] = true; 

                        if (12 - y > highestStack) {
                            highestStack = 12 - y;
                        }
                    }
                } else if (y == 12 && !PlayGrid[new Point(blockX, y)]) {
                    PlayGrid[new Point(blockX, y)] = true; 
                }
            }

            Score = MathHelpers.Clamp(Score - 1, 0, 30); 
            Speed = MathHelpers.Clamp(100 - (highestStack * 10), 5, 100);

            if (highestStack >= 9) {
                DisplayingScore = true;
            }
        }

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.Con;

            if (Timer > 0 && !DisplayingScore) {
                Mini.Print(0, 0, Timer.ToString().Align(HorizontalAlignment.Center, 70));
                Mini.Print(0, 2, ("Score: " + Score).Align(HorizontalAlignment.Center, 70));
                  
                if (LastTimeTick + 1000 < GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                    Timer--; 
                }

                if (LastMoveTime + Speed <= GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    LastMoveTime = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                    if (MovingLeft) {
                        blockX -= 1;
                        if (blockX <= 0) {
                            MovingLeft = false;
                        }
                    } else {
                        blockX += 1;
                        if (blockX >= 19) {
                            MovingLeft = true;
                        }
                    }
                }

                foreach (KeyValuePair<Point, bool> kv in PlayGrid) {
                    Point pos = kv.Key;
                    if (kv.Value) {
                        Mini.Print(25 + pos.X, 14 + pos.Y, 254.AsString(), Color.White);
                    } else {
                        Mini.Print(25 + pos.X, 14 + pos.Y, 254.AsString(), Color.DarkSlateGray);
                    }
                }

                Mini.Print(25 + blockX, 12, 254.AsString(), Color.White);
                Mini.DrawLine(new Point(25, 16), new Point(44, 16), 196, Color.Lime);

                Mini.DrawLine(new Point(24, 14), new Point(24, 26), 179, Color.White);
                Mini.DrawLine(new Point(45, 14), new Point(45, 26), 179, Color.White);

                Mini.Print(0, 29, "Press SPACE to drop a block".Align(HorizontalAlignment.Center, 70));
                Mini.Print(0, 31, "Stack past the green line to win!".Align(HorizontalAlignment.Center, 70));
            }
            else {
                if (!DisplayingScore) {
                    DisplayingScore = true;
                }

                if (Score > 0) {
                    Mini.Print(0, 3, ("You earned " + (Score) + " copper!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press ESC to close]").Align(HorizontalAlignment.Center, 70));
                }
                else {
                    Mini.Print(0, 3, ("Better luck next time!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press ESC to close]").Align(HorizontalAlignment.Center, 70));
                }
            }
        }

        public void MinigameClick(string ID) {

        }
        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Reset();
                Close();
            }

            if (!DisplayingScore) {
                if (GameHost.Instance.Keyboard.IsKeyDown(Key.Space) && !Dropped) {
                    DropBlock();
                    Dropped = true;
                } else if (!GameHost.Instance.Keyboard.IsKeyDown(Key.Space)) {
                    Dropped = false;
                }
            }
            else {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Escape)) {
                    GameLoop.World.Player.CopperCoins += (Score);
                    Reset();
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }
            }
        }
    }
}
