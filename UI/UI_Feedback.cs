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
    public class UI_Feedback : Lofi_UI { 
        public Feedback current;

        public UI_Feedback(int width, int height, string title) : base(width, height, title, "Feedback") { }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            if (current == null) {
                current = new();
            }

            Con.Clear();

            Con.Print(0, 1, "(1 is unhappy, 5 is happy)");
            Con.Print(0, 0, "Mood:");
            Con.PrintClickable(7, 0, new ColoredString("1", current.Mood == 1 ? Color.Red : Color.White, Color.Black), UI_Clicks, "Mood1");
            Con.PrintClickable(9, 0, new ColoredString("2", current.Mood == 2 ? Color.Orange : Color.White, Color.Black), UI_Clicks, "Mood2");
            Con.PrintClickable(11, 0, new ColoredString("3", current.Mood == 3 ? Color.Yellow : Color.White, Color.Black), UI_Clicks, "Mood3");
            Con.PrintClickable(13, 0, new ColoredString("4", current.Mood == 4 ? Color.Lime : Color.White, Color.Black), UI_Clicks, "Mood4");
            Con.PrintClickable(15, 0, new ColoredString("5", current.Mood == 5 ? Color.Cyan : Color.White, Color.Black), UI_Clicks, "Mood5");

            Con.Print(0, 3, "Type to enter your feedback below: ");
            Con.Print(0, 5, current.Message);

            Con.PrintClickable(69, 0, "X", UI_Clicks, "Close");

            Con.PrintClickable(64, 39, "SUBMIT", UI_Clicks, "Submit");
        }

        public override void UI_Clicks(string ID) {
            if (ID == "Mood1") { current.Mood = 1; }
            if (ID == "Mood2") { current.Mood = 2; }
            if (ID == "Mood3") { current.Mood = 3; }
            if (ID == "Mood4") { current.Mood = 4; }
            if (ID == "Mood5") { current.Mood = 5; }

            if (ID == "Submit") {
                if (current.Message.Length > 0) {
                    GameLoop.FirebaseManager.Push(current); 
                    Toggle();
                    current = null;
                }
            }

            if (ID == "Close") {
                Toggle();
                current = null;
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape) || GameHost.Instance.Keyboard.IsKeyReleased(Key.F8)) {
                Toggle();
                current = null;
            }

            if (current != null) { 
                foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                    if (key.Character >= 'A' && key.Character <= 'z' || (key.Character >= '0' && key.Character <= '9'
                        || key.Character == ';' || key.Character == ':' || key.Character == '|' || key.Character == '.' || key.Character == ','
                        || key.Character == '?' || key.Character == '!' || key.Character == '`')) {
                        current.Message += key.Character;
                    }
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    current.Message += " ";
                }

                if (current.Message.Length > 0) {
                    if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Back)) {
                        current.Message = current.Message[0..^1];
                    }
                }
            }
        }  
    }
}
