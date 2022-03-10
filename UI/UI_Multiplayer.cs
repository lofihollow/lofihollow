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
    public class UI_Multiplayer : Lofi_UI {
        public bool ShowingCode = false;

        public UI_Multiplayer(int width, int height, string title) : base(width, height, title, "Multiplayer") { }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            if (!ShowingCode) {
                Con.PrintClickable(0, 2, "[SHOW CODE]", UI_Clicks, "Show");
            }
            else {
                Con.Print(0, 0, GameLoop.NetworkManager.LobbyCode);
                Con.PrintClickable(0, 2, "[HIDE CODE]", UI_Clicks, "Hide");
            }

            if (GameLoop.NetworkManager == null)
                Con.PrintClickable(0, 4, "[HOST]", UI_Clicks, "Host");
        }

        public override void UI_Clicks(string ID) {
            if (ID == "Show") {
                ShowingCode = true;
            }

            else if (ID == "Hide") {
                ShowingCode = false;
            }

            else if (ID == "Host") {
                GameLoop.NetworkManager = new();
                GameLoop.NetworkManager.CreateSteamLobby();
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.F1) || GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            }
        }
    }
}
