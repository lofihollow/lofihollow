using System.Linq;
using SadRogue.Primitives;
using LofiHollow.Entities;
using LofiHollow.EntityData;
using System.Collections.Generic;
using ProtoBuf;
using Newtonsoft.Json;

namespace LofiHollow {
    [ProtoContract] 
    [JsonObject(MemberSerialization.OptOut)]
    public class Map {
        [ProtoMember(1)]
        public int MinimumMonsters = 0;
        [ProtoMember(2)]
        public int MaximumMonsters = 0;
        [ProtoMember(3)]
        public Tile[] Tiles;
        [ProtoMember(4)]
        public int Width;
        [ProtoMember(5)]
        public int Height;

        [ProtoMember(6)]
        public MinimapTile MinimapTile = new(',', new Color(0, 127, 0), Color.Black);

        [JsonIgnore]
        public int SpawnedMonsters = -1;

        [JsonIgnore]
        public GoRogue.MultiSpatialMap<Entity> Entities;
        [JsonIgnore]
        public GoRogue.Pathing.FastAStar MapPath;
        [JsonIgnore]
        public static GoRogue.IDGenerator IDGenerator = new();
        [JsonIgnore]
        public GoRogue.MapViews.LambdaMapView<bool> MapFOV;
        [JsonIgnore]
        public GoRogue.MapViews.LambdaMapView<double> LightRes;

        public Map() {
            Entities = new GoRogue.MultiSpatialMap<Entity>();

            var MapView = new GoRogue.MapViews.LambdaMapView<bool>(Width, Height, pos => IsTileWalkable(new Point(pos.X, pos.Y)));
            MapFOV = new GoRogue.MapViews.LambdaMapView<bool>(GameLoop.MapWidth, GameLoop.MapHeight, pos => BlockingLOS(new Point(pos.X, pos.Y)));
            MapPath = new GoRogue.Pathing.FastAStar(MapView, GoRogue.Distance.CHEBYSHEV);

            LightRes = new GoRogue.MapViews.LambdaMapView<double>(GameLoop.MapWidth, GameLoop.MapHeight, pos => GetLightRes(new Point(pos.X, pos.Y)));
        }

        public Map(int width, int height) {
            Width = width;
            Height = height;
            Tiles = new Tile[width * height];

            for (int i = 0; i < Tiles.Length; i++) {
                Tiles[i] = new Tile();
            }



            Entities = new GoRogue.MultiSpatialMap<Entity>();

            var MapView = new GoRogue.MapViews.LambdaMapView<bool>(width, height, pos => IsTileWalkable(new Point(pos.X, pos.Y)));
            MapFOV = new GoRogue.MapViews.LambdaMapView<bool>(GameLoop.MapWidth, GameLoop.MapHeight, pos => BlockingLOS(new Point(pos.X, pos.Y)));
            MapPath = new GoRogue.Pathing.FastAStar(MapView, GoRogue.Distance.CHEBYSHEV);

            LightRes = new GoRogue.MapViews.LambdaMapView<double>(GameLoop.MapWidth, GameLoop.MapHeight, pos => GetLightRes(new Point(pos.X, pos.Y)));
        }

        public double GetLightRes(Point location) {
            if (location.X < 0 || location.Y < 0 || location.X >= Width || location.Y >= Height)
                return 0;
            return Tiles[location.Y * Width + location.X].LightBlocked;
        }

        public bool IsTileWalkable(Point location) {
            if (location.X < 0 || location.Y < 0 || location.X >= Width || location.Y >= Height)
                return false;
            if (GetEntityAt<MonsterWrapper>(location) != null)
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
                Tiles[location.Y * Width + location.X].LightBlocked = Tiles[location.Y * Width + location.X].IsBlockingLOS ? 1 : 0;

                NetMsg tileUpdate = new("updateTile", Tiles[(location.Y * Width) + location.X].ToByteArray());
                tileUpdate.X = location.X;
                tileUpdate.Y = location.Y;
                tileUpdate.mX = mapPos.X;
                tileUpdate.mY = mapPos.Y;
                tileUpdate.mZ = mapPos.Z;
                GameLoop.SendMessageIfNeeded(tileUpdate, false, false);

                GameLoop.SoundManager.PlaySound("door");

                return;
            }

            return;
        }

        public T GetEntityAt<T>(Point location) where T : Entity {
            return Entities.GetItems(location.ToCoord()).OfType<T>().FirstOrDefault();
        }

        public List<T> GetAllEntities<T>(Point location) where T : Entity {
            return Entities.GetItems(location.ToCoord()).OfType<T>().ToList();
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

        public Tile GetTile(Point location) {
            return Tiles[location.ToIndex(GameLoop.MapWidth)];
        }

        public void SetTile(Point location, Tile tile) {
            Tiles[location.ToIndex(GameLoop.MapWidth)] = tile;
        }

        public void Remove(Entity entity) {
            Entities.Remove(entity);
            entity.PositionChanged -= OnPositionChange;
        }

        public void Add(Entity entity) {
            Entities.Add(entity, entity.Position.ToCoord());
            entity.PositionChanged += OnPositionChange;
        }

        private void OnPositionChange(object sender, SadConsole.ValueChangedEventArgs<SadRogue.Primitives.Point> e) {
            Entities.Move(sender as Entity, e.NewValue.ToCoord());
        }


        public void PopulateMonsters(Point3D MapPos) {
            int diff = MaximumMonsters - MinimumMonsters;
            int monsterAmount = GameLoop.rand.Next(diff) + MinimumMonsters;


            SpawnedMonsters = monsterAmount;
        }
    }
}
