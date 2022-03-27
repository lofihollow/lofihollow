using System;
using System.Collections.Generic;
using System.Linq;
using LofiHollow.DataTypes;
using LofiHollow.EntityData;
using LofiHollow.Managers;
using LofiHollow.Minigames;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using Steamworks;

namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptOut)]
    public class Monster { 
        public string Species = ""; 
        public string Package = "";
        public string UniqueID;

        public bool BaseForm = false;
         
        public int Health = 1; 
        public int Speed = 1; 
        public int Attack = 1; 
        public int Defense = 1;  
        public int MAttack = 1; 
        public int MDefense = 1;

        public int HealthGrowth = 0;
        public int SpeedGrowth = 0;
        public int AttackGrowth = 0;
        public int DefenseGrowth = 0;
        public int MAttackGrowth = 0;
        public int MDefenseGrowth = 0;

        public int HealthEXP = 0;
        public int SpeedEXP = 0;
        public int AttackEXP = 0;
        public int DefenseEXP = 0;
        public int MAttackEXP = 0;
        public int MDefenseEXP = 0;

        public int Level = 0;

        public int CaptureRate = 45;
         
        public string StatGranted = "";

        public int EvolutionLevel = 10;
        public string EvolvesInto = "";
        public string AlternateEvoMethod = "";
         
        public string SpawnLocation = "";
        public bool SpawnsInWater = false;

        public string ElementalAlignment = "Earth";
        public List<string> Types = new();
        public List<string> MoveList = new(); 
        public List<string> DropTable = new();
          
        public int ForegroundR = 0; 
        public int ForegroundG = 0; 
        public int ForegroundB = 0; 
        public int ForegroundA = 255; 
        public string ActorGlyph = "?";  

        [JsonConstructor]
        public Monster() { 
        }

        public static Monster Clone(string name) {
            if (GameLoop.World.monsterLibrary != null && GameLoop.World.monsterLibrary.ContainsKey(name)) {
                Monster temp = Helper.Clone(GameLoop.World.monsterLibrary[name]); 
                temp.UniqueID = Guid.NewGuid().ToString("N");

                return temp;
            }

            return null;
        }

        public static Monster Clone(Monster other) { 
            Monster temp = Helper.Clone(other);
            temp.UniqueID = Guid.NewGuid().ToString("N");

            return temp;  
        }

        public void Evolve() {
            Monster evolved = Clone(EvolvesInto);
            StatGranted = evolved.StatGranted;
            MoveList = evolved.MoveList;
            Species = evolved.Species;
            EvolutionLevel = evolved.EvolutionLevel;
            EvolvesInto = evolved.EvolvesInto;
            AlternateEvoMethod = evolved.AlternateEvoMethod;
        }


        public static int GetStat(int input, int Level, int Growth, int StatEXP, bool HP = false) {
            int trainingBonus = (int) Math.Floor((float) StatEXP / 4f);
            if (HP) {
                int hp = (int)Math.Floor(0.01 * ((2 * input) + Growth + trainingBonus + Level)) + Level + 10;
                return hp;
            }
            else {
                int stat = (int)Math.Floor(0.01 * ((2 * input) + Growth + trainingBonus + Level) + 5);
                return stat;
            }
        }

        public void GainStatEXP(string which, int amount) {
            int total = HealthEXP + SpeedEXP + AttackEXP + DefenseEXP + MAttackEXP + MDefenseEXP;


            int applying = total + amount > 512 ? (total + amount) - 512 : amount;

            switch (which) {
                case "Health": HealthEXP += applying; break;
                case "Speed": SpeedEXP += applying; break;
                case "Attack": AttackEXP += applying; break;
                case "Defense": DefenseEXP += applying; break;
                case "Magic Attack": MAttackEXP += applying; break; 
                case "Magic Defense": MDefenseEXP += applying; break;
            } 
        }

        public CombatParticipant GetCombatParticipant(SteamId Owner, int level, string name) {
            CombatParticipant mon = new();
            mon.Level = level;
            mon.Owner = Owner;
            mon.Name = name;
            mon.StatGranted = StatGranted;
            mon.ID = UniqueID;

            mon.ForeR = ForegroundR;
            mon.ForeG = ForegroundG;
            mon.ForeB = ForegroundB;
            mon.ForeA = ForegroundA;
            mon.Glyph = ActorGlyph[0];


            mon.Attack = GetStat(Attack, mon.Level, AttackGrowth, AttackEXP);
            mon.Defense = GetStat(Defense, mon.Level, DefenseGrowth, DefenseEXP);
            mon.Speed = GetStat(Speed, mon.Level, SpeedGrowth, SpeedEXP);
            mon.Health = GetStat(Health, mon.Level, HealthGrowth, HealthEXP);
            mon.MAttack = GetStat(MAttack, mon.Level, MAttackGrowth, MAttackEXP);
            mon.MDefense = GetStat(MDefense, mon.Level, MDefenseGrowth, MDefenseEXP);

            mon.HealthGrowth = HealthGrowth;
            mon.SpeedGrowth = SpeedGrowth;
            mon.AttackGrowth = AttackGrowth;
            mon.DefenseGrowth = DefenseGrowth;
            mon.MAttackGrowth = MAttackGrowth;
            mon.MDefenseGrowth = MDefenseGrowth;


            mon.MaxHP = GetStat(mon.Health, mon.Level, HealthGrowth, HealthEXP, true);
            mon.CurrentHP = mon.MaxHP;

            mon.CaptureRate = CaptureRate;
            mon.Species = Species;

            mon.Types.Add(ElementalAlignment);

            for (int i = 0; i < Types.Count; i++) {
                mon.Types.Add(Types[i]);
            }

            mon.ContainedMon = this;

            return mon;
        }


        public ColoredString GetAppearance() {
            return new ColoredString(ActorGlyph, new Color(ForegroundR, ForegroundG, ForegroundB, ForegroundA), Color.Black);
        }

        public CellDecorator AsDecorator() {
            return new CellDecorator(new Color(ForegroundR, ForegroundG, ForegroundB, ForegroundA), ActorGlyph[0], Mirror.Horizontal);
        } 

        public Decorator AsDec() {
            Decorator dec = new();
            dec.R = ForegroundR;
            dec.G = ForegroundG;
            dec.B = ForegroundB;
            dec.A = ForegroundA;
            dec.Glyph = ActorGlyph[0];

            return dec;
        }

        public string FullName() {
            return Package + ":" + Species;
        }
    }
}
