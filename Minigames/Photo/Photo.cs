using Newtonsoft.Json;
using ProtoBuf;
using System.Collections.Generic; 

namespace LofiHollow.Minigames.Photo {
    [JsonObject(MemberSerialization.OptOut)]
    [ProtoContract]
    public class Photo { 
        [ProtoMember(1)]
        public string PhotoName = "Photo";
        [ProtoMember(2)]
        public string SeasonTaken = "Spring";
        [ProtoMember(3)]
        public int DayTaken = 0;
        [ProtoMember(4)]
        public int MinutesTaken = 0;
        [ProtoMember(5)]
        public PhotoTile[] tiles;
        [ProtoMember(6)]
        public List<PhotoEntity> entities = new();

        [JsonConstructor]
        public Photo() { }

        public Photo(bool newPhoto) {
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
