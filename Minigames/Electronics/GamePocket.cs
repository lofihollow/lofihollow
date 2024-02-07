using LofiHollow.Entities;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using System.IO;
using NLua;
using LofiHollow.UI;

namespace LofiHollow.Minigames.Electronics {
    public class GamePocket : InstantUI {
        ColoredString[] screen = new ColoredString[900];

        public Lua RunningScript;
        LuaFunction ScriptUpdate;
        LuaFunction ScriptInput;

        public GamePocket(int width, int height, string title) : base(width, height, title, "GamePocket") { 
            for (int i = 0; i < screen.Length; i++) {
                screen[i] = new ColoredString(32.AsString());
            }
        }


        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            Helper.DrawBox(Con, 0, 0, 30, 30);
            Helper.DrawBox(Con, 26, 34, 1, 1); 
            Helper.DrawBox(Con, 21, 38, 1, 1);
            Con.Print(27, 35, "A");
            Con.Print(22, 39, "B");
            Con.Print(26, 44, "/////"); 
            Con.Print(25, 45, "/////"); 
            Con.Print(24, 46, "/////"); 
            Helper.DrawBox(Con, 5, 35, 1, 5);
            Helper.DrawBox(Con, 3, 37, 5, 1);
            Helper.DrawBox(Con, 5, 37, 1, 1);
            Con.Print(6, 36, 9.AsString());
            Con.Print(4, 38, 11.AsString());
            Con.Print(6, 38, "+");
            Con.Print(8, 38, 12.AsString());
            Con.Print(6, 40, 10.AsString()); 
            Helper.DrawBox(Con, 10, 45, 3, 0); 
            Helper.DrawBox(Con, 17, 45, 3, 0);


            for (int x = 0; x < 30; x++) {
                for (int y = 0; y < 30; y++) {
                    Con.Print(x + 1, y + 1, screen[x + (y * 30)]);
                }
            }


            if (RunningScript != null) {
                try {
                    ScriptUpdate.Call();
                }
                catch (NLua.Exceptions.LuaScriptException e) {
                    GameLoop.UIManager.AddMsg("LuaError: " + e.Message);
                }
            }
        }

        public override void UI_Clicks(string ID) {

        } 


        public void SetCell(int x, int y, int r, int g, int b, int ch) {
            ColoredString set = new(ch.AsString(), new Color(r, g, b), Color.Black);
            screen[x + (y * 30)] = set;
        }

        public void SetCell(int x, int y, string s) {
            ColoredString set = new(s, Color.White, Color.Black);
            screen[x + (y * 30)] = set;
        }

        public void SetCell(int x, int y, string s, Color col) {
            ColoredString set = new(s, col, Color.Black);
            screen[x + (y * 30)] = set;
        }


        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle(""); 
            }

            foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                if ((key.Character >= 'A' && key.Character <= 'z') || (key.Character >= '0' && key.Character <= '9'
                    || key.Character == ';' || key.Character == ':' || key.Character == '|' || key.Character == ' ')) {
                    if (RunningScript != null) {
                        try {
                            ScriptInput.Call(key.Character.ToString());
                        }
                        catch (NLua.Exceptions.LuaScriptException e) {
                            GameLoop.UIManager.AddMsg("LuaError: " + e.Message);
                        }
                    }
                }
            }
        }


        public void Toggle(string scriptName) {
            if (Win.IsVisible) { 
                Win.IsVisible = false; 
                RunningScript = null;
            }
            else {
                if (GameLoop.World.scriptLibrary.ContainsKey(scriptName)) { 
                    RunningScript = new Lua();
                    RunningScript["os"] = null;
                    RunningScript["lh"] = new ScriptInterface();
                    RunningScript.DoString(GameLoop.World.scriptLibrary[scriptName]);
                    ScriptUpdate = RunningScript["update"] as LuaFunction;
                    ScriptInput = RunningScript["input"] as LuaFunction;

                    Win.IsVisible = true;
                    Win.IsFocused = true;
                }
            }
        }
    }
}
