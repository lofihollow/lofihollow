using SadRogue.Primitives;
using SadConsole;
using SadConsole.UI;
using SadConsole.Input;
using System.IO;
using System; 
using LofiHollow.Managers;
using Key = SadConsole.Input.Keys;
using Newtonsoft.Json; 
using LofiHollow.DataTypes;

namespace LofiHollow.UI {
    public class UI_MenuBackdrop {
        public SadConsole.Console MenuBackdrop; 
        public SadRex.Image MenuImage; 

        public UI_ModMaker ModMaker; 

        public UI_MenuBackdrop() { 
            int menuConWidth = GameLoop.GameWidth;

            Stream menuXP = new FileStream("./data/island.xp", FileMode.Open);
            MenuImage = SadRex.Image.Load(menuXP);


            ColoredGlyph[] cells = new ColoredGlyph[GameLoop.GameWidth * GameLoop.GameHeight];

            for (int i = 0; i < MenuImage.Layers[0].Cells.Count && i < GameLoop.GameWidth * GameLoop.GameHeight; i++) {
                var cell = MenuImage.Layers[0].Cells[i];
                Color convertedFG = new(cell.Foreground.R, cell.Foreground.G, cell.Foreground.B);
                Color convertedBG = new(cell.Background.R, cell.Background.G, cell.Background.B);

                cells[i] = new ColoredGlyph(Color.Transparent, convertedFG, MenuImage.Layers[0].Cells[i].Character);
            }

            MenuBackdrop = new SadConsole.Console(GameLoop.GameWidth, GameLoop.GameHeight, cells); 

            MenuBackdrop.Position = new Point(0, 0);   

            ModMaker = new UI_ModMaker(72, 42);
             
            GameLoop.UIManager.Children.Add(MenuBackdrop);

            int leftEdge = 57;
            int topEdge = 3;

            for (int i = 0; i < MenuImage.Layers[0].Cells.Count; i++) {
                var cell = MenuImage.Layers[0].Cells[i];
                Color convertedFG = new(cell.Foreground.R, cell.Foreground.G, cell.Foreground.B);

                MenuBackdrop.SetCellAppearance(i % GameLoop.GameWidth, i / GameLoop.GameWidth, new ColoredGlyph(Color.Transparent, convertedFG, MenuImage.Layers[0].Cells[i].Character));
            }

            // L
            for (int i = 0; i < 5; i++) {
                MenuBackdrop.SetDecorator(leftEdge + 1, topEdge + i, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            }
            // MainMenuConsole.SetDecorator(28, 7, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None)); 



            // O
            MenuBackdrop.SetDecorator(leftEdge + 3, topEdge + 2, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 3, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 5, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 3, topEdge + 4, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

            // F
            MenuBackdrop.SetDecorator(leftEdge + 8, topEdge + 1, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 7, topEdge + 2, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 7, topEdge + 3, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 7, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

            // I
            MenuBackdrop.SetDecorator(leftEdge + 10, topEdge + 1, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 10, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 10, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));



            // H
            MenuBackdrop.SetDecorator(leftEdge + 14, topEdge, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 14, topEdge + 1, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 14, topEdge + 2, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 14, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 14, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 16, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 16, topEdge + 4, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

            // O
            MenuBackdrop.SetDecorator(leftEdge + 18, topEdge + 2, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 18, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 20, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 18, topEdge + 4, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

            // LL
            for (int i = 0; i < 5; i++) {
                MenuBackdrop.SetDecorator(leftEdge + 22, topEdge + i, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
                MenuBackdrop.SetDecorator(leftEdge + 24, topEdge + i, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            }

            // O
            MenuBackdrop.SetDecorator(leftEdge + 26, topEdge + 2, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 26, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 28, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 26, topEdge + 4, 3, new CellDecorator(Color.MediumPurple, 240, Mirror.None));

            // W
            MenuBackdrop.SetDecorator(leftEdge + 30, topEdge + 2, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 34, topEdge + 2, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 30, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 32, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 34, topEdge + 3, 1, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 30, topEdge + 4, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
            MenuBackdrop.SetDecorator(leftEdge + 33, topEdge + 4, 2, new CellDecorator(Color.MediumPurple, 240, Mirror.None));
        } 

        public void LoadFileClick(string ID) {
            GameLoop.World.LoadAllMods();
            GameLoop.World.LoadPlayer(ID); 
        }

        public void MenuClicks(string ID) {  
            
        } 
    }
}
