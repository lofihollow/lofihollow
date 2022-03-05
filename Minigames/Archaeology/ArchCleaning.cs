using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames.Archaeology {
    public class ArchCleaning : Minigame {
        public Item Current;
        public string CurrentTool = "5x Brush";
        public int Power = 1;

        public ArchCleaning() { 
        }

        public override void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            Console Mini = GameLoop.UIManager.Minigames.Con;

            Mini.Clear();
            Mini.ClearDecorators(0, 2800);

            Mini.PrintClickable(69, 0, "X", MinigameClick, "Close");

            Mini.Print(1, 3, "Current Tool: " + CurrentTool);
            Mini.Print(1, 5, "Power: ");
            Mini.PrintClickable(8, 5, "-", MinigameClick, "Power Down");

            Mini.Print(10, 5, Power.ToString().Align(HorizontalAlignment.Right, 2));

            Mini.PrintClickable(13, 5, "+", MinigameClick, "Power Up");

            Mini.Print(1, 9, "Tool Kit:");
            Mini.PrintClickable(2, 10, "| 5x Brush", MinigameClick, "5xBrush");

            if (GameLoop.World.Player.Skills.ContainsKey("Archaeology")) {
                int ArchLevel = GameLoop.World.Player.Skills["Archaeology"].Level;
                if (ArchLevel >= 10)
                    Mini.PrintClickable(2, 11, "| 5x Chisel", MinigameClick, "5xChisel");
                if (ArchLevel >= 20)
                    Mini.PrintClickable(2, 12, "| 3x Brush", MinigameClick, "3xBrush");
                if (ArchLevel >= 30)
                    Mini.PrintClickable(2, 13, "| 3x Chisel", MinigameClick, "3xChisel");
                if (ArchLevel >= 40)
                    Mini.PrintClickable(2, 14, "| 1x Brush", MinigameClick, "1xBrush");
                if (ArchLevel >= 50)
                    Mini.PrintClickable(2, 15, "| 1x Chisel", MinigameClick, "1xChisel");
            }



            Mini.DrawLine(new Point(29, 5), new Point(29, 34), 179, Color.White);
            Mini.DrawLine(new Point(60, 5), new Point(60, 34), 179, Color.White);
            Mini.DrawLine(new Point(30, 4), new Point(59, 4), 196, Color.White);
            Mini.DrawLine(new Point(30, 35), new Point(59, 35), 196, Color.White);


            for (int x = 0; x < 30; x++) {
                for (int y = 0; y < 30; y++) {
                    ArchTile thisTile = Current.Artifact.Tiles[x + (y * 30)];
                    Mini.PrintClickable(30 + x, 5 + y, thisTile.GetAppearance(), MinigameClick, x + "," + y);

                    if (thisTile.Condition != 11 && thisTile.Vital())
                        Mini.AddDecorator(30 + x, 5 + y, 1, thisTile.GetCondition());
                    if (thisTile.GetDirtCovering() != null) {
                        Mini.AddDecorator(30 + x, 5 + y, 1, thisTile.GetDirtCovering());
                    }
                }
            }
        }

        public void MinigameClick(string ID) { 
            if (ID == "Close") {
                Close();
            } 
            else if (ID == "Power Up") {
                if (Power < 10)
                    Power++;
            }
            else if (ID == "Power Down") {
                if (Power > 1)
                    Power--;
            }
            else if (ID == "5xBrush") { CurrentTool = "5x Brush"; }
            else if (ID == "3xBrush") { CurrentTool = "3x Brush"; }
            else if (ID == "1xBrush") { CurrentTool = "1x Brush"; }
            else if (ID == "5xChisel") { CurrentTool = "5x Chisel"; } 
            else if (ID == "3xChisel") { CurrentTool = "3x Chisel"; } 
            else if (ID == "1xChisel") { CurrentTool = "1x Chisel"; }

            else if (Current != null && Current.Artifact != null && !Current.Artifact.AlreadyClean) {
                string[] coords = ID.Split(",");
                int x = System.Int32.Parse(coords[0]);
                int y = System.Int32.Parse(coords[1]);

                if (CurrentTool == "5x Brush") {
                    int startX = System.Math.Clamp(x - 2, 0, 29);
                    int startY = System.Math.Clamp(y - 2, 0, 29);

                    for (int i = startX; i < startX + 5; i++) {
                        for (int j = startY; j < startY + 5; j++) {
                            Current.Artifact.BrushSpot(i, j, Power);
                        }
                    }
                }

                else if (CurrentTool == "3x Brush") {
                    int startX = System.Math.Clamp(x - 1, 0, 29);
                    int startY = System.Math.Clamp(y - 1, 0, 29);

                    for (int i = startX; i < startX + 3; i++) {
                        for (int j = startY; j < startY + 3; j++) {
                            Current.Artifact.BrushSpot(i, j, Power);
                        }
                    }
                }

                else if (CurrentTool == "1x Brush") { 
                    Current.Artifact.BrushSpot(x, y, Power); 
                }


                if (Current.Artifact.FullyCleaned()) {
                    Current.Artifact.TransformToCleaned(Current);
                }
            }
        } 

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) { 
                Close();
            }
        }
    }
}
