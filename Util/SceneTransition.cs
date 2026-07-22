using System;

namespace Monocle {
    public abstract class SceneTransition : Scene {
        public static bool Transitioning;

        public readonly Scene FromScene;
        public readonly Scene ToScene;
        public readonly float Duration;
        public RenderBuffer FromBuffer;
        public RenderBuffer ToBuffer;
        public float Timer;

        public float Progress => Timer / Duration;

        public SceneTransition(Scene fromScene, Scene toScene, float duration) {
            FromScene = fromScene;
            ToScene = toScene;
            Duration = duration;
        }

        public virtual void Start() {
            if (Transitioning) {
                return;
            }

            FromBuffer = new RenderBuffer();
            ToBuffer = new RenderBuffer();

            Engine.ReplaceSceneSilent(this);
            ToScene.Begin();
            Transitioning = true;
        }

        public virtual void Finish() {
            FromScene.End();
            Engine.ReplaceSceneSilent(ToScene);
            End();
        }

        public override void Update() {
            base.Update();
            FromScene.Update();
            ToScene.Update();

            Timer += Engine.DeltaTime;
            if (Progress >= 1f) {
                Timer = Duration;
                Finish();
            }
        }

        public override void Begin() {
            base.Begin();
            throw new NotImplementedException("Call Start() on SceneTransition instead of setting the Scene");
        }

        public override void End() {
            base.End();
            Transitioning = false;
            FromBuffer?.Dispose();
            ToBuffer?.Dispose();
        }

        public override void Render() {
            base.Render();

            FromBuffer.Resize(Engine.WindowWidth, Engine.WindowHeight);
            ToBuffer.Resize(Engine.WindowWidth, Engine.WindowHeight);

            Engine.Graphics.GraphicsDevice.SetRenderTarget(FromBuffer);
            Engine.Graphics.GraphicsDevice.Clear(Engine.ClearColor);
            FromScene.Render();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(ToBuffer);
            Engine.Graphics.GraphicsDevice.Clear(Engine.ClearColor);
            ToScene.Render();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
            Engine.Graphics.GraphicsDevice.Clear(Engine.ClearColor);
        }

        public override void BeforeUpdate() {
            base.BeforeUpdate();
            FromScene.BeforeUpdate();
            ToScene.BeforeUpdate();
        }

        public override void AfterUpdate() {
            base.AfterUpdate();
            FromScene.AfterUpdate();
            ToScene.AfterUpdate();
        }

        public override void BeforeRender() {
            base.BeforeRender();
            FromScene.BeforeRender();
            ToScene.BeforeRender();
        }

        public override void AfterRender() {
            base.AfterRender();
            FromScene.AfterRender();
            ToScene.AfterRender();
        }

        public override void HandleGraphicsReset() {
            base.HandleGraphicsReset();
            FromScene.HandleGraphicsReset();
            ToScene.HandleGraphicsReset();
        }

        public override void HandleGraphicsCreate() {
            base.HandleGraphicsCreate();
            FromScene.HandleGraphicsCreate();
            ToScene.HandleGraphicsCreate();
        }

        public override void GainFocus() {
            base.GainFocus();
            FromScene.GainFocus();
            ToScene.GainFocus();
        }

        public override void LoseFocus() {
            base.LoseFocus();
            FromScene.LoseFocus();
            ToScene.LoseFocus();
        }
    }
}
