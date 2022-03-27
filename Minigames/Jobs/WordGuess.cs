using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Steamworks.Data;
using System.Collections.Generic;
using Color = SadRogue.Primitives.Color;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class WordGuess : Minigame { 
        public int Timer = 60;
        public double LastTimeTick = 0;

        public bool DisplayingScore = false;
        public bool GuessedCorrectly = false;
        public bool TimerGoing = false;

        public string CurrentWord = "tests";
        public List<string> Guesses = new();
        public string CurrentGuess = "";

        public string Error = "";

        public WordGuess() {
            Reset();
        }

        public void Reset() { 
            Timer = 60;
            DisplayingScore = false;
            GuessedCorrectly = false;
            CurrentWord = GameLoop.World.wordGuessWords[GameLoop.rand.Next(GameLoop.World.wordGuessWords.Count)];
            Guesses.Clear();
            TimerGoing = false;
            CurrentGuess = "";
        }

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.Con;

            if (GameLoop.World.wordGuessWords.Count > 1000) {
                if (Timer > 0) {
                    Mini.Print(0, 0, Timer.ToString().Align(HorizontalAlignment.Center, 70)); 
                    if (LastTimeTick + 1000 < GameHost.Instance.GameRunningTotalTime.TotalMilliseconds && TimerGoing) {
                        LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                        Timer--;
                    }

                    for (int x = 0; x < 5; x++) {
                        for (int y = 0; y < 6; y++) {
                            if (y < Guesses.Count) {
                                string a = Guesses[y][x].ToString().ToLower();
                                string b = CurrentWord[x].ToString().ToLower();
                                Color bg = a == b ? Color.DarkGreen : CurrentWord.ToLower().Contains(a) ? Color.DarkGoldenrod : Color.DarkSlateGray;
                                Helper.DrawBox(Mini, 27 + (3 * x), 5 + (4 * y), 1, 1, bg.R, bg.G, bg.B); 
                                Mini.Print(28 + (3 * x), 6 + (4 * y), Guesses[y][x].ToString());
                            } else {
                                Helper.DrawBox(Mini, 27 + (3 * x), 5 + (4 * y), 1, 1);
                            }
                        }
                    }
                    
                    for (int i = 0; i < CurrentGuess.Length; i++) {
                        Mini.Print(28 + (3 * i), 6 + (4 * Guesses.Count), CurrentGuess[i].ToString());
                    }

                    Mini.Print(0, 35, Error.Align(HorizontalAlignment.Center, 70));

                }
                else {
                    if (!DisplayingScore) {
                        DisplayingScore = true;
                        GameLoop.SoundManager.PlaySound("failureJingle");
                    }

                    if (GuessedCorrectly) {
                        Mini.Print(0, 3, ("The word was: " + CurrentWord).Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 4, ("You guessed correctly!").Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 6, ("You earned 10 copper.").Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 38, ("[Press ESC to close]").Align(HorizontalAlignment.Center, 70));
                    }
                    else {
                        Mini.Print(0, 3, ("The word was: " + CurrentWord).Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 5, ("Better luck next time!").Align(HorizontalAlignment.Center, 70));
                        Mini.Print(0, 38, ("[Press ESC to close]").Align(HorizontalAlignment.Center, 70));
                    } 
                }
            } else {
                Mini.Print(0, 13, "Hmm, seems like your word file".Align(HorizontalAlignment.Center, 70));
                Mini.Print(0, 14, "doesn't have enough words in it.".Align(HorizontalAlignment.Center, 70));

                Mini.Print(0, 14, ("Word list has " + GameLoop.World.wordGuessWords.Count + " but needs at least 1000.").Align(HorizontalAlignment.Center, 70));

                Mini.Print(0, 38, ("[Press ESC to close]").Align(HorizontalAlignment.Center, 70));
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

                foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                    if (key.Character >= 'A' && key.Character <= 'z') {
                        if (CurrentGuess.Length < 5) 
                            CurrentGuess += key.Character.ToString().ToUpper();
                    }
                } 

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Back) && CurrentGuess.Length > 0) {
                    CurrentGuess = CurrentGuess[0..^1];
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Enter) && CurrentGuess.Length == 5) {
                    if (GameLoop.World.wordGuessWords.Contains(CurrentGuess.ToLower())) {
                        Guesses.Add(CurrentGuess);
                        if (CurrentGuess.ToLower() == CurrentWord.ToLower()) {
                            GuessedCorrectly = true;
                            DisplayingScore = true;
                            Timer = 0;
                            TimerGoing = false;
                            GameLoop.SoundManager.PlaySound("successJingle");
                        }
                        else if (Guesses.Count == 6) {
                            DisplayingScore = true;
                            Timer = 0;
                            TimerGoing = false;
                            GameLoop.SoundManager.PlaySound("failureJingle");
                        }
                        else {
                            CurrentGuess = "";
                            TimerGoing = true;
                        }

                        Error = "";
                    } else {
                        Error = "'" + CurrentGuess.ToLower() + "' is not in the word list.";
                    }


                }

            }
            else {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Escape)) {
                    GameLoop.World.Player.CopperCoins += 10;
                    Reset();
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }
            }
        }
    }
}
