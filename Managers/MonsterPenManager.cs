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
    public class MonsterPenManager : Minigame {
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

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.MinigameConsole, GameHost.Instance.Mouse).CellPosition;

            if (CurrentPen != null) {
                GameLoop.UIManager.Minigames.MinigameConsole.Print(69, 0, Helper.HoverColoredString("X", mousePos == new Point(69, 0)));

                string Name = CurrentPen.Monster.Name;
                if (Name != "(EMPTY)")
                    Name += " (" + CurrentPen.Monster.Species + ")";
                else if (CurrentPen.Egg != null)
                    Name = CurrentPen.Egg.HatchesInto.Species + " Egg";

                string Title = "Pen " + (CurrentPen.PenNumber + 1) + ": " + Name;

                GameLoop.UIManager.Minigames.MinigameConsole.Print(1, 0, Title.Align(HorizontalAlignment.Center, 68));
                GameLoop.UIManager.Minigames.MinigameConsole.DrawLine(new Point(0, 1), new Point(69, 1), 196, Color.White);

                if (CurrentPen.Egg == null && CurrentPen.Monster.Name == "(EMPTY)") {
                    GameLoop.UIManager.Minigames.MinigameConsole.Print(1, 2, Helper.HoverColoredString("Add Egg to Pen", mousePos.Y == 2 && mousePos.X < 15));
                } else {
                    GameLoop.UIManager.Minigames.MinigameConsole.Print(1, 2, "Hunger: " + CurrentPen.Monster.Hunger);
                    GameLoop.UIManager.Minigames.MinigameConsole.Print(1, 3, "Diet: " + CurrentPen.Monster.Eats);
                    GameLoop.UIManager.Minigames.MinigameConsole.Print(1, 4, Helper.HoverColoredString("[Feed]", mousePos.Y == 4 && mousePos.X < 7));

                    GameLoop.UIManager.Minigames.MinigameConsole.Print(1, 6, "Happiness: " + CurrentPen.Monster.Happiness);
                    GameLoop.UIManager.Minigames.MinigameConsole.Print(1, 7, "Relationship: " + CurrentPen.Monster.Relationship);
                }
            }
        }


        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.MinigameConsole, GameHost.Instance.Mouse).CellPosition; 
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Close();
            }

            if (CurrentPen != null) {

                if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                    if (GameLoop.UIManager.Sidebar.hotbarSelect + 1 < 9)
                        GameLoop.UIManager.Sidebar.hotbarSelect++;
                } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                    if (GameLoop.UIManager.Sidebar.hotbarSelect > 0)
                        GameLoop.UIManager.Sidebar.hotbarSelect--;
                }

                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (mousePos == new Point(69, 0)) {
                        Close();
                    }

                    if (mousePos.Y == 2 && mousePos.X < 15 && CurrentPen.Monster.Name == "(EMPTY)" && CurrentPen.Egg == null) {
                        if (GameLoop.World.Player.Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].Properties.ContainsKey("MonsterEgg")) {
                            CurrentPen.Egg = GameLoop.World.Player.Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].Properties.Get<Egg>("MonsterEgg");
                            GameLoop.World.Player.Inventory[GameLoop.UIManager.Sidebar.hotbarSelect] = new("lh:(EMPTY)");
                        }
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
        }

        public void DailyUpdate() {
            FirstPen.DailyUpdate();
            SecondPen.DailyUpdate();
            ThirdPen.DailyUpdate();
        }
    }
}
