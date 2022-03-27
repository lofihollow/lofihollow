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
using LofiHollow.Minigames.Photo;

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
                    int playerSkill = item.EquipSlot == 0 ? actor.GetSkillLevel("Attack") : actor.GetSkillLevel("Defense");
                    if (item.ItemTier <= playerSkill) {
                        Item temp = actor.Equipment[item.EquipSlot];
                        actor.Equipment[item.EquipSlot] = item;
                        actor.Inventory[slot] = temp;
                    } 
                    else {
                        string name = item.EquipSlot == 0 ? "Attack" : "Defense";
                        if (GameLoop.UIManager.Combat.Win.IsVisible) {
                            GameLoop.UIManager.Combat.AddMsg(new("You need " + item.ItemTier + " " + name + " to equip that.", Color.Yellow, Color.Black));
                        } else {
                            GameLoop.UIManager.AddMsg(new ColoredString("You need " + item.ItemTier + " " + name + " to equip that.", Color.Yellow, Color.Black));
                        }
                    }
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



        public static int Damage(CombatParticipant attacker, Move move, CombatParticipant target) {
            int attack = move.Physical ? attacker.StatWithStage("Attack") : attacker.StatWithStage("Magic Attack");
            int power = move.Power;
            int def = target.StatWithStage("Defense");
            int mdef = target.StatWithStage("Magic Defense");

            int damage;
            if (move.Physical)
                damage = (int) (((((2f * attacker.Level / 5f) + 2) * power * attack / def) / 50f) + 2);
            else
                damage = (int) (((((2f * attacker.Level / 5f) + 2) * power * attack / mdef) / 50f) + 2);

            bool crit = GameLoop.rand.Next(20) == 0;

            if (crit)
                damage *= 2;

            if (attacker.Types.Contains(move.Type))
                damage = (int)Math.Ceiling((double)damage * 1.5f);

            if (attacker.Types.Contains("Water")) {
                if (GameLoop.World.Player.Clock.Raining) {
                    damage = (int)Math.Ceiling((double)damage * 1.5f);
                } else {
                    damage = (int)Math.Ceiling((double)damage * 0.5f);
                }
            } else if (attacker.Types.Contains("Fire")) {
                if (!GameLoop.World.Player.Clock.Raining) {
                    damage = (int)Math.Ceiling((double)damage * 1.5f);
                }
                else {
                    damage = (int)Math.Ceiling((double)damage * 0.5f);
                }
            }

            bool minOneDamage = damage > 0;

            for (int i = 0; i < target.Types.Count; i++) {
                if (GameLoop.World.typeLibrary.ContainsKey(target.Types[i])) {
                    TypeDef type = GameLoop.World.typeLibrary[target.Types[i]];
                    damage = type.ModDamage(damage, move.Type);
                }
            }

            int minDamage = (minOneDamage && damage > 0) ? 1 : 0;
              
            damage += move.Physical ? attacker.AtkStage : attacker.MAtkStage;
            damage -= move.Physical ? target.DefStage : target.MDefStage;

            int atkBonus = move.Physical ? attacker.AtkBonus : attacker.MAtkBonus;
            int defBonus = move.Physical ? target.DefBonus : target.MDefBonus;

            int damBoost = (int) Math.Min(6, atkBonus / 2f);
            int damReduc = (int) Math.Min(6, defBonus / 2f);

            damage += damBoost;
            damage -= damReduc;

            if (damage < minDamage)
                damage = minDamage;

            return damage;
        }

        public static float TypeMultiplier(CombatParticipant target, Move attackMove) {
            float mult = 1.0f;

            for (int i = 0; i < target.Types.Count; i++) {
                if (GameLoop.World.typeLibrary.ContainsKey(target.Types[i])) {
                    TypeDef type = GameLoop.World.typeLibrary[target.Types[i]];
                    if (type.WeakAgainst.Contains(attackMove.Type)) {
                        mult *= 2f;
                    }
                    else if (type.StrongAgainst.Contains(attackMove.Type)) {
                        mult *= 0.5f;
                    }
                    else if (type.ImmuneTo.Contains(attackMove.Type)) {
                        mult = 0;
                    }
                }
            }

            return mult;
        }

        public static TurnResult Attack(CombatParticipant attacker, CombatParticipant defender, Move attackMove) { 
            int acc = (int)Math.Floor((double)attackMove.Accuracy * attacker.GetStatusMultiplier("Accuracy"));
            int attackRoll = GameLoop.rand.Next(100) + 1;

            bool defFullHP = defender.CurrentHP == defender.MaxHP;

            ColoredString battleString;
            ColoredString ownStatus;
            ColoredString targetStatus;

            TurnResult turn = new();

            if (attackRoll < acc) {
                if (attackMove.Power > 0) {
                    int damage = Damage(attacker, attackMove, defender);

                    turn.Hit = true;

                    if (damage < 0)
                        damage = 0;

                    if (damage > 0) {
                        ColoredString multString = new ColoredString("");
                        float multiplier = TypeMultiplier(defender, attackMove);
                        if (multiplier > 2f)
                            multString = new ColoredString("eff++", Color.Lime, Color.Black);
                        else if (multiplier > 1f)
                            multString = new ColoredString("eff+", Color.Green, Color.Black);
                        else if (multiplier < 1f)
                            multString = new ColoredString("eff-", Color.Yellow, Color.Black);

                        if (multiplier != 1f)
                            battleString = new ColoredString(attacker.Name + " used " + attackMove.Name + " on " + defender.Name + ". (" + damage + ", ") + multString + new ColoredString(")", Color.White, Color.Black);
                        else
                            battleString = new ColoredString(attacker.Name + " used " + attackMove.Name + " on " + defender.Name + ". (" + damage + ")");


                        attacker.CombatExp(damage);
                    }
                    else {
                        battleString = new ColoredString(attacker.Name + " used " + attackMove.Name + " on " + defender.Name + ", but did no damage!");
                    }

                    defender.TakeDamage(damage);
                     
                    turn.Damage = battleString;
                }

                if (attackMove.OwnStat != "" && attackMove.OwnStatChange != 0) {
                    attacker.FlatChange(attackMove.OwnStat, attackMove.OwnStatChange);
                    int ownStatChange = attackMove.OwnStatChange;
                    if (turn.Damage == null) {
                        turn.Damage = new ColoredString(attacker.Name + " used " + attackMove.Name + ".");
                    }

                    if (ownStatChange == -1) { ownStatus = new ColoredString(attacker.Name + "'s " + attackMove.OwnStat + " fell 1!", Color.Crimson, Color.Black); }
                    else if (ownStatChange < -1) { ownStatus = new ColoredString(attacker.Name + "'s " + attackMove.OwnStat + " fell " + ownStatChange + "!", Color.Red, Color.Black); }
                    else if (ownStatChange == 1) { ownStatus = new ColoredString(attacker.Name + "'s " + attackMove.OwnStat + " rose 1!", Color.Green, Color.Black); }
                    else { ownStatus = new ColoredString(attacker.Name + "'s " + attackMove.OwnStat + " rose " + ownStatChange + "!", Color.LimeGreen, Color.Black); }

                    turn.OwnStatus = ownStatus;
                }

                if (attackMove.EnemyStat != "" && attackMove.EnemyStatChange != 0) {
                    defender.FlatChange(attackMove.EnemyStat, attackMove.EnemyStatChange);
                    int enemyStatChange = attackMove.EnemyStatChange;

                    if (turn.Damage == null) {
                        turn.Damage = new ColoredString(attacker.Name + " used " + attackMove.Name + ".");
                    }

                    if (enemyStatChange == -1) { targetStatus = new ColoredString(defender.Name + "'s " + attackMove.EnemyStat + " fell 1!", Color.Crimson, Color.Black); }
                    else if (enemyStatChange < -1) { targetStatus = new ColoredString(defender.Name + "'s " + attackMove.EnemyStat + " fell " + enemyStatChange + "!", Color.Red, Color.Black); }
                    else if (enemyStatChange == 1) { targetStatus = new ColoredString(defender.Name + "'s " + attackMove.EnemyStat + " rose 1!", Color.Green, Color.Black); }
                    else { targetStatus = new ColoredString(defender.Name + "'s " + attackMove.EnemyStat + " rose " + enemyStatChange + "!", Color.LimeGreen, Color.Black); }

                    turn.TargetStatus = targetStatus;
                }




                NetMsg damageMon = new("updateCombat");
                damageMon.MiscInt = GameLoop.UIManager.Combat.Current.CombatID;
                damageMon.data = GameLoop.UIManager.Combat.Current.ToByteArray();
                GameLoop.SendMessageIfNeeded(damageMon, false, false);
                  
            } else {
                battleString = new ColoredString(attacker.Name + " attacked " + defender.Name + " but missed!"); 
                turn.Damage = battleString;
            }

            if (defender.CurrentHP <= 0) {
                if (attacker.Owner == SteamClient.SteamId) {
                    if (attacker.ID == "Player") {
                        GameLoop.World.Player.killList.Push(defender.GetAppearance());

                        string[] split = defender.StatGranted.Split(",");
                        GameLoop.World.Player.GainStatEXP(split[0], Int32.Parse(split[1]));

                        if (GameLoop.World.Player.CurrentKillTask == defender.Species && GameLoop.World.Player.KillTaskMonLevel == defender.Level) {
                            GameLoop.World.Player.GrantExp("Bounty Hunting", Math.Max(5, defender.Level));
                            GameLoop.World.Player.KillTaskProgress += 1;
                            if (GameLoop.World.Player.KillTaskProgress >= GameLoop.World.Player.KillTaskGoal) {
                                GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.Player.CurrentKillTask + " task complete.", Color.Lime, Color.Black));
                            }
                            else {
                                GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.Player.CurrentKillTask + " task progress: " + GameLoop.World.Player.KillTaskProgress + "/" + GameLoop.World.Player.KillTaskGoal, Color.Yellow, Color.Black));
                            }
                        }
                    }
                    else {
                        if (GameLoop.World.Player.Equipment[10].SoulPhoto != null) {
                            SoulPhoto soul = GameLoop.World.Player.Equipment[10].SoulPhoto;
                            string[] split = defender.StatGranted.Split(",");

                            soul.Contained1.GainStatEXP(split[0], Int32.Parse(split[1]));
                            soul.Contained2.GainStatEXP(split[0], Int32.Parse(split[1]));
                        }
                    }

                    if (defFullHP)
                        GameLoop.SteamManager.UnlockAchievement("ONE_HIT_WONDER");
                } else {
                    if (defFullHP)
                        GameLoop.SteamManager.UnlockAchievement("ONE_HIT_BLUNDER");
                }



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

                MissionManager.Increment("Kill", defender.Species, 1); 
            }




            return turn;
        }
    }
}
