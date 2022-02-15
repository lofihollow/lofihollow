using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Managers {
    public class ScriptManager {
        public Lua Running;

        public void SetupScript(string name) {
            if (GameLoop.World.scriptLibrary.ContainsKey(name)) {
                GameLoop.UIManager.selectedMenu = "ScriptMini";
                Running = new Lua();
                Running["os"] = null; 
                Running["lh"] = new ScriptInterface();
                Running.DoString(GameLoop.World.scriptLibrary[name]);

                LuaFunction main = Running["main"] as LuaFunction;
                main.Call();
            }
        }
    }
}
