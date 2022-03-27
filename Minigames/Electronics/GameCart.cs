using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Electronics {
    [JsonObject(MemberSerialization.OptOut)]
    public class GameCart {
        public string Name = "";
        private byte[] memory = new byte[1024];


        public GameCart() {
            for (int i = 0; i < memory.Length; i++) {
                memory[i] = new();
            }
        }

        
        public void SetByte(int index, byte input) {
            memory[index] = input;
        }
    }
}
