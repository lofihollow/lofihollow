using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrrKlang;
using LofiHollow.DataTypes;

namespace LofiHollow.Managers {
    public class SoundManager {
        public ISoundEngine engine = new();
        public ISoundEngine Music = new();
        public ISoundEngine speech = new();

        public Dictionary<string, ISound> PlayingSounds = new();
        public string CurrentSong = "None";
        public bool MusicEnabled = true;
        public Dictionary<string, Speaker> Speakers = new();

        public int speechCounter = 0;
        public int speechDelay = 5;

        public SoundManager() {
            engine.SoundVolume = 0.05f;
            Music.SoundVolume = 0.03f;
            speech.SoundVolume = 0.07f; 
        }

        public void QueueSpeech(string source, string path) {
            if (!Speakers.ContainsKey(source)) {
                Speakers.Add(source, new()); 
            }

            Speakers[source].QueuedSpeech.Add(path);
        }

        public void PopSpeech() {
            foreach (var kv in Speakers) {
                if (kv.Value.QueuedSpeech.Count > 0) {
                    if (File.Exists(kv.Value.QueuedSpeech[0])) {
                        var sound = speech.Play2D(kv.Value.QueuedSpeech[0], false);
                        sound.PlaybackSpeed = 2.0f + ((float) GameLoop.rand.NextDouble());
                    }
                    kv.Value.QueuedSpeech.RemoveAt(0);
                }

                if (kv.Value.QueuedSpeech.Count == 0) {
                    Speakers.Remove(kv.Key);
                }
            }
        }

        public void PlaySound(string name) {
            if (File.Exists("./sounds/" + name + ".ogg")) {
                var test = engine.Play2D("./sounds/" + name + ".ogg");
                if (!PlayingSounds.ContainsKey(name))
                    PlayingSounds.Add(name, test);
            }
        }

        public void StopSound(string name) {
            if (PlayingSounds.ContainsKey(name)) {
                PlayingSounds[name].Stop();
                PlayingSounds.Remove(name);
            }
        }

        public void UpdateSounds() {
            foreach (KeyValuePair<string, ISound> kv in PlayingSounds) {
                if (kv.Value.Finished)
                    PlayingSounds.Remove(kv.Key);
            }

            speechCounter++;

            if (speechCounter >= speechDelay) {
                PopSpeech();
                speechCounter = 0;
            }
        }


        public void PickMusic() {
            string NewSong = CurrentSong;
            if (MusicEnabled && !GameLoop.UIManager.MenuBackdrop.ModMaker.Win.IsVisible) {
                if (GameLoop.UIManager.MainMenu.Win.IsVisible) {
                    NewSong = "Title Theme";
                } 
                else if (GameLoop.UIManager.Minigames.CurrentGame != "None") {
                    NewSong = "Minigame Theme";
                } 
                else if (GameLoop.World.Player.Clock.GetCurrentTime() > 1110) {
                    NewSong = "Noonbreeze Night Theme";
                }
                else {
                    NewSong = "Noonbreeze Day Theme";
                }

                if (NewSong != CurrentSong) {
                    Music.StopAllSounds();
                    Music.Play2D("./sounds/" + NewSong + ".ogg", true);
                    CurrentSong = NewSong;
                }
            } else {
                Music.StopAllSounds();
                CurrentSong = "None";
            }
        }
    }
}
