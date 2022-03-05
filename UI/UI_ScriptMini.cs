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
    public class UI_ScriptMini : Lofi_UI {
        public string output = "";
        public Lua RunningScript;
        LuaFunction ScriptUpdate;
        LuaFunction ScriptInput;

        public UI_ScriptMini(int width, int height, string title) : base(width, height, title, "ScriptMini") { }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            if (RunningScript != null) {
                try {
                    ScriptUpdate.Call();
                } catch (NLua.Exceptions.LuaScriptException e) {
                    GameLoop.UIManager.AddMsg("LuaError: " + e.Message);
                }
            }
                
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
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
            if (Win.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                Win.IsVisible = false;
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

                    Win.IsVisible = true;
                    Win.IsFocused = true;
                }
            }
        }
    }
}
