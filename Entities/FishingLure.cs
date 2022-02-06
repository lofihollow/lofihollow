using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public FishingLure() : base(Color.White, Color.Transparent, '*') {
        }


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
                    TileBase tile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(Position);
                    if (tile.Name == "Water" || tile.Name == "Shallow Water") {
                        if (GameLoop.rand.Next(100) == 1) {
                            FishOnHook = true;
                            GameLoop.UIManager.AddMsg(new ColoredString("Something is on the hook!", Color.Yellow, Color.Black));
                            TimeHooked = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
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
