using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class Bartending : Minigame {
        public int Charge = 0;
        public int Score = 0;

        public int Timer = 61;
        public double LastTimeTick = 0;

        public List<Drink> drinks = new();
        public Drink CurrentDrink;

        public Dictionary<int, BarPatron> patrons = new();
        public string[] DrinkList = { "Beer", "Wine", "Whiskey", "Martini" };

        public int DrinkIndex = 0;
        public bool DisplayingScore = false;

        public int[] Seats = { 15, 35, 45, 55 };

        public SadConsole.Entities.Renderer Renderer;

        public Bartending() {
            Renderer = new();
        }

        public void CreatePatron() {
            BarPatron patron = new();
            patron.X = Seats[GameLoop.rand.Next(Seats.Length)];
            
            while (patrons.ContainsKey(patron.X)) {
                patron.X = Seats[GameLoop.rand.Next(Seats.Length)];
            }

            patron.DesiredDrink = DrinkList[GameLoop.rand.Next(DrinkList.Length)];

            patrons.Add(patron.X, patron);
        }

        public void CreateDrink() {
            Drink drink = new();
            drink.UsePixelPositioning = true;
            drink.Position = new Point(792, 408); 
            drink.Appearance.Foreground = Color.White;
            drink.DrinkName = DrinkList[DrinkIndex];

            if (drink.DrinkName == "Beer")
                drink.Appearance.Glyph = 340;
            if (drink.DrinkName == "Wine")
                drink.Appearance.Glyph = 341;
            if (drink.DrinkName == "Whiskey")
                drink.Appearance.Glyph = 342;
            if (drink.DrinkName == "Martini")
                drink.Appearance.Glyph = 343;

            Renderer.Add(drink);
            drinks.Add(drink);
            CurrentDrink = drink;
        }

        public void Reset() {
            Score = 0;
            Timer = 61;
            DisplayingScore = false;
        }

        public void Resync() {
            Renderer.RemoveAll();

            for (int i = 0; i < drinks.Count; i++)
                Renderer.Add(drinks[i]);

            GameLoop.UIManager.Minigames.MinigameConsole.SadComponents.Remove(Renderer);
            GameLoop.UIManager.Minigames.MinigameConsole.SadComponents.Add(Renderer);
            GameLoop.UIManager.Minigames.MinigameConsole.ForceRendererRefresh = true;
        }

        public void WrapUp() { 
            drinks.Clear();
            patrons.Clear();

            Resync();
        }

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.MinigameConsole, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.MinigameConsole;


            Mini.Print(0, 0, Timer.ToString().Align(HorizontalAlignment.Center, 70));
            Mini.Print(0, 2, ("Score: " + Score).Align(HorizontalAlignment.Center, 70));
            Mini.DrawLine(new Point(10, 35), new Point(70, 35), 205, new Color(110, 66, 33), Color.Black);
            Mini.Print(9, 35, new ColoredString(((char)213).ToString(), new Color(110, 66, 33), Color.Black));
            Mini.DrawLine(new Point(9, 36), new Point(9, 40), 179, new Color(110, 66, 33), Color.Black);

            if (Timer > 0) {
                if (LastTimeTick + 1000 < GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    LastTimeTick = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                    Timer--;

                    if (patrons.Count < 3) {
                        if (GameLoop.rand.Next(10) < 3) {
                            CreatePatron();
                        }
                    }

                    foreach (KeyValuePair<int, BarPatron> kv in patrons) {
                        kv.Value.Happiness--;

                        if (kv.Value.Happiness < 0) {
                            patrons.Remove(kv.Key);
                            Score--;
                            GameLoop.SoundManager.PlaySound("failureJingle");
                        }
                    }
                }

                if (!GameLoop.UIManager.Minigames.MinigameConsole.SadComponents.Contains(Renderer))
                    GameLoop.UIManager.Minigames.MinigameConsole.SadComponents.Add(Renderer);

                Mini.Print(48, 36, "POWER: [");
                for (int i = 10; i > 0; i--) {
                    if (i * 10 < Charge) {
                        Mini.Print(66 - i, 36, "X");
                    } else {
                        Mini.Print(66 - i, 36, " ");
                    }
                }
                Mini.Print(66, 36, "]");

                Mini.Print(48, 37, "Current Drink: " + DrinkList[DrinkIndex]);

                foreach (KeyValuePair<int, BarPatron> kv in patrons) {
                    Mini.Print(kv.Value.X, 34, new ColoredString("@", kv.Value.GetColor(), Color.Black));

                    switch (kv.Value.DesiredDrink) {
                        case "Beer":
                            Mini.Print(kv.Value.X, 32, kv.Value.GetDrink());
                            break;
                        case "Wine":
                            Mini.Print(kv.Value.X, 32, kv.Value.GetDrink());
                            break;
                        case "Whiskey":
                            Mini.Print(kv.Value.X, 32, kv.Value.GetDrink());
                            break;
                        case "Martini":
                            Mini.Print(kv.Value.X, 32, kv.Value.GetDrink());
                            break;
                    }

                    Mini.Print(kv.Value.X, 31, kv.Value.Mood());
                }

                for (int i = 0; i < drinks.Count; i++) {
                    if (drinks[i].DrinkVelocity > 0) {
                        drinks[i].Position = new Point(drinks[i].Position.X - (drinks[i].DrinkVelocity / 4), drinks[i].Position.Y);
                        drinks[i].DrinkVelocity--;
                    }

                    if (drinks[i].Position.X < 110) {
                        drinks[i].Position = new(drinks[i].Position.X, drinks[i].Position.Y + 4);
                    }

                    if (drinks[i].DrinkVelocity == 0 && drinks[i].Position.X > 110) {
                        int x = drinks[i].Position.X / 12;
                        bool drinkConsumed = false;

                        foreach (KeyValuePair<int, BarPatron> kv in patrons) {
                            if (kv.Key >= x - 4 && kv.Key <= x + 4) {
                                if (kv.Value.CheckDrink(drinks[i].DrinkName)) {
                                    Score += kv.Value.Tip();
                                    patrons.Remove(kv.Key);
                                    drinkConsumed = true;
                                    GameLoop.SoundManager.PlaySound("successJingle");
                                } else {
                                    kv.Value.Happiness--;
                                    drinkConsumed = true;
                                    Score--;
                                    GameLoop.SoundManager.PlaySound("failureJingle");
                                }
                            }
                        }

                        if (x < 66)
                            drinkConsumed = true;

                        if (drinkConsumed) {
                            Renderer.Remove(drinks[i]);
                            drinks.RemoveAt(i);
                            Resync();
                            break;
                        }
                    }

                    if (drinks[i].Position.Y > 470) {
                        Renderer.Remove(drinks[i]);
                        drinks.RemoveAt(i);
                        Resync();
                        Score--;
                        GameLoop.SoundManager.PlaySound("glassBreak");
                        break;
                    }
                }
            } else {
                if (!DisplayingScore) {
                    WrapUp();
                    DisplayingScore = true;
                }

                if (Score > 0) {
                    Mini.Print(0, 20, ("You earned " + Score + " copper!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 21, ("[Click anywhere to close]").Align(HorizontalAlignment.Center, 70));
                } else {
                    Mini.Print(0, 20, ("Better luck next time!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 21, ("[Click anywhere to close]").Align(HorizontalAlignment.Center, 70));
                }
            }
        } 

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.MinigameConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Close();
            }

            if (!DisplayingScore) {

                if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                    if (DrinkIndex + 1 < DrinkList.Length)
                        DrinkIndex++;
                    else
                        DrinkIndex = 0;
                } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                    if (DrinkIndex > 0)
                        DrinkIndex--;
                    else
                        DrinkIndex = DrinkList.Length - 1;
                }

                if (GameHost.Instance.Mouse.LeftButtonDown) {
                    if (Charge == 0) {
                        CreateDrink();
                    }

                    if (Charge <= 100) {
                        Charge++;
                    }
                } else {
                    if (Charge != 0) {
                        // Launch drink
                        CurrentDrink.DrinkVelocity = Charge;
                        Charge = 0;
                    }
                }
            } else {
                if (GameHost.Instance.Mouse.LeftClicked) {
                    GameLoop.World.Player.CopperCoins += Score;
                    Reset();
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }
            }
        }
    }
}
