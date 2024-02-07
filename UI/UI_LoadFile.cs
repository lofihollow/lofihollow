using LofiHollow.Entities;
using LofiHollow.Managers;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_LoadFile : InstantUI {
        public string[] Names;
        public string SelectedName = "";
        public bool ConfirmDelete = false;

        public UI_LoadFile(int width, int height) : base(width, height, "LoadFile") {
        }


        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            Con.Print(1, 0, "Select a File");

            if (SelectedName != "") {
                if (!ConfirmDelete) {
                    Con.PrintClickable(15, 0, new ColoredString("X", Color.Crimson, Color.Black), UI_Clicks, "confirmDelete");
                } else {
                    Con.PrintClickable(15, 0, new ColoredString(19.AsString(), Color.Red, Color.Black), UI_Clicks, "reallyDelete");
                }

                Con.PrintClickable(17, 0, new ColoredString(12.AsString(), Color.Lime, Color.Black), UI_Clicks, "loadFile");
            }

            Con.DrawLine(new Point(0, 1), new Point(18, 1), 196, Color.White);
            for (int i = 0; i < Names.Length; i++) {
                Con.PrintClickable(0, 2 + i, new ColoredString(Names[i].Align(HorizontalAlignment.Center, 18), Names[i] == SelectedName ? Color.Yellow : Color.White, Color.Black), LoadFileClick, Names[i]);
            }

            Con.PrintClickable(2, 25, "[BACK TO MENU]", UI_Clicks, "GoBack");
        }

        public override void Input() {
        }

        public void UpdateFileList() {
            if (Directory.Exists("./saves/")) {
                Names = Directory.GetFiles("./saves/");

                for (int i = 0; i < Names.Length; i++) {
                    string[] split = Names[i].Split("/");
                    Names[i] = split[^1];
                    Names[i] = Names[i].Split(".")[0];
                }
            }
        }

        public void LoadFileClick(string ID) {
            SelectedName = ID;
        }

        public override void UI_Clicks(string ID) {
            if (ID == "GoBack") {
                GameLoop.UIManager.ToggleUI("LoadFile");
                GameLoop.UIManager.ToggleUI("MainMenu");
                ConfirmDelete = false;
                SelectedName = "";
            } else if (ID == "loadFile") {
                GameLoop.World.LoadPlayer(SelectedName);
                ConfirmDelete = false;
                SelectedName = "";
            } else if (ID == "confirmDelete") {
                ConfirmDelete = true;
            } else if (ID == "reallyDelete") {
                File.Delete("./saves/" + SelectedName + ".json");
                ConfirmDelete = false;
                SelectedName = "";
                UpdateFileList();
            }
        }
    }
}
