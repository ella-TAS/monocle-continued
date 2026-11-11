using Microsoft.Xna.Framework;
using System;

namespace Monocle {
    public class SceneEntity<S> : Entity where S : Scene {
        public SceneEntity(Vector2 position) : base(position) { }

        public SceneEntity() { }

        public new S Scene => (S) base.Scene!;

        public override void Added(Scene scene) {
            if (scene is not S) {
                throw new Exception("Cannot add SceneEntity to Scene of wrong type.");
            }

            base.Added(scene);
        }
    }
}
