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
    public class UI_MainMenu : InstantUI { 
        Dictionary<string, int> sounds = new();
        List<string> toPlay = new();
        List<string> testList = new();

        public UI_MainMenu(int width, int height) : base(width, height, "MainMenu") { 
            
        }


        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            Con.PrintClickable(4, 2, "   New", UI_Clicks, "newChar");
            Con.PrintClickable(4, 4, "Load File", UI_Clicks, "GoToLoad");

            Con.PrintClickable(4, 6, "   Mod", UI_Clicks, "GoToMods");
            Con.PrintClickable(4, 8, " Test Voice", UI_Clicks, "TestVoice");

            Con.Print(0, 10, sounds.Count.ToString());

            for (int i = 0; i < testList.Count; i++) {
                Con.Print(0, 11 + i, testList[i]);
            }

            Con.PrintClickable(4, 24, "Exit Game", UI_Clicks, "ExitGame");
        }

        public override void Input() {  
        } 

        public override void UI_Clicks(string ID) {
            if (ID == "ExitGame") {
                System.Environment.Exit(0);
            }

            else if (ID == "GoToMods") {
            }

            else if (ID == "GoToLoad") {
                GameLoop.UIManager.ToggleUI("MainMenu");
                GameLoop.UIManager.LoadFile.UpdateFileList();
                GameLoop.UIManager.ToggleUI("LoadFile");
            }

            else if (ID == "newChar") {
                GameLoop.UIManager.ToggleUI("MainMenu");
                GameLoop.UIManager.ToggleUI("CharGen");
            }

            else if (ID == "TestVoice") {
                string[] keys = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "th", "sh", " ", "."};

                sounds.Clear();
                toPlay.Clear();
                testList.Clear();

                for (int i = 0; i < keys.Length; i++) {
                    sounds.Add(keys[i], i + 1);
                }

                string testLine = "The quick brown fox jumps over the lazy dog?";

                string cleaned = testLine.ToLower();

                string[] pitches = { "nanz", "nanz_low", "nanz_lower", "nanz_high", "nanz_higher" };
                string[] consonants = { "_", "k", "s", "t", "n", "h", "m", "y", "r", "w" };
                string[] vowels = { "a", "e", "i", "o", "u" };


                string pitch = pitches[GameLoop.rand.Next(pitches.Length)]; 

                string randomSound = consonants[GameLoop.rand.Next(consonants.Length)] + vowels[GameLoop.rand.Next(vowels.Length)];


                for (int i = 0; i < cleaned.Length; i++) {
                    if (cleaned[i] == '.') {
                        toPlay.Add("a");
                        toPlay.Add("a");
                        toPlay.Add("a");
                        toPlay.Add("a");
                    } else if (cleaned[i] == ' ') { 
                        toPlay.Add("a");
                        toPlay.Add("a");
                    } else { 
                        toPlay.Add("./sounds/speech/" + pitch + "/" + randomSound + ".wav");
                        //toPlay.Add("./sounds/speech/" + pitch + "/sound" + which.ToString().PadLeft(2, '0') + ".wav");
                    }

                    /*
                    if (cleaned[i] == 's' && i + 1 < cleaned.Length && cleaned[i+1] == 'h') {
                        toPlay.Add("./sounds/speech/" + pitch + "/" + "sound" + sounds["sh"].ToString().PadLeft(2, '0') + ".wav");
                        testList.Add("sound" + sounds["sh"].ToString().PadLeft(2, '0') + ".wav");
                        i++;
                    } else if (cleaned[i] == 't' && i + 1 < cleaned.Length && cleaned[i + 1] == 'h') {
                        toPlay.Add("./sounds/speech/" + pitch + "/" + "sound" + sounds["th"].ToString().PadLeft(2, '0') + ".wav");
                        testList.Add("sound" + sounds["th"].ToString().PadLeft(2, '0') + ".wav");
                        i++;
                    } else { 
                        toPlay.Add("./sounds/speech/" + pitch + "/" + "sound" + sounds[cleaned[i].ToString()].ToString().PadLeft(2, '0') + ".wav");
                        testList.Add("sound" + sounds[cleaned[i].ToString()].ToString().PadLeft(2, '0') + ".wav");
                    }
                    */
                }

                string ms = GameHost.Instance.GameRunningTotalTime.TotalMilliseconds.ToString();

                foreach (string v in toPlay) {
                    GameLoop.SoundManager.QueueSpeech("test-" + ms, v);
                }
            }
        }
    }
}
