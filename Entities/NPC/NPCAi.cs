using LofiHollow.DataTypes;
using Newtonsoft.Json;
using SadRogue.Primitives;
using System;

namespace LofiHollow.Entities.NPC {
    [JsonObject(MemberSerialization.OptIn)]
    public class NPCAi {
        [JsonProperty]
        public Schedule SpringNormal;
        [JsonProperty]
        public Schedule SpringRain;

        [JsonProperty]
        public Schedule SummerNormal;
        [JsonProperty]
        public Schedule SummerRain;

        [JsonProperty]
        public Schedule FallNormal;
        [JsonProperty]
        public Schedule FallRain;

        [JsonProperty]
        public Schedule WinterNormal;
        [JsonProperty]
        public Schedule WinterSnow;

        [JsonProperty]
        public Schedule Holiday;
        [JsonProperty]
        public Schedule Birthday;
        [JsonProperty]
        public Schedule Default;


        public Schedule Current;
        public GoRogue.Pathing.Path CurrentPath;
        public bool UpdatePath = true;
        public int pathPos = 0;


        public void SetSchedule(string season, string weather) {
            if (season == "Spring") {
                if (weather == "Sunny") {
                    if (SpringNormal != null && SpringNormal.Nodes.Count > 0)
                        Current = new(SpringNormal);
                } else {
                    if (SpringRain != null && SpringRain.Nodes.Count > 0)
                        Current = new(SpringRain);
                } 
            } if (season == "Summer") {
                if (weather == "Sunny") {
                    if (SummerNormal != null && SummerNormal.Nodes.Count > 0)
                        Current = new(SummerNormal);
                } else {
                    if (SummerRain != null && SummerRain.Nodes.Count > 0)
                        Current = new(SummerRain);
                }
            }

            if (season == "Fall") {
                if (weather == "Sunny") {
                    if (FallNormal != null && FallNormal.Nodes.Count > 0)
                        Current = new(FallNormal);
                } else {
                    if (FallRain != null && FallRain.Nodes.Count > 0)
                        Current = new(FallRain);
                }
            }

            if (season == "Winter") {
                if (weather == "Sunny") {
                    if (WinterNormal != null && WinterNormal.Nodes.Count > 0)
                        Current = new(WinterNormal);
                } else {
                    if (WinterSnow != null && WinterSnow.Nodes.Count > 0)
                        Current = new(WinterSnow);
                }
            }

            if (season == "Holiday") { 
                if (Holiday != null && Holiday.Nodes.Count > 0)
                    Current = new(Holiday); 
            }
            if (season == "Birthday") { 
                if (Birthday != null && Birthday.Nodes.Count > 0)
                    Current = new(Birthday); 
            }

            if (Current == null) { Current = new(Default); }

            if (Current.Nodes != null) {
                Current.CurrentNode = 0;
                int CurrentTime = Int32.Parse(Current.Nodes[0].Split(";")[0]);
                int NextTime = Int32.Parse(Current.Nodes[0].Split(";")[0]);

                if (Current.Nodes.Count > 1) {
                    string[] NextNode = Current.Nodes[1].Split(";");
                    NextTime = Int32.Parse(NextNode[0]);
                }

                Current.NextNodeTime = Current.Nodes.Count > 1 ? NextTime : CurrentTime;
            }
        }



        public void UpdateNode(int CurrentTime) {
            if (Current.Nodes != null) {
                if (CurrentTime > Current.NextNodeTime - 10) {
                    if (Current.CurrentNode + 1 < Current.Nodes.Count) {
                        Current.CurrentNode++;
                        if (Current.CurrentNode + 1 < Current.Nodes.Count)
                            Current.NextNodeTime = Int32.Parse(Current.Nodes[Current.CurrentNode + 1].Split(";")[0]);
                    }
                }

                if (CurrentTime == -1) {
                    Current.CurrentNode = 0;
                    Current.NextNodeTime = Int32.Parse(Current.Nodes[Current.CurrentNode + 1].Split(";")[0]);
                }
            }
        }

        public void MoveTowardsNode(int CurrentTime, NPC npc) { 
            UpdateNode(CurrentTime);

            string[] FullNode = Current.Nodes[Current.CurrentNode].Split(";");
            int mX = Int32.Parse(FullNode[1]);
            int mY = Int32.Parse(FullNode[2]);
            int mZ = Int32.Parse(FullNode[3]);

            int pX = Int32.Parse(FullNode[4]);
            int pY = Int32.Parse(FullNode[5]);

            Point3D destination = new(mX, mY, mZ);
            Point pos = new(pX, pY);
            bool recentlyMovedMaps = false;

            if (npc.MapPos == destination && npc.Position == pos)
                return;

            int edgeX = 0;
            int edgeY = 0;

            if (CurrentTime > Current.NextNodeTime + 30) { // If they're more than 30 minutes late and the player is out of sight, teleport to their node
                if (GameLoop.World.Player.MapPos != npc.MapPos && GameLoop.World.Player.MapPos != destination) {
                    npc.Position = pos;
                    npc.MapPos = destination;
                }
            } 

            if (npc.MapPos == destination) { // Node they're meant to go to is on the same map
                if (npc.Position == pos) { // They're already there, no need to go further.
                    CurrentPath = null;
                    pathPos = 0;
                    return;
                } else { 
                    edgeX = pos.X;
                    edgeY = pos.Y;
                }
            } else { // Node is on another map, path to the current maps edge 
                if (destination.X > npc.MapPos.X)
                    edgeX = GameLoop.MapWidth - 1;
                else if (destination.X == npc.MapPos.X)
                    edgeX = npc.Position.X;
                if (destination.Y > npc.MapPos.Y)
                    edgeY = GameLoop.MapHeight - 1;
                else if (destination.Y == npc.MapPos.Y)
                    edgeY = npc.Position.Y;

                if (destination.X == npc.MapPos.X && destination.Y == npc.MapPos.Y && destination.Z != npc.MapPos.Z) { 
                    // Right XY, wrong elevation
                    if (npc.Position == pos) {
                        Map map = Helper.ResolveMap(npc.MapPos);

                        if (map != null) {
                            if (map.MapPath.ShortestPath(npc.Position.ToCoord(), pos.ToCoord()) == null) { // At the right spot on the wrong level or couldn't path to the right spot, find the nearest staircase
                                int distanceToFound = 100;
                                string findName = destination.Z > npc.MapPos.Z ? "Up Stairs" : "Down Stairs";

                                for (int x = 0; x < GameLoop.MapWidth; x++) {
                                    for (int y = 0; y < GameLoop.MapHeight; y++) {
                                        if (destination.Z != npc.MapPos.Z) {
                                            if (map.GetTile(new Point(x, y)).Name == findName) {
                                                int distance = map.MapPath.ShortestPath(npc.Position.ToCoord(), new GoRogue.Coord(x, y)).LengthWithStart;
                                                if (distance < distanceToFound) {
                                                    distanceToFound = distance;
                                                    edgeX = x;
                                                    edgeY = y;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } else {
                        edgeX = pos.X;
                        edgeY = pos.Y;
                    }
                } 
            }

            if (CurrentPath == null) {
                Map map = Helper.ResolveMap(npc.MapPos);

                if (map != null) {
                    CurrentPath = map.MapPath.ShortestPath(npc.Position.ToCoord(), new GoRogue.Coord(edgeX, edgeY));
                    pathPos = 0;
                }
            }

            if (pathPos > 0)
                recentlyMovedMaps = false;


            if (CurrentPath != null) {
                if (npc.TimeLastActed + (120) > SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                    return;
                }
                npc.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;


                if (CurrentPath.LengthWithStart > pathPos) {
                    GoRogue.Coord nextStep = CurrentPath.GetStepWithStart(pathPos);

                    if (npc.MoveTo(new Point(nextStep.X, nextStep.Y), npc.MapPos))
                        pathPos++;
                }

                if (npc.Position == new Point(edgeX, edgeY)) {
                    CurrentPath = null;
                    pathPos = 0;
                }
                if (npc.MapPos != destination && !recentlyMovedMaps) {

                    if (npc.Position.X == 0 && !recentlyMovedMaps) {
                        npc.MoveTo(new Point(GameLoop.MapWidth - 2, npc.Position.Y), npc.MapPos + new Point3D(-1, 0, 0));
                        recentlyMovedMaps = true;
                    }
                    if (npc.Position.X == GameLoop.MapWidth - 1 && !recentlyMovedMaps) {
                        npc.MoveTo(new Point(1, npc.Position.Y), npc.MapPos + new Point3D(1, 0, 0));
                        recentlyMovedMaps = true;
                    }

                    if (npc.Position.Y == 0 && !recentlyMovedMaps) {
                        npc.MoveTo(new Point(npc.Position.X, GameLoop.MapHeight - 2), npc.MapPos + new Point3D(0, -1, 0));
                        recentlyMovedMaps = true;
                    }
                    if (npc.Position.Y == GameLoop.MapHeight - 1 && !recentlyMovedMaps) {
                        npc.MoveTo(new Point(npc.Position.X, 1), npc.MapPos + new Point3D(0, 1, 0));
                        recentlyMovedMaps = true;
                    }

                    Map map = Helper.ResolveMap(npc.MapPos);

                    if (map != null) {
                        if (map.GetTile(npc.Position).Name == "Up Stairs" && !recentlyMovedMaps) {
                            npc.MoveTo(new Point(npc.Position.X, npc.Position.Y), npc.MapPos + new Point3D(0, 0, 1));
                            recentlyMovedMaps = true;
                        }

                        if (map.GetTile(npc.Position).Name == "Down Stairs" && !recentlyMovedMaps) {
                            npc.MoveTo(new Point(npc.Position.X, npc.Position.Y), npc.MapPos + new Point3D(0, 0, -1));
                            recentlyMovedMaps = true;
                        }
                    }

                    if (recentlyMovedMaps) {
                        if (npc.MapPos != GameLoop.World.Player.MapPos || (npc.MapPos.X == GameLoop.World.Player.MapPos.X && npc.MapPos.Y == GameLoop.World.Player.MapPos.Y && npc.MapPos.Z > GameLoop.World.Player.MapPos.Z)) {
                            GameLoop.UIManager.Map.EntityRenderer.Remove(npc);
                        } else {
                            GameLoop.UIManager.Map.EntityRenderer.Add(npc);
                        }
                        CurrentPath = null;
                        pathPos = 0;
                    }
                }
            }
        } 
    }
}
