using LofiHollow.Minigames;
using Newtonsoft.Json;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Managers {
    [JsonObject(MemberSerialization.OptIn)]
    public class MonsterPenManager {
        public MonsterPen CurrentPen;
        [JsonProperty]
        public MonsterPen FirstPen = new(0);
        [JsonProperty]
        public MonsterPen SecondPen = new(1);
        [JsonProperty]
        public MonsterPen ThirdPen = new(2);

        public void Setup(int pen) {
            if (pen == 0)
                CurrentPen = FirstPen;
            if (pen == 1)
                CurrentPen = SecondPen;
            if (pen == 2)
                CurrentPen = ThirdPen;
        }


        public void Render() {
            if (CurrentPen != null) {
                PenDraw();
            }
        }

        public void Input() {
            if (CurrentPen != null) {
                PenInput();
            }
        }



        public void PenDraw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.MinigameConsole, GameHost.Instance.Mouse).CellPosition;
            GameLoop.UIManager.Minigames.MinigameConsole.Print(69, 0, Helper.HoverColoredString("X", mousePos == new Point(69, 0)));

            string Name = CurrentPen.Monster.Name;
            if (Name != "(EMPTY)")
                Name += " (" + CurrentPen.Monster.Species + ")";

            string Title = "Pen " + (CurrentPen.PenNumber + 1) + ": " + Name;

            GameLoop.UIManager.Minigames.MinigameConsole.Print(1, 0, Title.Align(HorizontalAlignment.Center, 68));
            GameLoop.UIManager.Minigames.MinigameConsole.DrawLine(new Point(0, 1), new Point(69, 1), 196, Color.White);
        }


        public void PenInput() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.MinigameConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos == new Point(69, 0)) {
                    Toggle();
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyPressed(Key.S)) {
                CurrentPen.Monster.Name = "Boblin";
                CurrentPen.Monster.Species = "Goblin";
                CurrentPen.Monster.Glyph = 'g';
                CurrentPen.Monster.ForeG = 127;
            }

            if (GameHost.Instance.Keyboard.IsKeyPressed(Key.A)) {
                CurrentPen.Monster.Name = "Wolf";
                CurrentPen.Monster.Species = "Wolf";
                CurrentPen.Monster.Glyph = 'C';
                CurrentPen.Monster.ForeR = 70;
                CurrentPen.Monster.ForeG = 70;
                CurrentPen.Monster.ForeB = 70;
            }

            if (GameHost.Instance.Keyboard.IsKeyPressed(Key.D)) {
                CurrentPen.Monster.Name = "Raccoon";
                CurrentPen.Monster.Species = "Raccoon";
                CurrentPen.Monster.Glyph = 'r';
                CurrentPen.Monster.ForeR = 110;
                CurrentPen.Monster.ForeG = 66;
                CurrentPen.Monster.ForeB = 33;
            }
        }


        public void Toggle() {
            GameLoop.UIManager.Minigames.CurrentGame = "None";
            GameLoop.UIManager.Minigames.ToggleMinigame();
        }
    }
}
