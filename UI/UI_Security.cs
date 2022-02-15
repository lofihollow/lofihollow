using LofiHollow.Entities;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using LofiHollow.EntityData;
using System.Linq;
using Steamworks;

namespace LofiHollow.UI {
    public class UI_Security {
        public SadConsole.Console SecurityConsole;
        public Window SecurityWindow;
        public Apartment Current;

        public List<TeleportTarget> Targets = new();

        public UI_Security(int width, int height, string title) {
            SecurityWindow = new(width, height);
            SecurityWindow.CanDrag = true;
            SecurityWindow.Position = new(11, 6);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            SecurityConsole = new(invConWidth, invConHeight);
            SecurityConsole.Position = new(1, 1);
            SecurityWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            SecurityWindow.Children.Add(SecurityConsole);
            GameLoop.UIManager.Children.Add(SecurityWindow);

            SecurityWindow.Show();
            SecurityWindow.IsVisible = false;
        }


        public void RenderSecurity() {
            Point mousePos = new MouseScreenObjectState(SecurityConsole, GameHost.Instance.Mouse).CellPosition;

            SecurityConsole.Clear();

            if (Current != null) {
                ColoredString whitelist = new ColoredString(4.AsString(), Color.Lime, Color.Black); 
                if (!Current.Whitelist)
                    whitelist = new ColoredString("x", Color.Red, Color.Black);
                SecurityConsole.Print(1, 0, "Whitelist: " + whitelist);

                ColoredString guestsout = new ColoredString(4.AsString(), Color.Lime, Color.Black);
                if (!Current.GuestsAllowedWhileOut)
                    guestsout = new ColoredString("x", Color.Red, Color.Black);
                SecurityConsole.Print(20, 0, "Guests (while out): " + guestsout);

                ColoredString guestsin = new ColoredString(4.AsString(), Color.Lime, Color.Black);
                if (!Current.GuestsAllowedWhileIn)
                    guestsin = new ColoredString("x", Color.Red, Color.Black);
                SecurityConsole.Print(45, 0, "Guests (while home): " + guestsin);

                int y = 4;

                SecurityConsole.Print(1, 2, "Connected Players");
                foreach(KeyValuePair<CSteamID, Player> kv in GameLoop.World.otherPlayers) {
                    if (!Current.AllowedGuests.Contains(kv.Key)) {
                        SecurityConsole.Print(1, y, new ColoredString(kv.Value.Name.Align(HorizontalAlignment.Left, 20)) + Helper.HoverColoredString(12.AsString(), mousePos == new Point(21, y)));
                        y++;
                    }
                }

                SecurityConsole.DrawLine(new Point(0, 3), new Point(79, 3), 196, Color.White);
                SecurityConsole.DrawLine(new Point(22, 2), new Point(22, 39), 179, Color.White);

                SecurityConsole.Print(24, 2, "Guest List");
                for (int i = 0; i < Current.AllowedGuests.Count; i++) {
                    string name = Helper.ResolveName(Current.AllowedGuests[i]);
                    SecurityConsole.Print(23, 4 + i, 11.AsString(), mousePos == new Point(23, 4+i) ? Color.Yellow : Color.White);
                    SecurityConsole.Print(25, 4 + i, name.Align(HorizontalAlignment.Left, 40));
                }
            }
        }

        public void SecurityInput() {
            Point mousePos = new MouseScreenObjectState(SecurityConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle("None");
            }


            if (Current != null) {
                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (mousePos.Y == 0) {
                        if (mousePos.X < 20)
                            Current.Whitelist = !Current.Whitelist;
                        else if (mousePos.X < 45)
                            Current.GuestsAllowedWhileOut = !Current.GuestsAllowedWhileOut;
                        else
                            Current.GuestsAllowedWhileIn = !Current.GuestsAllowedWhileIn;

                        NetMsg aptUpdate = new("apartment", Current.ToByteArray()); 
                        aptUpdate.MiscString1 = "Noonbreeze";
                        GameLoop.SendMessageIfNeeded(aptUpdate, false, true);
                    }

                    if (mousePos.Y - 4 >= 0 && mousePos.X < 22) {
                        int slot = mousePos.Y - 4;

                        if (slot < GameLoop.World.otherPlayers.Count) {
                            Current.AddGuest(GameLoop.World.otherPlayers.ElementAt(slot).Key);

                            NetMsg aptUpdate = new("apartment", Current.ToByteArray()); 
                            aptUpdate.MiscString1 = "Noonbreeze";
                            GameLoop.SendMessageIfNeeded(aptUpdate, false, true);
                        }
                    }

                    if (mousePos.Y - 4 >= 0 && mousePos.X > 22) {
                        int slot = mousePos.Y - 4;

                        if (slot < Current.AllowedGuests.Count) {
                            Current.RemoveGuest(Current.AllowedGuests[slot]);
                            NetMsg aptUpdate = new("apartment", Current.ToByteArray());
                            aptUpdate.MiscString1 = "Noonbreeze";
                            GameLoop.SendMessageIfNeeded(aptUpdate, false, true);
                        }
                    }
                }
            }
        }

        public void Toggle(string which) {
            if (SecurityWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                SecurityWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
            } else {
                GameLoop.UIManager.selectedMenu = "Security";

                if (which == "Noonbreeze")
                    Current = GameLoop.World.Player.NoonbreezeApt;

                SecurityWindow.IsVisible = true;
                SecurityWindow.IsFocused = true;
            }
        }
    }
}
