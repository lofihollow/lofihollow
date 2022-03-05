using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System; 
using LofiHollow.DataTypes;
using System.Collections.Generic;

namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptIn)]
    public class FishingLure : Entity {
        [JsonProperty]
        public int xVelocity = 0;

        [JsonProperty]
        public int yVelocity = 0;

        public double TimeLastMoved = 0;
        public double TimeHooked = 0;

        public bool FishOnHook = false;

        [JsonConstructor]
        public FishingLure() : base(Color.White, Color.Transparent, '*') {  }


        public void SetVelocity (int x, int y) {
            xVelocity = x;
            yVelocity = y;
        }


        public void Update() {
            if (xVelocity != 0 || yVelocity != 0) {
                if (TimeLastMoved + (60) > SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    return;
                }
                TimeLastMoved = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;

                int dx = xVelocity < 0 ? -1 : xVelocity > 0 ? 1 : 0;
                int dy = yVelocity < 0 ? -1 : yVelocity > 0 ? 1 : 0;

                Position += new Point(dx, dy);
                xVelocity -= dx;
                yVelocity -= dy;
            }

            if (FishOnHook && Effect == null) {
                Effect = new SadConsole.Effects.BlinkGlyph();
                ((SadConsole.Effects.BlinkGlyph)Effect).BlinkSpeed = new TimeSpan(0, 0, 0, 1);
                ((SadConsole.Effects.BlinkGlyph)Effect).GlyphIndex = '!';

            } else if (!FishOnHook) {
                Effect = null;
                if (Position != new Point(-1, -1) && xVelocity == 0 && yVelocity == 0) {
                    Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);
                    Point cellPos = Position;
                    if (map != null) {
                        Tile tile = map.GetTile(cellPos);
                        if (tile.Name == "Water" || tile.Name == "Shallow Water") {
                            if (GameLoop.rand.Next(100) == 1) {
                                List<FishDef> validFish = new();
                                string WaterType = tile.MiscString;
                                string Season = GameLoop.World.Player.Clock.GetSeason();
                                int CurrentTime = GameLoop.World.Player.Clock.GetCurrentTime();
                                int FishingLevel = GameLoop.World.Player.Skills.ContainsKey("Fishing") ? GameLoop.World.Player.Skills["Fishing"].Level : 1;

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
                                    FishOnHook = true;
                                    GameLoop.UIManager.AddMsg(new ColoredString("Something is on the hook!", Color.Yellow, Color.Black));
                                    TimeHooked = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
                                }
                                else {
                                    GameLoop.UIManager.Minigames.FishingManager.HookedFish = null;
                                    GameLoop.UIManager.Map.MapConsole.ClearDecorators(GameLoop.UIManager.Sidebar.LocalLure.Position.X, GameLoop.UIManager.Sidebar.LocalLure.Position.Y, 1);
                                    GameLoop.UIManager.Sidebar.LocalLure.Position = new Point(-1, -1);
                                    GameLoop.UIManager.Sidebar.LocalLure.FishOnHook = false;
                                    GameLoop.UIManager.Sidebar.LocalLure = new FishingLure();
                                    GameLoop.UIManager.Minigames.CurrentGame = "None";

                                    GameLoop.UIManager.AddMsg("You feel like nothing can be caught here.");
                                }
                            }
                        }
                    }
                }
            }

            if (TimeHooked + 4000 < SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds && TimeHooked != 0) {
                if (GameLoop.UIManager.selectedMenu != "Minigame") {
                    FishOnHook = false;
                    TimeHooked = 0;
                    GameLoop.UIManager.AddMsg(new ColoredString("Looks like it got away...", Color.Red, Color.Black));
                }
            }
        }
    }
}
