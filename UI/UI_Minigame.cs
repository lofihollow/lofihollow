using LofiHollow.Entities;
using LofiHollow.Managers;
using LofiHollow.Minigames.Jobs; 

using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace LofiHollow.UI {
    public class UI_Minigame {
        public SadConsole.Console MinigameConsole;
        public Window MinigameWindow;
        public string CurrentGame = "None";

        public MineManager MineManager;
        public FishingManager FishingManager;
        public MonsterPenManager MonsterPenManager;
        public Bartending Bartending;

        public UI_Minigame(int width, int height, string title) {
            MinigameWindow = new(width, height);
            MinigameWindow.CanDrag = false;
            MinigameWindow.Position = new Point(11, 6);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            MinigameConsole = new(invConWidth, invConHeight);
            MinigameConsole.Position = new(1, 1);
            MinigameWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            MinigameWindow.Children.Add(MinigameConsole);
            GameLoop.UIManager.Children.Add(MinigameWindow);

            MinigameWindow.Show();
            MinigameWindow.IsVisible = false;

            MineManager = new();
            FishingManager = new();
            MonsterPenManager = new();
            Bartending = new();
        }


        public void RenderMinigame() { 
            MinigameConsole.Clear();

            if (CurrentGame == "Fishing") {
                FishingManager.Render();
            }

            if (CurrentGame == "Mining") {
                MineManager.Render();
            }

            if (CurrentGame == "Monster Pen") {
                MonsterPenManager.Draw();
            }

            if (CurrentGame == "Bartending") {
                Bartending.Draw();
            }
        }

        public void MinigameInput() {
            if (CurrentGame == "Fishing") {
                FishingManager.Input();
            }

            if (CurrentGame == "Mining") {
                MineManager.Input();
            }

            if (CurrentGame == "Monster Pen") {
                MonsterPenManager.Input();
            }

            if (CurrentGame == "Bartending") {
                Bartending.Input();
            }
        } 
        public void ToggleMinigame(string which) {
            if (MinigameWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                MinigameWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
                MinigameConsole.Children.Clear();
            } else {
                GameLoop.UIManager.selectedMenu = "Minigame";
                MinigameWindow.IsVisible = true;
                MinigameWindow.IsFocused = true;
                CurrentGame = which;
                

                if (CurrentGame == "Mining" || CurrentGame == "Monster Pen") {
                    MinigameWindow.Position = new Point(0, 0);
                } else {
                    MinigameWindow.Position = new Point(11, 6);
                }
            }
        }
    }
}
