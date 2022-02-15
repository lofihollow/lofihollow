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

        public MonsterWrapper(string name) : base(Color.Black, 32) {
            if (GameLoop.World.itemLibrary.ContainsKey(name)) {
                monster = new(name);

                Appearance.Foreground = new(monster.ForegroundR, monster.ForegroundG, monster.ForegroundB, monster.ForegroundA);
                Appearance.Glyph = monster.ActorGlyph;
            }
        }

        public MonsterWrapper(Monster temp) : base(Color.Black, 32) {
            monster = new();
            monster.SetAll(temp);

            Appearance.Foreground = new(monster.ForegroundR, monster.ForegroundG, monster.ForegroundB, monster.ForegroundA);
            Appearance.Glyph = monster.ActorGlyph;
        }

        public void SpawnDrops() {
            for (int i = 0; i < monster.DropTable.Count; i++) {
                string[] split = monster.DropTable[i].Split(";");

                int roll = GameLoop.rand.Next(Int32.Parse(split[1]));

                if (roll == 0) {
                    ItemWrapper item = new(split[0]);

                    if (item.item.IsStackable) {
                        item.item.ItemQuantity = GameLoop.rand.Next(Int32.Parse(split[2])) + 1;
                        item.Position = Position;
                        item.MapPos = MapPos;
                        CommandManager.SpawnItem(item);
                    } else {
                        int qty = GameLoop.rand.Next(Int32.Parse(split[2])) + 1;

                        for (int j = 0; j < qty; j++) {
                            ItemWrapper itemNonStack = new(split[0]);
                            itemNonStack.Position = Position;
                            itemNonStack.MapPos = MapPos;
                            CommandManager.SpawnItem(itemNonStack);
                        }
                    }
                }
            }
        }
    }
}
