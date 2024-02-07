using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_FakeStart : InstantUI { 

        public UI_FakeStart(int width, int height) : base(width, height, "FakeStart") {

        }


        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            Win.IsFocused = true;

            Con.Print(0, 0, "Nice Try".Align(HorizontalAlignment.Center, 48));
            Con.Print(0, 2, ("We both know you knew that wasn't").Align(HorizontalAlignment.Center, 48));
            Con.Print(0, 3, ("going to work on a budget of " + GameLoop.UIManager.CharGen.SelectedPointBudget + " points.").Align(HorizontalAlignment.Center, 48));

            Con.PrintClickable(0, 7, ("[OKAY]").Align(HorizontalAlignment.Center, 48), UI_Clicks, "close");
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
        }

        public override void UI_Clicks(string ID) {
            if (ID == "close") { 
                GameLoop.UIManager.ToggleUI("FakeStart");
                GameLoop.SteamManager.UnlockAchievement("EASTER_EGG1");
            }
        }
    }
}
