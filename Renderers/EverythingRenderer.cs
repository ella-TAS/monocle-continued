using Microsoft.Xna.Framework.Graphics;

namespace Monocle {
    public class EverythingRenderer : Renderer {
        public BlendState BlendState;
        public SamplerState SamplerState;
        public Effect Effect;

        public EverythingRenderer() {
            BlendState = BlendState.AlphaBlend;
            SamplerState = SamplerState.LinearClamp;
        }

        public override void BeforeRender(Scene scene) {

        }

        public override void Render(Scene scene) {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState, DepthStencilState.None, RasterizerState.CullNone, Effect, scene.Camera.Matrix * Engine.ScreenMatrix);

            scene.Entities.Render();
            if (Engine.Commands.Open)
                scene.Entities.DebugRender(scene.Camera);

            Draw.SpriteBatch.End();
        }

        public override void AfterRender(Scene scene) {

        }
    }
}
