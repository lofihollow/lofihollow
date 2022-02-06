﻿using System.Collections.Generic;
using System.Linq;
using LofiHollow.Entities;
using SadRogue.Primitives;
using ProtoBuf;

namespace LofiHollow.Minigames.Mining {
    [ProtoContract]
    public class MineLevel {
        [ProtoMember(1)]
        public MineTile[] Mine;
        [ProtoMember(2)]
        public int Depth = 0;

        public GoRogue.MultiSpatialMap<Entity> Entities;
        public GoRogue.Pathing.FastAStar MapPath;
        public GoRogue.MapViews.LambdaMapView<bool> MapFOV;

        public MineLevel() {
            Entities = new GoRogue.MultiSpatialMap<Entity>();

            var MapView = new GoRogue.MapViews.LambdaMapView<bool>(70, 40, pos => IsTileWalkable(new Point(pos.X, pos.Y)));
            MapFOV = new GoRogue.MapViews.LambdaMapView<bool>(GameLoop.MapWidth, GameLoop.MapHeight, pos => BlockingLOS(new Point(pos.X, pos.Y)));
            MapPath = new GoRogue.Pathing.FastAStar(MapView, GoRogue.Distance.CHEBYSHEV);
        }

        public MineLevel(int dep, string loc) {
            Entities = new GoRogue.MultiSpatialMap<Entity>();
            Mine = new MineTile[70 * 40];
            Depth = dep;

            if (loc == "Mountain") {
                int topY = 0;
                if (Depth == 0) {
                    for (int x = 0; x < 70; x++) {
                        for (int y = 0; y < 8; y++) {
                            if (y == 4 && x == 35) {
                                Mine[x + (y * 70)] = new MineTile("Mine Entrance");
                            } else if (y < 5) {
                                Mine[x + (y * 70)] = new MineTile("Air");
                            } else if (y == 5) {
                                Mine[x + (y * 70)] = new MineTile("Grass");
                            } else {
                                Mine[x + (y * 70)] = new MineTile("Dirt");
                            }
                        }
                    }

                    topY = 8;
                }

                for (int x = 0; x < 70; x++) {
                    for (int y = topY; y < 40; y++) {
                        Mine[x + (y * 70)] = new MineTile("Stone");
                    }
                }


                foreach (KeyValuePair<string, MineTile> kv in GameLoop.World.mineTileLibrary) {
                    MineTile tile = new(kv.Value);

                    if (tile.MinDepth <= Depth && Depth <= tile.MaxDepth && tile.Spawns) {
                        for (int j = 0; j < tile.MaxPerMap; j++) {

                            if (GameLoop.rand.Next(100) + 1 < tile.Chance) {
                                int x = GameLoop.rand.Next(70);
                                int y = GameLoop.rand.Next(40);

                                if (GetTile(new Point(x, y)).Name == "Stone") {
                                    SetTile(new Point(x, y), new MineTile(tile));
                                }
                            }
                        }
                    }
                }
            }

            if (loc == "Lake") {
                int topY = 0;
                if (Depth == 0) {
                    for (int x = 0; x < 70; x++) {
                        for (int y = 0; y < 8; y++) {
                            if (y == 4 && x == 35) {
                                Mine[x + (y * 70)] = new MineTile("Mine Entrance");
                            } else if (y < 5) {
                                Mine[x + (y * 70)] = new MineTile("Air");
                            } else if (y == 5) {
                                Mine[x + (y * 70)] = new MineTile("Grass");
                            } else {
                                Mine[x + (y * 70)] = new MineTile("Dirt");
                            }
                        }
                    }

                    topY = 8;
                }

                for (int x = 0; x < 70; x++) {
                    for (int y = topY; y < 40; y++) {
                        Mine[x + (y * 70)] = new MineTile("Stone");
                    }
                }


                foreach (KeyValuePair<string, MineTile> kv in GameLoop.World.mineTileLibrary) {
                    MineTile tile = new(kv.Value);

                    if (tile.MinDepth <= Depth + 5 && Depth + 5 <= tile.MaxDepth && tile.Spawns) {
                        for (int j = 0; j < tile.MaxPerMap; j++) {

                            if (GameLoop.rand.Next(100) + 1 < tile.Chance) {
                                int x = GameLoop.rand.Next(70);
                                int y = GameLoop.rand.Next(40);

                                if (GetTile(new Point(x, y)).Name == "Stone") {
                                    SetTile(new Point(x, y), new MineTile(tile));
                                }
                            }
                        }
                    }
                }
            }


            var MapView = new GoRogue.MapViews.LambdaMapView<bool>(70, 40, pos => IsTileWalkable(new Point(pos.X, pos.Y)));
            MapFOV = new GoRogue.MapViews.LambdaMapView<bool>(GameLoop.MapWidth, GameLoop.MapHeight, pos => BlockingLOS(new Point(pos.X, pos.Y)));
            MapPath = new GoRogue.Pathing.FastAStar(MapView, GoRogue.Distance.CHEBYSHEV);
        }

        public bool IsTileWalkable(Point pos) {
            if (pos.X < 0 || pos.Y < 0 || pos.X > 70 || pos.Y > 40)
                return false;

            return !GetTile(pos).BlocksMove;
        }

        public bool BlockingLOS(Point pos) {
            if (pos.X < 0 || pos.Y < 0 || pos.X > 70 || pos.Y > 40)
                return true;

            return !GetTile(pos).BlocksLOS;
        }


        public MineTile GetTile(Point pos) {
            if (pos.X + (pos.Y * 70) >= Mine.Length || pos.X + (pos.Y * 70) < 0)
                return null;


            return Mine[pos.X + (pos.Y * 70)];
        }

        public void TileToAir(Point pos) {
            Mine[pos.X + (pos.Y * 70)] = new MineTile("Air");

            if (pos.Y < 5) {
                Mine[pos.X + (pos.Y * 70)].ForeR = 0;
                Mine[pos.X + (pos.Y * 70)].ForeG = 180;
                Mine[pos.X + (pos.Y * 70)].ForeB = 180;
            } else if (pos.Y < 8) {
                Mine[pos.X + (pos.Y * 70)].ForeR = 91;
                Mine[pos.X + (pos.Y * 70)].ForeG = 46;
                Mine[pos.X + (pos.Y * 70)].ForeB = 13;
            } else {
                Mine[pos.X + (pos.Y * 70)].ForeR = 81;
                Mine[pos.X + (pos.Y * 70)].ForeG = 81;
                Mine[pos.X + (pos.Y * 70)].ForeB = 81;
            }

            Mine[pos.X + (pos.Y * 70)].Unshade();
        }

        public void SetTile(Point pos, MineTile set) {
            Mine[pos.X + (pos.Y * 70)] = set;
        }

        public T GetEntityAt<T>(Point location) where T : Entity {
            return Entities.GetItems(location.ToCoord()).OfType<T>().FirstOrDefault();
        }

        public T GetEntityAt<T>(Point location, string name) where T : Entity {
            var allItems = Entities.GetItems(location.ToCoord()).OfType<T>().ToList();

            if (allItems.Count > 1) {
                for (int i = 0; i < allItems.Count; i++) {
                    if (allItems[i].Name == name) {
                        return allItems[i];
                    }
                }
            }

            return Entities.GetItems(location.ToCoord()).OfType<T>().FirstOrDefault();
        }

        public void Remove(Entity entity) {
            Entities.Remove(entity);
            entity.PositionChanged -= OnPositionChange;
        }

        public void Add(Entity entity) {
            Entities.Add(entity, entity.Position.ToCoord());
            entity.PositionChanged += OnPositionChange;
        }

        private void OnPositionChange(object sender, SadConsole.ValueChangedEventArgs<Point> e) {
            Entities.Move(sender as Entity, e.NewValue.ToCoord());
        }

    }
}
