using LofiHollow.Minigames;
using Newtonsoft.Json;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives; 
using Key = SadConsole.Input.Keys;

namespace LofiHollow.Minigames {
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Minigame {
        public virtual void Input() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.MinigameConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Close();
            }
        }

        public virtual void Draw() {
            Point mousePos = new MouseScreenObjectState(GameLoop.UIManager.Minigames.MinigameConsole, GameHost.Instance.Mouse).CellPosition;
        }

        public virtual void Close() { 
            GameLoop.UIManager.Minigames.ToggleMinigame("None");
        }
    }
}
