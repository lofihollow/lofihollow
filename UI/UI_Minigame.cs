using LofiHollow.Entities;
using LofiHollow.Managers;
using LofiHollow.Minigames.Archaeology;
using LofiHollow.Minigames.Jobs;
using LofiHollow.Minigames.Picross;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace LofiHollow.UI {
    public class UI_Minigame : Lofi_UI { 
        public string CurrentGame = "None";

        public string PreviousMenuScreen = "";

        public MineManager MineManager;
        public FishingManager FishingManager;
        public Bartending Bartending;
        public Blacksmithing Blacksmithing;
        public LightsOut LightsOut;
        public FruitGathering FruitCatch;
        public ArchCleaning ArchCleaning;
        public Picross Picross;
        public StackEm StackEm;
        public Minesweeper Minesweeper;
        public NumberCombo NumberCombo;

        public UI_Minigame(int width, int height, string title) : base(width, height, title, "Minigame") {  }


        public override void Render() { 
            Con.Clear();

            if (CurrentGame == "Fishing") {
                if (FishingManager == null)
                    FishingManager = new();
                FishingManager.Render(); 
            }
            else if (CurrentGame == "Mining") {
                if (MineManager == null)
                    MineManager = new();
                MineManager.Render(); 
            }

            else if (CurrentGame == "Bartending") {
                if (Bartending == null)
                    Bartending = new();
                Bartending.Draw(); 
            }

            else if (CurrentGame == "Blacksmithing") {
                if (Blacksmithing == null)
                    Blacksmithing = new();
                Blacksmithing.Draw(); 
            }

            else if (CurrentGame == "LightsOut") {
                if (LightsOut == null)
                    LightsOut = new();
                LightsOut.Draw();
            }

            else if (CurrentGame == "FruitCatch") {
                if (FruitCatch == null)
                    FruitCatch = new();
                FruitCatch.Draw(); 
            }

            else if (CurrentGame == "ArchCleaning") {
                if (ArchCleaning == null)
                    ArchCleaning = new();
                ArchCleaning.Draw(); 
            }

            else if (CurrentGame == "Picross") {
                if (Picross == null)
                    Picross = new();
                Picross.Draw(); 
            }

            else if (CurrentGame == "StackEm") {
                if (StackEm == null)
                    StackEm = new();
                StackEm.Draw(); 
            }

            else if (CurrentGame == "Minesweeper") {
                if (Minesweeper == null)
                    Minesweeper = new();
                Minesweeper.Draw(); 
            }

            else if (CurrentGame == "NumberCombo") {
                if (NumberCombo == null)
                    NumberCombo = new();
                NumberCombo.Draw();
            }
        }

        public override void Input() {
            if (CurrentGame == "Fishing") { FishingManager.Input(); } 
            else if (CurrentGame == "Mining") { MineManager.Input(); } 
            else if (CurrentGame == "Bartending") { Bartending.Input(); } 
            else if (CurrentGame == "Blacksmithing") { Blacksmithing.Input(); } 
            else if (CurrentGame == "LightsOut") { LightsOut.Input(); } 
            else if (CurrentGame == "FruitCatch") { FruitCatch.Input(); }
            else if (CurrentGame == "ArchCleaning") { ArchCleaning.Input(); }
            else if (CurrentGame == "Picross") { Picross.Input(); } 
            else if (CurrentGame == "StackEm") { StackEm.Input(); }
            else if (CurrentGame == "Minesweeper") { Minesweeper.Input(); }
            else if (CurrentGame == "NumberCombo") { NumberCombo.Input(); }
        } 
        public void ToggleMinigame(string which) {
            if (Win.IsVisible) {
                GameLoop.UIManager.selectedMenu = PreviousMenuScreen;
                CurrentGame = "None";
                Win.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
                Con.Children.Clear();
                Con.Clear();
                Win.Title = "";
                GameLoop.World.Player.MineLocation = "None";
            } else {
                PreviousMenuScreen = GameLoop.UIManager.selectedMenu;
                GameLoop.UIManager.selectedMenu = "Minigame";
                Win.IsVisible = true;
                Win.IsFocused = true;
                CurrentGame = which;
                

                if (CurrentGame == "Mining" || CurrentGame == "Monster Pen") {
                    Win.Position = new Point(0, 0);
                } else {
                    Win.Position = new Point(11, 6);
                }
            }
        }
    }
}
