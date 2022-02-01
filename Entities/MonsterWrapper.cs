using LofiHollow.Managers;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Entities {
    public class MonsterWrapper : Actor {
        public Monster monster;

        public GoRogue.Pathing.Path CurrentPath;
        public bool UpdatePath = true;
        public int pathPos = 0;
        public int trackingLength = 5;

        public MonsterWrapper(string name) : base(Color.Black, 32) {
            if (GameLoop.World.itemLibrary.ContainsKey(name)) {
                monster = new(name);

                Appearance.Foreground = new(monster.ForegroundR, monster.ForegroundG, monster.ForegroundB, monster.ForegroundA);
                Appearance.Glyph = monster.ActorGlyph;
            }
        }

        public MonsterWrapper(Monster temp) : base(Color.Black, 32) {
            monster = new();
            monster.SetAll(temp);

            Appearance.Foreground = new(monster.ForegroundR, monster.ForegroundG, monster.ForegroundB, monster.ForegroundA);
            Appearance.Glyph = monster.ActorGlyph;
        }

        public void SpawnDrops() {
            for (int i = 0; i < monster.DropTable.Count; i++) {
                string[] split = monster.DropTable[i].Split(";");

                int roll = GameLoop.rand.Next(Int32.Parse(split[1]));

                if (roll == 0) {
                    ItemWrapper item = new(split[0]);

                    if (item.item.IsStackable) {
                        item.item.ItemQuantity = GameLoop.rand.Next(Int32.Parse(split[2])) + 1;
                        item.Position = Position;
                        item.MapPos = MapPos;
                        CommandManager.SpawnItem(item);
                    } else {
                        int qty = GameLoop.rand.Next(Int32.Parse(split[2])) + 1;

                        for (int j = 0; j < qty; j++) {
                            ItemWrapper itemNonStack = new(split[0]);
                            itemNonStack.Position = Position;
                            itemNonStack.MapPos = MapPos;
                            CommandManager.SpawnItem(itemNonStack);
                        }
                    }
                }
            }
        }


        public void Update() {
            Point oldPos = new(Position.X, Position.Y);

            int distanceToNearest = 99;
            Point targetPoint = new(-1, -1);
            Actor defender = null;

            if (GameLoop.World.Player.MapPos == MapPos) {
                if (GameLoop.World.Player.CombatLevel < CombatLevel + monster.Confidence || monster.AlwaysAggro) {
                    defender = GameLoop.World.Player;
                    targetPoint = new Point(GameLoop.World.Player.Position.X, GameLoop.World.Player.Position.Y);
                    distanceToNearest = GameLoop.World.maps[MapPos].MapPath.ShortestPath(new GoRogue.Coord(Position.X, Position.Y), new GoRogue.Coord(targetPoint.X, targetPoint.Y)).Length;
                }
            }

            foreach (KeyValuePair<long, Player> kv in GameLoop.World.otherPlayers) {
                if (kv.Value.MapPos == MapPos) {
                    kv.Value.CalculateCombatLevel();
                    if (kv.Value.CombatLevel < CombatLevel + monster.Confidence || monster.AlwaysAggro) {
                        GoRogue.Coord otherPos = new(kv.Value.Position.X, kv.Value.Position.Y);
                        int newDist = GameLoop.World.maps[MapPos].MapPath.ShortestPath(new GoRogue.Coord(Position.X, Position.Y), otherPos).Length;
                        if (newDist < distanceToNearest) {
                            defender = kv.Value;
                            targetPoint = new Point(kv.Value.Position.X, kv.Value.Position.Y);
                            distanceToNearest = newDist;
                        }
                    }
                }
            }

            if (distanceToNearest < 25 && distanceToNearest > 1) {
                if (CurrentPath == null) {
                    CurrentPath = GameLoop.World.maps[MapPos].MapPath.ShortestPath(new GoRogue.Coord(Position.X, Position.Y), new GoRogue.Coord(targetPoint.X, targetPoint.Y));
                    pathPos = 0;
                }

                if (CurrentPath != null && targetPoint != new Point(-1, -1)) {
                    if (TimeLastActed + (120) > SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                        return;
                    }
                    TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;


                    if (CurrentPath.Length > pathPos) {
                        GoRogue.Coord nextStep = CurrentPath.GetStep(pathPos);

                        if (MoveTo(new Point(nextStep.X, nextStep.Y), MapPos))
                            pathPos++;
                    }

                    Point currEnd = new(CurrentPath.End.X, CurrentPath.End.Y);

                    if (targetPoint != currEnd) {
                        CurrentPath = GameLoop.World.maps[MapPos].MapPath.ShortestPath(new GoRogue.Coord(Position.X, Position.Y), new GoRogue.Coord(targetPoint.X, targetPoint.Y));
                        pathPos = 0;
                    }
                }

                if (oldPos != Position) {
                    GameLoop.SendMessageIfNeeded(new string[] { "moveMonster", monster.UniqueID, Position.X.ToString(), Position.Y.ToString(), MapPos.ToString() }, true, false);
                }
            } else if (distanceToNearest == 1) { // Already adjacent, make an attack
                if (defender != null) {
                    if (TimeLastActed + (120) > SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                        return;
                    }
                    TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;

                    CommandManager.Attack(this, defender, true);
                }
            }

            UpdatePosition();
        }
    }
}
