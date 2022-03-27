using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Steamworks.Data;
using System.Collections.Generic;
using Color = SadRogue.Primitives.Color;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class MailSort : Minigame {
        public int Score = 0;

        public int Timer = 61;
        public double LastTimeTick = 0;
        public bool DisplayingScore = false;

        public bool ShowingGlobalLeader = false;

        public int pX = 1;
        public int pY = 1;
        public int CurrentTarget = -1;

        public string[] Boxes = { 20.AsString(),  19.AsString(),  18.AsString(),
                                   4.AsString(),   5.AsString(),   6.AsString(),
                                   3.AsString(), 281.AsString(), 349.AsString() };
        public Color[] BoxColors = { Color.Gold, Color.Cyan, Color.Red,
                                    Color.LimeGreen, Color.Gray, Color.DarkGreen,
                                    Color.DeepPink, Color.SandyBrown, Color.White };

        public MailSort() {
            Reset();
        }

        public void Reset() {
            Score = 0;
            Timer = 61;
            DisplayingScore = false; 
            GameLoop.SteamManager.MostRecentResult = null;
            CurrentTarget = GameLoop.rand.Next(9);
            pX = 1;
            pY = 1;
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

                for (int x = 0; x < 3; x++) {
                    for (int y = 0; y < 3; y++) {
                        Helper.DrawBox(Mini, 26 + (x * 6), 7 + (y * 6), 3, 3);
                    }
                }

                Mini.Print(0, 38, "Arrow keys or WASD to move, SPACE to sort");
                Mini.Print(0, 39, "Match shapes and colors to earn points");

                Mini.Print(28, 7, 20.AsString(), Color.Gold);
                Mini.Print(34, 7, 19.AsString(), Color.Cyan);
                Mini.Print(40, 7, 18.AsString(), Color.Red);
                Mini.Print(28, 13, 4.AsString(), Color.LimeGreen);
                Mini.Print(34, 13, 5.AsString(), Color.Gray);
                Mini.Print(40, 13, 6.AsString(), Color.DarkGreen);
                Mini.Print(28, 19, 3.AsString(), Color.DeepPink);
                Mini.Print(34, 19, 281.AsString(), Color.SandyBrown);
                Mini.Print(40, 19, 349.AsString(), Color.White);

                Mini.Print(28 + (pX * 6), 9 + (pY * 6), "X");


                if (CurrentTarget >= 0 && CurrentTarget < Boxes.Length) {
                    Helper.DrawBox(Mini, 31, 27, 5, 2, 241, 213, 146);
                    Mini.Print(32, 28, Boxes[CurrentTarget], BoxColors[CurrentTarget]); 
                }
            }
            else {
                if (!DisplayingScore) {
                    DisplayingScore = true;
                    GameLoop.SteamManager.MostRecentResult = GameLoop.SteamManager.PostHighscore("MailSort", Score);

                    var scores = GameLoop.SteamManager.MailSortLB.GetScoresAsync(10);

                    if (scores.Result != null) {
                        GameLoop.SteamManager.GlobalLeader.Clear();

                        for (int i = 0; i < scores.Result.Length; i++) {
                            LeaderboardEntry entry = scores.Result[i];
                            LeaderboardSlot newSlot = new();
                            newSlot.Rank = entry.GlobalRank;
                            newSlot.Score = entry.Score;
                            newSlot.Name = entry.User.Name;
                            GameLoop.SteamManager.GlobalLeader.Add(newSlot);
                        }
                    }
                }

                if (Score > 0) {
                    Mini.Print(0, 3, ("You earned " + (Score / 4) + " copper!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press ESC to close]").Align(HorizontalAlignment.Center, 70));
                }
                else {
                    Mini.Print(0, 3, ("Better luck next time!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press ESC to close]").Align(HorizontalAlignment.Center, 70));
                }

                if (GameLoop.SteamManager.MostRecentResult != null) {
                    HighscoreResult res = GameLoop.SteamManager.MostRecentResult;


                    if (res.Success) {
                        Mini.Print(20, 10, "Successfully Posted Score!");
                        if (res.NewHigh)
                            Mini.Print(20, 12, "  Global Rank: " + res.GlobalRank);
                        else
                            Mini.Print(20, 12, "  Global Rank: " + res.PreviousRank);

                        Mini.Print(20, 13, "Highest Score: " + res.HighScore);

                        if (res.NewHigh)
                            Mini.Print(20, 17, "     !NEW HIGH SCORE!    ");

                        if (!ShowingGlobalLeader) {
                            Mini.PrintClickable(20, 19, "[Show Global Leaderboards]", MinigameClick, "ShowGlobal");
                        }
                        else {
                            Mini.PrintClickable(20, 19, "[Hide Global Leaderboards]", MinigameClick, "HideGlobal");

                            for (int i = 0; i < GameLoop.SteamManager.GlobalLeader.Count; i++) {
                                LeaderboardSlot thisOne = GameLoop.SteamManager.GlobalLeader[i];
                                Mini.Print(20, 21 + i, thisOne.Rank + " | " + thisOne.Name.Align(HorizontalAlignment.Center, 20) + " | " + thisOne.Score);
                            }
                        }
                    }
                    else {
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

        public void PlaceLetter(int x, int y) {
            int index = x + (y * 3);

            if (index != CurrentTarget) {
                GameLoop.SoundManager.PlaySound("failureJingle");
                Score--;
            }
            else {
                GameLoop.SoundManager.PlaySound("successJingle");
                Score++;
            }

            CurrentTarget = GameLoop.rand.Next(9);
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Reset();
                Close();
            }

            if (!DisplayingScore) { 
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Left) || GameHost.Instance.Keyboard.IsKeyPressed(Key.A)) {
                    if (pX > 0)
                        pX--;
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Right) || GameHost.Instance.Keyboard.IsKeyPressed(Key.D)) {
                    if (pX < 2)
                        pX++;
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Up) || GameHost.Instance.Keyboard.IsKeyPressed(Key.W)) {
                    if (pY > 0)
                        pY--;
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Down) || GameHost.Instance.Keyboard.IsKeyPressed(Key.S)) {
                    if (pY < 2)
                        pY++;
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    PlaceLetter(pX, pY);
                }

            }
            else {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Escape)) {
                    GameLoop.World.Player.CopperCoins += (Score / 4);
                    Reset();
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }
            }
        }
    }
}
