using LofiHollow.Managers;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using LofiHollow.DataTypes;

namespace LofiHollow.Entities {
    public class MonsterWrapper : Actor {
        public Monster monster;

        public GoRogue.Pathing.Path CurrentPath;
        public bool UpdatePath = true;
        public int pathPos = 0;
        public int trackingLength = 5;
        public int Level = 1;

        public MonsterWrapper(string name) : base(Color.Black, 32) {
            if (GameLoop.World.itemLibrary.ContainsKey(name)) {
                monster = Monster.Clone(name);

                Appearance.Foreground = new(monster.ForegroundR, monster.ForegroundG, monster.ForegroundB, monster.ForegroundA);
                Appearance.Glyph = monster.ActorGlyph;
                UpdateHP();
                Types = monster.Types;
                ElementalAlignment = monster.ElementalAlignment;
            }
        }

        public MonsterWrapper(Monster temp) : base(Color.Black, 32) {
            monster = Monster.Clone(temp); 

            Appearance.Foreground = new(monster.ForegroundR, monster.ForegroundG, monster.ForegroundB, monster.ForegroundA);
            Appearance.Glyph = monster.ActorGlyph;
            UpdateHP();
            Types = monster.Types;
            ElementalAlignment = monster.ElementalAlignment;
        }

        public void UpdateHP() {
            MaxHP = GetStat(monster.Health, true);
            CurrentHP = MaxHP;
        }

        public int GetStat(int input, bool HP = false) {
            if (HP) {
                int hp = (int) Math.Floor(0.01 * ((2 * input) + Level)) + Level + 10;
                return hp;
            } else {
                int stat = (int) Math.Floor(0.01 * ((2 * input) + Level) + 5);
                return stat;
            }
        }

        public void SpawnDrops(List<Item> drops) {
            for (int i = 0; i < monster.DropTable.Count; i++) {
                string[] split = monster.DropTable[i].Split(";");

                int roll = GameLoop.rand.Next(Int32.Parse(split[1]));

                if (roll == 0) {
                    Item item = Item.Copy(split[0]);

                    if (item.IsStackable) {
                        item.ItemQuantity = GameLoop.rand.Next(Int32.Parse(split[2])) + 1;
                        drops.Add(item);
                    } else {
                        int qty = GameLoop.rand.Next(Int32.Parse(split[2])) + 1;

                        for (int j = 0; j < qty; j++) {
                            Item itemNonStack = Item.Copy(split[0]);
                            drops.Add(itemNonStack);
                        }
                    }
                }
            }
        }
    }
}
