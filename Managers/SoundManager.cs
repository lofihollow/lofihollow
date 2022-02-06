using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrrKlang;

namespace LofiHollow.Managers {
    public class SoundManager {
        public ISoundEngine engine = new();
        public ISoundEngine Music = new();

        public Dictionary<string, ISound> PlayingSounds = new();
        public string CurrentSong = "None";

        public SoundManager() {
            engine.SoundVolume = 0.05f;
            Music.SoundVolume = 0.03f;
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
        }


        public void PickMusic() {
            string NewSong = CurrentSong;

            if (GameLoop.UIManager.MainMenu.MainMenuWindow.IsVisible) {
                NewSong = "Title Theme";
            } else if (GameLoop.World.Player.MineLocation != "None") {
                NewSong = "Mine Theme";
            } else if (GameLoop.UIManager.Minigames.CurrentGame != "None") {
                NewSong = "Minigame Theme";
            } else if (GameLoop.UIManager.DialogueWindow.dialogueOption == "Shop") {
                NewSong = "Shop Theme";
            } else if (GameLoop.World.Player.Clock.GetCurrentTime() > 1110) {
                NewSong = "Noonbreeze Night Theme";
            } else {
                NewSong = "Noonbreeze Day Theme";
            }

            if (NewSong != CurrentSong) {
                Music.StopAllSounds();
                Music.Play2D("./sounds/" + NewSong + ".ogg", true);
                CurrentSong = NewSong;
            }
        }
    }
}
