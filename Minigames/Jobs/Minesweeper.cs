using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using Console = SadConsole.Console;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class Minesweeper : Minigame { 
        public int Timer = 60;
        public double LastTimeTick = 0;
        public bool DisplayingScore = false;

        public Dictionary<Point, MinesweeperTile> PlayGrid = new();
        public Dictionary<Point, int> NumbersGrid = new();
        public string Difficulty = "";

        public bool ChoosingDifficulty = true;
        public bool Started = false;
        public bool Finished = false;
        public bool Lost = false;

        public int MistakesLeft = 1;
        public bool Forgiving = false;

        public int Width = 10;
        public int Height = 10;
        public int TimerStart = 30;
        public int MineCount = 10;

        List<Point> dirs = new();

        public Minesweeper() {
            dirs.Add(new(-1, 0));
            dirs.Add(new(-1, -1));
            dirs.Add(new(-1, 1));
            dirs.Add(new(1, 0));
            dirs.Add(new(1, -1));
            dirs.Add(new(1, 1));
            dirs.Add(new(0, -1));
            dirs.Add(new(0, 1));

            Reset();
        }
         
        public void Reset() { 
            Timer = 60;
            DisplayingScore = false;
            ChoosingDifficulty = true;
            LastTimeTick = 0;
            Difficulty = "";
            Finished = false;
            Lost = false;
            Started = false;
            TimerStart = 30;
            Height = 10;
            Width = 10;
            MistakesLeft = 1;
            FakeGrid();
        }

        public void SetDetails(int mineCount, int width, int height, string diff, int seconds, bool forgiving) {
            Difficulty = diff;
            Width = width;
            Height = height;
            MineCount = mineCount;
            TimerStart = seconds;
            Timer = seconds;
            ChoosingDifficulty = false;
            Started = false;
            Forgiving = forgiving;
            MistakesLeft = Forgiving ? 2 : 1;
            FakeGrid();
        }

        public void FakeGrid() {
            PlayGrid = new();
            PlayGrid.Clear();

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    PlayGrid.Add(new Point(x, y), new());
                }
            }
        }

        public void SetupGrid(Point startSpot) {  
            int placedMines = 0;

            while (placedMines < MineCount) {
                int randX = GameLoop.rand.Next(Width);
                int randY = GameLoop.rand.Next(Height);

                if (!PlayGrid[new Point(randX, randY)].Mine && new Point(randX, randY) != startSpot) {
                    placedMines++;
                    PlayGrid[new Point(randX, randY)].Mine = true;
                }
            }

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    if (PlayGrid[new Point(x, y)].Mine) {
                        Point current = new Point(x, y);
                        for (int i = 0; i < dirs.Count; i++) {
                            if (PlayGrid.ContainsKey(current + dirs[i])) {
                                PlayGrid[current + dirs[i]].Adjacent++;
                            }
                        }
                    }
                }
            }
        }

        public void MinigameClick(string ID) {
            if (ID == "Easy") { SetDetails(10, 10, 10, "Easy", 30, false); }
            else if (ID == "Medium") { SetDetails(40, 16, 16, "Medium", 60, false); }
            else if (ID == "Hard") { SetDetails(80, 30, 16, "Hard", 120, false); }
            else if (ID == "EasyF") { SetDetails(10, 10, 10, "Easy", 30, true); }
            else if (ID == "MediumF") { SetDetails(40, 16, 16, "Medium", 60, true); }
            else if (ID == "HardF") { SetDetails(80, 30, 16, "Hard", 120, true); }

            else {
                string[] split = ID.Split(",");
                int X = Int32.Parse(split[0]);
                int Y = Int32.Parse(split[1]);

                if (PlayGrid.ContainsKey(new(X, Y)) && !Finished) {
                    if (Reveal(new(X, Y))) {
                        MistakesLeft -= 1;
                        if (MistakesLeft <= 0) {
                            Finished = true;
                            Lost = true;
                        }
                    }
                    else {
                        if (PuzzleSolved()) {
                            DisplayingScore = true;
                            Finished = true;
                            Lost = false;
                        }
                    }
                }
            }
        }

        public Point GetOffset() {
            if (Difficulty == "Easy") {
                return new Point(30, 15);
            } else if (Difficulty == "Medium") {
                return new Point(27, 12);
            } else {
                return new Point(20, 12);
            }
        }

        public int GetReward() {
            if (Difficulty == "Easy") {
                return Forgiving ? 5 : 10;
            } else if (Difficulty == "Medium") {
                return Forgiving ? 10 : 20;
            } else {
                return Forgiving ? 20 : 40;
            }
        }

        public bool Reveal(Point where) {
            if (!Started) {
                SetupGrid(where); 
                Started = true;
                LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
            }


            if (PlayGrid.ContainsKey(where)) {
                PlayGrid[where].Revealed = true;
                if (PlayGrid[where].Mine)
                    return true;
                else {
                    for (int i = 0; i < dirs.Count; i++) {
                        if (PlayGrid.ContainsKey(where + dirs[i])) {
                            if (PlayGrid[where + dirs[i]].Adjacent == 0 && !PlayGrid[where + dirs[i]].Mine) {
                                for (int j = 0; j < dirs.Count; j++) {
                                    Point newPoint = (where + dirs[i]) + dirs[j];
                                    if (PlayGrid.ContainsKey(newPoint) && !PlayGrid[newPoint].Revealed)
                                        Reveal(newPoint);
                                }
                            }
                        }
                    }
                } 
            }

            return false;
        }

        public bool PuzzleSolved() {
            bool solved = true;
            
            foreach (KeyValuePair<Point, MinesweeperTile> kv in PlayGrid) {
                if (!kv.Value.Mine && !kv.Value.Revealed) {
                    solved = false;
                    break;
                }
            }

            return solved;
        }

        public void RevealAll() {
            foreach (KeyValuePair<Point, MinesweeperTile> kv in PlayGrid) {
                kv.Value.Revealed = true;
            }
        }

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.Con;

            if (!ChoosingDifficulty) {
                foreach (KeyValuePair<Point, MinesweeperTile> kv in PlayGrid) {
                    Color col = Color.Gray;

                    switch (kv.Value.Adjacent) {
                        case 1:
                            col = Color.Blue;
                            break;
                        case 2:
                            col = Color.Green;
                            break;
                        case 3:
                            col = Color.DarkRed;
                            break;
                        case 4:
                            col = Color.DarkBlue;
                            break;
                        case 5:
                            col = Color.Brown;
                            break;
                        case 6:
                            col = Color.Cyan;
                            break;
                        case 7:
                            col = Color.Yellow;
                            break;
                        case 8:
                            col = Color.DarkGray;
                            break;
                    }

                    Point offset = GetOffset();

                    if (kv.Value.Revealed) {
                        if (!kv.Value.Mine)
                            Mini.Print(offset.X + kv.Key.X, offset.Y + kv.Key.Y, kv.Value.Adjacent.ToString(), col);
                        else
                            Mini.Print(offset.X + kv.Key.X, offset.Y + kv.Key.Y, "*", Color.DarkSlateGray);
                    }
                    else {
                        if (kv.Value.Flagged)
                            Mini.Print(offset.X + kv.Key.X, offset.Y + kv.Key.Y, 36.AsString(), Color.Red);
                        else { 
                            Mini.PrintClickable(offset.X + kv.Key.X, offset.Y + kv.Key.Y, new ColoredString(254.AsString(), Color.LightGray, Color.Black), MinigameClick, kv.Key.X + "," + kv.Key.Y);
                        }
                    }
                }
                
                if (!Finished)
                    Mini.Print(0, 0, Timer.ToString().Align(HorizontalAlignment.Center, 70));

                if (Timer > 0 && !Finished) {
                    if (Started) {
                        if (LastTimeTick + 1000 < GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                            LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                            Timer--;
                        }

                        if (Forgiving) {
                            if (MistakesLeft == 2) {
                                Mini.Print(0, 2, "You can make one more mistake!".Align(HorizontalAlignment.Center, 70), Color.Lime);
                            }
                            else {
                                Mini.Print(0, 2, "Don't make any more mistakes!".Align(HorizontalAlignment.Center, 70), Color.Red);
                            }
                        } else {
                            Mini.Print(0, 2, "Don't make any mistakes!".Align(HorizontalAlignment.Center, 70), Color.Red);
                        }
                    }
                }
                else {
                    if (!DisplayingScore) {
                        DisplayingScore = true;
                        Finished = true;
                        if (Timer <= 0)
                            Lost = true;
                    }

                    if (!Lost) {
                        Mini.Print(0, 1, ("You earned " + GetReward() + " copper!").Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 2, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                    }
                    else {
                        Mini.Print(0, 1, ("Better luck next time!").Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 2, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                    }
                }
            } else {
                Mini.PrintClickable(0, 3, "Easy (10x10, 10 Mines, 30 seconds)".Align(HorizontalAlignment.Center, 70), MinigameClick, "Easy");
                Mini.PrintClickable(0, 5, "Medium (16x16, 40 Mines, 60 seconds)".Align(HorizontalAlignment.Center, 70), MinigameClick, "Medium");
                Mini.PrintClickable(0, 7, "Hard (30x16, 80 Mines, 120 seconds)".Align(HorizontalAlignment.Center, 70), MinigameClick, "Hard");

                Mini.PrintClickable(0, 11, "Easy (Easy, but you can make one mistake)".Align(HorizontalAlignment.Center, 70), MinigameClick, "EasyF");
                Mini.PrintClickable(0, 13, "Medium (Medium, but you can make one mistake)".Align(HorizontalAlignment.Center, 70), MinigameClick, "MediumF");
                Mini.PrintClickable(0, 15, "Hard (Hard, but you can make one mistake)".Align(HorizontalAlignment.Center, 70), MinigameClick, "HardF");
            }
        }
         
        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Reset();
                Close();
            }

            if (Started) {
                if (!DisplayingScore && !ChoosingDifficulty && !Finished) {
                    Point offset = GetOffset();
                    if (GameHost.Instance.Mouse.RightClicked) {
                        if (PlayGrid.ContainsKey(mousePos - offset)) {
                            PlayGrid[mousePos - offset].Flagged = !PlayGrid[mousePos - offset].Flagged;
                        }
                    }

                }
                else if (DisplayingScore) {
                    if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                        GameLoop.World.Player.Zeri += GetReward();
                        Reset();
                        GameLoop.UIManager.Minigames.ToggleMinigame("None");
                    }
                }
            }
        }
    }
}
