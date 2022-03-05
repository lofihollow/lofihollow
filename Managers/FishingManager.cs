using LofiHollow.Entities;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using LofiHollow.DataTypes;

namespace LofiHollow.Managers {
    public class FishingManager {
        public int FishDistance = 30;
        public FishDef HookedFish;
        public bool FishFighting = false;
        public int LineStress = 0;
        public double ReeledTime = 0;
        public double FishRunTime = 0;
        public int FishFightLeft = 0;
        public int FishCalmLeft = 0;

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
            SadConsole.Console Mini = GameLoop.UIManager.Minigames.Con;
            Mini.DrawLine(new Point(0, 35), new Point(10, 35), 196, new Color(0, 127, 0));
            Mini.DrawLine(new Point(11, 35), new Point(72, 35), '~', new Color(0, 94, 184));
            Mini.Print(10, 34, GameLoop.World.Player.GetAppearance());
            Mini.Print(11, 34, new ColoredString("/", new Color(110, 66, 33), Color.Black));
            Mini.Print(12, 33, new ColoredString("/", new Color(110, 66, 33), Color.Black));
            Mini.Print(13, 32, new ColoredString("/", new Color(110, 66, 33), Color.Black));
            Mini.Print(FishDistance, 36, HookedFish.GetAppearance());
            Mini.DrawLine(new Point(14, 32), new Point(FishDistance - 1, 36), '~', Color.White);

            Mini.Print(1, 1, "Line Stress: " + LineStress);

            if (FishFighting)
                GameLoop.UIManager.Minigames.Con.Print(1, 2, "Fish is fighting!", Color.Red);
            else
                GameLoop.UIManager.Minigames.Con.Print(1, 2, "Reel now!", Color.Lime);
        }

        public void FishingInput() {
            int fightChance = GameLoop.rand.Next(100) + 1;

            if (FishRunTime + 100 <= SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) { 
                FishRunTime = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                if (FishFightLeft > 0) {
                    FishFightLeft--;
                    if (FishFightLeft <= 0) {
                        FishFighting = false;
                        FishCalmLeft = HookedFish.FightLength / 2;
                    }
                } else if (FishCalmLeft > 0) {
                    FishCalmLeft--;
                }


                if (FishFighting && !GameHost.Instance.Mouse.LeftButtonDown) { 
                    FishDistance++;
                }
                else {
                    if (fightChance < HookedFish.FightChance && FishCalmLeft == 0) {
                        FishFighting = true;
                        FishFightLeft = HookedFish.FightLength;
                    }
                }
            }

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

                if (FishDistance <= 13) {
                    FinishFishing(true);
                }

                if (FishDistance >= 72 || LineStress >= 100) {
                    FinishFishing(false);
                }
            } else {
                if (FishDistance <= 13) {
                    FinishFishing(true);
                }

                if (FishDistance >= 72 || LineStress >= 100) {
                    FinishFishing(false);
                }
            }
        }

        public void InitiateFishing(string Season, int CurrentTime, int FishingLevel, string WaterType) {
            List<FishDef> validFish = new();

            foreach (KeyValuePair<string, FishDef> kv in GameLoop.World.fishLibrary) {
                if (kv.Value.CatchLocation.Contains(WaterType) || kv.Value.CatchLocation == "Any") {
                    if (kv.Value.Season.Contains(Season) || kv.Value.Season == "Any") {
                        if (kv.Value.EarliestTime < CurrentTime && kv.Value.LatestTime > CurrentTime) {
                            if (kv.Value.RequiredLevel <= FishingLevel) {
                                validFish.Add(kv.Value);
                            }
                        }
                    }
                }
            }

            if (validFish.Count > 0) {
                HookedFish = validFish[GameLoop.rand.Next(validFish.Count)];
                FishDistance = 30;
                LineStress = 0;
                GameLoop.UIManager.Minigames.ToggleMinigame("Fishing");
            } else {
                HookedFish = null;
                GameLoop.UIManager.Map.MapConsole.ClearDecorators(GameLoop.UIManager.Sidebar.LocalLure.Position.X, GameLoop.UIManager.Sidebar.LocalLure.Position.Y, 1);
                GameLoop.UIManager.Sidebar.LocalLure.Position = new Point(-1, -1);
                GameLoop.UIManager.Sidebar.LocalLure.FishOnHook = false;
                GameLoop.UIManager.Sidebar.LocalLure = new FishingLure();
                GameLoop.UIManager.Minigames.CurrentGame = "None";

                GameLoop.UIManager.AddMsg("You feel like nothing can be caught here.");
            }
        }

        public void FinishFishing(bool success) {
            if (success) {
                Item caughtFish = new(HookedFish.FishItemID); 

                int quality = GameLoop.rand.Next(HookedFish.MaxQuality) + 1;
                int QualityCap = (int)Math.Floor((GameLoop.World.Player.Skills["Fishing"].Level + 1f) / 10f) + 1;
                caughtFish.Quality = Math.Min(quality, QualityCap);

                Color fore = new(HookedFish.colR, HookedFish.colG, HookedFish.colB, HookedFish.colA);

                CommandManager.AddItemToInv(GameLoop.World.Player, caughtFish);
                GameLoop.UIManager.Minigames.ToggleMinigame("None");
                ColoredString caught = new("You caught a ", Color.Cyan, Color.Black);
                caught += new ColoredString(HookedFish.Name, fore, Color.Black);
                caught += new ColoredString("!", Color.Cyan, Color.Black);

                GameLoop.UIManager.AddMsg(caught);
                GameLoop.World.Player.Skills["Fishing"].GrantExp(HookedFish.GrantedExp);
            } else {
                GameLoop.UIManager.Minigames.ToggleMinigame("None");
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
