using LofiHollow.DataTypes;
using LofiHollow.Entities;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Photo {
    [JsonObject(MemberSerialization.OptOut)]
    public class SoulPhoto {
        public Monster Contained1; 
        public Monster Contained2;
        public string MonsterName;
        public int Level = 1;
        public int Experience = 0;
        public int TotalExperience = 0;
        public Point Position;
        public List<Move> KnownMoves = new();

        [JsonConstructor]
        public SoulPhoto() { }

        public SoulPhoto(Monster mon, int level) {
            Contained1 = mon;
            MonsterName = mon.Species;

            for (int i = 0; i < level; i++) {
                TotalExperience += ExpToLevel();
                Level++;
            }
        }

        public void TryLearnMove(Move move, int replace = -1) {
            if (KnownMoves.Count < 4) {
                KnownMoves.Add(Helper.Clone(move));
                GameLoop.UIManager.AddMsg(new ColoredString(MonsterName + " learned " + move.Name + "!", Color.Lime, Color.Black));
            } else if (KnownMoves.Count == 4 && replace != -1) {
                string oldName = KnownMoves[replace].Name;
                KnownMoves[replace] = Helper.Clone(move);
                GameLoop.UIManager.AddMsg(new ColoredString(MonsterName + " forgot " + oldName + " and learned " + move.Name + "!", Color.Lime, Color.Black));
            } else if (KnownMoves.Count == 4 && replace == -1) {
                // PromptMoveChange();
                GameLoop.UIManager.AddMsg("Still have to implement move replacing");
            }
        }

        public void TryEvolve(string method = "") {
            bool EvolveSuccess = false;
            bool FusionSuccess = false;
            string old1 = "";
            string old2 = "";

            if (Contained1.EvolvesInto != "") {
                if (Level >= Contained1.EvolutionLevel || (Contained1.AlternateEvoMethod != "" && Contained1.AlternateEvoMethod.Contains(method))) {
                    if (GameLoop.World.monsterLibrary.ContainsKey(Contained1.EvolvesInto)) {
                        old1 = Contained1.Species;

                        Monster evolved = Monster.Clone(Contained1.EvolvesInto);
                        Contained1.StatGrowths = evolved.StatGrowths;
                        Contained1.MoveList = evolved.MoveList;
                        Contained1.Species = evolved.Species;
                        Contained1.EvolutionLevel = evolved.EvolutionLevel;
                        Contained1.EvolvesInto = evolved.EvolvesInto;
                        Contained1.AlternateEvoMethod = evolved.AlternateEvoMethod;
                        EvolveSuccess = true;
                    }
                }
            }

            if (Contained2.EvolvesInto != "") {
                if (Level >= Contained2.EvolutionLevel || (Contained2.AlternateEvoMethod != "" && Contained2.AlternateEvoMethod.Contains(method))) {
                    if (GameLoop.World.monsterLibrary.ContainsKey(Contained2.EvolvesInto)) {
                        old2 = Contained2.Species;

                        Monster evolved = Monster.Clone(Contained2.EvolvesInto);
                        Contained2.StatGrowths = evolved.StatGrowths;
                        Contained2.MoveList = evolved.MoveList;
                        Contained2.Species = evolved.Species;
                        Contained2.EvolutionLevel = evolved.EvolutionLevel;
                        Contained2.EvolvesInto = evolved.EvolvesInto;
                        Contained2.AlternateEvoMethod = evolved.AlternateEvoMethod;
                        EvolveSuccess = true;
                        FusionSuccess = true;
                    }
                }
            }

            if (EvolveSuccess) {
                if (Contained2 != null) {
                    GameLoop.UIManager.AddMsg(new ColoredString("The " + old1 + " half of " + MonsterName + " evolved into " + Contained1.Species, Color.Yellow, Color.Black));
                } else {
                    if (Contained1.Species.StartsWith("a") || Contained1.Species.StartsWith("e") || Contained1.Species.StartsWith("i")
                         || Contained1.Species.StartsWith("o") || Contained1.Species.StartsWith("u")) {
                        GameLoop.UIManager.AddMsg(new ColoredString(MonsterName + " evolved into an " + Contained1.Species, Color.Yellow, Color.Black)); 
                    } else { 
                        GameLoop.UIManager.AddMsg(new ColoredString(MonsterName + " evolved into a " + Contained1.Species, Color.Yellow, Color.Black));
                    }
                }



                if (GameLoop.SteamManager.UnlockAchievement("MONSTER_ANEW")) {
                    GameLoop.UIManager.AddMsg("Achievement: A journey half done!");
                }
            }

            if (FusionSuccess) {
                GameLoop.UIManager.AddMsg(new ColoredString("The " + old2 + " half of " + MonsterName + " evolved into " + Contained2.Species, Color.Yellow, Color.Black));
                if (GameLoop.SteamManager.UnlockAchievement("MONSTER_FUSION")) {
                    GameLoop.UIManager.AddMsg("Achievement: A journey half done!");
                }
            }
        }

        public void GetExperience(int gained) {
            Experience += gained;
            if (Experience >= ExpToLevel()) {
                TotalExperience += ExpToLevel();
                Experience -= ExpToLevel();
                Level++;
                GameLoop.UIManager.AddMsg(new ColoredString(MonsterName + " leveled " + Level + "!", Color.Cyan, Color.Black));

                if (Level == 92) {
                    if (GameLoop.SteamManager.UnlockAchievement("MONSTER_92")) {
                        GameLoop.UIManager.AddMsg("Achievement: A journey half done!");
                    } 
                }

                if (Level == 99) {
                    if (GameLoop.SteamManager.UnlockAchievement("MONSTER_99")) {
                        GameLoop.UIManager.AddMsg("Achievement: A journey completed!");
                    }
                }


                TryEvolve();

                for (int i = 0; i < Contained1.MoveList.Count; i++) {
                    string[] splitMove = Contained1.MoveList[i].Split(",");
                    if (Level == Int32.Parse(splitMove[0])) { 
                        if (Contained1.Types.Contains(splitMove[1]) || splitMove[1] == "Any" || splitMove[1] == "") {
                            if (GameLoop.World.moveLibrary.ContainsKey(splitMove[2])) {
                                TryLearnMove(Helper.Clone(GameLoop.World.moveLibrary[splitMove[2]]));
                            }
                        }
                    }
                }

                if (Contained2 != null) {
                    for (int i = 0; i < Contained2.MoveList.Count; i++) {
                        string[] splitMove = Contained2.MoveList[i].Split(",");
                        if (Level == Int32.Parse(splitMove[0])) {
                            if (Contained2.Types.Contains(splitMove[1]) || splitMove[1] == "Any" || splitMove[1] == "") {
                                if (GameLoop.World.moveLibrary.ContainsKey(splitMove[2])) {
                                    TryLearnMove(Helper.Clone(GameLoop.World.moveLibrary[splitMove[2]]));
                                }
                            }
                        }
                    }
                }
            }
        }

        public int ExpToLevel() {
            double exp = 0.25 * Math.Floor(Level - 1.0 + 300.0 * (Math.Pow(2.0, (Level - 1.0) / 7.0)));
            return (int)Math.Floor(exp);
        }

        public string Name() {
            if (Contained2 != null)
                return Contained1.Species + "-" + Contained2.Species;
            return Contained1.Species;
        }

        public MonsterWrapper GetCombined() {
            if (Contained2 == null) {
                return new MonsterWrapper(Contained1);
            }

            Monster mon = Helper.Clone(Contained1);
            mon.Attack = Helper.Average(Contained1.Attack, Contained2.Attack);
            mon.Defense = Helper.Average(Contained1.Defense, Contained2.Defense);
            mon.Speed = Helper.Average(Contained1.Speed, Contained2.Speed);
            mon.Health = Helper.Average(Contained1.Health, Contained2.Health);
            mon.MAttack = Helper.Average(Contained1.MAttack, Contained2.MAttack);
            mon.MDefense = Helper.Average(Contained1.MDefense, Contained2.MDefense);
            
            for (int i = 0; i < Contained2.Types.Count; i++) {
                if (!mon.Types.Contains(Contained2.Types[i])) {
                    mon.Types.Add(Contained2.Types[i]);
                }
            }

            mon.Species += "-" + Contained2.Species;

            return new MonsterWrapper(mon);
        }

        public DecoratedString GetAppearance() {
            if (Contained1 != null && Contained2 != null) {
                CellDecorator[] decs = new CellDecorator[1];
                decs[0] = Contained2.AsDecorator();
                DecoratedString str = new(Contained1.GetAppearance(), decs);

                return str;
            } else {
                return new(Contained1.GetAppearance());
            }
        }
    }
}
