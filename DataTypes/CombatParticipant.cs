using LofiHollow.Entities;
using LofiHollow.EntityData;
using LofiHollow.Minigames.Photo;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using Steamworks;
using System.Collections.Generic;

namespace LofiHollow.DataTypes {
    public class CombatParticipant {
        public bool Enemy = false;
        public SteamId Owner;
        public string ID;
        public string Name;
        public string StatGranted; 

        public Monster ContainedMon;

        public List<Move> KnownMoves = new();

        public int ForeR;
        public int ForeG;
        public int ForeB;
        public int ForeA = 255;
        public int Glyph;
        public Decorator Dec;

        public int Attack;
        public int Defense;
        public int Health;
        public int Speed;
        public int MAttack;
        public int MDefense;

        public int HealthGrowth = 0;
        public int SpeedGrowth = 0;
        public int AttackGrowth = 0;
        public int DefenseGrowth = 0;
        public int MAttackGrowth = 0;
        public int MDefenseGrowth = 0;

        public int AtkStage = 0;
        public int DefStage = 0; 
        public int SpdStage = 0;
        public int MAtkStage = 0;
        public int MDefStage = 0;

        public int AtkBonus = 0;
        public int DefBonus = 0;
        public int SpdBonus = 0;
        public int MAtkBonus = 0;
        public int MDefBonus = 0;

        public int AccuracyStage = 0;
        public int EvasionStage = 0;

        public int CurrentHP;
        public int MaxHP;

        public int Level;
        public int CaptureRate;
        public string Species;

        public List<string> Types = new();

        [JsonConstructor]
        public CombatParticipant() {

        }

        public void UpdateHP() {
            MaxHP = Monster.GetStat(Health, Level, ContainedMon.HealthGrowth, ContainedMon.HealthEXP, true);
            CurrentHP = MaxHP;
        }

        public int StatWithStage(string which) { 
            if (which == "Speed") { return (int)System.Math.Ceiling((float) Speed * GetStatusMultiplier("Speed")) + SpdBonus; }
            if (which == "Attack") { return (int)System.Math.Ceiling((float) Attack * GetStatusMultiplier("Attack")) + AtkBonus; }
            if (which == "Defense") { return (int)System.Math.Ceiling((float)Defense * GetStatusMultiplier("Defense")) + DefBonus; }
            if (which == "Magic Attack") { return (int)System.Math.Ceiling((float)MAttack * GetStatusMultiplier("Magic Attack")) + MAtkBonus; }
            if (which == "Magic Defense") { return (int)System.Math.Ceiling((float) MDefense * GetStatusMultiplier("Magic Defense")) + MDefBonus; }

            return 0;
        }

        public void FlatChange(string which, int amount) {
            if (which == "Speed") { SpdBonus += amount; }
            if (which == "Attack") { AtkBonus += amount; }
            if (which == "Defense") { DefBonus += amount; }
            if (which == "Magic Attack") { MAtkBonus += amount; }
            if (which == "Magic Defense") { MDefBonus += amount; }
        }

        public int StatusChange(string which, int amount) {
            int changed = 0;

            if (amount > 0) {  
                // Gain speed stages
                if (which == "Speed" && SpdStage < 6) {
                    if (SpdStage + amount <= 6) {
                        SpdStage = SpdStage + amount;
                        changed = amount;
                    }
                    else if (SpdStage + amount > 6) {
                        changed = 6 - SpdStage;
                        SpdStage = 6;
                    }
                }
                else if (which == "Speed" && SpdStage == 6) {
                    changed = 99;
                }

                // Gain attack stages
                if (which == "Attack" && AtkStage < 6) {
                    if (AtkStage + amount <= 6) {
                        AtkStage = AtkStage + amount;
                        changed = amount;
                    }
                    else if (AtkStage + amount > 6) {
                        changed = 6 - AtkStage;
                        AtkStage = 6;
                    }
                } else if (which == "Attack" && AtkStage == 6) {
                    changed = 99;
                }

                // Gain defense stages
                if (which == "Defense" && DefStage < 6) {
                    if (DefStage + amount <= 6) {
                        DefStage = DefStage + amount;
                        changed = amount;
                    }
                    else if (DefStage + amount > 6) {
                        changed = 6 - DefStage;
                        DefStage = 6;
                    }
                }
                else if (which == "Defense" && DefStage == 6) {
                    changed = 99;
                }

                // Gain magic attack stages
                if (which == "Magic Attack" && MAtkStage < 6) {
                    if (MAtkStage + amount <= 6) {
                        MAtkStage = MAtkStage + amount;
                        changed = amount;
                    }
                    else if (MAtkStage + amount > 6) {
                        changed = 6 - MAtkStage;
                        MAtkStage = 6;
                    }
                }
                else if (which == "Magic Attack" && MAtkStage == 6) {
                    changed = 99;
                }

                // Gain defense stages
                if (which == "Magic Defense" && MDefStage < 6) {
                    if (MDefStage + amount <= 6) {
                        MDefStage = MDefStage + amount;
                        changed = amount;
                    }
                    else if (MDefStage + amount > 6) {
                        changed = 6 - MDefStage;
                        MDefStage = 6;
                    }
                }
                else if (which == "Magic Defense" && MDefStage == 6) {
                    changed = 99;
                }

                // Gain accuracy stages
                if (which == "Accuracy" && AccuracyStage < 6) {
                    if (AccuracyStage + amount <= 6) {
                        AccuracyStage = AccuracyStage + amount;
                        changed = amount;
                    }
                    else if (AccuracyStage + amount > 6) {
                        changed = 6 - AccuracyStage;
                        AccuracyStage = 6;
                    }
                }
                else if (which == "Accuracy" && AccuracyStage == 6) {
                    changed = 99;
                }

                // Gain evasion stages
                if (which == "Evasion" && EvasionStage < 6) {
                    if (EvasionStage + amount <= 6) {
                        EvasionStage = EvasionStage + amount;
                        changed = amount;
                    }
                    else if (EvasionStage + amount > 6) {
                        changed = 6 - EvasionStage;
                        EvasionStage = 6;
                    }
                }
                else if (which == "Evasion" && EvasionStage == 6) {
                    changed = 99;
                }
            } 
            else if (amount < 0) {  
                // Lose speed stages
                if (which == "Speed" && SpdStage > -6) {
                    if (SpdStage + amount >= -6) {
                        SpdStage = SpdStage + amount;
                        changed = amount;
                    }
                    else if (SpdStage + amount < -6) {
                        changed = -6 + SpdStage;
                        SpdStage = -6;
                    }
                }
                else if (which == "Speed" && SpdStage == -6) {
                    changed = -99;
                }

                // Lose attack stages
                if (which == "Attack" && AtkStage > -6) {
                    if (AtkStage + amount >= -6) {
                        AtkStage = AtkStage + amount;
                        changed = amount;
                    }
                    else if (AtkStage + amount < -6) {
                        changed = -6 + AtkStage;
                        AtkStage = -6;
                    }
                }
                else if (which == "Attack" && AtkStage == -6) {
                    changed = -99;
                }

                // Lose defense stages
                if (which == "Defense" && DefStage > -6) {
                    if (DefStage + amount >= -6) {
                        DefStage = DefStage + amount;
                        changed = amount;
                    }
                    else if (DefStage + amount < -6) {
                        changed = -6 + DefStage;
                        DefStage = -6;
                    }
                }
                else if (which == "Defense" && DefStage == -6) {
                    changed = -99;
                }

                // Lose magic attack stages
                if (which == "Magic Attack" && MAtkStage > -6) {
                    if (MAtkStage + amount >= -6) {
                        MAtkStage = MAtkStage + amount;
                        changed = amount;
                    }
                    else if (MAtkStage + amount < -6) {
                        changed = -6 + MAtkStage;
                        MAtkStage = -6;
                    }
                }
                else if (which == "Magic Attack" && MAtkStage == -6) {
                    changed = -99;
                }

                // Lose defense stages
                if (which == "Magic Defense" && MDefStage > -6) {
                    if (MDefStage + amount >= -6) {
                        MDefStage = MDefStage + amount;
                        changed = amount;
                    }
                    else if (MDefStage + amount < -6) {
                        changed = -6 + MDefStage;
                        MDefStage = -6;
                    }
                }
                else if (which == "Magic Defense" && MDefStage == -6) {
                    changed = -99;
                }

                // Lose accuracy stages
                if (which == "Accuracy" && AccuracyStage > -6) {
                    if (AccuracyStage + amount >= -6) {
                        AccuracyStage = AccuracyStage + amount;
                        changed = amount;
                    }
                    else if (AccuracyStage + amount < -6) {
                        changed = -6 + AccuracyStage;
                        AccuracyStage = -6;
                    }
                }
                else if (which == "Accuracy" && AccuracyStage == -6) {
                    changed = -99;
                }

                // Lose evasion stages
                if (which == "Evasion" && EvasionStage > -6) {
                    if (EvasionStage + amount >= -6) {
                        EvasionStage = EvasionStage + amount;
                        changed = amount;
                    }
                    else if (EvasionStage + amount < -6) {
                        changed = -6 + EvasionStage;
                        EvasionStage = -6;
                    }
                }
                else if (which == "Evasion" && EvasionStage == -6) {
                    changed = -99;
                }
            }

            return changed;
        }

        public float GetStatusMultiplier(string which) {
            int stage = 0;

            if (which == "Attack") { stage = AtkStage; }
            else if (which == "Defense") { stage = DefStage; } 
            else if (which == "Speed") { stage = SpdStage; }
            else if (which == "Magic Attack") { stage = MAtkStage; }
            else if (which == "Magic Defense") { stage = MDefStage; }
            else if (which == "Accuracy") { stage = AccuracyStage; }
            else if (which == "Evasion") { stage = EvasionStage; }

            if (stage == -6)
                return 3 / 9f;
            else if (stage == -5)
                return 3 / 8f;
            else if (stage == -4)
                return 3 / 7f;
            else if (stage == -3)
                return 3 / 6f;
            else if (stage == -2)
                return 3 / 5f;
            else if (stage == -1)
                return 3 / 4f;
            else if (stage == 0)
                return 3 / 3f;
            else if (stage == 1)
                return 4 / 3f;
            else if (stage == 2)
                return 5 / 3f;
            else if (stage == 3)
                return 6 / 3f;
            else if (stage == 4)
                return 7 / 3f;
            else if (stage == 5)
                return 8 / 3f;
            else if (stage == 6)
                return 9 / 3f;
            else return 1f;
        }


        public ColoredString GetAppearance() {
            return new ColoredString(Glyph.AsString(), new Color(ForeR, ForeG, ForeB, ForeA), Color.Black);
        }

        public void TakeDamage(int damage) {
            CurrentHP = MathHelpers.Clamp(CurrentHP - damage, 0, MaxHP);



            if (Owner != 0) { // Not a wild monster, have to find it
                if (Owner == SteamClient.SteamId) { // Only track damage for locally owned monsters
                    if (ID == "Player") { // It's the local player
                        GameLoop.World.Player.TakeDamage(damage);
                    } else { // Assume it's the equipped monster
                        if (GameLoop.World.Player.Equipment[10].SoulPhoto != null) {
                            SoulPhoto photo = GameLoop.World.Player.Equipment[10].SoulPhoto;
                        }
                    }                                    
                } 
            }
        }


        public void CombatExp(int damage) {
            if (Owner != 0) { // Not a wild monster, which don't need to get exp
                if (Owner == SteamClient.SteamId) { // Owner is local player
                    if (ID == "Player") { // This participant is actually the local player 
                        if (GameLoop.World.Player.CombatMode == "Attack") {
                            GameLoop.World.Player.Skills["Attack"].GrantExp(damage * 4);
                        }
                        else if (GameLoop.World.Player.CombatMode == "Strength") {
                            GameLoop.World.Player.Skills["Strength"].GrantExp(damage * 4);
                        }
                        else if (GameLoop.World.Player.CombatMode == "Defense") {
                            GameLoop.World.Player.Skills["Defense"].GrantExp(damage * 4);
                        }
                        else if (GameLoop.World.Player.CombatMode == "Balanced") {
                            GameLoop.World.Player.Skills["Attack"].GrantExp(damage);
                            GameLoop.World.Player.Skills["Strength"].GrantExp(damage);
                            GameLoop.World.Player.Skills["Defense"].GrantExp(damage);
                            GameLoop.World.Player.Skills["Constitution"].GrantExp(damage);
                        }

                        GameLoop.World.Player.Skills["Constitution"].GrantExp(damage * 2); 
                    } else { // Must be a monster owned by the local player
                        if (GameLoop.World.Player.Equipment[10] != null) {
                            SoulPhoto photo = GameLoop.World.Player.Equipment[10].SoulPhoto;
                            if (photo != null) {
                                photo.GetExperience(damage * 4);
                            }
                        }
                    }
                } else { // Must be a monster owned by another player

                }
            }
        }
    }
}
