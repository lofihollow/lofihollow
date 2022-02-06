using LofiHollow.Entities;
using LofiHollow.Minigames.Mining;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;
using System.Linq;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Managers {
    public class MineManager {
        public Mine MountainMine;
        public Mine LakeMine;
        public bool MineJumpUp = false;
        public bool MineJumpApex = false;
        public int MineJump = 0;
        public bool Flying = false;
        public GoRogue.FOV MiningFOV;
        public SadConsole.Entities.Renderer MiningEntities;

        public void Input() {
            if (GameLoop.World.Player.player.MineLocation == "Mountain") {
                MiningInput(MountainMine);
            } else if (GameLoop.World.Player.player.MineLocation == "Lake") {
                MiningInput(LakeMine);
            }
        }

        public void Render() {
            if (GameLoop.World.Player.player.MineLocation == "Mountain") {
                MiningDraw(MountainMine);
            } else if (GameLoop.World.Player.player.MineLocation == "Lake") {
                MiningDraw(LakeMine);
            }
        }

        public void MiningDraw(Mine CurrentMine) {
            MiningCheckFall(GameLoop.World.Player, CurrentMine);
            UpdateMiningVision(CurrentMine);

            for (int x = 0; x < 70; x++) {
                for (int y = 0; y < 40; y++) {
                    if (CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].GetTile(new Point(x, y)).Visible) {
                        GameLoop.UIManager.Minigames.MinigameConsole.Print(x, y, CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].GetTile(new Point(x, y)).GetAppearance());
                        if (CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].GetTile(new Point(x, y)).Dec != null) {
                            GameLoop.UIManager.Minigames.MinigameConsole.SetDecorator(x, y, 1, CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].GetTile(new Point(x, y)).Decorator());
                        } else {
                            GameLoop.UIManager.Minigames.MinigameConsole.ClearDecorators(x, y, 1);
                        }
                    }
                }
            }
        }

        public void SyncMiningEntities(Mine CurrentMine) {
            if (CurrentMine != null) {
                GameLoop.UIManager.Minigames.MinigameConsole.ForceRendererRefresh = true;
                MiningEntities.RemoveAll();

                MiningEntities.Add(GameLoop.World.Player);

                foreach (Entity entity in CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].Entities.Items) {
                    MiningEntities.Add(entity);
                }

                foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
                    if (kv.Value.player.MineLocation == GameLoop.World.Player.player.MineLocation && kv.Value.player.MineDepth == GameLoop.World.Player.player.MineDepth) {
                        MiningEntities.Add(kv.Value);
                    }
                }

                MiningFOV = new GoRogue.FOV(CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].MapFOV);

                UpdateMiningVision(CurrentMine);
            }
        }

        public void UpdateMiningVision(Mine CurrentMine) {
            if (GameLoop.World.Player.player.Position.X >= 0 && GameLoop.World.Player.player.Position.Y >= 0 && GameLoop.World.Player.player.Position.X <= GameLoop.MapWidth * 12 && GameLoop.World.Player.player.Position.Y <= GameLoop.MapHeight * 12) {
                MiningFOV.Calculate(GameLoop.World.Player.player.Position.X / 12, GameLoop.World.Player.player.Position.Y / 12, GameLoop.World.Player.player.Vision);
                foreach (var position in MiningFOV.NewlyUnseen) {
                    CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].GetTile(new Point(position.X, position.Y)).Shade();
                }

                foreach (var position in MiningFOV.CurrentFOV) {
                    CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].GetTile(new Point(position.X, position.Y)).Visible = true;
                    CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].GetTile(new Point(position.X, position.Y)).Unshade();
                }

                foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
                    if (kv.Value.player.MineLocation == GameLoop.World.Player.player.MineLocation && kv.Value.player.MineDepth == GameLoop.World.Player.player.MineDepth) {
                        if (MiningFOV.CurrentFOV.Contains(new GoRogue.Coord(kv.Value.player.Position.X / 12, kv.Value.player.Position.Y / 12))) {
                            MiningEntities.Add(kv.Value);
                            kv.Value.UsePixelPositioning = true;
                        } else {
                            MiningEntities.Remove(kv.Value);
                        }
                    }
                }
            }
        }


        public void MiningInput(Mine CurrentMine) {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.MinigameConsole, GameHost.Instance.Mouse).CellPosition;

            if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                if (GameLoop.UIManager.Sidebar.hotbarSelect + 1 < GameLoop.World.Player.player.Inventory.Length)
                    GameLoop.UIManager.Sidebar.hotbarSelect++;
            } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                if (GameLoop.UIManager.Sidebar.hotbarSelect > 0)
                    GameLoop.UIManager.Sidebar.hotbarSelect--;
            }

            if (GameHost.Instance.Keyboard.IsKeyDown(Key.Space) && !MineJumpApex) {
                MineJumpUp = true;
            } else {
                MineJumpUp = false;
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Space)) {
                MineJumpApex = true;
                MineJump = 0;
            }

            if (GameHost.Instance.Mouse.LeftButtonDown) {
                int AgilityLevel = 0;

                if (GameLoop.World.Player.player.Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemCat == "Pickaxe") {
                    if (GameLoop.World.Player.player.TimeLastActed + (120 - (AgilityLevel)) < SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                        GameLoop.World.Player.player.TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;

                        CurrentMine.BreakTileAt(GameLoop.World.Player.player, GameLoop.World.Player.player.MineDepth, mousePos);
                    }
                } else if (GameLoop.World.Player.player.Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].Name == "Ladder") {
                    if (CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].GetTile(mousePos).Name == "Air") {
                        CurrentMine.Levels[GameLoop.World.Player.player.MineDepth].SetTile(mousePos, new MineTile("Ladder"));
                        GameLoop.World.Player.player.Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemQuantity -= 1;
                        if (GameLoop.World.Player.player.Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemQuantity <= 0)
                            GameLoop.World.Player.player.Inventory[GameLoop.UIManager.Sidebar.hotbarSelect] = new("lh:(EMPTY)");
                    }
                }
            }

            Point sidebarMouse = new MouseScreenObjectState(GameLoop.UIManager.Sidebar.SidebarConsole, GameHost.Instance.Mouse).CellPosition;
            if (sidebarMouse.X >= 0 && sidebarMouse.Y >= 0) {
                if (GameHost.Instance.Mouse.LeftClicked) {
                    int backpackSlot = sidebarMouse.Y - 37;

                    if (backpackSlot >= 0 && backpackSlot <= 8) {
                        CurrentMine.DropItem(GameLoop.World.Player.player, backpackSlot, GameLoop.World.Player.player.MineDepth);
                    }
                }
            }


            if (GameHost.Instance.Keyboard.IsKeyDown(Key.A)) {
                if (!CollisionLeft(GameLoop.World.Player, CurrentMine.Levels[GameLoop.World.Player.player.MineDepth])) {
                    bool moved = CurrentMine.MovePlayerTo(GameLoop.World.Player.player, GameLoop.World.Player.player.MineDepth, GameLoop.World.Player.player.Position + new Point(-2, 0), false);
                    if (moved) {
                        NetMsg movedMsg = new("updatePlayerMine", null);
                        movedMsg.SetPosition(GameLoop.World.Player.player.Position);
                        movedMsg.MiscString = GameLoop.World.Player.player.MineLocation;
                        movedMsg.MiscInt = GameLoop.World.Player.player.MineDepth;
                        GameLoop.SendMessageIfNeeded(movedMsg, false, true);
                    }
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyDown(Key.D)) {
                if (!CollisionRight(GameLoop.World.Player, CurrentMine.Levels[GameLoop.World.Player.player.MineDepth])) {
                    bool moved = CurrentMine.MovePlayerTo(GameLoop.World.Player.player, GameLoop.World.Player.player.MineDepth, GameLoop.World.Player.player.Position + new Point(2, 0), false);
                    if (moved) {
                        NetMsg movedMsg = new("updatePlayerMine", null);
                        movedMsg.SetPosition(GameLoop.World.Player.player.Position);
                        movedMsg.MiscString = GameLoop.World.Player.player.MineLocation;
                        movedMsg.MiscInt = GameLoop.World.Player.player.MineDepth;
                        GameLoop.SendMessageIfNeeded(movedMsg, false, true);
                    }
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyDown(Key.S)) {
                if (!CollisionBottom(GameLoop.World.Player, CurrentMine.Levels[GameLoop.World.Player.player.MineDepth]) && PlayerOnTile(GameLoop.World.Player, CurrentMine.Levels[GameLoop.World.Player.player.MineDepth], "Ladder")) {
                    bool moved = CurrentMine.MovePlayerTo(GameLoop.World.Player.player, GameLoop.World.Player.player.MineDepth, GameLoop.World.Player.player.Position + new Point(0, 2), false);
                    Flying = true;
                    if (moved) {
                        NetMsg movedMsg = new("updatePlayerMine", null);
                        movedMsg.SetPosition(GameLoop.World.Player.player.Position);
                        movedMsg.MiscString = GameLoop.World.Player.player.MineLocation;
                        movedMsg.MiscInt = GameLoop.World.Player.player.MineDepth;
                        GameLoop.SendMessageIfNeeded(movedMsg, false, true);
                    }
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyDown(Key.W)) {
                if (!CollisionTop(GameLoop.World.Player, CurrentMine.Levels[GameLoop.World.Player.player.MineDepth]) && PlayerOnTile(GameLoop.World.Player, CurrentMine.Levels[GameLoop.World.Player.player.MineDepth], "Ladder")) {
                    bool moved = CurrentMine.MovePlayerTo(GameLoop.World.Player.player, GameLoop.World.Player.player.MineDepth, GameLoop.World.Player.player.Position + new Point(0, -2), false);
                    Flying = true;
                    if (moved) {
                        NetMsg movedMsg = new("updatePlayerMine", null);
                        movedMsg.SetPosition(GameLoop.World.Player.player.Position);
                        movedMsg.MiscString = GameLoop.World.Player.player.MineLocation;
                        movedMsg.MiscInt = GameLoop.World.Player.player.MineDepth;
                        GameLoop.SendMessageIfNeeded(movedMsg, false, true);
                    }
                }
            }

            if (!GameHost.Instance.Keyboard.IsKeyDown(Key.W) && !GameHost.Instance.Keyboard.IsKeyDown(Key.S)) {
                if (!PlayerOnTile(GameLoop.World.Player, CurrentMine.Levels[GameLoop.World.Player.player.MineDepth], "Ladder"))
                    Flying = false;
                else
                    Flying = true;
            }


            if (GameHost.Instance.Keyboard.IsKeyPressed(Key.G)) {
                CurrentMine.PickupItem(GameLoop.World.Player.player);
            }

            if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift) && GameHost.Instance.Keyboard.IsKeyPressed(Key.OemComma)) {
                if (GameLoop.World.Player.player.Position / 12 == new Point(35, 4) || GameLoop.World.Player.player.Position / 12 == new Point(34, 4) || GameLoop.World.Player.player.Position / 12 == new Point(36, 4)) {
                    GameLoop.UIManager.selectedMenu = "None";
                    GameLoop.World.Player.UsePixelPositioning = false;
                    GameLoop.World.Player.player.Position = GameLoop.World.Player.player.MineEnteredAt;
                    GameLoop.UIManager.Minigames.ToggleMinigame("None");
                }
            }

            if (PlayerOnTile(GameLoop.World.Player, CurrentMine.Levels[GameLoop.World.Player.player.MineDepth], "Ladder")) {
                MineJumpApex = false;
            }
        }

        public bool CollisionTop(Entity ent, MineLevel level) {
            MineTile left = level.GetTile(new Point((ent.Position.X) / 12, (ent.Position.Y - 1) / 12));
            MineTile center = level.GetTile(new Point((ent.Position.X + 5) / 12, (ent.Position.Y - 1) / 12));
            MineTile right = level.GetTile(new Point((ent.Position.X + 11) / 12, (ent.Position.Y - 1) / 12));

            if (left != null && center != null && right != null) {
                if (!left.BlocksMove && !center.BlocksMove && !right.BlocksMove) {
                    return false;
                }
            }
            return true;
        }

        public bool CollisionBottom(Entity ent, MineLevel level) {
            MineTile left = level.GetTile(new Point((ent.Position.X) / 12, (ent.Position.Y + 12) / 12));
            MineTile center = level.GetTile(new Point((ent.Position.X + 5) / 12, (ent.Position.Y + 12) / 12));
            MineTile right = level.GetTile(new Point((ent.Position.X + 11) / 12, (ent.Position.Y + 12) / 12));

            if (left != null && center != null && right != null) {
                if (!left.BlocksMove && !center.BlocksMove && !right.BlocksMove) {
                    return false;
                }
            }
            return true;
        }

        public bool PlayerOnTile(Entity ent, MineLevel level, string Name) {
            MineTile left = level.GetTile(new Point((ent.Position.X) / 12, (ent.Position.Y + 5) / 12));
            MineTile center = level.GetTile(new Point((ent.Position.X + 5) / 12, (ent.Position.Y + 5) / 12));
            MineTile right = level.GetTile(new Point((ent.Position.X + 11) / 12, (ent.Position.Y + 5) / 12));

            if (left != null && center != null && right != null) {
                if (left.Name == Name || center.Name == Name || right.Name == Name)
                    return true;
                else
                    return false;
            }
            return true;
        }

        public bool CollisionLeft(Entity ent, MineLevel level) {
            MineTile topLeft = level.GetTile(new Point((ent.Position.X - 1) / 12, (ent.Position.Y + 11) / 12));
            MineTile centerLeft = level.GetTile(new Point((ent.Position.X - 1) / 12, (ent.Position.Y + 5) / 12));
            MineTile bottomLeft = level.GetTile(new Point((ent.Position.X - 1) / 12, (ent.Position.Y) / 12));
            if (topLeft != null && centerLeft != null && bottomLeft != null) {
                if (!topLeft.BlocksMove && !centerLeft.BlocksMove && !bottomLeft.BlocksMove) {
                    return false;
                }
            }
            return true;
        }

        public bool CollisionRight(Entity ent, MineLevel level) {
            MineTile topLeft = level.GetTile(new Point((ent.Position.X + 12) / 12, (ent.Position.Y + 11) / 12));
            MineTile centerLeft = level.GetTile(new Point((ent.Position.X + 12) / 12, (ent.Position.Y + 5) / 12));
            MineTile bottomLeft = level.GetTile(new Point((ent.Position.X + 12) / 12, (ent.Position.Y) / 12));
            if (topLeft != null && centerLeft != null && bottomLeft != null) {
                if (!topLeft.BlocksMove && !centerLeft.BlocksMove && !bottomLeft.BlocksMove) {
                    return false;
                }
            }
            return true;
        }

        public void MiningCheckFall(PlayerWrapper player, Mine CurrentMine) {
            Point oldPos = new Point(player.player.Position.X, player.player.Position.Y);
            if (player.player.Position.Y / 12 < 39) {
                if (!MineJumpUp) {
                    if (!CollisionBottom(player, CurrentMine.Levels[player.player.MineDepth]) && !Flying) {
                        CurrentMine.MovePlayerTo(player.player, player.player.MineDepth, player.player.Position + new Point(0, 2), false);
                        MineJumpApex = true;
                        MineJump++;
                    } else if (CollisionBottom(player, CurrentMine.Levels[player.player.MineDepth])) {
                        MineJumpApex = false;
                        if (MineJump > 0) {
                            float blocksFallen = (float)MineJump / 6f;
                            if (blocksFallen > 8) {
                                GameLoop.World.Player.TakeDamage((int)((blocksFallen - 8f) / 2f));
                            }
                        }

                        MineJump = 0;
                    }
                } else {
                    if (!CollisionTop(player, CurrentMine.Levels[player.player.MineDepth]) || PlayerOnTile(player, CurrentMine.Levels[player.player.MineDepth], "Ladder")) {
                        MineJump += 4;
                        CurrentMine.MovePlayerBy(player.player, player.player.MineDepth, new Point(0, -4));
                        if (MineJump > 40) {
                            MineJumpApex = true;
                            MineJumpUp = false;
                            MineJump = 0;
                        }
                    } else {
                        MineJumpApex = true;
                    }
                }
            }

            if (oldPos != GameLoop.World.Player.player.Position) {
                NetMsg playerMoved = new("updatePlayerMine", null);
                playerMoved.SetPosition(GameLoop.World.Player.player.Position);
                playerMoved.MiscString = GameLoop.World.Player.player.MineLocation;
                playerMoved.MiscInt = GameLoop.World.Player.player.MineDepth;
                GameLoop.SendMessageIfNeeded(playerMoved, false, true);
            }

            foreach (Entity ent in CurrentMine.Levels[player.player.MineDepth].Entities.Items) {
                if (ent is ItemWrapper item) {
                    if (item.Position.Y < 39) {
                        if (CurrentMine.Levels[player.player.MineDepth].GetTile(item.Position + new Point(0, 1)).Name == "Air") {
                            item.Position += new Point(0, 1);
                        }
                    } else {
                        if (CurrentMine.Levels.ContainsKey(player.player.MineDepth + 1)) {
                            if (CurrentMine.Levels[player.player.MineDepth + 1].GetTile(item.Position.WithY(0)).Name == "Air") {
                                item.Position = item.Position.WithY(0);
                                CurrentMine.Levels[player.player.MineDepth].Remove(item);
                                CurrentMine.Levels[player.player.MineDepth + 1].Add(item);
                            }
                        }
                    }
                }
            }
        }

        public void MiningSetup(string loc) {
            GameLoop.World.Player.player.Position = new Point(35 * 12, 4 * 12);
            GameLoop.World.Player.UsePixelPositioning = true;

            MiningEntities = new SadConsole.Entities.Renderer();
            GameLoop.UIManager.Minigames.MinigameConsole.SadComponents.Add(MiningEntities);
            if (loc == "Mountain") {
                if (MountainMine == null)
                    MountainMine = new("Mountain");

                GameLoop.World.Player.player.MineLocation = "Mountain";
                MiningFOV = new GoRogue.FOV(MountainMine.Levels[GameLoop.World.Player.player.MineDepth].MapFOV);
                SyncMiningEntities(MountainMine);

                if (!MountainMine.SyncedFromHost) {
                    NetMsg msg = new("requestMine", null);
                    msg.MiscString = "Mountain";
                    msg.MiscInt = 0;
                    GameLoop.SendMessageIfNeeded(msg, false, false);
                }
            }

            if (loc == "Lake") {
                if (LakeMine == null)
                    LakeMine = new("Lake");

                GameLoop.World.Player.player.MineLocation = "Lake";
                MiningFOV = new GoRogue.FOV(LakeMine.Levels[GameLoop.World.Player.player.MineDepth].MapFOV);
                SyncMiningEntities(LakeMine);

                if (!LakeMine.SyncedFromHost) {
                    NetMsg msg = new("requestMine", null);
                    msg.MiscString = "Lake";
                    msg.MiscInt = 0;
                    GameLoop.SendMessageIfNeeded(msg, false, false);
                }
            }

            GameLoop.UIManager.Minigames.MinigameWindow.Title = loc + " Mine - Depth: 0";
            GameLoop.UIManager.Minigames.MinigameWindow.TitleAlignment = SadConsole.HorizontalAlignment.Center;
        }
    }
}
