using LofiHollow.Entities;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using Steamworks;
using LofiHollow.Minigames.Archaeology;
using Newtonsoft.Json;
using System.IO;

namespace LofiHollow.UI {
    public class UI_ArtifactMaker {
        public SadConsole.Console ArtMakerConsole;
        public Window ArtMakerWindow;

        public ArchArtifact Current;

        public int PaintGlyph = 32;
        public int PaintR = 127;
        public int PaintG = 127;
        public int PaintB = 127;

        public UI_ArtifactMaker(int width, int height, string title) {
            ArtMakerWindow = new(width, height);
            ArtMakerWindow.CanDrag = false;
            ArtMakerWindow.Position = new(11, 6);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            ArtMakerConsole = new(invConWidth, invConHeight);
            ArtMakerConsole.Position = new(1, 1);
            ArtMakerWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            ArtMakerWindow.Children.Add(ArtMakerConsole);
            GameLoop.UIManager.Children.Add(ArtMakerWindow);

            ArtMakerWindow.Show();
            ArtMakerWindow.IsVisible = false;
        }


        public void RenderArtifactMaker() {
            Point mousePos = new MouseScreenObjectState(ArtMakerConsole, GameHost.Instance.Mouse).CellPosition;

            ArtMakerConsole.Clear();

            ArtMakerConsole.Print(0, 2, "Name: " + Current.Name);
            ArtMakerConsole.Print(0, 4, "Glyph: " + PaintGlyph.AsString(), new Color(PaintR, PaintG, PaintB));
            ArtMakerConsole.Print(0, 6, "ForeR: " + PaintR);  
            ArtMakerConsole.Print(0, 8, "ForeG: " + PaintG); 
            ArtMakerConsole.Print(0, 10, "ForeB: " + PaintB); 

            ArtMakerConsole.PrintClickable(0, 39, "SAVE", ArtClick, "Save");

            ArtMakerConsole.DrawLine(new Point(19, 5), new Point(19, 34), 179, Color.White);
            ArtMakerConsole.DrawLine(new Point(50, 5), new Point(50, 34), 179, Color.White);
            ArtMakerConsole.DrawLine(new Point(20, 4), new Point(49, 4), 196, Color.White);
            ArtMakerConsole.DrawLine(new Point(20, 35), new Point(49, 35), 196, Color.White);


            for (int x = 0; x < 30; x++) {
                for (int y = 0; y < 30; y++) {
                    ArtMakerConsole.Print(20 + x, 5 + y, Current.Tiles[x + (y * 30)].GetAppearance());
                }
            }
            
        }

        public void ArtClick(string ID) {
            if (ID == "Save") {
                Directory.CreateDirectory("./data/artifacts/");
                Helper.SerializeToFile(Current, "./data/artifacts/" + Current.Name + ".art");
            } 
        }

        public void ArtifactMakerInput() {
            Point mousePos = new MouseScreenObjectState(ArtMakerConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            }

            foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                if (key.Character >= 'A' && key.Character <= 'z') {
                    Current.Name += key.Character; 
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                Current.Name += " "; 
            }

            if (Current.Name.Length > 0) {
                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Back)) {
                    Current.Name = Current.Name[0..^1]; 
                }
            }

            if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                if (mousePos.Y == 4)
                    PaintGlyph++;
                else if (mousePos.Y == 6)
                    if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                        PaintR += 10;
                    else
                        PaintR++;
                else if (mousePos.Y == 8)
                    if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                        PaintG += 10;
                    else
                        PaintG++;
                else if (mousePos.Y == 10)
                    if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                        PaintB += 10;
                    else
                        PaintB++;
            } else if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                if (mousePos.Y == 4)
                    PaintGlyph--;
                else if (mousePos.Y == 6)
                    if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                        PaintR -= 10;
                    else
                        PaintR--;
                else if (mousePos.Y == 8)
                    if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                        PaintG -= 10;
                    else
                        PaintG--;
                else if (mousePos.Y == 10)
                    if (GameHost.Instance.Keyboard.IsKeyDown(Key.LeftShift))
                        PaintB -= 10;
                    else
                        PaintB--;
            }

            if (PaintR < 0)
                PaintR = 0;
            if (PaintG < 0)
                PaintG = 0;
            if (PaintB < 0)
                PaintB = 0;

            if (PaintR > 255)
                PaintR = 255;
            if (PaintG > 255)
                PaintG = 255;
            if (PaintB > 255)
                PaintB = 255;


            if (GameHost.Instance.Mouse.LeftClicked) {
                Point offset = mousePos - new Point(20, 5);

                if (offset.X >= 0 && offset.Y >= 0 && offset.X <= 29 && offset.Y <= 29) {
                    Current.Tiles[offset.ToIndex(30)].ForeR = PaintR;
                    Current.Tiles[offset.ToIndex(30)].ForeG = PaintG;
                    Current.Tiles[offset.ToIndex(30)].ForeB = PaintB;
                    Current.Tiles[offset.ToIndex(30)].Glyph = PaintGlyph;
                }
            }
        } 

        public void Toggle() {
            if (ArtMakerWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                ArtMakerWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
            }
            else {
                Current = new ArchArtifact();
                GameLoop.UIManager.selectedMenu = "ArtMaker";
                ArtMakerWindow.IsVisible = true;
                ArtMakerWindow.IsFocused = true;
            }
        }
    }
}
