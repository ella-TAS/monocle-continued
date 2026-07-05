#nullable enable
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Monocle {
    /// <summary>
    /// Represents a game object that can have components attached and participate in scenes.
    /// </summary>
    /// <remarks>
    /// Entities are the core building blocks of the Monocle engine. They can be positioned,
    /// have components for behavior, participate in collision detection, and be organized
    /// with tags for efficient querying.
    /// </remarks>
    public class Entity : IEnumerable<Component>, IEnumerable {
        /// <summary>
        /// Gets or sets whether this entity is active and should be updated.
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// Gets or sets whether this entity is visible and should be rendered.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets whether this entity can participate in collision detection.
        /// </summary>
        public bool Collidable { get; set; } = true;

        /// <summary>
        /// The position of this entity in world space.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Gets the scene that contains this entity, or null if not in a scene.
        /// </summary>
        public Scene? Scene { get; private set; }

        /// <summary>
        /// Gets the list of components attached to this entity.
        /// </summary>
        public ComponentList Components { get; private set; } = null!; // Assigned in constructor

        private int tag;
        private Collider? collider;
        internal int depth = 0;
        internal double actualDepth = 0;

        /// <summary>
        /// Initializes a new instance of the Entity class at the specified position.
        /// </summary>
        /// <param name="position">The initial position of the entity.</param>
        public Entity(Vector2 position) {
            Position = position;
            Components = new ComponentList(this);
        }

        /// <summary>
        /// Initializes a new instance of the Entity class at the origin.
        /// </summary>
        public Entity() : this(Vector2.Zero) {
        }

        /// <summary>
        /// Called when the containing scene begins.
        /// </summary>
        /// <param name="scene">The scene that is beginning.</param>
        public virtual void SceneBegin(Scene scene) {
            // Override in derived classes to respond to scene beginning
        }

        /// <summary>
        /// Called when the containing scene ends.
        /// </summary>
        /// <param name="scene">The scene that is ending.</param>
        public virtual void SceneEnd(Scene scene) {
            foreach (var component in Components)
                component.SceneEnd(scene);
        }

        /// <summary>
        /// Called before the frame starts, after entities are added and removed.
        /// </summary>
        /// <param name="scene">The scene containing this entity.</param>
        /// <remarks>
        /// This is called on the frame that the entity was added, after all entities
        /// have been added/removed but before they start updating. Useful when entities
        /// need to detect each other before the first update.
        /// </remarks>
        public virtual void Awake(Scene scene) {
            foreach (var component in Components)
                component.EntityAwake();
        }

        /// <summary>
        /// Called when this entity is added to a scene.
        /// </summary>
        /// <param name="scene">The scene this entity was added to.</param>
        /// <remarks>
        /// This occurs immediately before each update. Other entities may be added
        /// after this entity in the same frame. See Awake() for logic that should
        /// run after all entities are added but before updates begin.
        /// </remarks>
        public virtual void Added(Scene scene) {
            Scene = scene;
            foreach (var component in Components)
                component.EntityAdded(scene);
            Scene.SetActualDepth(this);
        }

        /// <summary>
        /// Called when the entity is removed from a scene.
        /// </summary>
        /// <param name="scene">The scene this entity was removed from.</param>
        public virtual void Removed(Scene scene) {
            foreach (var component in Components)
                component.EntityRemoved(scene);
            Scene = null;
        }

        /// <summary>
        /// Updates this entity's logic.
        /// </summary>
        /// <remarks>
        /// Called every frame when the entity is Active. Do game logic here,
        /// but do not perform rendering in this method.
        /// </remarks>
        public virtual void Update() {
            Components.Update();
        }

        /// <summary>
        /// Renders this entity.
        /// </summary>
        /// <remarks>
        /// Called every frame when the entity is Visible. Perform all
        /// drawing operations here.
        /// </remarks>
        public virtual void Render() {
            Components.Render();
        }

        /// <summary>
        /// Renders debug information for this entity.
        /// </summary>
        /// <param name="camera">The camera to use for debug rendering.</param>
        /// <remarks>
        /// Only called when the debug console is open. Called even when the
        /// entity is not Visible, allowing debugging of invisible entities.
        /// </remarks>
        public virtual void DebugRender(Camera camera) {
            Collider?.Render(camera, Collidable ? Color.Red : Color.DarkRed);
            Components.DebugRender(camera);
        }

        /// <summary>
        /// Called when the graphics device resets.
        /// </summary>
        /// <remarks>
        /// When this happens, any RenderTargets or other contents of VRAM will be
        /// wiped and need to be regenerated. Override in derived classes to
        /// recreate graphics resources.
        /// </remarks>
        public virtual void HandleGraphicsReset() {
            Components.HandleGraphicsReset();
        }

        /// <summary>
        /// Called when the graphics device is created.
        /// </summary>
        /// <remarks>
        /// Override in derived classes to initialize graphics resources.
        /// </remarks>
        public virtual void HandleGraphicsCreate() {
            Components.HandleGraphicsCreate();
        }

        /// <summary>
        /// Removes this entity from its containing scene.
        /// </summary>
        public void RemoveSelf() {
            Scene?.Entities.Remove(this);
        }

        /// <summary>
        /// Gets or sets the depth of this entity for rendering order.
        /// </summary>
        /// <remarks>
        /// Lower values are rendered first (behind), higher values are rendered
        /// last (in front). Changing this value updates the scene's depth sorting.
        /// </remarks>
        public int Depth {
            get => depth;
            set {
                if (depth != value) {
                    depth = value;
                    Scene?.SetActualDepth(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the X coordinate of this entity's position.
        /// </summary>
        public float X {
            get => Position.X;
            set => Position = new Vector2(value, Position.Y);
        }

        /// <summary>
        /// Gets or sets the Y coordinate of this entity's position.
        /// </summary>
        public float Y {
            get => Position.Y;
            set => Position = new Vector2(Position.X, value);
        }

        #region Collider

        /// <summary>
        /// Gets or sets the collider for this entity.
        /// </summary>
        /// <remarks>
        /// The collider defines the shape used for collision detection.
        /// Setting this to null disables collision for this entity.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to set a collider that is already in use by another entity.
        /// </exception>
        public Collider? Collider {
            get => collider;
            set {
                if (ReferenceEquals(value, collider))
                    return;
#if DEBUG
                if (value?.Entity != null)
                    throw new InvalidOperationException("Cannot assign a collider that is already in use by another entity.");
#endif
                collider?.Removed();
                collider = value;
                collider?.Added(this);
            }
        }

        /// <summary>
        /// Gets the width of this entity's collider, or 0 if no collider is set.
        /// </summary>
        public float Width => collider?.Width ?? 0f;

        /// <summary>
        /// Gets the height of this entity's collider, or 0 if no collider is set.
        /// </summary>
        public float Height => collider?.Height ?? 0f;

        /// <summary>
        /// Gets or sets the left edge of this entity's collider bounds.
        /// </summary>
        public float Left {
            get => collider?.Left + Position.X ?? X;
            set => Position = new Vector2(value - (collider?.Left ?? 0f), Position.Y);
        }

        /// <summary>
        /// Gets or sets the right edge of this entity's collider bounds.
        /// </summary>
        public float Right {
            get => collider?.Right + Position.X ?? Position.X;
            set => Position = new Vector2(value - (collider?.Right ?? 0f), Position.Y);
        }

        /// <summary>
        /// Gets or sets the top edge of this entity's collider bounds.
        /// </summary>
        public float Top {
            get => collider?.Top + Position.Y ?? Position.Y;
            set => Position = new Vector2(Position.X, value - (collider?.Top ?? 0f));
        }

        /// <summary>
        /// Gets or sets the bottom edge of this entity's collider bounds.
        /// </summary>
        public float Bottom {
            get => collider?.Bottom + Position.Y ?? Position.Y;
            set => Position = new Vector2(Position.X, value - (collider?.Bottom ?? 0f));
        }

        /// <summary>
        /// Gets or sets the horizontal center of this entity's collider bounds.
        /// </summary>
        public float CenterX {
            get => collider?.CenterX + Position.X ?? Position.X;
            set => Position = new Vector2(value - (collider?.CenterX ?? 0f), Position.Y);
        }

        /// <summary>
        /// Gets or sets the vertical center of this entity's collider bounds.
        /// </summary>
        public float CenterY {
            get => collider?.CenterY + Position.Y ?? Position.Y;
            set => Position = new Vector2(Position.X, value - (collider?.CenterY ?? 0f));
        }

        /// <summary>
        /// Gets or sets the top-left corner of this entity's collider bounds.
        /// </summary>
        public Vector2 TopLeft {
            get => new(Left, Top);
            set => (Left, Top) = (value.X, value.Y);
        }

        /// <summary>
        /// Gets or sets the top-right corner of this entity's collider bounds.
        /// </summary>
        public Vector2 TopRight {
            get => new(Right, Top);
            set => (Right, Top) = (value.X, value.Y);
        }

        /// <summary>
        /// Gets or sets the bottom-left corner of this entity's collider bounds.
        /// </summary>
        public Vector2 BottomLeft {
            get => new(Left, Bottom);
            set => (Left, Bottom) = (value.X, value.Y);
        }

        /// <summary>
        /// Gets or sets the bottom-right corner of this entity's collider bounds.
        /// </summary>
        public Vector2 BottomRight {
            get => new(Right, Bottom);
            set => (Right, Bottom) = (value.X, value.Y);
        }

        /// <summary>
        /// Gets or sets the center point of this entity's collider bounds.
        /// </summary>
        public Vector2 Center {
            get => new(CenterX, CenterY);
            set => (CenterX, CenterY) = (value.X, value.Y);
        }

        /// <summary>
        /// Gets or sets the center-left point of this entity's collider bounds.
        /// </summary>
        public Vector2 CenterLeft {
            get => new(Left, CenterY);
            set => (Left, CenterY) = (value.X, value.Y);
        }

        /// <summary>
        /// Gets or sets the center-right point of this entity's collider bounds.
        /// </summary>
        public Vector2 CenterRight {
            get => new(Right, CenterY);
            set => (Right, CenterY) = (value.X, value.Y);
        }

        /// <summary>
        /// Gets or sets the top-center point of this entity's collider bounds.
        /// </summary>
        public Vector2 TopCenter {
            get => new(CenterX, Top);
            set => (CenterX, Top) = (value.X, value.Y);
        }

        /// <summary>
        /// Gets or sets the bottom-center point of this entity's collider bounds.
        /// </summary>
        public Vector2 BottomCenter {
            get => new(CenterX, Bottom);
            set => (CenterX, Bottom) = (value.X, value.Y);
        }

        #endregion

        #region Tag

        public int Tag {
            get {
                return tag;
            }

            set {
                if (tag != value) {
                    if (Scene != null) {
                        for (int i = 0; i < Monocle.BitTag.TotalTags; i++) {
                            int check = 1 << i;
                            bool add = (value & check) != 0;
                            bool has = (Tag & check) != 0;

                            if (has != add) {
                                if (add)
                                    Scene.TagLists[i].Add(this);
                                else
                                    Scene.TagLists[i].Remove(this);
                            }
                        }
                    }

                    tag = value;
                }
            }
        }

        public bool TagFullCheck(int tag) {
            return (this.tag & tag) == tag;
        }

        public bool TagCheck(int tag) {
            return (this.tag & tag) != 0;
        }

        public void AddTag(int tag) {
            Tag |= tag;
        }

        public void RemoveTag(int tag) {
            Tag &= ~tag;
        }

        #endregion

        #region Collision Shortcuts

        #region Collide Check

        public bool CollideCheck(Entity other) {
            return Collide.Check(this, other);
        }

        public bool CollideCheck(Entity other, Vector2 at) {
            return Collide.Check(this, other, at);
        }

        public bool CollideCheck(CollidableComponent other) {
            return Collide.Check(this, other);
        }

        public bool CollideCheck(CollidableComponent other, Vector2 at) {
            return Collide.Check(this, other, at);
        }

        public bool CollideCheck(BitTag tag) {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against a tag list when it is not a member of a Scene");
#endif
            return Collide.Check(this, Scene?[tag] ?? []);
        }

        public bool CollideCheck(BitTag tag, Vector2 at) {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against a tag list when it is not a member of a Scene");
#endif
            return Collide.Check(this, Scene?[tag] ?? [], at);
        }

        public bool CollideCheck<T>() where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif

            return Collide.Check(this, Scene?.Tracker.Entities[typeof(T)] ?? []);
        }

        public bool CollideCheck<T>(Vector2 at) where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif
            return Collide.Check(this, Scene?.Tracker.Entities[typeof(T)] ?? [], at);
        }

        public bool CollideCheck<T, Exclude>() where T : Entity where Exclude : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked objects when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(Exclude)))
                throw new Exception("Excluded type is an untracked Entity type!");
#endif

            var exclude = Scene?.Tracker.Entities[typeof(Exclude)] ?? [];
            foreach (var e in Scene?.Tracker.Entities[typeof(T)] ?? [])
                if (!exclude.Contains(e))
                    if (Collide.Check(this, e))
                        return true;
            return false;
        }

        public bool CollideCheck<T, Exclude>(Vector2 at) where T : Entity where Exclude : Entity {
            var was = Position;
            Position = at;
            var ret = CollideCheck<T, Exclude>();
            Position = was;
            return ret;
        }

        public bool CollideCheckByComponent<T>() where T : CollidableComponent {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked CollidableComponents when it is not a member of a Scene");
            else if (!Scene.Tracker.Components.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked CollidableComponent type");
#endif

            foreach (var c in Scene?.Tracker.CollidableComponents[typeof(T)] ?? [])
                if (Collide.Check(this, c))
                    return true;
            return false;
        }

        public bool CollideCheckByComponent<T>(Vector2 at) where T : CollidableComponent {
            Vector2 old = Position;
            Position = at;
            bool ret = CollideCheckByComponent<T>();
            Position = old;
            return ret;
        }

        #endregion

        #region Collide CheckOutside

        public bool CollideCheckOutside(Entity other, Vector2 at) {
            return !Collide.Check(this, other) && Collide.Check(this, other, at);
        }

        public bool CollideCheckOutside(BitTag tag, Vector2 at) {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against a tag list when it is not a member of a Scene");
#endif

            foreach (var entity in Scene?[tag] ?? [])
                if (!Collide.Check(this, entity) && Collide.Check(this, entity, at))
                    return true;

            return false;
        }

        public bool CollideCheckOutside<T>(Vector2 at) where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif

            foreach (var entity in Scene?.Tracker.Entities[typeof(T)] ?? [])
                if (!Collide.Check(this, entity) && Collide.Check(this, entity, at))
                    return true;
            return false;
        }

        public bool CollideCheckOutsideByComponent<T>(Vector2 at) where T : CollidableComponent {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked CollidableComponents when it is not a member of a Scene");
            else if (!Scene.Tracker.CollidableComponents.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked CollidableComponent type");
#endif

            foreach (var component in Scene?.Tracker.CollidableComponents[typeof(T)] ?? [])
                if (!Collide.Check(this, component) && Collide.Check(this, component, at))
                    return true;
            return false;
        }

        #endregion

        #region Collide First

        public Entity? CollideFirst(BitTag tag) {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against a tag list when it is not a member of a Scene");
#endif
            return Collide.First(this, Scene?[tag] ?? []);
        }

        public Entity? CollideFirst(BitTag tag, Vector2 at) {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against a tag list when it is not a member of a Scene");
#endif
            return Collide.First(this, Scene?[tag] ?? [], at);
        }

        public T? CollideFirst<T>() where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif
            return Collide.First(this, Scene?.Tracker.Entities[typeof(T)] ?? []) as T;
        }

        public T? CollideFirst<T>(Vector2 at) where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif
            return Collide.First(this, Scene?.Tracker.Entities[typeof(T)] ?? [], at) as T;
        }

        public T? CollideFirstByComponent<T>() where T : CollidableComponent {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked CollidableComponents when it is not a member of a Scene");
            else if (!Scene.Tracker.CollidableComponents.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked CollidableComponent type");
#endif

            foreach (var component in Scene?.Tracker.CollidableComponents[typeof(T)] ?? [])
                if (Collide.Check(this, component))
                    return component as T;
            return null;
        }

        public T? CollideFirstByComponent<T>(Vector2 at) where T : CollidableComponent {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked CollidableComponents when it is not a member of a Scene");
            else if (!Scene.Tracker.CollidableComponents.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked CollidableComponent type");
#endif

            foreach (var component in Scene?.Tracker.CollidableComponents[typeof(T)] ?? [])
                if (Collide.Check(this, component, at))
                    return component as T;
            return null;
        }

        #endregion

        #region Collide FirstOutside

        public Entity? CollideFirstOutside(BitTag tag, Vector2 at) {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against a tag list when it is not a member of a Scene");
#endif

            foreach (var entity in Scene?[tag] ?? [])
                if (!Collide.Check(this, entity) && Collide.Check(this, entity, at))
                    return entity;
            return null;
        }

        public T? CollideFirstOutside<T>(Vector2 at) where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif

            foreach (var entity in Scene?.Tracker.Entities[typeof(T)] ?? [])
                if (!Collide.Check(this, entity) && Collide.Check(this, entity, at))
                    return entity as T;
            return null;
        }

        public T? CollideFirstOutsideByComponent<T>(Vector2 at) where T : CollidableComponent {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked CollidableComponents when it is not a member of a Scene");
            else if (!Scene.Tracker.CollidableComponents.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked CollidableComponent type");
#endif

            foreach (var component in Scene?.Tracker.CollidableComponents[typeof(T)] ?? [])
                if (!Collide.Check(this, component) && Collide.Check(this, component, at))
                    return component as T;
            return null;
        }

        #endregion

        #region Collide All

        public List<Entity> CollideAll(BitTag tag) {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against a tag list when it is not a member of a Scene");
#endif
            return Collide.All(this, Scene?[tag] ?? []);
        }

        public List<Entity> CollideAll(BitTag tag, Vector2 at) {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against a tag list when it is not a member of a Scene");
#endif
            return Collide.All(this, Scene?[tag] ?? [], at);
        }

        public List<Entity> CollideAll<T>() where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif

            return Collide.All(this, Scene?.Tracker.Entities[typeof(T)] ?? []);
        }

        public List<Entity> CollideAll<T>(Vector2 at) where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif

            return Collide.All(this, Scene?.Tracker.Entities[typeof(T)] ?? [], at);
        }

        public List<Entity> CollideAll<T>(Vector2 at, List<Entity> into) where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif

            into.Clear();
            return Collide.All(this, Scene?.Tracker.Entities[typeof(T)] ?? [], into, at);
        }

        public List<T> CollideAllByComponent<T>() where T : CollidableComponent {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked CollidableComponents when it is not a member of a Scene");
            else if (!Scene.Tracker.CollidableComponents.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked CollidableComponent type");
#endif

            List<T> list = new List<T>();
            foreach (var component in Scene?.Tracker.CollidableComponents[typeof(T)] ?? [])
                if (Collide.Check(this, component))
                    if (component is T typed)
                        list.Add(typed);
            return list;
        }

        public List<T> CollideAllByComponent<T>(Vector2 at) where T : CollidableComponent {
            Vector2 old = Position;
            Position = at;
            var ret = CollideAllByComponent<T>();
            Position = old;
            return ret;
        }

        #endregion

        #region Collide Do

        public bool CollideDo(BitTag tag, Action<Entity> action) {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against a tag list when it is not a member of a Scene");
#endif

            bool hit = false;
            foreach (var other in Scene?[tag] ?? []) {
                if (CollideCheck(other)) {
                    action(other);
                    hit = true;
                }
            }
            return hit;
        }

        public bool CollideDo(BitTag tag, Action<Entity> action, Vector2 at) {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against a tag list when it is not a member of a Scene");
#endif

            bool hit = false;
            var was = Position;
            Position = at;

            foreach (var other in Scene?[tag] ?? []) {
                if (CollideCheck(other)) {
                    action(other);
                    hit = true;
                }
            }

            Position = was;
            return hit;
        }

        public bool CollideDo<T>(Action<T> action) where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif

            bool hit = false;
            foreach (var other in Scene?.Tracker.Entities[typeof(T)] ?? []) {
                if (CollideCheck(other)) {
                    if (other is T typed)
                        action(typed);
                    hit = true;
                }
            }
            return hit;
        }

        public bool CollideDo<T>(Action<T> action, Vector2 at) where T : Entity {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked Entities when it is not a member of a Scene");
            else if (!Scene.Tracker.Entities.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked Entity type");
#endif

            bool hit = false;
            var was = Position;
            Position = at;

            foreach (var other in Scene?.Tracker.Entities[typeof(T)] ?? []) {
                if (CollideCheck(other)) {
                    if (other is T typed)
                        action(typed);
                    hit = true;
                }
            }

            Position = was;
            return hit;
        }

        public bool CollideDoByComponent<T>(Action<T> action) where T : CollidableComponent {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked CollidableComponents when it is not a member of a Scene");
            else if (!Scene.Tracker.CollidableComponents.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked CollidableComponent type");
#endif

            bool hit = false;
            foreach (var component in Scene?.Tracker.CollidableComponents[typeof(T)] ?? []) {
                if (CollideCheck(component)) {
                    if (component is T typed)
                        action(typed);
                    hit = true;
                }
            }
            return hit;
        }

        public bool CollideDoByComponent<T>(Action<T> action, Vector2 at) where T : CollidableComponent {
#if DEBUG
            if (Scene == null)
                throw new Exception("Can't collide check an Entity against tracked CollidableComponents when it is not a member of a Scene");
            else if (!Scene.Tracker.CollidableComponents.ContainsKey(typeof(T)))
                throw new Exception("Can't collide check an Entity against an untracked CollidableComponent type");
#endif

            bool hit = false;
            var was = Position;
            Position = at;

            foreach (var component in Scene?.Tracker.CollidableComponents[typeof(T)] ?? []) {
                if (CollideCheck(component)) {
                    if (component is T typed)
                        action(typed);
                    hit = true;
                }
            }

            Position = was;
            return hit;
        }

        #endregion

        #region Collide Geometry

        public bool CollidePoint(Vector2 point) {
            return Collide.CheckPoint(this, point);
        }

        public bool CollidePoint(Vector2 point, Vector2 at) {
            return Collide.CheckPoint(this, point, at);
        }

        public bool CollideLine(Vector2 from, Vector2 to) {
            return Collide.CheckLine(this, from, to);
        }

        public bool CollideLine(Vector2 from, Vector2 to, Vector2 at) {
            return Collide.CheckLine(this, from, to, at);
        }

        public bool CollideRect(Rectangle rect) {
            return Collide.CheckRect(this, rect);
        }

        public bool CollideRect(Rectangle rect, Vector2 at) {
            return Collide.CheckRect(this, rect, at);
        }

        #endregion

        #endregion

        #region Components Shortcuts

        /// <summary>
        /// Shortcut function for adding a Component to the Entity's Components list
        /// </summary>
        /// <param name="component">The Component to add</param>
        public void Add(Component component) {
            Components.Add(component);
        }

        /// <summary>
        /// Shortcut function for removing an Component from the Entity's Components list
        /// </summary>
        /// <param name="component">The Component to remove</param>
        public void Remove(Component component) {
            Components.Remove(component);
        }

        /// <summary>
        /// Shortcut function for adding a set of Components from the Entity's Components list
        /// </summary>
        /// <param name="components">The Components to add</param>
        public void Add(params Component[] components) {
            Components.Add(components);
        }

        /// <summary>
        /// Shortcut function for removing a set of Components from the Entity's Components list
        /// </summary>
        /// <param name="components">The Components to remove</param>
        public void Remove(params Component[] components) {
            Components.Remove(components);
        }

        public T Get<T>() where T : Component {
            return Components.Get<T>();
        }

        /// <summary>
        /// Allows you to iterate through all Components in the Entity
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Component> GetEnumerator() {
            return Components.GetEnumerator();
        }

        /// <summary>
        /// Allows you to iterate through all Components in the Entity
        /// </summary>
        /// <returns></returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        #region Misc Utils

        public Entity Closest(params Entity[] entities) {
            Entity closest = entities[0];
            float dist = Vector2.DistanceSquared(Position, closest.Position);

            for (int i = 1; i < entities.Length; i++) {
                float current = Vector2.DistanceSquared(Position, entities[i].Position);
                if (current < dist) {
                    closest = entities[i];
                    dist = current;
                }
            }

            return closest;
        }

        public Entity? Closest(BitTag tag) {
            var list = Scene?[tag] ?? [];
            Entity? closest = null;
            float dist;

            if (list?.Count >= 1) {
                closest = list![0];
                dist = Vector2.DistanceSquared(Position, closest.Position);

                for (int i = 1; i < list.Count; i++) {
                    float current = Vector2.DistanceSquared(Position, list[i].Position);
                    if (current < dist) {
                        closest = list[i];
                        dist = current;
                    }
                }
            }

            return closest;
        }

        /// <summary>
        /// Gets the current scene cast to the specified type.
        /// </summary>
        /// <typeparam name="T">The scene type to cast to.</typeparam>
        /// <returns>The scene cast to type T, or null if the cast fails or no scene is set.</returns>
        [return: MaybeNull]
        public T SceneAs<T>() where T : Scene {
            return Scene as T;
        }

        #endregion
    }
}
