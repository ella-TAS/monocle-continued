using System.Diagnostics.CodeAnalysis;

namespace Monocle {
    /// <summary>
    /// Base class for all components that can be attached to entities.
    /// </summary>
    /// <remarks>
    /// Components provide modular functionality to entities through composition.
    /// Each component has a lifecycle managed by its parent entity and scene.
    /// </remarks>
    public class Component {
        /// <summary>
        /// Gets the entity this component is attached to, or null if not attached.
        /// </summary>
        public Entity? Entity { get; private set; }

        /// <summary>
        /// Gets or sets whether this component is active and should be updated.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets whether this component is visible and should be rendered.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Initializes a new instance of the Component class.
        /// </summary>
        /// <param name="active">Whether the component should be active initially.</param>
        /// <param name="visible">Whether the component should be visible initially.</param>
        public Component(bool active, bool visible) {
            Active = active;
            Visible = visible;
        }

        /// <summary>
        /// Called when this component is added to an entity.
        /// </summary>
        /// <param name="entity">The entity this component was added to.</param>
        public virtual void Added(Entity entity) {
            Entity = entity;
            Scene?.Tracker.ComponentAdded(this);
        }

        /// <summary>
        /// Called when this component is removed from an entity.
        /// </summary>
        /// <param name="entity">The entity this component was removed from.</param>
        public virtual void Removed(Entity entity) {
            Scene?.Tracker.ComponentRemoved(this);
            Entity = null;
        }

        /// <summary>
        /// Called when the parent entity is added to a scene.
        /// </summary>
        /// <param name="scene">The scene the entity was added to.</param>
        public virtual void EntityAdded(Scene scene) {
            scene.Tracker.ComponentAdded(this);
        }

        /// <summary>
        /// Called when the parent entity is removed from a scene.
        /// </summary>
        /// <param name="scene">The scene the entity was removed from.</param>
        public virtual void EntityRemoved(Scene scene) {
            scene.Tracker.ComponentRemoved(this);
        }

        /// <summary>
        /// Called when the scene containing this component is ending.
        /// </summary>
        /// <param name="scene">The scene that is ending.</param>
        public virtual void SceneEnd(Scene scene) {
            // Override in derived classes to perform cleanup
        }

        /// <summary>
        /// Called when the parent entity becomes awake/active.
        /// </summary>
        public virtual void EntityAwake() {
            // Override in derived classes to respond to entity awakening
        }

        /// <summary>
        /// Called every frame to update this component's logic.
        /// </summary>
        /// <remarks>
        /// Only called when the component is Active.
        /// </remarks>
        public virtual void Update() {
            // Override in derived classes to implement update logic
        }

        /// <summary>
        /// Called every frame to render this component.
        /// </summary>
        /// <remarks>
        /// Only called when the component is Visible.
        /// </remarks>
        public virtual void Render() {
            // Override in derived classes to implement rendering
        }

        /// <summary>
        /// Called to render debug information for this component.
        /// </summary>
        /// <param name="camera">The camera to use for debug rendering.</param>
        public virtual void DebugRender(Camera camera) {
            // Override in derived classes to implement debug rendering
        }

        /// <summary>
        /// Called when the graphics device is reset.
        /// </summary>
        /// <remarks>
        /// Use this to recreate any graphics resources that need to be rebuilt.
        /// </remarks>
        public virtual void HandleGraphicsReset() {
            // Override in derived classes to handle graphics reset
        }

        /// <summary>
        /// Called when the graphics device is created.
        /// </summary>
        /// <remarks>
        /// Use this to initialize graphics resources.
        /// </remarks>
        public virtual void HandleGraphicsCreate() {
            // Override in derived classes to handle graphics creation
        }

        /// <summary>
        /// Removes this component from its parent entity.
        /// </summary>
        public void RemoveSelf() {
            Entity?.Remove(this);
        }

        /// <summary>
        /// Gets the current scene cast to the specified type.
        /// </summary>
        /// <typeparam name="T">The scene type to cast to.</typeparam>
        /// <returns>The scene cast to type T, or null if the cast fails.</returns>
        [return: MaybeNull]
        public T SceneAs<T>() where T : Scene {
            return Scene as T;
        }

        /// <summary>
        /// Gets the parent entity cast to the specified type.
        /// </summary>
        /// <typeparam name="T">The entity type to cast to.</typeparam>
        /// <returns>The entity cast to type T, or null if the cast fails.</returns>
        [return: MaybeNull]
        public T EntityAs<T>() where T : Entity {
            return Entity as T;
        }

        /// <summary>
        /// Gets the scene that contains this component's entity.
        /// </summary>
        /// <returns>The scene containing the parent entity, or null if not in a scene.</returns>
        public Scene? Scene => Entity?.Scene;
    }
}
