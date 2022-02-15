using LofiHollow.Entities;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using System;
using LofiHollow.Managers;

namespace LofiHollow.UI {
    public class UI_DevConsole {
        public SadConsole.Console DevConsole;
        public Window DevWindow;
        public MessageLogWindow ConsoleLog;

        public string CurrentLine = "";

        public UI_DevConsole(int width, int height, string title) {
            DevWindow = new(width, height);
            DevWindow.CanDrag = true;
            DevWindow.Position = new(11, 6);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            DevConsole = new(invConWidth, invConHeight);
            DevConsole.Position = new(1, 1);
            DevWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            DevWindow.Children.Add(DevConsole);
            GameLoop.UIManager.Children.Add(DevWindow);

            ConsoleLog = new MessageLogWindow(invConWidth, invConHeight - 2, "");
            ConsoleLog.Position = new(1, 1);

            DevWindow.Children.Add(ConsoleLog);
            ConsoleLog.Show();
            ConsoleLog.IsVisible = false;

            DevWindow.Show();
            DevWindow.IsVisible = false;
        }


        public void RenderDevConsole() {
            DevConsole.Clear();
            DevConsole.Print(0, 39, ((char) 12) + CurrentLine);
        }

        public void DevConsoleInput() {
            Point mousePos = new MouseScreenObjectState(DevConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            }

            foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                if ((key.Character >= 'A' && key.Character <= 'z') || (key.Character >= '0' && key.Character <= '9'
                    || key.Character == ';' || key.Character == ':' || key.Character == '|' || key.Character == '"')) {
                    CurrentLine += key.Character;
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                CurrentLine += " ";
            }

            if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Back) && CurrentLine.Length > 0) {
                CurrentLine = CurrentLine[0..^1];
            }

            if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Enter)) {
                ExecuteCommand(CurrentLine);
                CurrentLine = "";
            }
        } 


        public void ExecuteCommand(string com) {
            List<string> args = new();

            int lastSpaceIndex = 0;
            bool inQuotes = false;
            for (int i = 0; i < com.Length; i++) {
                if (com[i] == '"') {
                    if (!inQuotes) {
                        inQuotes = true;
                        lastSpaceIndex = i;
                    } else {
                        string upto = com[lastSpaceIndex..i];
                        args.Add(upto);
                        lastSpaceIndex = i + 1;
                        inQuotes = false;
                    }
                } 

                if (com[i].ToString() == " " && !inQuotes && i != lastSpaceIndex) {
                    string bit = com[lastSpaceIndex..i].ToString();
                    args.Add(bit);
                    lastSpaceIndex = i + 1;
                }
            }

            args.Add(com[lastSpaceIndex..].ToString());

            for (int i = 0; i < args.Count; i++) {
                if (args[i].Contains('"'))
                    args[i] = args[i][1..];
                args[i] = args[i].Trim();
            }

            //string[] args = com.Split(" ");

            if (args[0] == "give") {
                if (GameLoop.World.itemLibrary.ContainsKey(args[1])) {
                    int qty = args.Count > 2 ? Int32.Parse(args[2]) : 1;
                    int quality = args.Count > 3 ? Int32.Parse(args[3]) : 1;
                    
                    Item spawn = new(args[1], qty);
                    spawn.Quality = quality;
                    CommandManager.AddItemToInv(GameLoop.World.Player, spawn);

                    ConsoleLog.Add("Spawned " + qty + " " + spawn.Name + " (Quality " + quality + ")");
                } else {
                    ConsoleLog.Add("Item ID not found. IDs are case-sensitive.");
                }
            } else {
                ConsoleLog.Add("Command not found.");
            }
        }


        public void Toggle() {
            if (DevWindow.IsVisible) {
                DevWindow.IsVisible = false;
                ConsoleLog.IsVisible = false;
            } else {
                DevWindow.IsVisible = true;
                ConsoleLog.IsVisible = true;
                DevWindow.IsFocused = true;
            }
        }
    }
}
