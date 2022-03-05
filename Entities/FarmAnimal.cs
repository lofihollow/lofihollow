using LofiHollow.DataTypes;
using LofiHollow.EntityData;
using Newtonsoft.Json;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptIn)]
    public class FarmAnimal : Actor {
        [JsonProperty]
        public string Species = "";
        [JsonProperty]
        public string Package = "";
        [JsonProperty]
        public string Nickname = "";
        [JsonProperty]
        public string MilkItem = ""; 
        [JsonProperty]
        public string ShearItem = ""; 
        [JsonProperty]
        public int Happiness = 50;
        [JsonProperty]
        public int Relationship = 0;
        [JsonProperty]
        public int Age = 0;
        [JsonProperty]
        public int AdultAge = 0; 
        [JsonProperty]
        public bool Sick = false;
        [JsonProperty]
        public bool Pet = false;
        [JsonProperty]
        public Point RestSpot;

        public bool FedToday = false;
        public bool ShearedToday = false;
        public bool MilkedToday = false;
        public bool PattedToday = false;
        public bool BrushedToday = false;

       
        public GoRogue.Pathing.Path CurrentPath;
        public int pathPos = 0;

        [JsonConstructor]
        public FarmAnimal(Color foreground, int glyph) : base(foreground, glyph) {
        }

        public Item ToItem(string animalType) {
            SpawnAnimal ani = new(this);
            Item farm = new();
            farm.ItemGlyph = ani.Glyph;
            farm.ForegroundR = ani.R;
            farm.ForegroundG = ani.G;
            farm.ForegroundB = ani.B;
            farm.SpawnAnimal = ani;
            farm.ItemCat = animalType;
            farm.Name = ani.Packaged.Nickname; 

            return farm;
        }


        public bool Milkable() {
            return (Age >= AdultAge) && !MilkedToday && Happiness >= 50 && MilkItem != "";
        }

        public bool Shearable() {
            return (Age >= AdultAge) && !ShearedToday && Happiness >= 50 && ShearItem != "";
        }

        public void Brush() {
            if (!BrushedToday) {
                BrushedToday = true;
                Happiness = Math.Clamp(Happiness + 10, 0, 100);

                if (Happiness > 50) {
                    Relationship = Math.Clamp(Relationship + 5, 0, 100);
                }

                string feeling;

                if (Happiness < 25)
                    feeling = "sad";
                else if (Happiness < 50)
                    feeling = "bored";
                else if (Happiness < 75)
                    feeling = "happy";
                else
                    feeling = "ecstatic";

                if (Nickname != Species)
                    GameLoop.UIManager.AddMsg("You brush " + Nickname + ". They seem " + feeling + ".");
                else
                    GameLoop.UIManager.AddMsg("You brush the " + Nickname + ". They seem " + feeling + ".");
            }
        }

        public void Pat() {
            if (!PattedToday) {
                PattedToday = true;
                Happiness = Math.Clamp(Happiness + 10, 0, 100);

                if (Happiness > 50) {
                    Relationship = Math.Clamp(Relationship + 5, 0, 100);
                }
            }

            string feeling;

            if (Happiness < 25)
                feeling = "sad";
            else if (Happiness < 50)
                feeling = "bored";
            else if (Happiness < 75)
                feeling = "happy";
            else
                feeling = "ecstatic";

            if (Nickname != Species)
                GameLoop.UIManager.AddMsg("You pat " + Nickname + ". They seem " + feeling + ".");
            else
                GameLoop.UIManager.AddMsg("You pat the " + Nickname + ". They seem " + feeling + ".");
        }

        public void Update() {
            if (Name == "") {
                Name = Nickname;
            }

            if (TimeLastActed + 2000 < SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;

                int CurrentTime = GameLoop.World.Player.Clock.GetCurrentTime();
                if (CurrentTime > 360 && CurrentTime < 1260) { // Wander around
                    bool moved = false;

                    List<Point> dirs = new();

                    dirs.Add(new(0, 0));

                    if (Position.X > 1) {
                        dirs.Add(new(-1, 0));

                        if (Position.Y > 1)
                            dirs.Add(new(-1, -1));
                        if (Position.Y < GameLoop.MapHeight - 1)
                            dirs.Add(new(-1, 1));
                            
                    }

                    if (Position.X < GameLoop.MapWidth - 1) {
                        dirs.Add(new(1, 0));

                        if (Position.Y > 1)
                            dirs.Add(new(1, -1));
                        if (Position.Y < GameLoop.MapHeight - 1)
                            dirs.Add(new(1, 1));
                    }

                    if (Position.Y > 1)
                        dirs.Add(new(0, -1));
                    if (Position.Y < GameLoop.MapHeight - 1)
                        dirs.Add(new(0, 1)); 

                    while (!moved) {
                        Point move = dirs[GameLoop.rand.Next(dirs.Count)];

                        moved = MoveBy(move);
                    }
                }
                else {
                    if (Position == RestSpot)
                        return;

                    if (CurrentTime > 1290) {
                        if (GameLoop.World.Player.MapPos != MapPos) {
                            Position = RestSpot;
                        }
                    }

                    if (CurrentPath == null) {
                        Map map = Helper.ResolveMap(MapPos);

                        if (map != null) {
                            CurrentPath = map.MapPath.ShortestPath(Position.ToCoord(), RestSpot.ToCoord());
                            pathPos = 0;
                        }
                    }


                    if (CurrentPath != null) {
                        if (CurrentPath.LengthWithStart > pathPos) {
                            GoRogue.Coord nextStep = CurrentPath.GetStepWithStart(pathPos);

                            if (MoveTo(new Point(nextStep.X, nextStep.Y), MapPos))
                                pathPos++;
                        }

                        if (Position == RestSpot) {
                            CurrentPath = null;
                            pathPos = 0;
                        }
                    }
                }
            }
        }


        public string FullName() {
            return Package + ":" + Species;
        }
    }
}
