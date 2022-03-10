using System;
using SadRogue.Primitives;
using System.Text;
using LofiHollow.Entities;
using GoRogue.DiceNotation;
using SadConsole;
using Newtonsoft.Json;
using System.Collections.Generic;
using LofiHollow.EntityData;
using LofiHollow.DataTypes;
using Steamworks;

namespace LofiHollow.Managers {
    public class CommandManager {
        public CommandManager() { } 

        public static bool MoveActorBy(Actor actor, Point position) {
            Point3D oldMap = actor.MapPos;
            Point oldPos = actor.Position;
            bool moved = actor.MoveBy(position);
            if (moved) {
                NetMsg movePlayer = new("movePlayer");
                movePlayer.SetFullPos(actor.Position, actor.MapPos);
                GameLoop.SendMessageIfNeeded(movePlayer, false, true);
                if (actor is Player play && play.Equipment[10].Name != "(EMPTY)") {
                    play.Equipment[10].SoulPhoto.Position = oldPos;
                }
                    
                if (oldMap != actor.MapPos) {
                    NetMsg reqMap = new("fullMap");
                    reqMap.SetMap(actor.MapPos);
                    GameLoop.SendMessageIfNeeded(reqMap, false, false, 0);

                    if (actor is Player playa && playa.Equipment[10].Name != "(EMPTY)") {
                        playa.Equipment[10].SoulPhoto.Position = playa.Position;
                    }
                }


                if (actor is Player player) {
                    if (player.Sleeping) {
                        GameLoop.UIManager.AddMsg(new ColoredString("You decide not to sleep yet.", Color.Green, Color.Black));
                        GameLoop.World.Player.Sleeping = false;
                        NetMsg sleep = new("sleep");
                        sleep.Flag = false;
                        GameLoop.SendMessageIfNeeded(sleep, false, true);
                    }

                    Map map = Helper.ResolveMap(player.MapPos);

                    if (map != null) {
                        if (map.GetTile(player.Position).TeleportTile != null) {
                            TeleportTile tele = map.GetTile(player.Position).TeleportTile;
                            player.MoveTo(tele.Pos, tele.MapPos); 
                        }
                    }
                } 
            }

            


            return moved;
        }

        public static bool MoveActorTo(Actor actor, Point position, Point3D mapPos) {
            bool moved = actor.MoveTo(position, mapPos);
            Point3D oldMap = actor.MapPos;
            if (moved) {
                NetMsg movePlayer = new("movePlayer");
                movePlayer.SetFullPos(actor.Position, actor.MapPos);
                GameLoop.SendMessageIfNeeded(movePlayer, false, true);

                if (oldMap != actor.MapPos) {
                    NetMsg reqMap = new("fullMap");
                    reqMap.SetMap(actor.MapPos);
                    GameLoop.SendMessageIfNeeded(reqMap, false, false, 0);
                } 
            }

            return moved;
        } 

        public static void DropItem(Player actor, int slot) {
            if (actor.Inventory.Length > slot && actor.Inventory[slot].Name != "(EMPTY)") {
                ItemWrapper item = new(actor.Inventory[slot]); 
                item.Position = actor.Position;
                item.MapPos = actor.MapPos;

                item.UpdateAppearance();
                    
                actor.Inventory[slot] = Item.Copy("lh:(EMPTY)");

                SendItem(item);
                SpawnItem(item);
            }
        }

        public static void SendItem(ItemWrapper wrap) {
            NetMsg spawnItem = new("spawnItem", wrap.item.ToByteArray());
            spawnItem.SetFullPos(wrap.Position, wrap.MapPos);
            GameLoop.SendMessageIfNeeded(spawnItem, false, false);
        }

        public static void SpawnItem(ItemWrapper item) {
            Map map = Helper.ResolveMap(item.MapPos);

            if (map != null) {
                map.Add(item); 
                GameLoop.UIManager.Map.SyncMapEntities(map);
            }
        }

        public static void SendPickup(ItemWrapper wrap) {
            NetMsg sendPickup = new("destroyItem", wrap.item.ToByteArray());
            sendPickup.SetFullPos(wrap.Position, wrap.MapPos); 
            GameLoop.SendMessageIfNeeded(sendPickup, false, false);
        }

        public static void DestroyItem(ItemWrapper item) { 
            Map map = Helper.ResolveMap(item.MapPos);

            if (map != null) {
                ItemWrapper localCopy = map.GetEntityAt<ItemWrapper>(item.Position, item.Name);
                if (localCopy != null) {
                    GameLoop.UIManager.Map.EntityRenderer.Remove(localCopy);
                    map.Entities.Remove(localCopy);
                }

                GameLoop.UIManager.Map.SyncMapEntities(map);
            }
        }

        public static void PickupItem(Player actor) {
            Map map = Helper.ResolveMap(actor.MapPos);

            if (map != null) {
                ItemWrapper wrap = map.GetEntityAt<ItemWrapper>(actor.Position);
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
            wrap.Position = actor.Position;
            wrap.MapPos = actor.MapPos;
            SpawnItem(wrap);
        }

        public static string UseItem(Actor actor, Item item) {
            if (item.Heal != null) {
                Heal heal = item.Heal;
                if (actor.CurrentHP != actor.MaxHP) {
                    int healAmount = GoRogue.DiceNotation.Dice.Roll(heal.HealAmount);

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

                if (item.EquipSlot >= 0 && item.EquipSlot <= 10) {
                    Item temp = actor.Equipment[item.EquipSlot];
                    actor.Equipment[item.EquipSlot] = item;
                    actor.Inventory[slot] = temp;
                }
            }
        }

        public static void UnequipItem(Player actor, int slot) {
            if (slot >= 0 && slot <= 10) {
                if (actor.Equipment[slot].Name != "(EMPTY)") {
                    AddItemToInv(actor, actor.Equipment[slot]); 
                    actor.Equipment[slot] = Item.Copy("lh:(EMPTY)"); 
                } 
            }
        }

        public static string RemoveOneItem(Player actor, int slot) {
            string returnID = "";
            if (slot >= 0 && slot <= actor.Inventory.Length) {
                if (actor.Inventory[slot].Name != "(EMPTY)") {
                    if (!actor.Inventory[slot].IsStackable || (actor.Inventory[slot].IsStackable && actor.Inventory[slot].ItemQuantity == 1)) {
                        returnID = actor.Inventory[slot].FullName();
                        actor.Inventory[slot] = Item.Copy("lh:(EMPTY)");
                    } else if (actor.Inventory[slot].IsStackable && actor.Inventory[slot].ItemQuantity > 1) {
                        actor.Inventory[slot].ItemQuantity--;
                        returnID = actor.Inventory[slot].FullName();
                    }
                }
            }

            return returnID;
        }

        public static void SendMonster(MonsterWrapper wrap) {
            NetMsg spawnMon = new("spawnMonster");
            spawnMon.MiscString1 = wrap.monster.FullName();
            spawnMon.SetFullPos(wrap.Position, wrap.MapPos);
            GameLoop.SendMessageIfNeeded(spawnMon, false, false); 
        }


        public static void SpawnMonster(string name, Point3D MapPos, Point Pos) {
            if (GameLoop.World.monsterLibrary.ContainsKey(name)) {
                MonsterWrapper wrap = new(name);
                wrap.MapPos = MapPos;
                wrap.Position = Pos;
                GameLoop.World.maps[wrap.MapPos].Add(wrap);

                wrap.UpdateAppearance();
                if (wrap.MapPos == GameLoop.World.Player.MapPos) {
                    Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);

                    if (map != null) {
                        GameLoop.UIManager.Map.SyncMapEntities(map);
                    }
                }
            } else {
                GameLoop.UIManager.AddMsg("Monster ID not found: " + name);
            }
        }

        public static void MoveMonster(string id, Point3D MapPos, Point newPos) {
            foreach (Entity ent in GameLoop.World.maps[MapPos].Entities.Items) {
                if (ent is MonsterWrapper mon) { 
                    if (mon.monster.UniqueID == id) {
                        mon.MoveTo(newPos, MapPos);
                    }
                }
            }
        }

        public static void DamageMonster(string id, Point3D MapPos, int damage, string battleString, string color, string atkType) {
            foreach (Entity ent in GameLoop.World.maps[MapPos].Entities.Items) {
                if (ent is MonsterWrapper mon) {
                    if (mon.monster.UniqueID == id) {
                        Color stringColor = color == "Green" ? Color.Green : color == "Red" ? Color.Red : Color.White;
                         
                        if (MapPos == GameLoop.World.Player.MapPos) {
                            GameLoop.UIManager.AddMsg(new ColoredString(battleString, stringColor, Color.Black));
                        }

                        mon.TakeDamage(damage, atkType);
                    }
                }
            }
        }

        public static void DamagePlayer(SteamId id, int damage, string battleString, string color, string atkType) {
            Color hitColor = color == "Green" ? Color.Green : color == "Red" ? Color.Red : Color.White;
            if (!GameLoop.World.otherPlayers.ContainsKey(id)) {
                if (SteamClient.SteamId == id) { 
                    GameLoop.UIManager.AddMsg(new ColoredString(battleString, hitColor, Color.Black));
                    GameLoop.World.Player.TakeDamage(damage, atkType);
                } else {
                    return;
                }
            } else {
                if (GameLoop.World.otherPlayers[id].MapPos == GameLoop.World.Player.MapPos)
                    GameLoop.UIManager.AddMsg(new ColoredString(battleString, hitColor, Color.Black));

                GameLoop.World.otherPlayers[id].TakeDamage(damage, atkType);
            }
        }

        public static int PlayerDamage(Player play, int targetDef) {
            play.CalculateCombatLevel();
            int atk = play.Equipment[0].Stats != null ? play.Equipment[0].Stats.Accuracy : 30;
            int pow = play.Equipment[0].Stats != null ? play.Equipment[0].Stats.Power : 1; 

            int attack = play.Skills["Attack"].Level + atk;
            int power = play.Skills["Strength"].Level + pow;


            int damage = ((((((2 * play.CombatLevel) / 5) + 2) * power * (attack / targetDef)) / 50) + 2);

            bool crit = GameLoop.rand.Next(20) == 0 ? true : false;

            if (crit)
                damage *= 2;

            if (play.GetDamageType() == play.ElementalAlignment)
                damage = (int)Math.Ceiling((double)damage * 1.5f);

            if (play.GetDamageType() == "Water") {
                if (GameLoop.World.Player.Clock.Raining) {
                    damage = (int)Math.Ceiling((double)damage * 1.5f);
                } else {
                    damage = (int)Math.Ceiling((double)damage * 0.5f);
                }
            } else if (play.GetDamageType() == "Fire") {
                if (!GameLoop.World.Player.Clock.Raining) {
                    damage = (int)Math.Ceiling((double)damage * 1.5f);
                }
                else {
                    damage = (int)Math.Ceiling((double)damage * 0.5f);
                }
            }

            return damage;
        }

        // Player attacks a monster
        public static ColoredString Attack(Player attacker, MonsterWrapper defender) {
            string damageType = attacker.GetDamageType();
            int atk = attacker.Equipment[0].Stats != null ? attacker.Equipment[0].Stats.Accuracy : 30;
            int acc = (int) Math.Floor((attacker.Skills["Attack"].Level / 2f) + atk); 
            int attackRoll = GameLoop.rand.Next(100) + 1;

            ColoredString battleString;
            string battleColor = "White";

            if (attackRoll < acc) {
                int damage = PlayerDamage(attacker, defender.monster.Defense);

                if (damage < 0)
                    damage = 0;

                if (damage > 0) {
                    battleString = new ColoredString(attacker.Name + " dealt " + damage + " damage to " + defender.monster.Species); 
                    attacker.CombatExp(damage);
                } else {
                    battleString = new ColoredString(attacker.Name + " hit " + defender.monster.Species + " but dealt no damage!");
                }

                defender.TakeDamage(damage, damageType);
                 
                NetMsg damageMon = new("damageMonster");
                damageMon.MiscInt = damage; 
                damageMon.MiscInt2 = GameLoop.UIManager.Combat.Current.CombatID;
                damageMon.MiscString1 = defender.monster.UniqueID;
                damageMon.MiscString2 = battleString.String;
                damageMon.MiscString3 = battleColor;
                damageMon.MiscString4 = attacker.GetDamageType();
                GameLoop.SendMessageIfNeeded(damageMon, false, false);
                  
            } else {
                battleString = new ColoredString(attacker.Name + " attacked " + defender.monster.Species + " but missed!");
            }

            if (defender.CurrentHP <= 0) { 
                attacker.killList.Push(defender.monster.GetAppearance());



                if (GameLoop.World.Player.KillFeed().Contains("lol")) {
                    GameLoop.SteamManager.UnlockAchievement("FEED_FUNNY1");
                    if (!GameLoop.World.Player.feed_funny1) {
                        GameLoop.UIManager.AddMsg("Achievement: Funny Business (" + Helper.TimeSinceDayStart() + ")");
                        GameLoop.World.Player.feed_funny1 = true;
                    }
                }

                if (GameLoop.World.Player.KillFeed().Contains("lmao")) {
                    GameLoop.SteamManager.UnlockAchievement("FEED_FUNNY2");
                    if (!GameLoop.World.Player.feed_funny2) {
                        GameLoop.UIManager.AddMsg("Achievement: Funnier Business (" + Helper.TimeSinceDayStart() + ")");
                        GameLoop.World.Player.feed_funny2 = true;
                    }
                }

                if (GameLoop.World.Player.KillFeed().Contains("lmfao")) {
                    GameLoop.SteamManager.UnlockAchievement("FEED_FUNNY3");
                    if (!GameLoop.World.Player.feed_funny3) {
                        GameLoop.UIManager.AddMsg("Achievement: Funniest Business (" + Helper.TimeSinceDayStart() + ")");
                        GameLoop.World.Player.feed_funny3 = true;
                    }
                }

                if (GameLoop.World.Player.KillFeed().Contains("boobs")) {
                    GameLoop.SteamManager.UnlockAchievement("FEED_MATURITY");
                    if (!GameLoop.World.Player.feed_maturity) {
                        GameLoop.UIManager.AddMsg("Achievement: Maturity (" + Helper.TimeSinceDayStart() + ")");
                        GameLoop.World.Player.feed_maturity = true;
                    }
                }

                if (GameLoop.World.Player.KillFeed().Contains("abcdefghijklmnopqrstuvwxyz")) {
                    GameLoop.SteamManager.UnlockAchievement("FEED_METICULOUS1");
                    if (!GameLoop.World.Player.feed_meticulous1) {
                        GameLoop.UIManager.AddMsg("Achievement: Meticulous (" + Helper.TimeSinceDayStart() + ")");
                        GameLoop.World.Player.feed_meticulous1 = true;
                    }
                }

                if (GameLoop.World.Player.KillFeed().Contains("ABCDEFGHIJKLMNOPQRSTUVWXYZ")) {
                    GameLoop.SteamManager.UnlockAchievement("FEED_METICULOUS2");
                    if (!GameLoop.World.Player.feed_meticulous2) {
                        GameLoop.UIManager.AddMsg("Achievement: METICULOUS (" + Helper.TimeSinceDayStart() + ")");
                        GameLoop.World.Player.feed_meticulous2 = true;
                    }
                }

                if (GameLoop.World.Player.KillFeed().Contains("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")) {
                    GameLoop.SteamManager.UnlockAchievement("FEED_OCD");
                    if (!GameLoop.World.Player.feed_ocd) {
                        GameLoop.UIManager.AddMsg("Achievement: Obsessive and Compulsive (" + Helper.TimeSinceDayStart() + ")");
                        GameLoop.World.Player.feed_ocd = true;
                    }
                }

                MissionManager.Increment("Kill", defender.monster.Species, 1); 
            }

            return battleString;
        }
    }
}
