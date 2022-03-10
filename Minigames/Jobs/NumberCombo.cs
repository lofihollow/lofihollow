using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using Console = SadConsole.Console;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class NumberCombo : Minigame {
        public int Timer = 60;
        public double LastTimeTick = 0;
        public bool DisplayingScore = false;

        public bool Finished = false;

        public Dictionary<Point, int> PlayGrid = new();
         

        public NumberCombo() {
            Reset();
        }

        public void Reset() {
            Timer = 60;
            DisplayingScore = false;
            LastTimeTick = 0;
            Finished = false;

            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 4; y++) {
                    if (!PlayGrid.ContainsKey(new(x, y)))
                        PlayGrid.Add(new(x, y), 0);
                    else
                        PlayGrid[new(x, y)] = 0;
                }
            }

            Place();
            Place();
        }

        public bool Place() {
            List<Point> OpenSpots = AllEmptySpots();

            if (OpenSpots.Count > 0) {
                Point random = OpenSpots[GameLoop.rand.Next(OpenSpots.Count)];
                PlayGrid[random] = 1;
            } else {
                // Check for valid moves
            }

            return false;
        }

        public List<Point> AllEmptySpots() {
            List<Point> spots = new();

            foreach(KeyValuePair<Point, int> kv in PlayGrid) {
                if (kv.Value == 0)
                    spots.Add(kv.Key);
            }

            return spots;
        }

        public bool CanShift(int x, int y, Point dir) {
            Point MoveTo = new Point(x, y) + dir;
            if (PlayGrid.ContainsKey(MoveTo) && PlayGrid.ContainsKey(new(x, y)) && PlayGrid[new(x, y)] != 0) {
                if (PlayGrid[MoveTo] == 0 || PlayGrid[new Point(x, y)] == PlayGrid[MoveTo]) {
                    return true;
                }
            }

            return false;
        } 

        public void CheckGameOver() {
            bool anyCanMove = false;

            foreach(KeyValuePair<Point, int> kv in PlayGrid) {
                if (CanShift(kv.Key.X, kv.Key.Y, new(0, -1)))
                    anyCanMove = true;
                if (CanShift(kv.Key.X, kv.Key.Y, new(0, 1)))
                    anyCanMove = true;
                if (CanShift(kv.Key.X, kv.Key.Y, new(-1, 0)))
                    anyCanMove = true;
                if (CanShift(kv.Key.X, kv.Key.Y, new(1, 0)))
                    anyCanMove = true;
            }

            if (!anyCanMove) {
                Finished = true;
                DisplayingScore = true;
            }
        }

        public void ShiftColumnsUp() {
            bool moved = false;
            for (int col = 0; col < 4; col++) {
                for (int row = 3; row >= 0; row--) {
                    while (CanShift(col, row, new Point(0, -1))) {  
                        Point MoveTo = new Point(col, row) + new Point(0, -1);
                        Point Current  = new Point(col, row);
                        if (PlayGrid.ContainsKey(MoveTo)) {
                            if (PlayGrid[MoveTo] == 0) {
                                PlayGrid[MoveTo] = PlayGrid[Current];
                                PlayGrid[Current] = 0;
                                moved = true;
                            } else if (PlayGrid[MoveTo] == PlayGrid[Current]) {
                                PlayGrid[MoveTo]++;
                                PlayGrid[Current] = 0;
                                moved = true;
                            }
                        } 
                    }
                }
            }

            if (moved)
                Place();
            else
                CheckGameOver();
        }

        public void ShiftColumnsDown() {
            bool moved = false;
            for (int col = 0; col < 4; col++) {
                for (int row = 0; row < 4; row++) {
                    while (CanShift(col, row, new Point(0, 1))) {
                        Point MoveTo = new Point(col, row) + new Point(0, 1);
                        Point Current = new Point(col, row);
                        if (PlayGrid.ContainsKey(MoveTo)) {
                            if (PlayGrid[MoveTo] == 0) {
                                PlayGrid[MoveTo] = PlayGrid[Current];
                                PlayGrid[Current] = 0;
                                moved = true;
                            }
                            else if (PlayGrid[MoveTo] == PlayGrid[Current]) {
                                PlayGrid[MoveTo]++;
                                PlayGrid[Current] = 0;
                                moved = true;
                            }
                        }
                    }
                }
            }
            if (moved)
                Place();
            else
                CheckGameOver();
        }

        public void ShiftRowsLeft() {
            bool moved = false;
            for (int row = 0; row < 4; row++) {
                for (int col = 3; col >= 0; col--) {
                    while (CanShift(col, row, new Point(-1, 0))) {  
                        Point MoveTo = new Point(col, row) + new Point(-1, 0);
                        Point Current = new Point(col, row);
                        if (PlayGrid.ContainsKey(MoveTo)) {
                            if (PlayGrid[MoveTo] == 0) {
                                PlayGrid[MoveTo] = PlayGrid[Current];
                                PlayGrid[Current] = 0;
                                moved = true;
                            }
                            else if (PlayGrid[MoveTo] == PlayGrid[Current]) {
                                PlayGrid[MoveTo]++;
                                PlayGrid[Current] = 0;
                                moved = true;
                            }
                        } 
                    }
                }
            }
            if (moved)
                Place();
            else
                CheckGameOver();
        }

        public void ShiftRowsRight() {
            bool moved = false;
            for (int row = 0; row < 4; row++) {
                for (int col = 0; col < 4; col++) {
                    while(CanShift(col, row, new Point(1, 0))) {
                        Point MoveTo = new Point(col, row) + new Point(1, 0);
                        Point Current = new Point(col, row);
                        if (PlayGrid.ContainsKey(MoveTo)) {
                            if (PlayGrid[MoveTo] == 0) {
                                PlayGrid[MoveTo] = PlayGrid[Current];
                                PlayGrid[Current] = 0;
                                moved = true;
                            }
                            else if (PlayGrid[MoveTo] == PlayGrid[Current]) {
                                PlayGrid[MoveTo]++;
                                PlayGrid[Current] = 0;
                                moved = true;
                            }
                        }
                    }
                }
            }
            if (moved)
                Place();
            else
                CheckGameOver();
        }

        public int GetReward() {
            int highest = 0;

            foreach (KeyValuePair<Point, int> kv in PlayGrid) {
                if (kv.Value > highest)
                    highest = kv.Value;
            }

            return highest * 2;
        }

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.Con;


            Mini.DrawLine(new Point(32, 18), new Point(32, 21), 179, Color.White);
            Mini.DrawLine(new Point(37, 18), new Point(37, 21), 179, Color.White);
            Mini.DrawLine(new Point(33, 17), new Point(36, 17), 196, Color.White);
            Mini.DrawLine(new Point(33, 22), new Point(36, 22), 196, Color.White);


            foreach (KeyValuePair<Point, int> kv in PlayGrid) {
                if (kv.Value == 0) {
                    Mini.Print(33 + kv.Key.X, 18 + kv.Key.Y, 254.AsString(), Color.DarkSlateGray);
                }
                else if (kv.Value < 10) {
                    Mini.Print(33 + kv.Key.X, 18 + kv.Key.Y, kv.Value.ToString());
                }
                else {
                    int over10 = 357 + (kv.Value - 10);
                    Mini.Print(33 + kv.Key.X, 18 + kv.Key.Y, over10.AsString());
                }
            }

            if (Timer > 0 && !Finished) { 
                if (LastTimeTick + 1000 < GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                    Timer--;
                }
                Mini.Print(0, 1, Timer.ToString().Align(HorizontalAlignment.Center, 70), Color.White);
            }
            else {
                if (!DisplayingScore) {
                    DisplayingScore = true;
                    Finished = true; 
                }

                if (GetReward() < 20) {
                    if (Finished && Timer == 0) {
                        Mini.Print(0, 1, ("Out of time!").Align(HorizontalAlignment.Center, 70), Color.Yellow);
                        Mini.Print(0, 3, ("You earned " + GetReward() + " copper!").Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 4, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                    }
                    else if (Finished && Timer > 0) {
                        Mini.Print(0, 1, ("No more moves possible.").Align(HorizontalAlignment.Center, 70), Color.Red);
                        Mini.Print(0, 3, ("You earned " + GetReward() + " copper!").Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 4, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                    }
                } else {
                    Mini.Print(0, 1, ("You made a 10 - you win!").Align(HorizontalAlignment.Center, 70), Color.Lime);
                    Mini.Print(0, 3, ("You earned " + GetReward() + " copper!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 4, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                }
            } 
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Reset();
                Close();
            }

            if (!Finished) { 
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.A) || GameHost.Instance.Keyboard.IsKeyPressed(Key.Left)) {
                    ShiftRowsLeft();
                }
                else if (GameHost.Instance.Keyboard.IsKeyPressed(Key.D) || GameHost.Instance.Keyboard.IsKeyPressed(Key.Right)) {
                    ShiftRowsRight();
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.W) || GameHost.Instance.Keyboard.IsKeyPressed(Key.Up)) {
                    ShiftColumnsUp();
                }
                else if (GameHost.Instance.Keyboard.IsKeyPressed(Key.S) || GameHost.Instance.Keyboard.IsKeyPressed(Key.Down)) {
                    ShiftColumnsDown();
                } 
            }
            else {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    GameLoop.World.Player.CopperCoins += GetReward();
                    Reset();
                    Close();
                }
            } 
        }
    }
}
