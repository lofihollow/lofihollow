using Newtonsoft.Json;
using SadConsole;

namespace LofiHollow.Minigames {
    public class KillJob {
        public string Target = "";
        public int Amount = -1;
        public int Reward = -1;
        public int Level = -1;

        public ColoredString Appearance;

        [JsonConstructor]
        public KillJob() { }

        public KillJob(ColoredString app, string target, int num, int rew) {
            Appearance = app;
            Target = target;
            Amount = num;
            Reward = rew;
        }
    }
}
