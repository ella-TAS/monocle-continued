using Microsoft.Xna.Framework.Graphics;

namespace Monocle {
    public class SingleTagRenderer : Renderer {
        public BitTag Tag;
        public BlendState BlendState;
        public SamplerState SamplerState;
        public Effect Effect;

        public SingleTagRenderer(BitTag tag) {
            Tag = tag;
            BlendState = BlendState.AlphaBlend;
            SamplerState = SamplerState.LinearClamp;
        }

        public override void BeforeRender(Scene scene) {

        }

        public override void Render(Scene scene) {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState, DepthStencilState.None, RasterizerState.CullNone, Effect, scene.Camera.Matrix * Engine.ScreenMatrix);

            foreach (var entity in scene[Tag])
                if (entity.Visible)
                    entity.Render();

            if (Engine.Commands.Open)
                foreach (var entity in scene[Tag])
                    entity.DebugRender(scene.Camera);

            Draw.SpriteBatch.End();
        }

        public override void AfterRender(Scene scene) {

        }
    }
}
