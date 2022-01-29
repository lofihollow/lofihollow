using System;
using SadRogue.Primitives;
using System.Text;
using LofiHollow.Entities;
using GoRogue.DiceNotation;
using SadConsole;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LofiHollow.Managers {
    public class CommandManager {
        public CommandManager() { } 

        public static bool MoveActorBy(Actor actor, Point position) {
            bool moved = actor.MoveBy(position);
            if (moved) {
                GameLoop.SendMessageIfNeeded(new string[] { "movePlayer", actor.Position.X.ToString(), actor.Position.Y.ToString(), actor.MapPos.ToString()}, false, true);
                

                if (actor is Player player) {
                    if (player.Sleeping) {
                        GameLoop.UIManager.AddMsg(new ColoredString("You decide not to sleep yet.", Color.Green, Color.Black));
                        GameLoop.World.Player.Sleeping = false;
                        GameLoop.SendMessageIfNeeded(new string[] { "sleep", "false" }, false, true); 
                    }
                }

                if (actor.ScreenAppearance == null) {
                    actor.UpdateAppearance(); 
                }
                actor.UpdatePosition();
            }

            


            return moved;
        }

        public static bool MoveActorTo(Actor actor, Point position, Point3D mapPos) {
            bool moved = actor.MoveTo(position, mapPos);

            if (moved) {
                if (actor.ScreenAppearance == null) {
                    actor.UpdateAppearance();
                }
                actor.UpdatePosition();
            }

            return moved;
        } 

        public static void DropItem(Actor actor, int slot) {
            if (actor.Inventory.Length > slot && actor.Inventory[slot].Name != "(EMPTY)") {
                ItemWrapper item = new(actor.Inventory[slot]); 
                item.Position = actor.Position;
                item.MapPos = actor.MapPos;

                item.UpdateAppearance();
                    
                actor.Inventory[slot] = new Item("lh:(EMPTY)");

                SendItem(item);
                SpawnItem(item);
            }
        }

        public static void SendItem(ItemWrapper wrap) {
            string json = JsonConvert.SerializeObject(wrap, Formatting.Indented);
            GameLoop.SendMessageIfNeeded(new string[] { "spawnItem", json }, false, false);
        }

        public static void SpawnItem(ItemWrapper item) {
            if (!GameLoop.World.maps.ContainsKey(item.MapPos))
                GameLoop.World.LoadMapAt(item.MapPos);

            GameLoop.World.maps[item.MapPos].Add(item);

            GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.MapPos]);
        }

        public static void SendPickup(ItemWrapper item) {
            string json = JsonConvert.SerializeObject(item, Formatting.Indented);
            GameLoop.SendMessageIfNeeded(new string[] { "destroyItem", json }, false, false);
        }

        public static void DestroyItem(ItemWrapper item) {
            if (!GameLoop.World.maps.ContainsKey(item.MapPos))
                GameLoop.World.LoadMapAt(item.MapPos);

            ItemWrapper localCopy = GameLoop.World.maps[item.MapPos].GetEntityAt<ItemWrapper>(item.Position, item.Name);
            if (localCopy != null) {
                GameLoop.UIManager.Map.EntityRenderer.Remove(localCopy);
                GameLoop.World.maps[item.MapPos].Entities.Remove(localCopy); 
            }

            GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.MapPos]);
        }

        public static void PickupItem(Actor actor) {
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

        public static void AddItemToInv(Actor actor, Item item) { 
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
            wrap.Position = actor.Position;
            wrap.MapPos = actor.MapPos;
            SpawnItem(wrap);
        }

        public static string UseItem(Actor actor, Item item) {
            if (item.Heal != null) {
                if (actor.CurrentHP != actor.MaxHP) {
                    int healAmount = GoRogue.DiceNotation.Dice.Roll(item.Heal.HealAmount);

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

        public static void EquipItem(Actor actor, int slot, Item item) {
            if (actor.Inventory.Length > slot && slot >= 0) {

                if (item.EquipSlot >= 0 && item.EquipSlot <= 6) {
                    Item temp = actor.Equipment[item.EquipSlot];
                    actor.Equipment[item.EquipSlot] = item;
                    actor.Inventory[slot] = temp;
                } 
            }
        }

        public static void UnequipItem(Actor actor, int slot) {
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

        public static string RemoveOneItem(Actor actor, int slot) {
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

        public static void SendMonster(Monster monster) {
            string json = JsonConvert.SerializeObject(monster, Formatting.Indented);
            GameLoop.SendMessageIfNeeded(new string[] { "spawnMonster", json }, false, false); 
        }


        public static void SpawnMonster(Monster monster) {
            GameLoop.World.maps[monster.MapPos].Add(monster);

            monster.UpdateAppearance();
            if (monster.MapPos == GameLoop.World.Player.MapPos) {
                //  GameLoop.UIManager.Map.EntityRenderer.Add(monster);
                if (monster.ScreenAppearance == null)
                    monster.UpdateAppearance();
                GameLoop.UIManager.Map.MapConsole.Children.Add(monster.ScreenAppearance);
                GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.MapPos]);
            }
        }

        public static void MoveMonster(string id, Point3D MapPos, Point newPos) {
            foreach (Entity ent in GameLoop.World.maps[MapPos].Entities.Items) {
                if (ent is Monster mon) { 
                    if (mon.UniqueID == id) {
                        mon.MoveTo(newPos, MapPos);
                    }
                }
            }
        }

        public static void DamageMonster(string id, Point3D MapPos, int damage, string battleString, string color) {
            foreach (Entity ent in GameLoop.World.maps[MapPos].Entities.Items) {
                if (ent is Monster mon) {
                    if (mon.UniqueID == id) {
                        Color stringColor = color == "Green" ? Color.Green : color == "Red" ? Color.Red : Color.White;
                         
                        if (MapPos == GameLoop.World.Player.MapPos) {
                            GameLoop.UIManager.AddMsg(new ColoredString(battleString, stringColor, Color.Black));
                        }

                        mon.TakeDamage(damage);
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
                if (GameLoop.World.otherPlayers[id].MapPos == GameLoop.World.Player.MapPos)
                    GameLoop.UIManager.AddMsg(new ColoredString(battleString, hitColor, Color.Black));

                GameLoop.World.otherPlayers[id].TakeDamage(damage);
            }
        }



        public static void Attack(Actor attacker, Actor defender, bool melee) {
            if (attacker == GameLoop.World.Player || defender == GameLoop.World.Player) {
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
                        battleString = attacker.GetAppearance() + new ColoredString(" " + newDamage + " " + ((char) 20) + " ", Color.Red, Color.Black) + defender.GetAppearance();
                    } else {
                        battleString = attacker.GetAppearance() + new ColoredString(" 0 " + ((char) 20) + " ", Color.White, Color.Black) + defender.GetAppearance();
                    }


                    if (attacker is Player && newDamage > 0) {
                        attacker.CombatExp(newDamage);
                        if (!((Monster) defender).AlwaysAggro) {
                            ((Monster)defender).AlwaysAggro = true;
                        }
                    }
                } else {
                    battleString = attacker.GetAppearance() + new ColoredString(" 0 " + ((char)20) + " ", Color.White, Color.Black) + defender.GetAppearance();
                }

                if (defender is Monster mon) {
                    GameLoop.SendMessageIfNeeded(new string[] { "damageMonster", mon.MapPos.ToString(), newDamage.ToString(), battleString.String, battleColor }, false, false);
                } else if (defender is Player player) {
                    if (player == GameLoop.World.Player) {
                        GameLoop.SendMessageIfNeeded(new string[] { "damagePlayer", newDamage.ToString(), battleString.String, battleColor }, false, true);
                    } else {
                        foreach (KeyValuePair<long, Player> kv in GameLoop.World.otherPlayers) {
                            if (player == kv.Value) {
                                GameLoop.SendMessageIfNeeded(new string[] { "damagePlayer", kv.Key.ToString(), newDamage.ToString(), battleString.String, battleColor }, false, false);
                                break;
                            }
                        }
                    }
                }

                if (attacker.MapPos == GameLoop.World.Player.MapPos)
                    GameLoop.UIManager.BattleMsg(battleString);

                defender.TakeDamage(newDamage);

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
