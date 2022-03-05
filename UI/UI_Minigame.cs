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

        public MineManager MineManager;
        public FishingManager FishingManager;
        public Bartending Bartending;
        public Blacksmithing Blacksmithing;
        public LightsOut LightsOut;
        public FruitGathering FruitCatch;
        public ArchCleaning ArchCleaning;
        public Picross Picross;
        public StackEm StackEm;

        public UI_Minigame(int width, int height, string title) : base(width, height, title, "Minigame") {
            MineManager = new();
            FishingManager = new(); 
            Bartending = new();
            Blacksmithing = new();
            LightsOut = new();
            FruitCatch = new();
            ArchCleaning = new();
            Picross = new();
            StackEm = new();
        }


        public override void Render() { 
            Con.Clear();

            if (CurrentGame == "Fishing") { FishingManager.Render(); } 
            else if (CurrentGame == "Mining") { MineManager.Render(); } 
            else if (CurrentGame == "Bartending") { Bartending.Draw(); } 
            else if (CurrentGame == "Blacksmithing") { Blacksmithing.Draw(); } 
            else if (CurrentGame == "LightsOut") { LightsOut.Draw(); } 
            else if (CurrentGame == "FruitCatch") { FruitCatch.Draw(); }
            else if (CurrentGame == "ArchCleaning") { ArchCleaning.Draw(); }
            else if (CurrentGame == "Picross") { Picross.Draw(); }
            else if (CurrentGame == "StackEm") { StackEm.Draw(); }
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
        } 
        public void ToggleMinigame(string which) {
            if (Win.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                CurrentGame = "None";
                Win.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
                Con.Children.Clear();
                Con.Clear();
                Win.Title = "";
                GameLoop.World.Player.MineLocation = "None";
            } else {
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
