using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Picross {
    public class Picross : Minigame {
        public int Timer = 61;
        public double LastTimeTick = 0;
        public bool DisplayingComplete = false;
        public bool Playing = false;

        public PicrossPuzzle Current; 
        public bool DragMode = false;
        public bool SetMode = false;
        public bool Solved = false;
        public bool PuzzleWasRandom = false;

        public Picross() {
        }

        public void Setup(string diff) {
            if (diff == "Easy" || diff == "Medium" || diff == "Hard") {
                List<PicrossPuzzle> possible = new();

                foreach (KeyValuePair<string, PicrossPuzzle> kv in GameLoop.World.picrossLibrary) {
                    if (kv.Value.Difficulty == diff) {
                        possible.Add(new PicrossPuzzle(kv.Value));
                    }
                }

                PuzzleWasRandom = false;
                Current = possible[GameLoop.rand.Next(possible.Count)];
                Playing = true;
            } else {
                PuzzleWasRandom = true;
                int size = diff == "EasyRandom" ? 5 : diff == "MediumRandom" ? 10 : 15;
                Current = new(size);
                Playing = true;
            }
        }


        public void Reset() {
            Timer = 61;
            DisplayingComplete = false;
            Current = null;
            Playing = false;
            SetMode = false;
            Solved = false;
        }

        public void MinigameClick(string ID) {
            if (ID == "Easy") { Setup("Easy"); Timer = 31; }
            else if (ID == "Medium") { Setup("Medium"); Timer = 61; }
            else if (ID == "Hard") { Setup("Hard"); Timer = 121; }
            else if (ID == "EasyRandom") { Setup("EasyRandom"); Timer = 31; }
            else if (ID == "MediumRandom") { Setup("MediumRandom"); Timer = 61; }
            else if (ID == "HardRandom") { Setup("HardRandom"); Timer = 121; }
        } 

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.Con;
            Mini.Clear();

            if (!Playing) {
                Mini.Print(0, 10, "Select a Difficulty: ".Align(HorizontalAlignment.Center, 70));
                Mini.PrintClickable(0, 12, "Easy (5x5)".Align(HorizontalAlignment.Center, 70), MinigameClick, "Easy");
                Mini.PrintClickable(0, 14, "Medium (10x10)".Align(HorizontalAlignment.Center, 70), MinigameClick, "Medium");
                Mini.PrintClickable(0, 16, "Hard (15x15)".Align(HorizontalAlignment.Center, 70), MinigameClick, "Hard");

                Mini.Print(0, 22, "Or Random Puzzles (Double Reward): ".Align(HorizontalAlignment.Center, 70));
                Mini.PrintClickable(0, 24, "Random Easy (5x5)".Align(HorizontalAlignment.Center, 70), MinigameClick, "EasyRandom");
                Mini.PrintClickable(0, 26, "Random Medium (10x10)".Align(HorizontalAlignment.Center, 70), MinigameClick, "MediumRandom");
                Mini.PrintClickable(0, 28, "Random Hard (15x15)".Align(HorizontalAlignment.Center, 70), MinigameClick, "HardRandom");
            }
            else {
                if (Timer > 0 && !DisplayingComplete) {
                    Mini.Print(0, 0, Timer.ToString().Align(HorizontalAlignment.Center, 70));
                    if (LastTimeTick + 1000 < GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                        LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                        Timer--;
                    }

                    Mini.Print(0, 2, "Left Click: Mark tiles as part of the solution.");
                    Mini.Print(0, 3, "Right Click: Cross tiles off");
                    Mini.Print(0, 4, "Click and drag to apply to multiple tiles."); 


                    for (int row = 0; row < Current.Height; row++) {
                        List<int> clues = Current.RowClues(row);
                        bool rowDone = Current.RowSolved(row);
                        if (clues.Count == 0)
                            clues.Add(0);

                        int x = 35 - (clues.Count * 2); 

                        for (int i = 0; i < clues.Count; i++) {
                            // 357
                            if (clues[i] < 10)
                                Mini.Print(x + (i * 2), 20 + row, clues[i].ToString(), rowDone ? Color.Lime : Color.White);
                            else {
                                int index = 357 + (clues[i] - 10);
                                Mini.Print(x + (i * 2), 20 + row, index.AsString(), rowDone ? Color.Lime : Color.White);
                            }

                        }
                    }

                    for (int col = 0; col < Current.Height; col++) {
                        List<int> clues = Current.ColumnClues(col); 
                        bool colDone = Current.ColumnSolved(col);
                        if (clues.Count == 0)
                            clues.Add(0);

                        int y = 20 - (clues.Count * 2);

                        for (int i = 0; i < clues.Count; i++) { 
                            if (clues[i] < 10)
                                Mini.Print(35 + col, y + (i * 2), clues[i].ToString(), colDone ? Color.Lime : Color.White);
                            else {
                                int index = 357 + (clues[i] - 10);
                                Mini.Print(35 + col, y + (i * 2), index.AsString(), colDone ? Color.Lime : Color.White);
                            }

                        }
                    }

                    for (int x = 0; x < Current.Width; x++) {
                        for (int y = 0; y < Current.Height; y++) {
                            PicrossTile tile = Current.Grid[x + (y * Current.Width)];
                            Mini.Print(35 + x, 20 + y, tile.GetAppearance(false));
                        }
                    }  
                }
                else {
                    DisplayingComplete = true;
                    if (Solved) {
                        int amount = Current.Difficulty == "Easy" ? 5 : Current.Difficulty == "Medium" ? 10 : 20;

                        if (PuzzleWasRandom)
                            amount *= 2;

                        Mini.Print(0, 10, "You solved the puzzle!".Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 12, ("You got " + amount + " coppers!").Align(HorizontalAlignment.Center, 70));
                    } else {
                        int amount = Current.Difficulty == "Easy" ? 5 : Current.Difficulty == "Medium" ? 10 : 20; 
                        
                        if (PuzzleWasRandom)
                            amount *= 2;

                        double completion = Current.Completion();
                        int Reward = (int)System.Math.Floor(completion * (double)amount);

                        Mini.Print(0, 10, "Better luck next time!".Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 12, ("For completing " + System.Math.Floor(completion * 100) + "% of the puzzle, you earn a reduced".ToString()).Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 13, ("prize of " + Reward + " coppers.").Align(HorizontalAlignment.Center, 70));
                    }

                    Mini.Print(0, 15, "Press [SPACE] to exit.".Align(HorizontalAlignment.Center, 70));



                    Mini.Print(35 - Current.Width, 19, Current.Name.Align(HorizontalAlignment.Center, Current.Width * 3));
                    for (int x = 0; x < Current.Width; x++) {
                        for (int y = 0; y < Current.Height; y++) {
                            PicrossTile tile = Current.Grid[x + (y * Current.Width)];
                            Mini.Print(35 + x, 20 + y, tile.GetAppearance(true));
                        }
                    }
                }
            }
        } 

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Reset();
                Close();
            }

            if (!DisplayingComplete) {
                Point offset = mousePos - new Point(35, 20);
                if (Current != null) {
                    if (GameHost.Instance.Mouse.LeftButtonDown) {
                        if (offset.X >= 0 && offset.Y >= 0 && offset.X < Current.Width && offset.Y < Current.Height) {
                            if (!SetMode) {
                                SetMode = true;
                                DragMode = !Current.Grid[offset.ToIndex(Current.Width)].Checked;
                            }
                            if (Current.Grid[offset.ToIndex(Current.Width)].Checked != DragMode) {
                                Current.Grid[offset.ToIndex(Current.Width)].Checked = DragMode;
                                Current.Grid[offset.ToIndex(Current.Width)].Crossed = false;
                            }
                        }
                    }
                    else if (GameHost.Instance.Mouse.RightButtonDown) {
                        if (offset.X >= 0 && offset.Y >= 0 && offset.X < Current.Width && offset.Y < Current.Height) {
                            if (!SetMode) {
                                SetMode = true;
                                DragMode = !Current.Grid[offset.ToIndex(Current.Width)].Crossed;
                            }
                            if (Current.Grid[offset.ToIndex(Current.Width)].Crossed != DragMode) {
                                Current.Grid[offset.ToIndex(Current.Width)].Checked = false;
                                Current.Grid[offset.ToIndex(Current.Width)].Crossed = DragMode;
                            }
                        }
                    }
                    else {
                        if (SetMode == true) { // They had to have changed tiles up to now
                            if (Current.PuzzleSolved()) {
                                Solved = true;
                                DisplayingComplete = true;
                            }
                        }
                        SetMode = false;
                    }
                }
            }
            else {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    if (Solved) {
                        int amount = Current.Difficulty == "Easy" ? 5 : Current.Difficulty == "Medium" ? 10 : 20;
                        if (PuzzleWasRandom)
                            amount *= 2;

                        GameLoop.World.Player.CopperCoins += amount;
                    } else {
                        int amount = Current.Difficulty == "Easy" ? 5 : Current.Difficulty == "Medium" ? 10 : 20;
                        if (PuzzleWasRandom)
                            amount *= 2;

                        double completion = Current.Completion();
                        int Reward = (int)System.Math.Ceiling(completion * (double)amount);

                        GameLoop.World.Player.CopperCoins += Reward;
                    }
                    
                    Reset();
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }
            }
        }
    }
}
