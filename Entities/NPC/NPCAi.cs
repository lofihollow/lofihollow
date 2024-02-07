using LofiHollow.DataTypes;
using Newtonsoft.Json;
using SadRogue.Primitives;
using System;

namespace LofiHollow.Entities.NPC {
    [JsonObject(MemberSerialization.OptIn)]
    public class NPCAi {
        [JsonProperty]
        public Schedule SpringNormal;
        [JsonProperty]
        public Schedule SpringRain;

        [JsonProperty]
        public Schedule SummerNormal;
        [JsonProperty]
        public Schedule SummerRain;

        [JsonProperty]
        public Schedule FallNormal;
        [JsonProperty]
        public Schedule FallRain;

        [JsonProperty]
        public Schedule WinterNormal;
        [JsonProperty]
        public Schedule WinterSnow;

        [JsonProperty]
        public Schedule Holiday;
        [JsonProperty]
        public Schedule Birthday;
        [JsonProperty]
        public Schedule Default;


        public Schedule Current;
        public GoRogue.Pathing.Path CurrentPath;
        public bool UpdatePath = true;
        public int pathPos = 0;


        public void SetSchedule(string season, string weather) {
            if (season == "Spring") {
                if (weather == "Sunny") {
                    if (SpringNormal != null && SpringNormal.Nodes.Count > 0)
                        Current = new(SpringNormal);
                } else {
                    if (SpringRain != null && SpringRain.Nodes.Count > 0)
                        Current = new(SpringRain);
                } 
            } if (season == "Summer") {
                if (weather == "Sunny") {
                    if (SummerNormal != null && SummerNormal.Nodes.Count > 0)
                        Current = new(SummerNormal);
                } else {
                    if (SummerRain != null && SummerRain.Nodes.Count > 0)
                        Current = new(SummerRain);
                }
            }

            if (season == "Fall") {
                if (weather == "Sunny") {
                    if (FallNormal != null && FallNormal.Nodes.Count > 0)
                        Current = new(FallNormal);
                } else {
                    if (FallRain != null && FallRain.Nodes.Count > 0)
                        Current = new(FallRain);
                }
            }

            if (season == "Winter") {
                if (weather == "Sunny") {
                    if (WinterNormal != null && WinterNormal.Nodes.Count > 0)
                        Current = new(WinterNormal);
                } else {
                    if (WinterSnow != null && WinterSnow.Nodes.Count > 0)
                        Current = new(WinterSnow);
                }
            }

            if (season == "Holiday") { 
                if (Holiday != null && Holiday.Nodes.Count > 0)
                    Current = new(Holiday); 
            }
            if (season == "Birthday") { 
                if (Birthday != null && Birthday.Nodes.Count > 0)
                    Current = new(Birthday); 
            }

            if (Current == null) { Current = new(Default); }

            if (Current.Nodes != null) {
                Current.CurrentNode = 0;
                int CurrentTime = Int32.Parse(Current.Nodes[0].Split(";")[0]);
                int NextTime = Int32.Parse(Current.Nodes[0].Split(";")[0]);

                if (Current.Nodes.Count > 1) {
                    string[] NextNode = Current.Nodes[1].Split(";");
                    NextTime = Int32.Parse(NextNode[0]);
                }

                Current.NextNodeTime = Current.Nodes.Count > 1 ? NextTime : CurrentTime;
            }
        }



        public void UpdateNode(int CurrentTime) {
            if (Current.Nodes != null) {
                if (CurrentTime > Current.NextNodeTime - 10) {
                    if (Current.CurrentNode + 1 < Current.Nodes.Count) {
                        Current.CurrentNode++;
                        if (Current.CurrentNode + 1 < Current.Nodes.Count)
                            Current.NextNodeTime = Int32.Parse(Current.Nodes[Current.CurrentNode + 1].Split(";")[0]);
                    }
                }

                if (CurrentTime == -1) {
                    Current.CurrentNode = 0;
                    Current.NextNodeTime = Int32.Parse(Current.Nodes[Current.CurrentNode + 1].Split(";")[0]);
                }
            }
        }
         
    }
}
