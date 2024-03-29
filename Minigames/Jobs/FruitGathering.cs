﻿using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Steamworks.Data;
using System.Collections.Generic;
using Color = SadRogue.Primitives.Color;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class FruitGathering : Minigame {
        public int Score = 0;

        public int Timer = 61;
        public double LastTimeTick = 0;
        public bool DisplayingScore = false; 
        public bool ShowingGlobalLeader = false;

        public int playerX = 25;
        public int playerY = 34;

        public List<FallingObject> Objects = new();


        public FruitGathering() {
        }

        public void Gravity() {
            for (int i = Objects.Count - 1; i >= 0; i--) {
                if (Objects[i].Y >= 35) {
                    if (Objects[i].Name != "Spider") {
                        GameLoop.SoundManager.PlaySound("failureJingle");
                        Score--;
                    } else { 
                        GameLoop.SoundManager.PlaySound("successJingle");
                        Score++;
                    }

                    Objects.RemoveAt(i); 
                } else {
                    if (Objects[i].LastTimeMoved + Objects[i].FallingSpeed < SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                        Objects[i].LastTimeMoved = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                        Objects[i].Y++;

                        if (Objects[i].Y == playerY - 1 && Objects[i].X >= playerX - 1 && Objects[i].X <= playerX + 1) {
                            if (Objects[i].Name != "Spider") {
                                Objects.RemoveAt(i);
                                Score++;
                                GameLoop.SoundManager.PlaySound("successJingle");
                            } else {
                                Objects.RemoveAt(i);
                                Score--;
                                GameLoop.SoundManager.PlaySound("failureJingle");
                            }
                        }
                    }
                }
            }
        }


        public void Reset() {
            Score = 0;
            Timer = 61;
            DisplayingScore = false;
        }

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.Con;

            if (Timer > 0) {
                Mini.Print(0, 0, Timer.ToString().Align(HorizontalAlignment.Center, 70));
                Mini.Print(0, 2, ("Score: " + Score).Align(HorizontalAlignment.Center, 70));

                Gravity();

                if (LastTimeTick + 1000 < GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                    Timer--;

                    Objects.Add(new FallingObject()); 
                }

                for (int i = 0; i < Objects.Count; i++) {
                    Mini.Print(Objects[i].X + 10, Objects[i].Y, Objects[i].Glyph.AsString(), Objects[i].Col);
                }

                Mini.DrawLine(new Point(10, 35), new Point(60, 35), 196, Color.Green);
                Mini.Print(playerX + 10, playerY, "@");
                Mini.Print(playerX + 9, playerY - 1, "\\");
                Mini.Print(playerX + 11, playerY - 1, "/");
            }
            else {
                if (!DisplayingScore) {
                    DisplayingScore = true; 
                    GameLoop.SteamManager.MostRecentResult = GameLoop.SteamManager.PostHighscore("00-WeeklyFruitCatch", Score);

                    var scores = GameLoop.SteamManager.FruitCatchLB.GetScoresAsync(10);

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
                    Mini.Print(0, 3, ("You earned " + (Score / 2) + " copper!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                }
                else {
                    Mini.Print(0, 3, ("Better luck next time!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
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

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Reset();
                Close();
            }

            if (!DisplayingScore) {
                Point offset = mousePos - new Point(5, 5);
                if (GameHost.Instance.Mouse.LeftClicked) {
                }

                if (GameHost.Instance.Keyboard.IsKeyDown(Key.A))
                    playerX--;
                if (GameHost.Instance.Keyboard.IsKeyDown(Key.D))
                    playerX++;
            }
            else {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    GameLoop.World.Player.Zeri += (Score / 2);
                    Reset();
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }
            }
        }
    }
}
