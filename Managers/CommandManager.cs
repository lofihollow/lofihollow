using SadRogue.Primitives;
using LofiHollow.Entities;
using GoRogue.DiceNotation;
using SadConsole;
using System.Collections.Generic;
using LofiHollow.EntityData;

namespace LofiHollow.Managers {
    public class CommandManager {
        public CommandManager() { }

        public static bool MoveActorBy(Actor actor, Point position) {
            bool moved = actor.MoveBy(position);
            if (moved) {
                NetMsg movedPlayer = new("movePlayer", null);
                movedPlayer.SetPosition(actor.Position);
                movedPlayer.SetMapPos(actor.MapPos);
                GameLoop.SendMessageIfNeeded(movedPlayer, false, true);


                if (actor is Player player) {
                    if (player.Sleeping) {
                        GameLoop.UIManager.AddMsg(new ColoredString("You decide not to sleep yet.", Color.Green, Color.Black));
                        NetMsg sleepEnd = new("sleep", false.ToByteArray());
                        GameLoop.World.Player.player.Sleeping = false;
                        GameLoop.SendMessageIfNeeded(sleepEnd, false, true);
                    }
                }
            }




            return moved;
        }

        public static bool MoveActorTo(Actor actor, Point position, Point3D mapPos) {
            bool moved = actor.MoveTo(position, mapPos);
            return moved;
        }

        public static void DropItem(Player actor, int slot) {
            if (actor.Inventory.Length > slot && actor.Inventory[slot].Name != "(EMPTY)") {
                ItemWrapper item = new(actor.Inventory[slot]);
                item.item.Position = actor.Position;
                item.item.MapPos = actor.MapPos;

                item.UpdateAppearance();

                actor.Inventory[slot] = new Item("lh:(EMPTY)");

                SendItem(item);
                SpawnItem(item);
            }
        }

        public static void SendItem(ItemWrapper wrap) {
            NetMsg msg = new("spawnItem", wrap.item.ToByteArray());
            GameLoop.SendMessageIfNeeded(msg, false, false);
        }

        public static void SpawnItem(ItemWrapper wrap) {
            Item item = wrap.item;

            if (!GameLoop.World.maps.ContainsKey(item.MapPos))
                GameLoop.World.LoadMapAt(item.MapPos);

            GameLoop.World.maps[item.MapPos].Add(wrap);

            GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.player.MapPos]);
        }

        public static void SendPickup(ItemWrapper item) {
            NetMsg msg = new("destroyItem", item.ToByteArray());
            GameLoop.SendMessageIfNeeded(msg, false, false);
        }

        public static void DestroyItem(ItemWrapper wrap) {
            Item item = wrap.item;

            if (!GameLoop.World.maps.ContainsKey(item.MapPos))
                GameLoop.World.LoadMapAt(item.MapPos);

            ItemWrapper localCopy = GameLoop.World.maps[item.MapPos].GetEntityAt<ItemWrapper>(item.Position, item.Name);
            if (localCopy != null) {
                GameLoop.UIManager.Map.EntityRenderer.Remove(localCopy);
                GameLoop.World.maps[item.MapPos].Entities.Remove(localCopy);
            }

            GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.player.MapPos]);
        }

        public static void PickupItem(Player actor) {
            ItemWrapper wrap = GameLoop.World.maps[actor.MapPos].GetEntityAt<ItemWrapper>(actor.Position);
            if (wrap != null) {
                for (int i = 0; i < actor.Inventory.Length; i++) {
                    if (actor.Inventory[i].StacksWith(wrap.item)) {
                        actor.Inventory[i].ItemQuantity++;

                        DestroyItem(wrap);
                        SendPickup(wrap);
                        MissionManager.CheckHasItems();
                        return;
                    }
                }

                for (int i = 0; i < actor.Inventory.Length; i++) {
                    if (actor.Inventory[i].Name == "(EMPTY)") {
                        actor.Inventory[i] = wrap.item;
                        DestroyItem(wrap);
                        SendPickup(wrap);
                        MissionManager.CheckHasItems();
                        break;
                    }
                }
            }
        }

        public static void AddItemToInv(Player actor, Item item) {
            if (item != null) {
                for (int i = 0; i < actor.Inventory.Length; i++) {
                    if (actor.Inventory[i].StacksWith(item)) {
                        actor.Inventory[i].ItemQuantity += item.ItemQuantity;
                        MissionManager.CheckHasItems();
                        return;
                    }
                }

                for (int i = 0; i < actor.Inventory.Length; i++) {
                    if (actor.Inventory[i].Name == "(EMPTY)") {
                        actor.Inventory[i] = item;
                        MissionManager.CheckHasItems();
                        return;
                    }
                }
            }

            ItemWrapper wrap = new(item);
            wrap.item.Position = actor.Position;
            wrap.item.MapPos = actor.MapPos;
            SpawnItem(wrap);
        }

        public static string UseItem(Actor actor, Item item) {
            if (item.Heal != null) {
                Heal heal = item.Heal;
                if (actor.CurrentHP != actor.MaxHP) {
                    int healAmount = Dice.Roll(heal.HealAmount);

                    if (actor.CurrentHP + healAmount > actor.MaxHP) {
                        healAmount = actor.MaxHP - actor.CurrentHP;
                    }

                    actor.CurrentHP += healAmount;
                    return "t|Healed " + healAmount + " hit points!";

                } else {
                    return "f|You're already at max HP!";
                }
            }

            return "f|Item usage not implemented.";
        }

        public static void EquipItem(Player actor, int slot, Item item) {
            if (actor.Inventory.Length > slot && slot >= 0) {

                if (item.EquipSlot >= 0 && item.EquipSlot <= 6) {
                    Item temp = actor.Equipment[item.EquipSlot];
                    actor.Equipment[item.EquipSlot] = item;
                    actor.Inventory[slot] = temp;
                }
            }
        }

        public static void UnequipItem(Player actor, int slot) {
            if (slot >= 0 && slot <= 15) {
                if (actor.Equipment[slot].Name != "(EMPTY)") {
                    for (int i = 0; i < actor.Inventory.Length; i++) {
                        if (actor.Inventory[i].Name == "(EMPTY)") {
                            actor.Inventory[i] = actor.Equipment[slot];
                            actor.Equipment[slot] = new Item("lh:(EMPTY)");
                        }
                    }
                }
            }
        }

        public static string RemoveOneItem(Player actor, int slot) {
            string returnID = "";
            if (slot >= 0 && slot <= actor.Inventory.Length) {
                if (actor.Inventory[slot].Name != "(EMPTY)") {
                    if (!actor.Inventory[slot].IsStackable || (actor.Inventory[slot].IsStackable && actor.Inventory[slot].ItemQuantity == 1)) {
                        returnID = actor.Inventory[slot].FullName();
                        actor.Inventory[slot] = new Item("lh:(EMPTY)");
                    } else if (actor.Inventory[slot].IsStackable && actor.Inventory[slot].ItemQuantity > 1) {
                        actor.Inventory[slot].ItemQuantity--;
                        returnID = actor.Inventory[slot].FullName();
                    }
                }
            }

            return returnID;
        }

        public static void SendMonster(MonsterWrapper wrap) {
            NetMsg spawnMonster = new("spawnMonster", wrap.Wrapped.ToByteArray());
            spawnMonster.SetMapPos(wrap.Wrapped.MapPos);
            spawnMonster.SetPosition(wrap.Wrapped.Position);
            GameLoop.SendMessageIfNeeded(spawnMonster, false, false);
        }


        public static void SpawnMonster(string name, Point3D MapPos, Point Pos) {
            if (GameLoop.World.monsterLibrary.ContainsKey(name)) {
                MonsterWrapper wrap = new(name);
                wrap.Wrapped.MapPos = MapPos;
                wrap.Wrapped.Position = Pos;
                GameLoop.World.maps[wrap.Wrapped.MapPos].Add(wrap);

                if (wrap.Wrapped.MapPos == GameLoop.World.Player.player.MapPos) {
                    GameLoop.UIManager.Map.EntityRenderer.Add(wrap);
                    GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.player.MapPos]);
                }
            } else {
                GameLoop.UIManager.AddMsg("Monster ID not found: " + name);
            }
        }

        public static void MoveMonster(string id, Point3D MapPos, Point newPos) {
            foreach (Entity ent in GameLoop.World.maps[MapPos].Entities.Items) {
                if (ent is MonsterWrapper mon) {
                    if (mon.Wrapped.UniqueID == id) {
                        mon.Wrapped.MoveTo(newPos, MapPos);
                    }
                }
            }
        }

        public static void DamageMonster(string id, Point3D MapPos, int damage, string battleString, string color) {
            foreach (Entity ent in GameLoop.World.maps[MapPos].Entities.Items) {
                if (ent is MonsterWrapper mon) {
                    if (mon.Wrapped.UniqueID == id) {
                        Color stringColor = color == "Green" ? Color.Green : color == "Red" ? Color.Red : Color.White;

                        if (MapPos == GameLoop.World.Player.player.MapPos) {
                            GameLoop.UIManager.AddMsg(new ColoredString(battleString, stringColor, Color.Black));
                        }

                        mon.Wrapped.TakeDamage(damage);
                    }
                }
            }
        }

        public static void DamagePlayer(long id, int damage, string battleString, string color) {
            Color hitColor = color == "Green" ? Color.Green : color == "Red" ? Color.Red : Color.White;
            if (!GameLoop.World.otherPlayers.ContainsKey(id)) {
                if (GameLoop.NetworkManager.ownID == id) {
                    GameLoop.UIManager.AddMsg(new ColoredString(battleString, hitColor, Color.Black));
                    GameLoop.World.Player.TakeDamage(damage);
                } else {
                    return;
                }
            } else {
                if (GameLoop.World.otherPlayers[id].player.MapPos == GameLoop.World.Player.player.MapPos)
                    GameLoop.UIManager.AddMsg(new ColoredString(battleString, hitColor, Color.Black));

                GameLoop.World.otherPlayers[id].TakeDamage(damage);
            }
        }



        public static void Attack(Actor attacker, Actor defender, bool melee) {
            if (attacker == GameLoop.World.Player.player || defender == GameLoop.World.Player.player) {
                string damageType = melee ? attacker.GetDamageType() : "Range";
                int attackRoll = attacker.AttackRoll(damageType);
                int defRoll = defender.DefenceRoll(damageType);



                int newDamage = 0;

                float hitChance;

                if (attackRoll > defRoll) {
                    hitChance = 1 - ((defRoll + 2f) / (2f * (attackRoll + 1f)));
                } else {
                    hitChance = attackRoll / (2f * (defRoll + 1f));
                }

                GameLoop.UIManager.AddMsg(attackRoll + " vs " + defRoll + " (" + hitChance + ")");

                hitChance *= 100;

                int roll = GameLoop.rand.Next(100) + 1;

                ColoredString battleString;
                string battleColor = "White";

                if (roll < hitChance) {
                    newDamage = attacker.DamageRoll(damageType);

                    if (newDamage < 0)
                        newDamage = 0;

                    if (newDamage > 0) {
                        battleString = attacker.GetAppearance() + new ColoredString(" " + newDamage + " " + ((char)20) + " ", Color.Red, Color.Black) + defender.GetAppearance();
                    } else {
                        battleString = attacker.GetAppearance() + new ColoredString(" 0 " + ((char)20) + " ", Color.White, Color.Black) + defender.GetAppearance();
                    }


                    if (attacker is Player && newDamage > 0) {
                        attacker.CombatExp(newDamage);
                        if (!((Monster)defender).AlwaysAggro) {
                            ((Monster)defender).AlwaysAggro = true;
                        }
                    }
                } else {
                    battleString = attacker.GetAppearance() + new ColoredString(" 0 " + ((char)20) + " ", Color.White, Color.Black) + defender.GetAppearance();
                }

                if (defender is Monster mon) {
                    NetMsg damagedMonster = new("damageMonster", null);
                    damagedMonster.SetMapPos(mon.MapPos);
                    damagedMonster.MiscString = mon.UniqueID;
                    damagedMonster.MiscInt = newDamage;
                    damagedMonster.MiscString1 = battleString.String;
                    damagedMonster.MiscString2 = battleColor;
                    GameLoop.SendMessageIfNeeded(damagedMonster, false, false);
                } else if (defender is Player player) {
                    if (player == GameLoop.World.Player.player) {
                        NetMsg damagedPlay = new("damagePlayer", null);
                        damagedPlay.MiscInt = newDamage;
                        damagedPlay.MiscString = battleString.String;
                        damagedPlay.MiscString1 = battleColor;
                        GameLoop.SendMessageIfNeeded(damagedPlay, false, true);
                    } else {
                        foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
                            if (player == kv.Value.player) {
                                NetMsg damagedPlay = new("damagePlayer", kv.Key.ToByteArray());
                                damagedPlay.MiscInt = newDamage;
                                damagedPlay.MiscString = battleString.String;
                                damagedPlay.MiscString1 = battleColor;
                                GameLoop.SendMessageIfNeeded(damagedPlay, false, false);
                                break;
                            }
                        }
                    }
                }

                if (attacker.MapPos == GameLoop.World.Player.player.MapPos)
                    GameLoop.UIManager.BattleMsg(battleString);

                if (defender is Monster monster)
                    monster.TakeDamage(newDamage);


                if (defender.CurrentHP <= 0) {
                    if (defender is Monster) {
                        ((Player)attacker).killList.Push(defender.GetAppearance());
                        MissionManager.Increment("Kill", defender.Name, 1);
                    }
                }
            }
        }
    }
}
