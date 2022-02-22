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
    public class UI_Feedback {
        public SadConsole.Console FeedbackConsole;
        public Window FeedbackWindow;

        public Feedback current;

        public UI_Feedback(int width, int height, string title) {
            FeedbackWindow = new(width, height);
            FeedbackWindow.CanDrag = false;
            FeedbackWindow.Position = new(11, 6);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            FeedbackConsole = new(invConWidth, invConHeight);
            FeedbackConsole.Position = new(1, 1);
            FeedbackWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            FeedbackWindow.Children.Add(FeedbackConsole);
            GameLoop.UIManager.Children.Add(FeedbackWindow);

            FeedbackWindow.Show();
            FeedbackWindow.IsVisible = false;
        }


        public void RenderFeedback() {
            Point mousePos = new MouseScreenObjectState(FeedbackConsole, GameHost.Instance.Mouse).CellPosition;

            FeedbackConsole.Clear();

            FeedbackConsole.Print(0, 1, "(1 is unhappy, 5 is happy)");
            FeedbackConsole.Print(0, 0, "Mood:");
            FeedbackConsole.PrintClickable(7, 0, new ColoredString("1", current.Mood == 1 ? Color.Red : Color.White, Color.Black), FeedbackClick, "Mood1");
            FeedbackConsole.PrintClickable(9, 0, new ColoredString("2", current.Mood == 2 ? Color.Orange : Color.White, Color.Black), FeedbackClick, "Mood2");
            FeedbackConsole.PrintClickable(11, 0, new ColoredString("3", current.Mood == 3 ? Color.Yellow : Color.White, Color.Black), FeedbackClick, "Mood3");
            FeedbackConsole.PrintClickable(13, 0, new ColoredString("4", current.Mood == 4 ? Color.Lime : Color.White, Color.Black), FeedbackClick, "Mood4");
            FeedbackConsole.PrintClickable(15, 0, new ColoredString("5", current.Mood == 5 ? Color.Cyan : Color.White, Color.Black), FeedbackClick, "Mood5");

            FeedbackConsole.Print(0, 3, "Type to enter your feedback below: ");
            FeedbackConsole.Print(0, 5, current.Message);

            FeedbackConsole.PrintClickable(69, 0, "X", FeedbackClick, "Close");

            FeedbackConsole.PrintClickable(64, 39, "SUBMIT", FeedbackClick, "Submit");
        }

        public void FeedbackClick(string ID) {
            if (ID == "Mood1") { current.Mood = 1; }
            if (ID == "Mood2") { current.Mood = 2; }
            if (ID == "Mood3") { current.Mood = 3; }
            if (ID == "Mood4") { current.Mood = 4; }
            if (ID == "Mood5") { current.Mood = 5; }

            if (ID == "Submit") {
                if (current.Message.Length > 0) {
                    GameLoop.FirebaseManager.Push(current);
                    Toggle();
                }
            }

            if (ID == "Close") {
                Toggle();
            }
        }

        public void FeedbackInput() {
            Point mousePos = new MouseScreenObjectState(FeedbackConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
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
        public void Toggle() {
            if (FeedbackWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                FeedbackWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
                current = null;
            }
            else {
                GameLoop.UIManager.selectedMenu = "Feedback";
                FeedbackWindow.IsVisible = true;
                FeedbackWindow.IsFocused = true;
                current = new();
            }
        }
    }
}
