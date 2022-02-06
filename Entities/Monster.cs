using System;
using System.Collections.Generic;
using LofiHollow.Managers;
using LofiHollow.Minigames;
using ProtoBuf;

namespace LofiHollow.Entities {
    [ProtoContract]
    public class Monster : Actor {
        [ProtoMember(2)]
        public string Package = "";
        [ProtoMember(3)]
        public string UniqueID;
        [ProtoMember(4)]
        public int MonConstitution = 1;
        [ProtoMember(5)]
        public int MonAttack = 1;
        [ProtoMember(6)]
        public int MonStrength = 1;
        [ProtoMember(7)]
        public int MonDefense = 1;
        [ProtoMember(8)]
        public int MonMagic = 1;
        [ProtoMember(9)]
        public int MonRanged = 1;

        [ProtoMember(10)]
        public string CombatType = "";
        [ProtoMember(11)]
        public string DamageType = "Crush";
        [ProtoMember(12)]
        public string SpecificWeakness = "";
        [ProtoMember(13)]
        public string SpawnLocation = "";

        [ProtoMember(14)]
        public int Confidence = 0;

        [ProtoMember(15)]
        public bool AlwaysAggro = false;

        [ProtoMember(16)]
        public bool CanDropEgg = false;
        [ProtoMember(17)]
        public Egg EggData;

        [ProtoMember(18)]
        public List<string> DropTable = new();


        public Monster() { }

        public Monster(string name) {
            if (GameLoop.World.monsterLibrary != null && GameLoop.World.monsterLibrary.ContainsKey(name)) {
                Monster temp = GameLoop.World.monsterLibrary[name];
                SetAll(temp);
            }
        }

        public void SpawnDrops() {
            for (int i = 0; i < DropTable.Count; i++) {
                string[] split = DropTable[i].Split(";");

                int roll = GameLoop.rand.Next(Int32.Parse(split[1]));

                if (roll == 0) {
                    ItemWrapper item = new(split[0]);

                    if (item.item.IsStackable) {
                        item.item.ItemQuantity = GameLoop.rand.Next(Int32.Parse(split[2])) + 1;
                        item.item.Position = Position;
                        item.item.MapPos = MapPos;
                        CommandManager.SpawnItem(item);
                    } else {
                        int qty = GameLoop.rand.Next(Int32.Parse(split[2])) + 1;

                        for (int j = 0; j < qty; j++) {
                            ItemWrapper itemNonStack = new(split[0]);
                            itemNonStack.item.Position = Position;
                            itemNonStack.item.MapPos = MapPos;
                            CommandManager.SpawnItem(itemNonStack);
                        }
                    }
                }
            }
        }

        public void SetAll(Monster temp) {
            UniqueID = Guid.NewGuid().ToString("N");

            Name = temp.Name;
            Package = temp.Package;

            MonConstitution = temp.MonConstitution;
            MonAttack = temp.MonAttack;
            MonStrength = temp.MonStrength;
            MonDefense = temp.MonDefense;
            MonMagic = temp.MonMagic;
            MonRanged = temp.MonRanged;

            CombatType = temp.CombatType;
            SpecificWeakness = temp.SpecificWeakness;
            DamageType = temp.DamageType;
            SpawnLocation = temp.SpawnLocation;

            CanDropEgg = temp.CanDropEgg;
            EggData = temp.EggData;

            ForegroundR = temp.ForegroundR;
            ForegroundG = temp.ForegroundG;
            ForegroundB = temp.ForegroundB;
            ActorGlyph = temp.ActorGlyph;
        }

        public string FullName() {
            return Package + ":" + Name;
        }
    }
}
