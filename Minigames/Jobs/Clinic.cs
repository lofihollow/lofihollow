using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Jobs {
    public class Clinic : Minigame {
        public int Score = 0;

        public int Timer = 61;
        public double LastTimeTick = 0;
        public bool DisplayingScore = false;

        public ClinicDisease Current;
        public List<ClinicSymptom> Symptoms = new();
         
        public Clinic() {
            Symptoms.Add(new("Cough", 2, 1, 0));
            Symptoms.Add(new("Runny Nose", 0, 1, 1));
            Symptoms.Add(new("Fever", 3, 0, 0));
            Symptoms.Add(new("Light-headed", 1, 1, 1));
            Symptoms.Add(new("Slimy", 0, 2, 2));
            Symptoms.Add(new("Swelling", 1, 0, 1));
            Symptoms.Add(new("Sweaty Palms", 0, 1, 2));
            Symptoms.Add(new("Weak Knees", 1, 2, 1));
        }

        public void Reset() {
            Score = 0;
            Timer = 61;
            DisplayingScore = false; 
        }

        public void ApplyCure() {

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
                 
            }
            else {
                if (!DisplayingScore) {
                    DisplayingScore = true; 
                }

                if (Score > 0) {
                    Mini.Print(0, 3, ("You earned " + (Score / 10) + " copper!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                }
                else {
                    Mini.Print(0, 3, ("Better luck next time!").Align(HorizontalAlignment.Center, 70));
                    Mini.Print(0, 38, ("[Press SPACE to close]").Align(HorizontalAlignment.Center, 70));
                } 
            }
        }

        public void MinigameClick(string ID) { 

        }
         

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Reset();
                Close();
            }

            if (!DisplayingScore) { 
                if (GameHost.Instance.Mouse.LeftClicked) { 
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
