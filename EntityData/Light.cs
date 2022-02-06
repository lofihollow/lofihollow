using ProtoBuf;

namespace LofiHollow.EntityData {
    [ProtoContract]
    public class Light {
        [ProtoMember(1)]
        public double Intensity = 1.0;
        [ProtoMember(2)]
        public int Radius = 5;
    }
}
