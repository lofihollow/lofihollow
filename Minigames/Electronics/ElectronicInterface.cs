﻿using SadConsole;
using SadRogue.Primitives;

namespace LofiHollow.Minigames.Electronics {
    class ElectronicInterface {
        public void Draw(int x, int y, string s) {
            GameLoop.UIManager.GamePocket.SetCell(x, y, s);
        }

        public void Draw(int x, int y, string s, int preset) {
            Color col = Color.White;
            switch (preset) {
                case 0: col = Color.Black; break;
                case 1: col = Color.Maroon; break;
                case 2: col = Color.Green; break;
                case 3: col = Color.Olive; break;
                case 4: col = Color.Navy; break;
                case 5: col = Color.Purple; break;
                case 6: col = Color.Teal; break;
                case 7: col = Color.Silver; break;
                case 8: col = Color.Gray; break;
                case 9: col = Color.Red; break;
                case 10: col = Color.Lime; break;
                case 11: col = Color.Yellow; break;
                case 12: col = Color.Blue; break;
                case 13: col = Color.Fuchsia; break;
                case 14: col = Color.Aqua; break;
                case 15: col = Color.White; break;
            }

            GameLoop.UIManager.GamePocket.SetCell(x, y, s, col);
        }

        public void Draw(int x, int y, string s, int r, int g, int b) {
            GameLoop.UIManager.GamePocket.SetCell(x, y, s, new Color(r, g, b));
        }

        public void Draw(int x, int y, string s, int r, int g, int b, int a) {
            GameLoop.UIManager.GamePocket.SetCell(x, y, s, new Color(r, g, b, a));
        }

        public void StartMinigame(string scriptName) {
            GameLoop.UIManager.GamePocket.Toggle(scriptName);
        }

        public string IntToString(int index) {
            return ((char)index).ToString();
        }
    }
}
