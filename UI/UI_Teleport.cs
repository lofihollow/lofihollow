using LofiHollow.Entities; 
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using Steamworks;

namespace LofiHollow.UI {
    public class UI_Teleport {
        public SadConsole.Console TeleportConsole;
        public Window TeleportWindow;

        public List<TeleportTarget> Targets = new();

        public UI_Teleport(int width, int height, string title) {
            TeleportWindow = new(width, height);
            TeleportWindow.CanDrag = false;
            TeleportWindow.Position = new(11, 6);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            TeleportConsole = new(invConWidth, invConHeight);
            TeleportConsole.Position = new(1, 1);
            TeleportWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            TeleportWindow.Children.Add(TeleportConsole);
            GameLoop.UIManager.Children.Add(TeleportWindow);

            TeleportWindow.Show();
            TeleportWindow.IsVisible = false;
        }


        public void RenderTeleports() {
            Point mousePos = new MouseScreenObjectState(TeleportConsole, GameHost.Instance.Mouse).CellPosition;

            TeleportConsole.Clear();

            TeleportConsole.Print(0, 0, "Location".Align(HorizontalAlignment.Center, 40) + "|");
            TeleportConsole.DrawLine(new Point(0, 1), new Point(69, 1), 196, Color.White);
            for (int i = 0; i < Targets.Count; i++) {
                TeleportConsole.Print(0, 2 + (i * 2), new ColoredString(Targets[i].Name.Align(HorizontalAlignment.Center, 40) + "|", Color.White, Color.Black) + new ColoredString(" [GO]", Targets[i].CanGo ? Color.Lime : Color.Red, Color.Black));
            }
        }

        public void TeleportInput() {
            Point mousePos = new MouseScreenObjectState(TeleportConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle("None");
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos.Y - 2 >= 0) {
                    if ((mousePos.Y - 2f) / 2f == (mousePos.Y - 2) / 2) {
                        int slot = (mousePos.Y - 2) / 2;

                        if (Targets.Count > slot) {
                            Map map = Helper.ResolveMap(Targets[slot].MapPos);

                            if (map != null && Targets[slot].CanGo) {
                                GameLoop.World.Player.FlexibleMapMove(Targets[slot].Pos * 12, Targets[slot].MapPos, map);
                                Toggle("None");
                            }
                        }
                    }
                }
            }
        }

        public void InitializeTeleports(string where) {
            Targets.Clear();

            if (where == "Noonbreeze Apartments") {
                int enterX = 30;
                int enterY = 20;

                if (GameLoop.World.Player.NoonbreezeApt != null) {
                    TeleportTarget ownApt = new();
                    ownApt.Name = GameLoop.World.Player.NoonbreezeApt.map.MinimapTile.name;
                    ownApt.MapPos = new(ownApt.Name, 0, 0, 0);
                    ownApt.Pos = new(enterX, enterY);
                    ownApt.CanGo = GameLoop.World.Player.NoonbreezeApt.DaysLeft >= 0;
                    Targets.Add(ownApt);
                }

                foreach (KeyValuePair<CSteamID, Player> kv in GameLoop.World.otherPlayers) {
                    if (kv.Value.NoonbreezeApt != null) {
                        TeleportTarget theirApt = new();
                        theirApt.Name = kv.Value.NoonbreezeApt.map.MinimapTile.name;
                        theirApt.MapPos = new(theirApt.Name, 0, 0, 0);
                        theirApt.Pos = new(enterX, enterY);
                        theirApt.CanGo = kv.Value.NoonbreezeApt.CanEnter(GameLoop.NetworkManager.ownID, kv.Value.MapPos.WorldArea == kv.Value.Name + " Apartment");
                        GameLoop.UIManager.AddMsg(kv.Value.NoonbreezeApt.GuestsAllowedWhileIn + "," + kv.Value.NoonbreezeApt.GuestsAllowedWhileOut + theirApt.CanGo);
                        Targets.Add(theirApt);
                    }
                }
            }
        }


        public void Toggle(string where) {
            if (TeleportWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                TeleportWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
            } else {
                GameLoop.UIManager.selectedMenu = "Teleport";
                InitializeTeleports(where);
                TeleportWindow.IsVisible = true;
                TeleportWindow.IsFocused = true;
            }
        }
    }
}
