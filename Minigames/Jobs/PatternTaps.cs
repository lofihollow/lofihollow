using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Steamworks.Data;
using System.Collections.Generic;
using Color = SadRogue.Primitives.Color;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class PatternTaps : Minigame {
        public int Timer = 61;
        public double LastTimeTick = 0;

        public int Score = 0;

        public bool DisplayingScore = false;
        public bool GuessedCorrectly = false; 

        public List<string> CurrentPattern = new();
        public List<string> Pattern = new();
        public int position = 0;

        public PatternTaps() {
            Reset();
        }

        public void Reset() {
            Timer = 61;
            Score = 0;
            DisplayingScore = false;  
            SetPattern();
        }

        public void SetPattern() {
            Pattern.Clear();
            CurrentPattern.Clear();
            position = 0;

            for (int i = 0; i < Score + 1; i++) {
                int random = GameLoop.rand.Next(4);
                if (random == 0)
                    CurrentPattern.Add(9.AsString());
                else if (random == 1)
                    CurrentPattern.Add(10.AsString());
                else if (random == 2)
                    CurrentPattern.Add(11.AsString());
                else
                    CurrentPattern.Add(12.AsString());
            }

            SyncLists();
        }

        public void SyncLists() {
            position = 0;
            Pattern.Clear();

            for (int i = 0; i < CurrentPattern.Count; i++) {
                Pattern.Add(CurrentPattern[i]);
            }
        }

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.Con;
             
            if (Timer > 0) {
                Mini.Print(0, 0, Timer.ToString().Align(HorizontalAlignment.Center, 70));
                Mini.Print(0, 1, ("Score: " + Score).Align(HorizontalAlignment.Center, 70));
                if (LastTimeTick + 1000 < GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                    Timer--;
                }

                string patt = "";

                for (int i = 0; i < Pattern.Count; i++) {
                    patt += Pattern[i];
                }

                Mini.Print(0, 10, patt.Align(HorizontalAlignment.Center, 70));

                Mini.Print(0, 37, "Tap the arrow keys or WASD in order from left to right.");
                Mini.Print(0, 39, "Mistakes reset the current pattern.");
            }
            else {
                if (!DisplayingScore) {
                    DisplayingScore = true;
                    if (Score > 0)
                        GameLoop.SoundManager.PlaySound("successJingle");
                    else
                        GameLoop.SoundManager.PlaySound("failureJingle");
                } 

                Mini.Print(0, 3, ("You finished " + Score + " patterns.").Align(HorizontalAlignment.Center, 70));
                Mini.Print(0, 5, ("You earned " + (Score / 2) + " copper!").Align(HorizontalAlignment.Center, 70));
                Mini.Print(0, 38, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70)); 
            } 
        }

        public void MinigameClick(string ID) {
        }


        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;

            if (!DisplayingScore) {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Escape)) {
                    Reset();
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }

                if (Helper.KeyPressed(Key.W) || Helper.KeyPressed(Key.Up)) {
                    if (Pattern[position] == 9.AsString()) { 
                        Pattern[position] = " ";
                        position++; 
                    } else {
                        GameLoop.SoundManager.PlaySound("failureJingle");
                        SyncLists();
                    }

                    if (Pattern.Count == position) {
                        Score += 1;
                        GameLoop.SoundManager.PlaySound("successJingle");
                        SetPattern();
                    }
                }

                if (Helper.KeyPressed(Key.S) || Helper.KeyPressed(Key.Down)) {
                    if (Pattern[position] == 10.AsString()) {
                        Pattern[position] = " ";
                        position++;
                    }
                    else {
                        GameLoop.SoundManager.PlaySound("failureJingle");
                        SyncLists();
                    }

                    if (Pattern.Count == position) {
                        Score += 1;
                        GameLoop.SoundManager.PlaySound("successJingle");
                        SetPattern();
                    }
                }

                if (Helper.KeyPressed(Key.A) || Helper.KeyPressed(Key.Left)) {
                    if (Pattern[position] == 11.AsString()) {
                        Pattern[position] = " ";
                        position++;
                    }
                    else {
                        GameLoop.SoundManager.PlaySound("failureJingle");
                        SyncLists();
                    }

                    if (Pattern.Count == position) {
                        Score += 1;
                        GameLoop.SoundManager.PlaySound("successJingle");
                        SetPattern();
                    }
                }

                if (Helper.KeyPressed(Key.D) || Helper.KeyPressed(Key.Right)) {
                    if (Pattern[position] == 12.AsString()) {
                        Pattern[position] = " ";
                        position++;
                    }
                    else {
                        GameLoop.SoundManager.PlaySound("failureJingle"); 
                        SyncLists();
                    }

                    if (Pattern.Count == position) {
                        Score += 1;
                        GameLoop.SoundManager.PlaySound("successJingle");
                        SetPattern();
                    }
                }
            }
            else {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    GameLoop.World.Player.CopperCoins += 10;
                    Reset();
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }
            }
        }
    }
}
