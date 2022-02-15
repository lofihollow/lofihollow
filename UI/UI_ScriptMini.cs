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

namespace LofiHollow.UI {
    public class UI_ScriptMini {
        public SadConsole.Console ScriptConsole;
        public Window ScriptWindow;

        public string output = "";
        public Lua RunningScript;
        LuaFunction ScriptUpdate;
        LuaFunction ScriptInput;

        public UI_ScriptMini(int width, int height, string title) {
            ScriptWindow = new(width, height);
            ScriptWindow.CanDrag = false;
            ScriptWindow.Position = new(0, 0);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            ScriptConsole = new(invConWidth, invConHeight);
            ScriptConsole.Position = new(1, 1);
            ScriptWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            ScriptWindow.Children.Add(ScriptConsole);
            GameLoop.UIManager.Children.Add(ScriptWindow);

            ScriptWindow.Show();
            ScriptWindow.IsVisible = false;
        }


        public void RenderScriptMini() {
            Point mousePos = new MouseScreenObjectState(ScriptConsole, GameHost.Instance.Mouse).CellPosition;

            ScriptConsole.Clear();

            if (RunningScript != null) {
                try {
                    ScriptUpdate.Call();
                } catch (NLua.Exceptions.LuaScriptException e) {
                    GameLoop.UIManager.AddMsg("LuaError: " + e.Message);
                }
            }
                
        }

        public void ScriptMiniInput() {
            Point mousePos = new MouseScreenObjectState(ScriptConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle("None");
            }

            foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                if ((key.Character >= 'A' && key.Character <= 'z') || (key.Character >= '0' && key.Character <= '9'
                    || key.Character == ';' || key.Character == ':' || key.Character == '|' || key.Character == ' ')) { 
                    if (RunningScript != null) {
                        try { 
                            ScriptInput.Call(key.Character.ToString());
                        } catch (NLua.Exceptions.LuaScriptException e) {
                            GameLoop.UIManager.AddMsg("LuaError: " + e.Message);
                        }
                    }
                }
            }
        } 

        public void Toggle(string scriptName) {
            if (ScriptWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                ScriptWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
                RunningScript = null;
            } else {
                if (GameLoop.World.scriptLibrary.ContainsKey(scriptName)) {
                    GameLoop.UIManager.selectedMenu = "ScriptMini";
                    RunningScript = new Lua();
                    RunningScript["os"] = null;
                    RunningScript["lh"] = new ScriptInterface();
                    RunningScript.DoString(GameLoop.World.scriptLibrary[scriptName]);
                    ScriptUpdate = RunningScript["update"] as LuaFunction;
                    ScriptInput = RunningScript["input"] as LuaFunction;

                    ScriptWindow.IsVisible = true;
                    ScriptWindow.IsFocused = true;
                }
            }
        }
    }
}
