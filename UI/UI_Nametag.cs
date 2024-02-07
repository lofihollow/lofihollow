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
    public class UI_Nametag : InstantUI {
        public Item current;

        public UI_Nametag(int width, int height, string title) : base(width, height, title, "Nametag") { }


        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            if (current != null) {
                Con.Print(0, 0, "Type to rename: ");
                Con.Print(0, 1, current.Name);
            }
        }

        public override void UI_Clicks(string ID) {
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                GameLoop.UIManager.ToggleUI("Nametag");
                current = null;
            }

            if (current != null) {
                foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                    if (key.Character >= 'A' && key.Character <= 'z' || (key.Character >= '0' && key.Character <= '9'
                        || key.Character == ';' || key.Character == ':' || key.Character == '|' || key.Character == '.' || key.Character == ','
                        || key.Character == '?' || key.Character == '!' || key.Character == '`')) {
                        current.Name += key.Character;
                    }
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    current.Name += " ";
                }

                if (current.Name.Length > 0) {
                    if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Back)) {
                        current.Name = current.Name[0..^1];
                    }
                }
            }
        }
    }
}
