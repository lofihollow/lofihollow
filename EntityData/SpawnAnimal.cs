using LofiHollow.Entities;
using Newtonsoft.Json;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptOut)]
    public class SpawnAnimal {
        public string Species = "";
        public int Glyph = 0;
        public int R = 0;
        public int G = 0;
        public int B = 0;
        public string MilkItem = "";
        public string ShearItem = "";
        public int AdultAge = 0;
        public bool Pet = false;

        public FarmAnimal Packaged = null;


        [JsonConstructor]
        public SpawnAnimal() { }

        public SpawnAnimal(FarmAnimal farm) {
            Species = farm.Species;
            Glyph = farm.ActorGlyph;
            R = farm.ForegroundR;
            G = farm.ForegroundG;
            B = farm.ForegroundB;
            Packaged = farm;
            Pet = farm.Pet;
        }


        public FarmAnimal GetAnimal(Point pos) {
            FarmAnimal animal = new(new SadRogue.Primitives.Color(R, G, B), Glyph);
            animal.ActorGlyph = Glyph;
            animal.ForegroundR = R;
            animal.ForegroundG = G;
            animal.ForegroundB = B;
            animal.Species = Species;
            animal.Nickname = Species;
            animal.RestSpot = pos;
            animal.MilkItem = MilkItem;
            animal.ShearItem = ShearItem;
            animal.AdultAge = AdultAge;
            animal.Pet = Pet;

            return animal;
        }
    }
}
