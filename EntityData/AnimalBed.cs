using LofiHollow.DataTypes;
using LofiHollow.Entities;
using Newtonsoft.Json;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.EntityData {
    public class AnimalBed {
        public FarmAnimal Animal;
        public Point Location;
        public Point3D MapPos;

        [JsonConstructor]
        public AnimalBed() { }


        public AnimalBed(Point loc, Point3D map) {
            Location = loc;
            MapPos = map;
        }

        public void SpawnAnimal() {
            if (Animal != null) {
                Map map = Helper.ResolveMap(MapPos);
                if (map != null) {
                    Animal.Position = Location;
                    Animal.MapPos = MapPos;
                    Animal.RestSpot = Location;
                    Animal.Name = Animal.Nickname;
                    map.Add(Animal);
                    if (MapPos == GameLoop.World.Player.MapPos) {
                        GameLoop.UIManager.Map.SyncMapEntities(map);
                    }
                }
            }
        }

        public void PlaceBed(Point loc, Point3D map) {
            Location = loc;
            MapPos = map;
            SpawnAnimal();
        }

    }
}
