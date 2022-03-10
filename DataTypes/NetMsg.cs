using Newtonsoft.Json;
using SadRogue.Primitives;
using Steamworks;

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptOut)]
    public class NetMsg {
        public SteamId senderID;
        public SteamId recipient = 0;
        public string ident = "";
        public bool Flag = false;
        public string MiscString1 = "";
        public string MiscString2 = "";
        public string MiscString3 = "";
        public string MiscString4 = "";
        public int MiscInt = 0;
        public int MiscInt2 = 0;

        public string WorldArea = "Overworld";
        public int X = 0;
        public int Y = 0;
        public int mX = 0;
        public int mY = 0;
        public int mZ = 0;

        public byte[] data;

        [JsonConstructor]
        public NetMsg() { }

        public NetMsg(string id) {
            ident = id;
        }

        public NetMsg(string id, byte[] payload) {
            ident = id;
            data = payload;
        }

        public void SetPos(int x, int y) {
            X = x;
            Y = y;
        }

        public void SetPos(Point point) {
            X = point.X;
            Y = point.Y;
        }

        public void SetMap(string world, int x, int y, int z) {
            WorldArea = world;
            mX = x;
            mY = y;
            mZ = z;
        }

        public void SetMap(Point3D point) {
            WorldArea = point.WorldArea;
            mX = point.X;
            mY = point.Y;
            mZ = point.Z;
        }

        public void SetFullPos(Point pos, Point3D mapPos) {
            X = pos.X;
            Y = pos.Y;
            WorldArea = mapPos.WorldArea;
            mX = mapPos.X;
            mY = mapPos.Y;
            mZ = mapPos.Z;
        }

        public Point GetPos() {
            return new Point(X, Y);
        }

        public Point3D GetMapPos() {
            return new Point3D(WorldArea, mX, mY, mZ);
        }
    }
}
