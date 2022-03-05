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
    public abstract class Lofi_UI {
        public SadConsole.Console Con;
        public Window Win;
        public string UI_Name = "";

        public Lofi_UI(int width, int height, string title, string name) {
            Win = new(width, height);
            Win.CanDrag = true;
            Win.Position = new(11, 6);

            int conWidth = width - 2;
            int conHeight = height - 2;

            Con = new(conWidth, conHeight);
            Con.Position = new(1, 1);
            Win.Title = title.Align(HorizontalAlignment.Center, conWidth, (char)196);


            Win.Children.Add(Con);
            GameLoop.UIManager.Children.Add(Win);

            Win.Show();
            Win.IsVisible = false;

            UI_Name = name;
        }


        public virtual void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear(); 
        }

        public virtual void UI_Clicks(string ID) { 
        }

        public virtual void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            }
        }



        public virtual void Toggle() {
            if (Win.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                Win.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
                Con.Clear();
            }
            else {
                GameLoop.UIManager.selectedMenu = UI_Name;
                Win.IsVisible = true;
                Win.IsFocused = true;
            }
        }
    }
}
