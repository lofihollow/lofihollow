using LofiHollow.Entities;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Managers {
    public class FishingManager {
        public int FishDistance = 30;
        public FishDef HookedFish;
        public bool FishFighting = false;
        public int LineStress = 0;
        public double ReeledTime = 0;
        public double FishRunTime = 0;
        public int FishFightLeft = 0;

        public void Render() {
            if (HookedFish != null) {
                FishingDraw();
            }
        }

        public void Input() {
            if (HookedFish != null) {
                FishingInput();
            }
        }


        public void FishingDraw() {
            GameLoop.UIManager.Minigames.MinigameConsole.DrawLine(new Point(0, 35), new Point(10, 35), 196, new Color(0, 127, 0));
            GameLoop.UIManager.Minigames.MinigameConsole.DrawLine(new Point(11, 35), new Point(72, 35), '~', new Color(0, 94, 184));
            GameLoop.UIManager.Minigames.MinigameConsole.Print(10, 34, GameLoop.World.Player.GetAppearance());
            GameLoop.UIManager.Minigames.MinigameConsole.Print(11, 34, new ColoredString("/", new Color(110, 66, 33), Color.Black));
            GameLoop.UIManager.Minigames.MinigameConsole.Print(12, 33, new ColoredString("/", new Color(110, 66, 33), Color.Black));
            GameLoop.UIManager.Minigames.MinigameConsole.Print(13, 32, new ColoredString("/", new Color(110, 66, 33), Color.Black));
            GameLoop.UIManager.Minigames.MinigameConsole.Print(FishDistance, 36, HookedFish.GetAppearance());
            GameLoop.UIManager.Minigames.MinigameConsole.DrawLine(new Point(14, 32), new Point(FishDistance - 1, 36), '~', Color.White);

            GameLoop.UIManager.Minigames.MinigameConsole.Print(1, 1, "Line Stress: " + LineStress);
        }

        public void FishingInput() {
            if (GameHost.Instance.Mouse.LeftButtonDown) {
                if (ReeledTime + 100 > SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    return;
                }
                ReeledTime = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;

                if (FishFighting) {
                    FishDistance -= 1;
                    LineStress += HookedFish.Strength;
                } else {
                    FishDistance -= 2;
                    if (LineStress >= 2)
                        LineStress -= 2;
                    else
                        LineStress = 0;
                }

                int fightChance = GameLoop.rand.Next(100) + 1;

                if (fightChance < HookedFish.FightChance) {
                    FishFighting = true;
                    FishFightLeft = HookedFish.FightLength;
                } else {
                    FishFightLeft--;
                    if (FishFightLeft <= 0) {
                        FishFighting = false;
                    }
                }

                if (FishDistance <= 13) {
                    FinishFishing(true);
                }

                if (FishDistance >= 72 || LineStress >= 100) {
                    FinishFishing(false);
                }
            } else {
                int fightChance = GameLoop.rand.Next(100) + 1;

                if (FishFighting) {
                    if (FishRunTime + 100 > SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                        return;
                    }
                    FishRunTime = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;

                    FishDistance++;
                    FishFightLeft--;
                } else if (fightChance < HookedFish.FightChance && !FishFighting) {
                    FishFighting = true;
                    FishFightLeft = HookedFish.FightLength;
                }

                if (FishDistance <= 13) {
                    FinishFishing(true);
                }

                if (FishDistance >= 72 || LineStress >= 100) {
                    FinishFishing(false);
                }
            }
        }

        public void InitiateFishing(string Season, string Area, int CurrentTime, int FishingLevel) {
            List<FishDef> validFish = new();

            foreach (KeyValuePair<string, FishDef> kv in GameLoop.World.fishLibrary) {
                if (kv.Value.CatchLocation == Area || kv.Value.CatchLocation == "Any") {
                    if (kv.Value.Season == Season || kv.Value.Season == "Any") {
                        if (kv.Value.EarliestTime < CurrentTime && kv.Value.LatestTime > CurrentTime) {
                            if (kv.Value.RequiredLevel <= FishingLevel) {
                                validFish.Add(kv.Value);
                            }
                        }
                    }
                }
            }

            HookedFish = validFish[GameLoop.rand.Next(validFish.Count)];
            GameLoop.UIManager.Minigames.CurrentGame = "Fishing";
            FishDistance = 30;
            LineStress = 0;
            GameLoop.UIManager.Minigames.ToggleMinigame();
        }

        public void FinishFishing(bool success) {
            if (success) {
                Item caughtFish = new(HookedFish.RawFish.FullName());

                int quality = GameLoop.rand.Next(HookedFish.MaxQuality) + 1;
                int QualityCap = (int)Math.Floor((GameLoop.World.Player.Skills["Fishing"].Level + 1f) / 10f) + 1;
                caughtFish.Quality = Math.Min(quality, QualityCap);

                Color fore = new(HookedFish.colR, HookedFish.colG, HookedFish.colB, HookedFish.colA);

                CommandManager.AddItemToInv(GameLoop.World.Player, caughtFish);
                GameLoop.UIManager.Minigames.ToggleMinigame();
                ColoredString caught = new("You caught a ", Color.Cyan, Color.Black);
                caught += new ColoredString(HookedFish.Name, fore, Color.Black);
                caught += new ColoredString(" !", Color.Cyan, Color.Black);

                GameLoop.UIManager.AddMsg(caught);
                GameLoop.World.Player.Skills["Fishing"].GrantExp(HookedFish.GrantedExp);
            } else {
                GameLoop.UIManager.Minigames.ToggleMinigame();
                if (LineStress >= 100) {
                    GameLoop.UIManager.AddMsg(new ColoredString("The line snapped!", Color.Red, Color.Black));
                } else {
                    GameLoop.UIManager.AddMsg(new ColoredString("Looks like it got away...", Color.Red, Color.Black));
                }
            }

            HookedFish = null;
            GameLoop.UIManager.Map.MapConsole.ClearDecorators(GameLoop.UIManager.Sidebar.LocalLure.Position.X, GameLoop.UIManager.Sidebar.LocalLure.Position.Y, 1);
            GameLoop.UIManager.Sidebar.LocalLure.Position = new Point(-1, -1);
            GameLoop.UIManager.Sidebar.LocalLure.FishOnHook = false;
            GameLoop.UIManager.Sidebar.LocalLure = new FishingLure();
            GameLoop.UIManager.Minigames.CurrentGame = "None";
        }

    }
}
