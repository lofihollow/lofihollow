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
    public class UI_Security : Lofi_UI { 
        public Apartment Current; 
        public List<TeleportTarget> Targets = new();

        public UI_Security(int width, int height, string title) : base(width, height, title, "Security") { }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            if (Current != null) {
                ColoredString whitelist = new ColoredString(4.AsString(), Color.Lime, Color.Black); 
                if (!Current.Whitelist)
                    whitelist = new ColoredString("x", Color.Red, Color.Black);
                Con.Print(1, 0, "Whitelist: " + whitelist);

                ColoredString guestsout = new ColoredString(4.AsString(), Color.Lime, Color.Black);
                if (!Current.GuestsAllowedWhileOut)
                    guestsout = new ColoredString("x", Color.Red, Color.Black);
                Con.Print(20, 0, "Guests (while out): " + guestsout);

                ColoredString guestsin = new ColoredString(4.AsString(), Color.Lime, Color.Black);
                if (!Current.GuestsAllowedWhileIn)
                    guestsin = new ColoredString("x", Color.Red, Color.Black);
                Con.Print(45, 0, "Guests (while home): " + guestsin);

                int y = 4;

                Con.Print(1, 2, "Connected Players");
                foreach(KeyValuePair<CSteamID, Player> kv in GameLoop.World.otherPlayers) {
                    if (!Current.AllowedGuests.Contains(kv.Key)) {
                        Con.Print(1, y, new ColoredString(kv.Value.Name.Align(HorizontalAlignment.Left, 20)) + Helper.HoverColoredString(12.AsString(), mousePos == new Point(21, y)));
                        y++;
                    }
                }

                Con.DrawLine(new Point(0, 3), new Point(79, 3), 196, Color.White);
                Con.DrawLine(new Point(22, 2), new Point(22, 39), 179, Color.White);

                Con.Print(24, 2, "Guest List");
                for (int i = 0; i < Current.AllowedGuests.Count; i++) {
                    string name = Helper.ResolveName(Current.AllowedGuests[i]);
                    Con.Print(23, 4 + i, 11.AsString(), mousePos == new Point(23, 4+i) ? Color.Yellow : Color.White);
                    Con.Print(25, 4 + i, name.Align(HorizontalAlignment.Left, 40));
                }
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
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
            if (Win.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                Win.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
            } else {
                GameLoop.UIManager.selectedMenu = "Security";

                if (which == "Noonbreeze")
                    Current = GameLoop.World.Player.NoonbreezeApt;

                Win.IsVisible = true;
                Win.IsFocused = true;
            }
        }
    }
}
