using System;
using System.Linq;
using SadRogue.Primitives;
using LofiHollow.Entities;
using Newtonsoft.Json;
using LofiHollow.EntityData;
using System.Collections.Generic;
using LofiHollow.Managers;

namespace LofiHollow {
    [JsonObject(MemberSerialization.OptIn)]
    public class Map {
        [JsonProperty]
        public Dictionary<int,int> MonsterWeights = new();
        [JsonProperty]
        public int MinimumMonsters = 0;
        [JsonProperty]
        public int MaximumMonsters = 0;
        [JsonProperty]
        public TileBase[] Tiles;
        [JsonProperty]
        public int Width;
        [JsonProperty]
        public int Height;

        [JsonProperty]
        public MinimapTile MinimapTile = new(',', new Color(0, 127, 0), Color.Black);

        public int SpawnedMonsters = -1;
       

        public GoRogue.MultiSpatialMap<Entity> Entities;
        public GoRogue.Pathing.FastAStar MapPath;
        public static GoRogue.IDGenerator IDGenerator = new();

        public GoRogue.MapViews.LambdaMapView<bool> MapFOV;


        public Map(int width, int height) {
            Width = width;
            Height = height;
            Tiles = new TileBase[width * height];

            for (int i = 0; i < Tiles.Length; i++) {
                Tiles[i] = new TileBase(); 
            }

            Entities = new GoRogue.MultiSpatialMap<Entity>();

            var MapView = new GoRogue.MapViews.LambdaMapView<bool>(width, height, pos => IsTileWalkable(new Point(pos.X, pos.Y)));
            MapFOV = new GoRogue.MapViews.LambdaMapView<bool>(GameLoop.MapWidth, GameLoop.MapHeight, pos => BlockingLOS(new Point(pos.X, pos.Y)));
            MapPath = new GoRogue.Pathing.FastAStar(MapView, GoRogue.Distance.CHEBYSHEV);
        }
         
        public bool IsTileWalkable(Point location) { 
            if (location.X < 0 || location.Y < 0 || location.X >= Width || location.Y >= Height)
                return false;
            if (GetEntityAt<Monster>(location) != null)
                return false;
            return !Tiles[location.Y * Width + location.X].IsBlockingMove;
        }

        public bool BlockingLOS(Point location) {
            if (location.X < 0 || location.Y < 0 || location.X >= Width || location.Y >= Height)
                return true;
            return !Tiles[location.Y * Width + location.X].IsBlockingLOS;
        }

        public void ToggleLock(Point location, Point3D mapPos) {
            if (location.X < 0 || location.Y < 0 || location.X >= Width || location.Y >= Height)
                return;
            if (Tiles[location.Y * Width + location.X].Lock != null) {
                LockOwner lockData = Tiles[location.Y * Width + location.X].Lock;

                if (lockData.Closed) {
                    Tiles[location.Y * Width + location.X].Glyph = lockData.OpenedGlyph;
                    Tiles[location.Y * Width + location.X].IsBlockingMove = lockData.OpenBlocksMove;
                    Tiles[location.Y * Width + location.X].IsBlockingLOS = false;
                } else {
                    Tiles[location.Y * Width + location.X].Glyph = lockData.ClosedGlyph;
                    Tiles[location.Y * Width + location.X].IsBlockingMove = true;
                    Tiles[location.Y * Width + location.X].IsBlockingLOS = lockData.ClosedBlocksLOS;
                }

                Tiles[location.Y * Width + location.X].Lock.Closed = !Tiles[location.Y * Width + location.X].Lock.Closed;
                String serialized = JsonConvert.SerializeObject(Tiles[location.Y * Width + location.X], Formatting.Indented);
                GameLoop.SendMessageIfNeeded(new string[] { "updateTile", location.X.ToString(), location.Y.ToString(), mapPos.ToString(), serialized}, false, false);
                  
                return;
            }

            return;
        }

        public T GetEntityAt<T>(Point location) where T : Entity {
            return Entities.GetItems(new GoRogue.Coord(location.X, location.Y)).OfType<T>().FirstOrDefault();
        }

        public List<T> GetAllEntities<T>(Point location) where T : Entity {
            return Entities.GetItems(new GoRogue.Coord(location.X, location.Y)).OfType<T>().ToList();
        }

        public T GetEntityAt<T>(Point location, string name) where T : Entity {
            var allItems = Entities.GetItems(new GoRogue.Coord(location.X, location.Y)).OfType<T>().ToList();

            if (allItems.Count > 1) {
                for (int i = 0; i < allItems.Count; i++) {
                    if (allItems[i].Name == name) {
                        return allItems[i];
                    }
                }
            }

            return Entities.GetItems(new GoRogue.Coord(location.X, location.Y)).OfType<T>().FirstOrDefault();
        }

        public TileBase GetTile(Point location) {
            return Tiles[location.ToIndex(GameLoop.MapWidth)];
        }

        public void SetTile(Point location, TileBase tile) {
           Tiles[location.ToIndex(GameLoop.MapWidth)] = tile;
        }

        public void Remove(Entity entity) { 
            Entities.Remove(entity);
            entity.PositionChanged -= OnPositionChange; 
        }
         
        public void Add(Entity entity) { 
            Entities.Add(entity, new GoRogue.Coord(entity.Position.X, entity.Position.Y));
            entity.PositionChanged += OnPositionChange; 
        }

        private void OnPositionChange(object sender, SadConsole.ValueChangedEventArgs<Point> e) {
            Entities.Move(sender as Entity, new GoRogue.Coord(e.NewValue.X, e.NewValue.Y)); 
        } 


        public void PopulateMonsters(Point3D MapPos) {
            int diff = MaximumMonsters - MinimumMonsters;
            int monsterAmount = GameLoop.rand.Next(diff) + MinimumMonsters;

            int weight = 0;

            if (MonsterWeights.Count > 1) {
                foreach(KeyValuePair<int, int> kv in MonsterWeights) {
                    weight += kv.Value;
                }
            } else if (MonsterWeights.Count == 1) {
                weight = MonsterWeights.ElementAt(0).Value;
            }

            int count1 = 0;
            int count2 = 0;
            int count3 = 0;

            SpawnedMonsters = monsterAmount;

            for (int i = 0; i < monsterAmount; i++) {
                if (MonsterWeights.Count > 0) {
                    int randomInWeight = GameLoop.rand.Next(weight);
                    int weightCount = 0;
                    int randomID = 0;

                   
                    for (int j = 0; j < MonsterWeights.Count; j++) {
                        if (randomInWeight <= weightCount + MonsterWeights.ElementAt(j).Value) {
                            randomID = MonsterWeights.ElementAt(j).Key;
                            if (randomID == 1)
                                count1++;
                            else if (randomID == 2)
                                count2++;
                            else if (randomID == 3)
                                count3++;
                            break;
                        } else {
                            weightCount += MonsterWeights.ElementAt(j).Value;
                        }
                    }

                    Monster monster = new(randomID);
                    monster.CalculateCombatLevel();
                    monster.UpdateHP();

                    monster.Position = new Point(GameLoop.rand.Next(Width), GameLoop.rand.Next(Height));
                    
                    while (!IsTileWalkable(monster.Position) || GetEntityAt<Monster>(monster.Position) != null) {
                        monster.Position = new Point(GameLoop.rand.Next(Width), GameLoop.rand.Next(Height));
                    }

                    monster.MapPos = MapPos;

                    CommandManager.SpawnMonster(monster);
                   // GameLoop.CommandManager.SendMonster(monster);
                }
            } 
        } 
    }
}
