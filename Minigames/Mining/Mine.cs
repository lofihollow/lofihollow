using LofiHollow.Entities;
using ProtoBuf;
using SadRogue.Primitives;
using System.Collections.Generic;

namespace LofiHollow.Minigames.Mining {
    [ProtoContract]
    public class Mine {
        [ProtoMember(1)]
        public string Location = "";
        [ProtoMember(2)]
        public Dictionary<int, MineLevel> Levels = new();

        public bool SyncedFromHost = false;

        public Mine(string loc) {
            Location = loc;

            MineLevel zero = new(0, loc);
            Levels.Add(0, zero);
        }

        public void DropItem(Player actor, int slot, int depth) {
            if (actor.Inventory.Length > slot && actor.Inventory[slot].Name != "(EMPTY)") {
                ItemWrapper wrap = new(actor.Inventory[slot]);
                wrap.Position = actor.Position / 12;


                actor.Inventory[slot] = new Item("lh:(EMPTY)");

                SendItem(wrap, depth);
                SpawnItem(wrap, depth);
            }
        }

        public void SendItem(ItemWrapper item, int depth) {
            item.MineDepth = depth;
            NetMsg msg = new("mineItem", item.ToByteArray());
            GameLoop.SendMessageIfNeeded(msg, false, false);
        }

        public void PickupItem(Player player) {
            ItemWrapper wrap = Levels[player.MineDepth].GetEntityAt<ItemWrapper>(player.Position / 12);
            if (wrap != null) {
                for (int i = 0; i < player.Inventory.Length; i++) {
                    if (player.Inventory[i].StacksWith(wrap.item)) {
                        player.Inventory[i].ItemQuantity++;

                        DestroyItem(wrap, player.MineDepth);
                        SendPickup(wrap, player.MineDepth);

                        return;
                    }
                }

                for (int i = 0; i < player.Inventory.Length; i++) {
                    if (player.Inventory[i].Name == "(EMPTY)") {
                        player.Inventory[i] = wrap.item;
                        DestroyItem(wrap, player.MineDepth);
                        SendPickup(wrap, player.MineDepth);
                        break;
                    }
                }
            }
        }

        public void AddItemToInv(Player player, Item item) {
            if (item != null) {
                for (int i = 0; i < player.Inventory.Length; i++) {
                    if (player.Inventory[i].StacksWith(item)) {
                        player.Inventory[i].ItemQuantity += item.ItemQuantity;
                        return;
                    }
                }

                for (int i = 0; i < player.Inventory.Length; i++) {
                    if (player.Inventory[i].Name == "(EMPTY)") {
                        player.Inventory[i] = item;
                        return;
                    }
                }
            }

            ItemWrapper wrap = new(item);
            wrap.Position = player.Position / 12;
            SpawnItem(wrap, player.MineDepth);
        }

        public void SpawnItem(ItemWrapper item, int Depth) {
            if (Levels.ContainsKey(Depth)) {
                Levels[Depth].Add(item);

                if (Depth == GameLoop.World.Player.player.MineDepth) {
                    GameLoop.UIManager.Minigames.MineManager.MiningEntities.Add(item);
                }
            }
        }

        public void SendPickup(ItemWrapper item, int Depth) {
            item.MineDepth = Depth;
            NetMsg msg = new("mineDestroy", item.ToByteArray());
            GameLoop.SendMessageIfNeeded(msg, false, false);
        }

        public void DestroyItem(ItemWrapper wrap, int Depth) {
            if (Levels.ContainsKey(Depth)) {
                ItemWrapper localCopy = Levels[Depth].GetEntityAt<ItemWrapper>(wrap.Position, wrap.item.Name);
                if (localCopy != null) {
                    Levels[Depth].Remove(localCopy);
                    GameLoop.UIManager.Minigames.MineManager.MiningEntities.Remove(localCopy);
                }
            }
        }



        public bool MovePlayerTo(Player player, int depth, Point newPos, bool refreshMap) {
            if (!Levels.ContainsKey(depth)) {
                MineLevel newLevel = new(depth, Location);
                Levels.Add(depth, newLevel);
            }

            player.Position = newPos;
            player.MineDepth = depth;

            GameLoop.UIManager.Minigames.MinigameWindow.Title = Location + " Mine - Depth: " + (depth * -50);
            GameLoop.UIManager.Minigames.MinigameWindow.TitleAlignment = SadConsole.HorizontalAlignment.Center;


            if (refreshMap)
                if (player == GameLoop.World.Player.player)
                    GameLoop.UIManager.Minigames.MineManager.MiningFOV = new GoRogue.FOV(Levels[player.MineDepth].MapFOV);


            return true;
        }

        public bool BreakTileAt(Player player, int depth, Point breakPos) {
            if (breakPos.X < 0 || breakPos.X > 70)
                return false;
            if (breakPos.Y < 0 || breakPos.Y > 40)
                return false;
            int newDist = 10;
            if (Levels[depth].MapPath.ShortestPath(new GoRogue.Coord(player.Position.X / 12, player.Position.Y / 12), new GoRogue.Coord(breakPos.X, breakPos.Y)) != null)
                newDist = Levels[depth].MapPath.ShortestPath(new GoRogue.Coord(player.Position.X / 12, player.Position.Y / 12), new GoRogue.Coord(breakPos.X, breakPos.Y)).Length;

            if (newDist < 6) {
                if (Levels[depth].GetTile(breakPos).Name != "Air") {
                    int ToolTier = player.GetToolTier("Pickaxe");
                    if (ToolTier >= Levels[player.MineDepth].GetTile(breakPos).RequiredTier) {
                        Levels[player.MineDepth].GetTile(breakPos).Damage(ToolTier);
                        if (Levels[player.MineDepth].GetTile(breakPos).TileHP <= 0) {
                            if (Levels[player.MineDepth].GetTile(breakPos).OutputID != "") {
                                Item item = new(Levels[player.MineDepth].GetTile(breakPos).OutputID);
                                AddItemToInv(player, item);
                                player.Skills["Mining"].GrantExp(Levels[player.MineDepth].GetTile(breakPos).GrantedExp);
                                Levels[player.MineDepth].TileToAir(breakPos);
                            }
                            GameLoop.SoundManager.PlaySound("mineBreak");
                        } else {
                            GameLoop.SoundManager.PlaySound("mineHit");
                        }

                        NetMsg msg = new("updateMine", Levels[player.MineDepth].GetTile(breakPos).ToByteArray());
                        msg.X = breakPos.X;
                        msg.Y = breakPos.Y;
                        msg.MiscString = Location;
                        msg.MiscInt = player.MineDepth;

                        GameLoop.SendMessageIfNeeded(msg, false, false);

                        return true;
                    } else {
                        return false;
                    }
                }
            }

            return false;
        }

        public bool MovePlayerBy(Player player, int depth, Point newPos) {
            Point newPosition = player.Position + newPos;

            if (newPosition.X / 12 < 0 || newPosition.X / 12 >= 70)
                return false;

            if (newPosition.Y < 0 && depth > 0) {
                // int newDepth = depth - 1;
                // MovePlayerTo(player, newDepth, newPosition.WithY(39 * 12), true);
                // return true;
                return false;
            }

            if (newPosition.Y / 12 > 39) {
                //   int newDepth = depth + 1;
                //    MovePlayerTo(player, newDepth, newPosition.WithY(0), true);
                //   return true;
                return false;
            }



            if (Levels.ContainsKey(player.MineDepth)) {
                if (Levels[player.MineDepth].GetTile(new Point((newPosition.X + 6) / 12, (newPosition.Y + 12) / 12)).Name == "Air") {
                    player.Position = newPosition;
                    return true;
                } else {
                    return false;
                }
            }

            return false;
        }
    }
}
