using LofiHollow.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Photo {
    [JsonObject(MemberSerialization.OptOut)]
    public class Photo { 
        public string PhotoName = "Photo";
        public string SeasonTaken = "Spring";
        public int DayTaken = 0;
        public int MinutesTaken = 0;
        public PhotoTile[] tiles;
        public List<PhotoEntity> entities = new();

        public Photo() {
            tiles = new PhotoTile[441];
        }


        public bool Contains(string name, string type) {
            if (type != "Tile") {
                for (int i = 0; i < entities.Count; i++) {
                    if (entities[i].Name == name || entities[i].Name.Contains(name) && (entities[i].Type == type || entities[i].Type.Contains(type) || type == "Any"))
                        return true;
                }
            } else {
                for (int i = 0; i < tiles.Length; i++) {
                    if (tiles[i].Name == name || tiles[i].Name.Contains(name))
                        return true;
                }
            }

            return false;
        }
    }
}
