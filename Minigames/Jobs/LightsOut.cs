using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives; 
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class LightsOut : Minigame {  
        public int Timer = 61;
        public double LastTimeTick = 0;
        public bool DisplayingScore = false;
        public bool Solved = false;
        public bool ColorblindMode = false;


        Dictionary<Point, bool> PlayGrid = new(); 


        public LightsOut() {
            PlayGrid.Clear(); 

            for (int x = 0; x < 5; x++) {
                for (int y = 0; y < 5; y++) {
                    PlayGrid.Add(new Point(x, y), true);
                }
            }

            for (int i = 0; i < 15; i++) {
                int x = GameLoop.rand.Next(5);
                int y = GameLoop.rand.Next(5);
                ToggleSpot(x, y);
            }
        } 

        public void ToggleSpot(int x, int y) {
            if (PlayGrid.ContainsKey(new Point(x, y))) {
                PlayGrid[new Point(x, y)] = !PlayGrid[new Point(x, y)];
            }

            if (PlayGrid.ContainsKey(new Point(x - 1, y))) {
                PlayGrid[new Point(x - 1, y)] = !PlayGrid[new Point(x - 1, y)];
            }

            if (PlayGrid.ContainsKey(new Point(x + 1, y))) {
                PlayGrid[new Point(x + 1, y)] = !PlayGrid[new Point(x + 1, y)];
            }

            if (PlayGrid.ContainsKey(new Point(x, y - 1))) {
                PlayGrid[new Point(x, y - 1)] = !PlayGrid[new Point(x, y - 1)];
            }

            if (PlayGrid.ContainsKey(new Point(x, y + 1))) {
                PlayGrid[new Point(x, y + 1)] = !PlayGrid[new Point(x, y + 1)];
            }

            Solved = true;
            foreach (KeyValuePair<Point, bool> kv in PlayGrid) {
                if (!kv.Value)
                    Solved = false;
            }

            if (Solved) {
                DisplayingScore = true;
                GameLoop.SoundManager.PlaySound("successJingle");
            }
        }

        public void Reset() { 
            Timer = 61;
            DisplayingScore = false;
            PlayGrid.Clear();
            Solved = false;

            for (int x = 0; x < 5; x++) {
                for (int y = 0; y < 5; y++) {
                    PlayGrid.Add(new Point(x, y), true);
                }
            }

            for (int i = 0; i < 15; i++) {
                int x = GameLoop.rand.Next(5);
                int y = GameLoop.rand.Next(5);
                ToggleSpot(x, y);
            } 
        }

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.Con;

            Color on = ColorblindMode ? Color.Cyan : Color.Lime;
            Color off = ColorblindMode ? Color.Yellow : Color.Red;

            if (Timer > 0 && !DisplayingScore) {
                Mini.Print(0, 0, Timer.ToString().Align(HorizontalAlignment.Center, 70)); 
                if (LastTimeTick + 1000 < GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                    Timer--;
                }

                Mini.PrintClickable(0, 0, "Alt Colors", MinigameClick, "Colorblind");

                for (int x = 0; x < 5; x++) {
                    for (int y = 0; y < 5; y++) {
                        if (PlayGrid.ContainsKey(new Point(x, y))) {
                            if (!PlayGrid[new Point(x, y)]) {
                                Mini.PrintClickable(20 + (x * 6), 5 + (y * 6), new ColoredString("XXXXX", off, Color.Black), MinigameClick, x + "," + y);
                                Mini.PrintClickable(20 + (x * 6), 6 + (y * 6), new ColoredString("XXXXX", off, Color.Black), MinigameClick, x + "," + y);
                                Mini.PrintClickable(20 + (x * 6), 7 + (y * 6), new ColoredString("XXXXX", off, Color.Black), MinigameClick, x + "," + y);
                                Mini.PrintClickable(20 + (x * 6), 8 + (y * 6), new ColoredString("XXXXX", off, Color.Black), MinigameClick, x + "," + y);
                                Mini.PrintClickable(20 + (x * 6), 9 + (y * 6), new ColoredString("XXXXX", off, Color.Black), MinigameClick, x + "," + y);
                            } else {
                                Mini.PrintClickable(20 + (x * 6), 5 + (y * 6), new ColoredString("OOOOO", on, Color.Black), MinigameClick, x + "," + y);
                                Mini.PrintClickable(20 + (x * 6), 6 + (y * 6), new ColoredString("OOOOO", on, Color.Black), MinigameClick, x + "," + y);
                                Mini.PrintClickable(20 + (x * 6), 7 + (y * 6), new ColoredString("OOOOO", on, Color.Black), MinigameClick, x + "," + y);
                                Mini.PrintClickable(20 + (x * 6), 8 + (y * 6), new ColoredString("OOOOO", on, Color.Black), MinigameClick, x + "," + y);
                                Mini.PrintClickable(20 + (x * 6), 9 + (y * 6), new ColoredString("OOOOO", on, Color.Black), MinigameClick, x + "," + y);
                            }
                        } 
                    }
                }
            }
            else {
                if (!DisplayingScore) {
                    DisplayingScore = true;
                    GameLoop.SoundManager.PlaySound("failureJingle");
                }

                if (Timer > 0) {
                    Mini.Print(0, 3, ("You earned " + (Timer / 2) + " copper!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                }
                else {
                    Mini.Print(0, 3, ("Better luck next time!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                } 
            }
        }

        public void MinigameClick(string ID) {
            if (ID == "Colorblind") {
                ColorblindMode = !ColorblindMode;
            }
            else {
                string[] coords = ID.Split(",");
                int x = System.Int32.Parse(coords[0]);
                int y = System.Int32.Parse(coords[1]);

                ToggleSpot(x, y);
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
            }
            else {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    GameLoop.World.Player.Zeri += (Timer / 2);
                    Reset();
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }
            }
        }
    }
}
