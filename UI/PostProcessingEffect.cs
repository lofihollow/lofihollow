using LofiHollow.Properties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadConsole.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.UI {
    public class PostProcessingEffect : DrawableGameComponent {
        public PostProcessingEffect() : base(SadConsole.Game.Instance.MonoGameInstance) {
            DrawOrder = 6;

            Effect effect = new Effect(SadConsole.Game.Instance.MonoGameInstance.GraphicsDevice, Resources.crt_lottes_mg);

        }

        //When we need to draw to the screen, it's done here.
        public override void Draw(GameTime gameTime) {
           
        }
    }
}
