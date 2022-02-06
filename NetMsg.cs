using ProtoBuf;
using SadRogue.Primitives;

namespace LofiHollow {
    [ProtoContract]
    public class NetMsg {
        [ProtoMember(1)]
        public string msgID = "";
        [ProtoMember(2)]
        public long senderID = -1;
        [ProtoMember(3)]
        public int X = -1;
        [ProtoMember(4)]
        public int Y = -1;
        [ProtoMember(5)]
        public int mX = -1;
        [ProtoMember(6)]
        public int mY = -1;
        [ProtoMember(7)]
        public int mZ = -1;
        [ProtoMember(8)]
        public string MiscString = "";
        [ProtoMember(9)]
        public string MiscString1 = "";
        [ProtoMember(10)]
        public string MiscString2 = "";
        [ProtoMember(11)]
        public string MiscString3 = "";
        [ProtoMember(12)]
        public int MiscInt = 0;
        [ProtoMember(20)]
        public byte[] data;

        public NetMsg() { }

        public NetMsg(string ident, byte[] payload) {
            msgID = ident;
            data = payload;
        }

        public void SetMapPos(Point3D mapPos) {
            mX = mapPos.X;
            mY = mapPos.Y;
            mZ = mapPos.Z;
        }

        public void SetPosition(Point point) {
            X = point.X;
            Y = point.Y;
        }
    }
}
