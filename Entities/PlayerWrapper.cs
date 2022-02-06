using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Entities {
    public class PlayerWrapper : Entity {
        public Player player;

        public PlayerWrapper(Player play) : base(Color.White, Color.Transparent, '@') {
            player = play;
            Appearance.Foreground = new Color(player.ForegroundR, player.ForegroundG, player.ForegroundB);
            Appearance.Glyph = player.ActorGlyph;
        }

        public int TakeDamage(int damage) {
            int damageTaken = 0;
            if (damage > player.CurrentHP) {
                damageTaken = player.CurrentHP;
                player.CurrentHP = 0;
            } else {
                player.CurrentHP -= damage;
                damageTaken = damage;
            }

            if (player.CurrentHP <= 0) {
                player.PlayerDied();
            }

            return damageTaken;
        }

        public void UpdateAppearance() {
            Appearance.Foreground = new(player.ForegroundR, player.ForegroundG, player.ForegroundB, player.ForegroundA);
            Appearance.Glyph = player.ActorGlyph;
        }
    }
}
