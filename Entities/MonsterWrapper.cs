using LofiHollow.Managers;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Entities {
    public class MonsterWrapper : Entity {
        public Monster Wrapped;
        public GoRogue.Pathing.Path CurrentPath;
        public bool UpdatePath = true;
        public int pathPos = 0;
        public int trackingLength = 5;

        public MonsterWrapper(string name) : base(Color.White, Color.Black, 32) {
            if (GameLoop.World.itemLibrary.ContainsKey(name)) {
                Wrapped = new Monster(name);

                Appearance.Foreground = new(Wrapped.ForegroundR, Wrapped.ForegroundG, Wrapped.ForegroundB, Wrapped.ForegroundA);
                Appearance.Glyph = Wrapped.ActorGlyph;
            }
        }

        public MonsterWrapper(Monster temp) : base(Color.White, Color.Black, 32) {
            Wrapped = new Monster();
            Wrapped.SetAll(temp);

            Appearance.Foreground = new(Wrapped.ForegroundR, Wrapped.ForegroundG, Wrapped.ForegroundB, Wrapped.ForegroundA);
            Appearance.Glyph = Wrapped.ActorGlyph;
        }

        public void Update() {
            Point oldPos = new(Position.X, Position.Y);

            int distanceToNearest = 99;
            Point targetPoint = new(-1, -1);
            Actor defender = null;

            if (GameLoop.World.Player.player.MapPos == Wrapped.MapPos) {
                if (GameLoop.World.Player.player.CombatLevel < Wrapped.CombatLevel + Wrapped.Confidence || Wrapped.AlwaysAggro) {
                    defender = GameLoop.World.Player.player;
                    targetPoint = new Point(GameLoop.World.Player.player.Position.X, GameLoop.World.Player.player.Position.Y);
                    distanceToNearest = GameLoop.World.maps[Wrapped.MapPos].MapPath.ShortestPath(Position.ToCoord(), targetPoint.ToCoord()).Length;
                }
            }

            foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
                if (kv.Value.player.MapPos == Wrapped.MapPos) {
                    kv.Value.player.CalculateCombatLevel();
                    if (kv.Value.player.CombatLevel < Wrapped.CombatLevel + Wrapped.Confidence || Wrapped.AlwaysAggro) {
                        int newDist = GameLoop.World.maps[Wrapped.MapPos].MapPath.ShortestPath(Position.ToCoord(), kv.Value.player.Position.ToCoord()).Length;
                        if (newDist < distanceToNearest) {
                            defender = kv.Value.player;
                            targetPoint = new Point(kv.Value.player.Position.X, kv.Value.player.Position.Y);
                            distanceToNearest = newDist;
                        }
                    }
                }
            }

            if (distanceToNearest < 25 && distanceToNearest > 1) {
                if (CurrentPath == null) {
                    CurrentPath = GameLoop.World.maps[Wrapped.MapPos].MapPath.ShortestPath(Position.ToCoord(), targetPoint.ToCoord());
                    pathPos = 0;
                }

                if (CurrentPath != null && targetPoint != new Point(-1, -1)) {
                    if (Wrapped.TimeLastActed + (120) > SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                        return;
                    }
                    Wrapped.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;


                    if (CurrentPath.Length > pathPos) {
                        GoRogue.Coord nextStep = CurrentPath.GetStep(pathPos);

                        if (Wrapped.MoveTo(new Point(nextStep.X, nextStep.Y), Wrapped.MapPos))
                            pathPos++;
                    }

                    Point currEnd = new(CurrentPath.End.X, CurrentPath.End.Y);

                    if (targetPoint != currEnd) {
                        CurrentPath = GameLoop.World.maps[Wrapped.MapPos].MapPath.ShortestPath(Position.ToCoord(), targetPoint.ToCoord());
                        pathPos = 0;
                    }
                }

                if (!oldPos.Equals(Position)) {
                    NetMsg monsterMoved = new("moveMonster", null);
                    monsterMoved.SetMapPos(Wrapped.MapPos);
                    monsterMoved.SetPosition(Wrapped.Position);
                    monsterMoved.MiscString = Wrapped.UniqueID;
                    GameLoop.SendMessageIfNeeded(monsterMoved, true, false);
                }
            } else if (distanceToNearest == 1) { // Already adjacent, make an attack
                if (defender != null) {
                    if (Wrapped.TimeLastActed + (120) > SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                        return;
                    }
                    Wrapped.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;

                    CommandManager.Attack(Wrapped, defender, true);
                }
            }

            if (!Wrapped.Position.Equals(Position)) {
                Position = Wrapped.Position;
            }
        }
    }
}
