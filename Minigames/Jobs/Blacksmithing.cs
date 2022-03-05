using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class Blacksmithing : Minigame { 
        public int Score = 0;

        public int Timer = 61;
        public double LastTimeTick = 0;
        public bool DisplayingScore = false;

        public bool ShowingGlobalLeader = false;


        Dictionary<Point, BlacksmithNode> PlayGrid = new();
        public int GridWidth = 60;
        public int GridHeight = 30;


        public Blacksmithing() {
            PlayGrid.Clear();
            for (int x = 0; x < GridWidth; x++) {
                for (int y = 0; y < GridHeight; y++) {
                    PlayGrid.Add(new Point(x, y), new BlacksmithNode());
                }
            }
        }

        public void Reset() {
            Score = 0;
            Timer = 61;
            DisplayingScore = false;
            PlayGrid.Clear();
            GameLoop.SteamManager.MostRecentResult = null;
            for (int x = 0; x < GridWidth; x++) {
                for (int y = 0; y < GridHeight; y++) {
                    PlayGrid.Add(new Point(x, y), new BlacksmithNode());
                }
            } 
        }

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.Con; 

            if (Timer > 0) {
                Mini.Print(0, 0, Timer.ToString().Align(HorizontalAlignment.Center, 70));
                Mini.Print(0, 2, ("Score: " + Score).Align(HorizontalAlignment.Center, 70));
                if (LastTimeTick + 1000 < GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                    Timer--; 
                }

                foreach (KeyValuePair<Point, BlacksmithNode> kv in PlayGrid) {
                    if (kv.Value.Symbol != " ") {
                        if (PlayGrid.ContainsKey(kv.Key + new Point(0, 1)) && PlayGrid[kv.Key + new Point(0, 1)].Symbol == " ") {
                            PlayGrid[kv.Key + new Point(0, 1)] = new(kv.Value);
                            kv.Value.Symbol = " ";
                        }
                        else {
                            Mini.Print(5 + kv.Key.X, 5 + kv.Key.Y, kv.Value.Glyph.AsString(), kv.Value.Col);
                        }
                    }
                }

                for (int x = 0; x < GridWidth; x++) {
                    if (PlayGrid.ContainsKey(new Point(x, GridHeight - 1))) {
                        if (PlayGrid[new Point(x, GridHeight - 1)].Symbol == " ") {
                            if (ColumnEmpty(x)) {
                                for (int y = 0; y < GridHeight; y++) {
                                    if (PlayGrid.ContainsKey(new Point(x - 1, y)) && PlayGrid[new Point(x - 1, y)].Symbol != " ") {
                                        PlayGrid[new Point(x, y)] = new(PlayGrid[new Point(x - 1, y)]);
                                        PlayGrid[new Point(x - 1, y)].Symbol = " ";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else {
                if (!DisplayingScore) {
                    DisplayingScore = true;
                    GameLoop.SteamManager.PostHighscore("Blacksmithing", Score);
                }

                if (Score > 0) {
                    Mini.Print(0, 3, ("You earned " + (Score / 10) + " copper!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                }
                else {
                    Mini.Print(0, 3, ("Better luck next time!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                }

                if (GameLoop.SteamManager.MostRecentResult != null) {
                    HighscoreResult res = GameLoop.SteamManager.MostRecentResult;
                    if (res.Success == 1) {
                        Mini.Print(20, 10, "Successfully Posted Score!");
                        Mini.Print(20, 12, "  Global Rank: " + res.GlobalRank);
                        Mini.Print(20, 13, "Highest Score: " + res.HighScore);
                        Mini.Print(20, 17, "     !NEW HIGH SCORE!    ");

                        if (!ShowingGlobalLeader) {
                            Mini.PrintClickable(20, 19, "[Show Global Leaderboards]", MinigameClick, "ShowGlobal");
                        } else {
                            Mini.PrintClickable(20, 19, "[Hide Global Leaderboards]", MinigameClick, "HideGlobal");

                            for (int i = 0; i < GameLoop.SteamManager.GlobalLeader.Count; i++) {
                                LeaderboardSlot thisOne = GameLoop.SteamManager.GlobalLeader[i];
                                Mini.Print(20, 21 + i, thisOne.Rank + " | " + thisOne.Name.Align(HorizontalAlignment.Center, 20) + " | " + thisOne.Score);
                            }
                        }
                    } else {
                        Mini.Print(20, 10, "Couldn't post highscore");
                        Mini.Print(20, 12, "Score: " + Score);
                    }
                }
            }
        }

        public void MinigameClick(string ID) {
            if (ID == "ShowGlobal") {
                ShowingGlobalLeader = true;
            }
            else if (ID == "HideGlobal") {
                ShowingGlobalLeader = false;
            }
            
        }

        public bool ColumnEmpty(int col) {
            for (int y = 0; y < GridHeight; y++) {
                if (PlayGrid.ContainsKey(new Point(col, y))) {
                    if (PlayGrid[new Point(col, y)].Symbol != " ")
                        return false;
                }
            }

            return true;
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Reset();
                Close();
            }

            if (!DisplayingScore) {
                Point offset = mousePos - new Point(5, 5);
                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (PlayGrid.ContainsKey(offset) && PlayGrid[offset].Symbol != " ") {
                        Score += PlayGrid[offset].ClickClear(offset.X, offset.Y, PlayGrid, true);

                    }
                }
            }
            else {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    GameLoop.World.Player.CopperCoins += (Score / 10);
                    Reset();
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }
            }
        }
    }
}
