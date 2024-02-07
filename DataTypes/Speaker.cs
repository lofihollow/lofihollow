using IrrKlang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class Speaker {
        public ISoundEngine speech = new();
        public List<string> QueuedSpeech = new(); 

        public Speaker() {
            speech.SoundVolume = 0.07f; 
        }
    }
}
