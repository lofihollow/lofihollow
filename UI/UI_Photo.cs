using LofiHollow.Entities;
using LofiHollow.EntityData;
using LofiHollow.Managers;
using LofiHollow.Minigames.Photo;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using System.Linq;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Photo : Lofi_UI { 
        public Photo CurrentPhoto;
        public SadConsole.Entities.Renderer EntitiesInPhoto; 
        public List<PhotoJob> DailyJobs = new();
        public bool ShowingBoard = false;

        public UI_Photo(int width, int height, string title) : base(width, height, title, "Photo") { } 

        public bool HasPhotoOf(Player play, string targetName, string type) {
            for (int i = 0; i < play.Inventory.Length; i++) {
                if (play.Inventory[i].Photo != null) {
                    Photo photo = play.Inventory[i].Photo;

                    if (photo.Contains(targetName, type)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ConsumePhoto(Player play, string targetName, string type, int reward) {
            for (int i = 0; i < play.Inventory.Length; i++) {
                if (play.Inventory[i].Photo != null) {
                    Photo photo = play.Inventory[i].Photo;
                    if (photo.Contains(targetName, type)) {
                        CommandManager.RemoveOneItem(play, i);
                        play.CopperCoins += reward;
                        return;
                    }
                }
            }
        }

        public void PopulateJobList() {
            DailyJobs.Clear();

            for (int i = 0; i < 12; i++) {
                int type = GameLoop.rand.Next(4);
                string targetType;
                string targetName;
                int rewardAmount;
                ColoredString app;
                Decorator dec = null;

                if (type == 0)
                    targetType = "Item";
                else if (type == 1)
                    targetType = "NPC";
                else
                    targetType = "Monster"; 


                if (targetType == "Item") {
                    int index = GameLoop.rand.Next(GameLoop.World.itemLibrary.Count - 1) + 1;
                    targetName = GameLoop.World.itemLibrary.ElementAt(index).Value.Name;
                    app = GameLoop.World.itemLibrary.ElementAt(index).Value.AsColoredGlyph();
                    if (GameLoop.World.itemLibrary.ElementAt(index).Value.Dec != null)
                        dec = new(GameLoop.World.itemLibrary.ElementAt(index).Value.Dec);
                    rewardAmount = GameLoop.rand.Next(8) + 2;
                } else if (targetType == "NPC") {
                    int index = GameLoop.rand.Next(GameLoop.World.npcLibrary.Count);
                    targetName = GameLoop.World.npcLibrary.ElementAt(index).Value.Name; 
                    app = GameLoop.World.npcLibrary.ElementAt(index).Value.GetAppearance();
                    rewardAmount = GameLoop.rand.Next(8) + 2;
                } else {
                    int index = GameLoop.rand.Next(GameLoop.World.monsterLibrary.Count);
                    targetName = GameLoop.World.monsterLibrary.ElementAt(index).Value.Species; 
                    app = GameLoop.World.monsterLibrary.ElementAt(index).Value.GetAppearance();
                    rewardAmount = GameLoop.rand.Next(GameLoop.World.monsterLibrary.ElementAt(index).Value.MinLevel) + 2;
                }

                PhotoJob newJob = new(app, dec, targetName, targetType, rewardAmount);
                DailyJobs.Add(newJob);
            }
        }

        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            Con.Clear();
            if (!ShowingBoard) {
                if (CurrentPhoto != null && CurrentPhoto.tiles != null) {
                    Con.Fill(Color.White, Color.Black, 32);
                    Con.Print(26, 0, 350.AsString(), Color.Lime);
                    Con.Print(0, 1, CurrentPhoto.PhotoName.Align(HorizontalAlignment.Center, 29));
                    Con.DrawLine(new Point(4, 3), new Point(24, 3), 196, Color.White);
                    Con.DrawLine(new Point(4, 25), new Point(24, 25), 196, Color.White);
                    Con.DrawLine(new Point(3, 4), new Point(3, 24), 179, Color.White);
                    Con.DrawLine(new Point(25, 4), new Point(25, 24), 179, Color.White);
                    Con.Print(3, 3, 218.AsString());
                    Con.Print(3, 25, 192.AsString());
                    Con.Print(25, 3, 191.AsString());
                    Con.Print(25, 25, 217.AsString());

                    if (mousePos.X >= 10 && mousePos.Y >= 10 && mousePos.X < 19 && mousePos.Y < 19) {
                        bool foundEnt = false;

                        for (int i = 0; i < CurrentPhoto.entities.Count; i++) {
                            if (CurrentPhoto.entities[i].X == mousePos.X - 4 && CurrentPhoto.entities[i].Y == mousePos.Y - 4) {
                                foundEnt = true;
                                Con.Print(0, 2, CurrentPhoto.entities[i].Name.Align(HorizontalAlignment.Center, 29));
                            }
                        }

                        if (!foundEnt) {
                            PhotoTile tile = CurrentPhoto.tiles[(mousePos.X - 4) + ((mousePos.Y - 4) * 21)];
                            Con.Print(0, 2, tile.Name.Align(HorizontalAlignment.Center, 29));

                        }
                    }

                    for (int i = 0; i < CurrentPhoto.tiles.Length; i++) {
                        Con.Print((i % 21) + 4, (i / 21) + 4, CurrentPhoto.tiles[i].GetAppearance());
                        if (CurrentPhoto.tiles[i].Dec != null)
                            Con.SetDecorator((i % 21) + 4, (i / 21) + 4, 1, CurrentPhoto.tiles[i].GetDec());
                    }

                    for (int i = 0; i < CurrentPhoto.entities.Count; i++) {
                        PhotoEntity ent = CurrentPhoto.entities[i];
                        Con.Print(ent.X + 4, ent.Y + 4, ent.Appearance());
                        if (ent.Dec != null)
                            Con.SetDecorator(ent.X + 4, ent.Y + 4, 1, ent.GetDec());
                    }

                    Con.Print(0, 27, (CurrentPhoto.SeasonTaken + " " + CurrentPhoto.DayTaken + " / " + TimeManager.MinutesToTime(CurrentPhoto.MinutesTaken)).Align(HorizontalAlignment.Center, 29));
                }
            } else {
                Con.Print(0, 1, ("Subject".Align(HorizontalAlignment.Center, 17) + "|" + "Reward".Align(HorizontalAlignment.Center, 6) + "|"));
                Con.DrawLine(new Point(0, 2), new Point(28, 2), 196, Color.White);
                for (int i = 0; i < DailyJobs.Count; i++) {
                    Con.Print(0, 3 + (i * 2), DailyJobs[i].Appearance + 
                        new ColoredString((" " + DailyJobs[i].Target).Align(HorizontalAlignment.Left, 16) 
                        + "|" + DailyJobs[i].RewardCoppers.ToString().Align(HorizontalAlignment.Center, 6) + "|"));

                    Con.Print(25, 3 + (i * 2), new ColoredString("DONE", HasPhotoOf(GameLoop.World.Player, DailyJobs[i].Target, DailyJobs[i].Type) ? Color.Lime : Color.Red, Color.Black));
                    if (DailyJobs[i].Dec != null)
                        Con.SetDecorator(0, 3 + (i * 2), 1, DailyJobs[i].Dec.GetDec());
                    Con.DrawLine(new Point(0, 4 + (i * 2)), new Point(28, 4 + (i * 2)), '-');
                }
            }


            Con.Print(28, 0, "X");
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos == new Point(28, 0)) {
                    Toggle();
                }
            }

            if (!ShowingBoard) {
                foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                    if ((key.Character >= 'A' && key.Character <= 'z') || (key.Character >= '0' && key.Character <= '9'
                        || key.Character == ';' || key.Character == ':' || key.Character == '|')) {
                        CurrentPhoto.PhotoName += key.Character;
                    }
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    CurrentPhoto.PhotoName += " ";
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Back)) {
                    CurrentPhoto.PhotoName = CurrentPhoto.PhotoName[0..^1];
                }

                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (mousePos == new Point(26, 0)) {
                        string photoPath = "./photos/" + CurrentPhoto.PhotoName + ".png";
                        GameLoop.UIManager.AddMsg(new ColoredString("Saved photo to file: " + photoPath, Color.Lime, Color.Black));
                        ((SadConsole.Host.GameTexture)Con.Renderer.Output).Texture.Save(photoPath);
                    }
                }
            } else {
                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (mousePos.X >= 25 && mousePos.Y - 3 >= 0) {
                        if ((float)((mousePos.Y - 3) / 2f) == (mousePos.Y - 3) / 2) {
                            int slot = (mousePos.Y - 3) / 2;
                            if (slot < DailyJobs.Count) {
                                if (HasPhotoOf(GameLoop.World.Player, DailyJobs[slot].Target, DailyJobs[slot].Type)) {
                                    ConsumePhoto(GameLoop.World.Player, DailyJobs[slot].Target, DailyJobs[slot].Type, DailyJobs[slot].RewardCoppers);
                                    DailyJobs.RemoveAt(slot);
                                }
                            }
                        }
                    }
                }
            }
        } 
    }
}
