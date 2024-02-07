using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Entities {
    public class CombatStat {
        public string Name = "";
        public int Value;
        public int Exp;
        public int Growth;
        public int Stage;
        public int FlatBonus;

        public int StatWithStage(int Level) {
            float v = (float)Value;
            float e = (float)Exp;
            float g = (float)Growth;
            float l = (float)Level;
            double f = 0f;
            if (Name == "Health")
                f = (Math.Floor(((2 * v + g + Math.Floor(e / 4f)) * l) / 100f)) + Level + 10;
            else
                f = (Math.Floor(((2 * v + g + Math.Floor(e / 4f)) * l) / 100f)) + 5;

            return (int) Math.Ceiling(f * GetStatusMultiplier()) + FlatBonus; 
        }

        public float GetStatusMultiplier() {
            if (Stage < 0)
                return 3 / (float)(3 + (-1 * Stage));
            else
                return (3 + Stage) / 3f; 
        }

        public int StatusChange(int amount) {
            int changed = 0;

            if (amount > 0) {
                if (Stage < 6) {
                    if (Stage + amount <= 6) {
                        Stage = Stage + amount;
                        changed = amount;
                    }
                    else if (Stage + amount > 6) {
                        changed = 6 - Stage;
                        Stage = 6;
                    }
                }
                else if (Stage == 6) {
                    changed = 99;
                } 
            }
            else if (amount < 0) {  
                if (Stage > -6) {
                    if (Stage + amount >= -6) {
                        Stage = Stage + amount;
                        changed = amount;
                    }
                    else if (Stage + amount < -6) {
                        changed = -6 + Stage;
                        Stage = -6;
                    }
                }
                else if (Stage == -6) {
                    changed = -99;
                } 
            }

            return changed;
        }
    }
}
