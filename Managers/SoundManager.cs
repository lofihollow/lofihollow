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
        public Dictionary<string, ISound> PlayingSounds = new();

        public SoundManager() {
            engine.SoundVolume = 0.05f;
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
    }
}
