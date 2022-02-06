using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Entities {
    public class NPCWrapper : Entity {
        public NPC.NPC npc;

        public NPCWrapper(NPC.NPC wrap) : base(Color.White, Color.Transparent, 32) {
            npc = wrap;
            Appearance.Foreground = new Color(npc.ForegroundR, npc.ForegroundG, npc.ForegroundB, npc.ForegroundA);
            Appearance.Glyph = npc.ActorGlyph;

            Position = npc.Position;
        }

        public void Update(bool newSchedule) {
            if (newSchedule || npc.AI.Current == null) {
                string season = GameLoop.World.Player.player.Clock.GetSeason();

                if (GameLoop.World.Player.player.Clock.IsItThisDay(npc.BirthMonth, npc.BirthDay))
                    season = "Birthday";


                npc.AI.SetSchedule(season, "Sunny");
            }

            Point oldPos = new(Position.X, Position.Y);
            npc.AI.MoveTowardsNode(GameLoop.World.Player.player.Clock.GetCurrentTime(), this);

            if (oldPos != Position) {
                NetMsg msg = new("moveNPC", null);
                msg.SetPosition(npc.Position);
                msg.SetMapPos(npc.MapPos);
                msg.MiscString = npc.Name;
                GameLoop.SendMessageIfNeeded(msg, true, false);
            }

            if (Position != npc.Position) {
                Position = npc.Position;
            }
        }
    }
}
