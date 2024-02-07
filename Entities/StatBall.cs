using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using LofiHollow.EntityData;
using System.Text;
using LofiHollow.DataTypes;
using LofiHollow.Managers; 


namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class StatBall {
        public string Name = "";
        public int CurrentHP = 10; 
        public int MaxHP = 10;
         
        public int CurrentStamina = 100; 
        public int MaxStamina = 100;
            
        public int foreR = 255;
        public int foreG = 255;
        public int foreB = 255; 
         
        public int CombatLevel = 1; 
         
        public string NavLoc;
        public string RespawnLoc;

        public Item? EquippedLeftHand;
        public Item? EquippedRightHand;
        public Item? EquippedHelmet;
        public Item? EquippedTorso;
        public Item? EquippedLegs;
        public Item? EquippedGloves;
        public Item? EquippedBoots;
        public Item? EquippedRing;
        public Item? EquippedAmulet;
        public Item? EquippedBack;

        public Dictionary<string, Skill> Skills = new();
        public List<string> ClassSkills = new();  

        public void CombatExp(int damage, string weaponType) {
            TryAddExp(damage * 4, weaponType); 
            TryAddExp(damage * 2, "Hitpoints"); 
        } 

        public void TryAddExp(int amt, string name) {
            if (Skills.ContainsKey(name)) {
                Skills[name].GrantExp(amt);
            }
        }

        protected StatBall(Color foreground) {
            foreR = foreground.R;
            foreG = foreground.G;
            foreB = foreground.B;

            MaxHP = 10;
            CurrentHP = MaxHP;

            MaxStamina = 100;
            CurrentStamina = MaxStamina; 
        }

        public Color GetColor() {
            return new Color(foreR, foreG, foreB);
        }

        public ColoredString GetAppearance() {
            return new ColoredString("@", GetColor(), Color.Transparent);
        }  

        public int TakeDamage(int damage) {
            int currHp = CurrentHP;

            CurrentHP = Math.Clamp(CurrentHP - damage, 0, MaxHP);

            int damageTaken = currHp - CurrentHP;

            if (damageTaken > 0)
                ArmorExp(damageTaken);

            if (CurrentHP <= 0) {
                if (this is Player player)
                    player.PlayerDied();
                else
                    Death(true);
            }

            return damageTaken;
        }

        public void ArmorExp(int dmg) {

        }
             
        public void Death(bool drops = true) {  
            GameLoop.UIManager.AddMsg(this.Name + " died."); 
        }

        public void ExpendStamina(int amount) {
            CurrentStamina -= amount;
            if (CurrentStamina < 0) {
                if (this == GameLoop.World.Player) { 
                    GameLoop.World.Player.Clock.NextDay(true);  
                }
            }
        }
    }
}
