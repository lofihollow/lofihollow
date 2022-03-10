using LofiHollow.Entities;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using Steamworks;
using System;

namespace LofiHollow.UI {
    public class UI_Options : Lofi_UI {  
        public UI_Options(int width, int height, string title) : base(width, height, title, "Options") { 
        }


        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            int gameVol = (int) Math.Floor(GameLoop.SoundManager.engine.SoundVolume * 100);
            int musicVol = (int) Math.Floor(GameLoop.SoundManager.Music.SoundVolume * 100);

            Con.Print(1, 1, "Hold Shift to move 10 at a time");
            Con.Print(1, 3, " Game Volume: ");
            Con.PrintClickable(15, 3, "-", UI_Clicks, "DecreaseGameVolume");
            Con.Print(16, 3, gameVol.ToString().Align(HorizontalAlignment.Right, 3, ' ')); 
            Con.PrintClickable(20, 3, "+", UI_Clicks, "IncreaseGameVolume");

            Con.Print(1, 5, "Music Volume: ");
            Con.PrintClickable(15, 5, "-", UI_Clicks, "DecreaseMusicVolume");
            Con.Print(16, 5, musicVol.ToString().Align(HorizontalAlignment.Right, 3, ' '));
            Con.PrintClickable(20, 5, "+", UI_Clicks, "IncreaseMusicVolume");

            Con.PrintClickable(1, 7, "Music Enabled: " + Helper.Checkmark(GameLoop.SoundManager.MusicEnabled), UI_Clicks, "ToggleMusic");

            Con.PrintClickable(55, 39, "Exit to Menu", UI_Clicks, "ExitToMenu");
            Con.Print(54, 38, "(Does NOT Save)");
        }

        public override void UI_Clicks(string ID) {
            if (GameLoop.EitherShift()) {
                if (ID == "DecreaseGameVolume") {
                    GameLoop.SoundManager.engine.SoundVolume = Math.Clamp(GameLoop.SoundManager.engine.SoundVolume - 0.1f, 0.0f, 1.0f);
                }
                else if (ID == "IncreaseGameVolume") { 
                    GameLoop.SoundManager.engine.SoundVolume = Math.Clamp(GameLoop.SoundManager.engine.SoundVolume + 0.1f, 0.0f, 1.0f);
                }
                else if (ID == "DecreaseMusicVolume") { 
                    GameLoop.SoundManager.Music.SoundVolume = Math.Clamp(GameLoop.SoundManager.Music.SoundVolume - 0.1f, 0.0f, 1.0f);
                }
                else if (ID == "IncreaseMusicVolume") {
                    GameLoop.SoundManager.Music.SoundVolume = Math.Clamp(GameLoop.SoundManager.Music.SoundVolume + 0.1f, 0.0f, 1.0f);
                }
            } else {
                if (ID == "DecreaseGameVolume") {
                    GameLoop.SoundManager.engine.SoundVolume = Math.Clamp(GameLoop.SoundManager.engine.SoundVolume - 0.01f, 0.0f, 1.0f);
                }
                else if (ID == "IncreaseGameVolume") {
                    GameLoop.SoundManager.engine.SoundVolume = Math.Clamp(GameLoop.SoundManager.engine.SoundVolume + 0.01f, 0.0f, 1.0f);
                }
                else if (ID == "DecreaseMusicVolume") {
                    GameLoop.SoundManager.Music.SoundVolume = Math.Clamp(GameLoop.SoundManager.Music.SoundVolume - 0.01f, 0.0f, 1.0f);
                }
                else if (ID == "IncreaseMusicVolume") {
                    GameLoop.SoundManager.Music.SoundVolume = Math.Clamp(GameLoop.SoundManager.Music.SoundVolume + 0.01f, 0.0f, 1.0f);
                }
            }

            if (ID == "ToggleMusic") {
                GameLoop.SoundManager.MusicEnabled = !GameLoop.SoundManager.MusicEnabled;
            }
            else if (ID == "ExitToMenu") {
                GameLoop.UIManager.MainMenu.MainMenuWindow.IsVisible = true;
                GameLoop.UIManager.Map.MapWindow.IsVisible = false;
                GameLoop.UIManager.Map.MessageLog.IsVisible = false;
                GameLoop.UIManager.Sidebar.SidebarWindow.IsVisible = false;
                GameLoop.UIManager.selectedMenu = "MainMenu";

                if (GameLoop.NetworkManager != null) {
                    GameLoop.NetworkManager.LeaveLobby();
                }
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            } 
        } 
    }
}
