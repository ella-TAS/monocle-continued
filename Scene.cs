#nullable enable
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Monocle {
    /// <summary>
    /// Represents a game scene that manages entities, rendering, and collision detection.
    /// </summary>
    /// <remarks>
    /// Scene is the core container class in Monocle that manages all game objects and their lifecycle.
    /// It provides entity management, collision detection, rendering coordination, and timing functionality.
    /// Each scene maintains its own entity list, tag lists, renderer list, and tracker for efficient
    /// object management and collision detection.
    /// </remarks>
    public class Scene : IEnumerable<Entity>, IEnumerable {
        /// <summary>
        /// Gets or sets whether the scene is currently paused, preventing updates from occurring.
        /// </summary>
        public bool Paused { get; set; }

        /// <summary>
        /// Gets the total time the scene has been active, excluding paused time.
        /// </summary>
        public float TimeActive { get; private set; }

        /// <summary>
        /// Gets the total raw time the scene has been active, including paused time.
        /// </summary>
        public float RawTimeActive { get; private set; }

        /// <summary>
        /// Gets whether the scene currently has focus and is actively updating.
        /// </summary>
        public bool Focused { get; private set; }

        /// <summary>
        /// Gets the entity list that manages all entities in this scene.
        /// </summary>
        public EntityList Entities { get; private set; } = null!; // Assigned in constructor

        /// <summary>
        /// Gets the tag lists that provide efficient access to entities by their bit tags.
        /// </summary>
        public TagLists TagLists { get; private set; } = null!; // Assigned in constructor

        /// <summary>
        /// Gets the renderer list that manages all renderers in this scene.
        /// </summary>
        public RendererList RendererList { get; private set; } = null!; // Assigned in constructor

        /// <summary>
        /// Gets the helper entity that provides utility functionality for the scene.
        /// </summary>
        public Entity HelperEntity { get; private set; } = null!; // Assigned in constructor

        /// <summary>
        /// Gets the tracker that manages tracked entity and component types for collision detection.
        /// </summary>
        public Tracker Tracker { get; private set; } = null!; // Assigned in constructor

        /// <summary>
        /// Dictionary that tracks actual depth values for entities to ensure proper rendering order.
        /// </summary>
        private readonly Dictionary<int, double> actualDepthLookup = new();

        /// <summary>
        /// Event that is raised at the end of each frame, after all updates have completed.
        /// </summary>
        public event Action? OnEndOfFrame;

        /// <summary>
        /// Initializes a new instance of the Scene class.
        /// </summary>
        /// <remarks>
        /// Creates and initializes all necessary components including the tracker, entity list,
        /// tag lists, renderer list, and helper entity. The scene is ready for use immediately
        /// after construction.
        /// </remarks>
        public Scene() {
            Tracker = new Tracker();
            Entities = new EntityList(this);
            TagLists = new TagLists();
            RendererList = new RendererList(this);

            HelperEntity = new Entity();
            Entities.Add(HelperEntity);
        }

        /// <summary>
        /// Called when the scene becomes active and begins running.
        /// </summary>
        /// <remarks>
        /// Sets the scene as focused and notifies all entities that the scene has begun.
        /// Override this method to perform custom initialization logic when the scene starts.
        /// </remarks>
        public virtual void Begin() {
            Focused = true;
            foreach (var entity in Entities)
                entity.SceneBegin(this);
        }

        /// <summary>
        /// Called when the scene is ending and will no longer be active.
        /// </summary>
        /// <remarks>
        /// Sets the scene as unfocused and notifies all entities that the scene is ending.
        /// Override this method to perform custom cleanup logic when the scene ends.
        /// </remarks>
        public virtual void End() {
            Focused = false;
            foreach (var entity in Entities)
                entity.SceneEnd(this);
        }

        /// <summary>
        /// Called before the main update loop to prepare the scene for updates.
        /// </summary>
        /// <remarks>
        /// Updates timing values and refreshes all entity, tag, and renderer lists.
        /// This method is called every frame regardless of pause state.
        /// </remarks>
        public virtual void BeforeUpdate() {
            if (!Paused)
                TimeActive += Engine.DeltaTime;
            RawTimeActive += Engine.RawDeltaTime;

            Entities.UpdateLists();
            TagLists.UpdateLists();
            RendererList.UpdateLists();
        }

        /// <summary>
        /// Performs the main update logic for the scene and all its entities.
        /// </summary>
        /// <remarks>
        /// Updates all entities and renderers if the scene is not paused.
        /// Override this method to add custom update logic for the scene.
        /// </remarks>
        public virtual void Update() {
            if (!Paused) {
                Entities.Update();
                RendererList.Update();
            }
        }

        /// <summary>
        /// Called after the main update loop to perform end-of-frame operations.
        /// </summary>
        /// <remarks>
        /// Raises the OnEndOfFrame event if any subscribers are registered.
        /// This method is called every frame regardless of pause state.
        /// </remarks>
        public virtual void AfterUpdate() {
            OnEndOfFrame?.Invoke();
            OnEndOfFrame = null;
        }

        /// <summary>
        /// Called before rendering begins to prepare the scene for drawing.
        /// </summary>
        /// <remarks>
        /// Notifies all renderers that rendering is about to begin.
        /// Override this method to add custom pre-rendering logic.
        /// </remarks>
        public virtual void BeforeRender() {
            RendererList.BeforeRender();
        }

        /// <summary>
        /// Performs the main rendering logic for the scene.
        /// </summary>
        /// <remarks>
        /// Renders all entities and components through the renderer list.
        /// Override this method to add custom rendering logic for the scene.
        /// </remarks>
        public virtual void Render() {
            RendererList.Render();
        }

        /// <summary>
        /// Called after rendering completes to perform post-rendering operations.
        /// </summary>
        /// <remarks>
        /// Notifies all renderers that rendering has completed.
        /// Override this method to add custom post-rendering logic.
        /// </remarks>
        public virtual void AfterRender() {
            RendererList.AfterRender();
        }

        /// <summary>
        /// Called when the graphics device is reset, allowing the scene to handle the reset event.
        /// </summary>
        /// <remarks>
        /// Notifies all entities that the graphics device has been reset.
        /// Override this method to add custom graphics reset handling logic.
        /// </remarks>
        public virtual void HandleGraphicsReset() {
            Entities.HandleGraphicsReset();
        }

        /// <summary>
        /// Called when the graphics device is created, allowing the scene to handle the creation event.
        /// </summary>
        /// <remarks>
        /// Notifies all entities that the graphics device has been created.
        /// Override this method to add custom graphics creation handling logic.
        /// </remarks>
        public virtual void HandleGraphicsCreate() {
            Entities.HandleGraphicsCreate();
        }

        /// <summary>
        /// Called when the scene gains focus and becomes the active scene.
        /// </summary>
        /// <remarks>
        /// Override this method to add custom logic when the scene gains focus.
        /// This is useful for resuming audio, animations, or other focus-dependent features.
        /// </remarks>
        public virtual void GainFocus() {

        }

        /// <summary>
        /// Called when the scene loses focus and is no longer the active scene.
        /// </summary>
        /// <remarks>
        /// Override this method to add custom logic when the scene loses focus.
        /// This is useful for pausing audio, animations, or other focus-dependent features.
        /// </remarks>
        public virtual void LoseFocus() {

        }

        #region Interval

        /// <summary>
        /// Returns whether the Scene timer has passed the given time interval since the last frame.
        /// </summary>
        /// <param name="interval">The time interval to check for in seconds.</param>
        /// <returns>True if the interval has just been crossed, false otherwise.</returns>
        /// <remarks>
        /// This method is useful for triggering events at regular intervals. For example,
        /// given 2.0f, this will return true once every 2 seconds.
        /// </remarks>
        public bool OnInterval(float interval) {
            if (interval <= 0f) return false;
            return (int) ((TimeActive - Engine.DeltaTime) / interval) < (int) (TimeActive / interval);
        }

        /// <summary>
        /// Returns whether the Scene timer has passed the given time interval since the last frame, with an offset.
        /// </summary>
        /// <param name="interval">The time interval to check for in seconds.</param>
        /// <param name="offset">The offset from the start time in seconds.</param>
        /// <returns>True if the interval has just been crossed, false otherwise.</returns>
        /// <remarks>
        /// This method is useful for triggering events at regular intervals with a specific offset.
        /// For example, given 2.0f and 1.0f, this will return true every 2 seconds starting at 1 second.
        /// </remarks>
        public bool OnInterval(float interval, float offset) {
            if (interval <= 0f) return false;
            return Math.Floor((TimeActive - offset - Engine.DeltaTime) / interval) < Math.Floor((TimeActive - offset) / interval);
        }

        /// <summary>
        /// Returns whether the Scene timer is currently within the given time interval.
        /// </summary>
        /// <param name="interval">The time interval to check for in seconds.</param>
        /// <returns>True if the current time is within the interval, false otherwise.</returns>
        public bool BetweenInterval(float interval) {
            return Calc.BetweenInterval(TimeActive, interval);
        }

        /// <summary>
        /// Returns whether the Scene raw timer has passed the given time interval since the last frame.
        /// </summary>
        /// <param name="interval">The time interval to check for in seconds.</param>
        /// <returns>True if the interval has just been crossed, false otherwise.</returns>
        /// <remarks>
        /// This method uses RawTimeActive which includes paused time, unlike OnInterval which uses TimeActive.
        /// </remarks>
        public bool OnRawInterval(float interval) {
            if (interval <= 0f) return false;
            return (int) ((RawTimeActive - Engine.RawDeltaTime) / interval) < (int) (RawTimeActive / interval);
        }

        /// <summary>
        /// Returns whether the Scene raw timer has passed the given time interval since the last frame, with an offset.
        /// </summary>
        /// <param name="interval">The time interval to check for in seconds.</param>
        /// <param name="offset">The offset from the start time in seconds.</param>
        /// <returns>True if the interval has just been crossed, false otherwise.</returns>
        /// <remarks>
        /// This method uses RawTimeActive which includes paused time, unlike OnInterval which uses TimeActive.
        /// </remarks>
        public bool OnRawInterval(float interval, float offset) {
            if (interval <= 0f) return false;
            return Math.Floor((RawTimeActive - offset - Engine.RawDeltaTime) / interval) < Math.Floor((RawTimeActive - offset) / interval);
        }

        /// <summary>
        /// Returns whether the Scene raw timer is currently within the given time interval.
        /// </summary>
        /// <param name="interval">The time interval to check for in seconds.</param>
        /// <returns>True if the current raw time is within the interval, false otherwise.</returns>
        /// <remarks>
        /// This method uses RawTimeActive which includes paused time, unlike BetweenInterval which uses TimeActive.
        /// </remarks>
        public bool BetweenRawInterval(float interval) {
            return Calc.BetweenInterval(RawTimeActive, interval);
        }

        #endregion

        #region Collisions v Tags

        /// <summary>
        /// Checks if a point collides with any entity that has the specified tag.
        /// </summary>
        /// <param name="point">The point to check for collision.</param>
        /// <param name="tag">The tag to check against.</param>
        /// <returns>True if a collision is detected, false otherwise.</returns>
        public bool CollideCheck(Vector2 point, int tag) {
            var list = TagLists[(int) tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollidePoint(point))
                    return true;
            return false;
        }

        /// <summary>
        /// Checks if a line segment collides with any entity that has the specified tag.
        /// </summary>
        /// <param name="from">The starting point of the line segment.</param>
        /// <param name="to">The ending point of the line segment.</param>
        /// <param name="tag">The tag to check against.</param>
        /// <returns>True if a collision is detected, false otherwise.</returns>
        public bool CollideCheck(Vector2 from, Vector2 to, int tag) {
            var list = TagLists[(int) tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideLine(from, to))
                    return true;
            return false;
        }

        /// <summary>
        /// Checks if a rectangle collides with any entity that has the specified tag.
        /// </summary>
        /// <param name="rect">The rectangle to check for collision.</param>
        /// <param name="tag">The tag to check against.</param>
        /// <returns>True if a collision is detected, false otherwise.</returns>
        public bool CollideCheck(Rectangle rect, int tag) {
            var list = TagLists[(int) tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideRect(rect))
                    return true;
            return false;
        }

        public bool CollideCheck(Rectangle rect, Entity entity) {
            return (entity.Collidable && entity.CollideRect(rect));
        }

        /// <summary>
        /// Finds the first entity with the specified tag that collides with the given point.
        /// </summary>
        /// <param name="point">The point to check for collision.</param>
        /// <param name="tag">The tag to check against.</param>
        /// <returns>The first colliding entity, or null if no collision is found.</returns>
        public Entity? CollideFirst(Vector2 point, int tag) {
            var list = TagLists[tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollidePoint(point))
                    return list[i];
            return null;
        }

        /// <summary>
        /// Finds the first entity with the specified tag that collides with the given line segment.
        /// </summary>
        /// <param name="from">The starting point of the line segment.</param>
        /// <param name="to">The ending point of the line segment.</param>
        /// <param name="tag">The tag to check against.</param>
        /// <returns>The first colliding entity, or null if no collision is found.</returns>
        public Entity? CollideFirst(Vector2 from, Vector2 to, int tag) {
            var list = TagLists[tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideLine(from, to))
                    return list[i];
            return null;
        }

        /// <summary>
        /// Finds the first entity with the specified tag that collides with the given rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to check for collision.</param>
        /// <param name="tag">The tag to check against.</param>
        /// <returns>The first colliding entity, or null if no collision is found.</returns>
        public Entity? CollideFirst(Rectangle rect, int tag) {
            var list = TagLists[tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideRect(rect))
                    return list[i];
            return null;
        }

        public void CollideInto(Vector2 point, int tag, List<Entity> hits) {
            var list = TagLists[(int) tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollidePoint(point))
                    hits.Add(list[i]);
        }

        public void CollideInto(Vector2 from, Vector2 to, int tag, List<Entity> hits) {
            var list = TagLists[(int) tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideLine(from, to))
                    hits.Add(list[i]);
        }

        public void CollideInto(Rectangle rect, int tag, List<Entity> hits) {
            var list = TagLists[(int) tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideRect(rect))
                    list.Add(list[i]);
        }

        /// <summary>
        /// Gets all entities with the specified tag that collide with the given point.
        /// </summary>
        /// <param name="point">The point to check for collision.</param>
        /// <param name="tag">The tag to check against.</param>
        /// <returns>A list of all colliding entities. The list is never null but may be empty.</returns>
        public List<Entity> CollideAll(Vector2 point, int tag) {
            List<Entity> results = new();
            CollideInto(point, tag, results);
            return results;
        }

        /// <summary>
        /// Gets all entities with the specified tag that collide with the given line segment.
        /// </summary>
        /// <param name="from">The starting point of the line segment.</param>
        /// <param name="to">The ending point of the line segment.</param>
        /// <param name="tag">The tag to check against.</param>
        /// <returns>A list of all colliding entities. The list is never null but may be empty.</returns>
        public List<Entity> CollideAll(Vector2 from, Vector2 to, int tag) {
            List<Entity> results = new();
            CollideInto(from, to, tag, results);
            return results;
        }

        /// <summary>
        /// Gets all entities with the specified tag that collide with the given rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to check for collision.</param>
        /// <param name="tag">The tag to check against.</param>
        /// <returns>A list of all colliding entities. The list is never null but may be empty.</returns>
        public List<Entity> CollideAll(Rectangle rect, int tag) {
            List<Entity> results = new();
            CollideInto(rect, tag, results);
            return results;
        }

        public void CollideDo(Vector2 point, int tag, Action<Entity> action) {
            var list = TagLists[(int) tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollidePoint(point))
                    action(list[i]);
        }

        public void CollideDo(Vector2 from, Vector2 to, int tag, Action<Entity> action) {
            var list = TagLists[(int) tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideLine(from, to))
                    action(list[i]);
        }

        public void CollideDo(Rectangle rect, int tag, Action<Entity> action) {
            var list = TagLists[(int) tag];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideRect(rect))
                    action(list[i]);
        }

        public Vector2 LineWalkCheck(Vector2 from, Vector2 to, int tag, float precision) {
            Vector2 add = to - from;
            add.Normalize();
            add *= precision;

            int amount = (int) Math.Floor((from - to).Length() / precision);
            Vector2 prev = from;
            Vector2 at = from + add;

            for (int i = 0; i <= amount; i++) {
                if (CollideCheck(at, tag))
                    return prev;
                prev = at;
                at += add;
            }

            return to;
        }

        #endregion

        #region Collisions v Tracked List Entities

        public bool CollideCheck<T>(Vector2 point) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollidePoint(point))
                    return true;
            return false;
        }

        public bool CollideCheck<T>(Vector2 from, Vector2 to) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideLine(from, to))
                    return true;
            return false;
        }

        public bool CollideCheck<T>(Rectangle rect) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideRect(rect))
                    return true;
            return false;
        }

        /// <summary>
        /// Finds the first entity of the specified type that collides with the given point.
        /// </summary>
        /// <typeparam name="T">The entity type to search for.</typeparam>
        /// <param name="point">The point to check for collision.</param>
        /// <returns>The first colliding entity of type T, or null if no collision is found.</returns>
        [return: MaybeNull]
        public T CollideFirst<T>(Vector2 point) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollidePoint(point))
                    return (T) list[i];
            return default(T);
        }

        /// <summary>
        /// Finds the first entity of the specified type that collides with the given line segment.
        /// </summary>
        /// <typeparam name="T">The entity type to search for.</typeparam>
        /// <param name="from">The starting point of the line segment.</param>
        /// <param name="to">The ending point of the line segment.</param>
        /// <returns>The first colliding entity of type T, or null if no collision is found.</returns>
        [return: MaybeNull]
        public T CollideFirst<T>(Vector2 from, Vector2 to) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideLine(from, to))
                    return (T) list[i];
            return default(T);
        }

        /// <summary>
        /// Finds the first entity of the specified type that collides with the given rectangle.
        /// </summary>
        /// <typeparam name="T">The entity type to search for.</typeparam>
        /// <param name="rect">The rectangle to check for collision.</param>
        /// <returns>The first colliding entity of type T, or null if no collision is found.</returns>
        [return: MaybeNull]
        public T CollideFirst<T>(Rectangle rect) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideRect(rect))
                    return (T) list[i];
            return default(T);
        }

        public void CollideInto<T>(Vector2 point, List<Entity> hits) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollidePoint(point))
                    hits.Add(list[i]);
        }

        public void CollideInto<T>(Vector2 from, Vector2 to, List<Entity> hits) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideLine(from, to))
                    hits.Add(list[i]);
        }

        public void CollideInto<T>(Rectangle rect, List<Entity> hits) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideRect(rect))
                    list.Add(list[i]);
        }

        public void CollideInto<T>(Vector2 point, List<T> hits) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollidePoint(point))
                    if (list[i] is T typed)
                        hits.Add(typed);
        }

        public void CollideInto<T>(Vector2 from, Vector2 to, List<T> hits) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideLine(from, to))
                    if (list[i] is T typed)
                        hits.Add(typed);
        }

        public void CollideInto<T>(Rectangle rect, List<T> hits) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideRect(rect))
                    if (list[i] is T typed)
                        hits.Add(typed);
        }

        public List<T> CollideAll<T>(Vector2 point) where T : Entity {
            List<T> list = new List<T>();
            CollideInto<T>(point, list);
            return list;
        }

        public List<T> CollideAll<T>(Vector2 from, Vector2 to) where T : Entity {
            List<T> list = new List<T>();
            CollideInto<T>(from, to, list);
            return list;
        }

        public List<T> CollideAll<T>(Rectangle rect) where T : Entity {
            List<T> list = new List<T>();
            CollideInto<T>(rect, list);
            return list;
        }

        public void CollideDo<T>(Vector2 point, Action<T> action) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollidePoint(point))
                    if (list[i] is T typed)
                        action(typed);
        }

        public void CollideDo<T>(Vector2 from, Vector2 to, Action<T> action) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideLine(from, to))
                    if (list[i] is T typed)
                        action(typed);
        }

        public void CollideDo<T>(Rectangle rect, Action<T> action) where T : Entity {
            var list = Tracker.Entities[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Collidable && list[i].CollideRect(rect))
                    if (list[i] is T typed)
                        action(typed);
        }

        public Vector2 LineWalkCheck<T>(Vector2 from, Vector2 to, float precision) where T : Entity {
            Vector2 add = to - from;
            add.Normalize();
            add *= precision;

            int amount = (int) Math.Floor((from - to).Length() / precision);
            Vector2 prev = from;
            Vector2 at = from + add;

            for (int i = 0; i <= amount; i++) {
                if (CollideCheck<T>(at))
                    return prev;
                prev = at;
                at += add;
            }

            return to;
        }

        #endregion

        #region Collisions v Tracked List Components

        public bool CollideCheckByComponent<T>(Vector2 point) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollidePoint(point) == true)
                    return true;
            return false;
        }

        public bool CollideCheckByComponent<T>(Vector2 from, Vector2 to) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollideLine(from, to) == true)
                    return true;
            return false;
        }

        public bool CollideCheckByComponent<T>(Rectangle rect) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollideRect(rect) == true)
                    return true;
            return false;
        }

        public T? CollideFirstByComponent<T>(Vector2 point) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollidePoint(point) == true)
                    return list[i] as T;
            return null;
        }

        public T? CollideFirstByComponent<T>(Vector2 from, Vector2 to) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollideLine(from, to) == true)
                    return list[i] as T;
            return null;
        }

        public T? CollideFirstByComponent<T>(Rectangle rect) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollideRect(rect) == true)
                    return list[i] as T;
            return null;
        }

        public void CollideIntoByComponent<T>(Vector2 point, List<Component> hits) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollidePoint(point) == true)
                    hits.Add(list[i]);
        }

        public void CollideIntoByComponent<T>(Vector2 from, Vector2 to, List<Component> hits) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollideLine(from, to) == true)
                    hits.Add(list[i]);
        }

        public void CollideIntoByComponent<T>(Rectangle rect, List<Component> hits) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollideRect(rect) == true)
                    list.Add(list[i]);
        }

        public void CollideIntoByComponent<T>(Vector2 point, List<T> hits) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollidePoint(point) == true)
                    if (list[i] is T typed)
                        hits.Add(typed);
        }

        public void CollideIntoByComponent<T>(Vector2 from, Vector2 to, List<T> hits) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollideLine(from, to) == true)
                    if (list[i] is T typed)
                        hits.Add(typed);
        }

        public void CollideIntoByComponent<T>(Rectangle rect, List<T> hits) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollideRect(rect) == true)
                    list.Add(list[i] as T);
        }

        public List<T> CollideAllByComponent<T>(Vector2 point) where T : Component {
            List<T> list = new List<T>();
            CollideIntoByComponent<T>(point, list);
            return list;
        }

        public List<T> CollideAllByComponent<T>(Vector2 from, Vector2 to) where T : Component {
            List<T> list = new List<T>();
            CollideIntoByComponent<T>(from, to, list);
            return list;
        }

        public List<T> CollideAllByComponent<T>(Rectangle rect) where T : Component {
            List<T> list = new List<T>();
            CollideIntoByComponent<T>(rect, list);
            return list;
        }

        public void CollideDoByComponent<T>(Vector2 point, Action<T> action) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollidePoint(point) == true)
                    if (list[i] is T typed)
                        action(typed);
        }

        public void CollideDoByComponent<T>(Vector2 from, Vector2 to, Action<T> action) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollideLine(from, to) == true)
                    if (list[i] is T typed)
                        action(typed);
        }

        public void CollideDoByComponent<T>(Rectangle rect, Action<T> action) where T : Component {
            var list = Tracker.Components[typeof(T)];

            for (int i = 0; i < list.Count; i++)
                if (list[i].Entity?.Collidable == true && list[i].Entity?.CollideRect(rect) == true)
                    if (list[i] is T typed)
                        action(typed);
        }

        public Vector2 LineWalkCheckByComponent<T>(Vector2 from, Vector2 to, float precision) where T : Component {
            Vector2 add = to - from;
            add.Normalize();
            add *= precision;

            int amount = (int) Math.Floor((from - to).Length() / precision);
            Vector2 prev = from;
            Vector2 at = from + add;

            for (int i = 0; i <= amount; i++) {
                if (CollideCheckByComponent<T>(at))
                    return prev;
                prev = at;
                at += add;
            }

            return to;
        }

        #endregion

        #region Utils

        /// <summary>
        /// Sets the actual depth value for an entity to ensure proper rendering order.
        /// </summary>
        /// <param name="entity">The entity to set the actual depth for.</param>
        /// <remarks>
        /// This method ensures that entities with the same depth value are rendered in a consistent order
        /// by assigning each a slightly different actual depth value. It also marks the relevant lists
        /// as unsorted to trigger re-sorting during the next update cycle.
        /// </remarks>
        internal void SetActualDepth(Entity entity) {
            const double theta = .000001f;

            double add = 0;
            if (actualDepthLookup.TryGetValue(entity.depth, out add))
                actualDepthLookup[entity.depth] += theta;
            else
                actualDepthLookup.Add(entity.depth, theta);
            entity.actualDepth = entity.depth - add;

            //Mark lists unsorted
            Entities.MarkUnsorted();
            for (int i = 0; i < BitTag.TotalTags; i++)
                if (entity.TagCheck(1 << i))
                    TagLists.MarkUnsorted(i);
        }

        #endregion

        #region Entity Shortcuts

        /// <summary>
        /// Creates a pooled entity, adds it to this scene, and returns it.
        /// </summary>
        /// <typeparam name="T">The pooled entity type to create. Must be marked with the Pooled attribute.</typeparam>
        /// <returns>The created and added entity.</returns>
        /// <remarks>
        /// This is a convenience method that combines entity creation and scene addition.
        /// The entity type must be marked with the Pooled attribute to work with the pooler.
        /// </remarks>
        public T CreateAndAdd<T>() where T : Entity, new() {
            var entity = Engine.Pooler!.Create<T>();
            Add(entity);
            return entity;
        }

        /// <summary>
        /// Provides quick access to entities by their bit tag.
        /// </summary>
        /// <param name="tag">The bit tag to fetch entities for.</param>
        /// <returns>A list of entities with the specified tag. The result is never null.</returns>
        public List<Entity> this[BitTag tag] {
            get {
                return TagLists[tag.ID];
            }
        }

        /// <summary>
        /// Adds an entity to the scene's entity list.
        /// </summary>
        /// <param name="entity">The entity to add to the scene.</param>
        /// <remarks>
        /// This is a shortcut method that delegates to the Entities list.
        /// The entity will be automatically managed by the scene and will receive
        /// lifecycle callbacks (Begin, Update, Render, End).
        /// </remarks>
        public void Add(Entity entity) {
            Entities.Add(entity);
        }

        /// <summary>
        /// Removes an entity from the scene's entity list.
        /// </summary>
        /// <param name="entity">The entity to remove from the scene.</param>
        /// <remarks>
        /// This is a shortcut method that delegates to the Entities list.
        /// The entity will no longer receive lifecycle callbacks from the scene.
        /// </remarks>
        public void Remove(Entity entity) {
            Entities.Remove(entity);
        }

        /// <summary>
        /// Shortcut function for adding a set of Entities from the Scene's Entities list
        /// </summary>
        /// <param name="entities">The Entities to add</param>
        public void Add(IEnumerable<Entity> entities) {
            Entities.Add(entities);
        }

        /// <summary>
        /// Shortcut function for removing a set of Entities from the Scene's Entities list
        /// </summary>
        /// <param name="entities">The Entities to remove</param>
        public void Remove(IEnumerable<Entity> entities) {
            Entities.Remove(entities);
        }

        /// <summary>
        /// Shortcut function for adding a set of Entities from the Scene's Entities list
        /// </summary>
        /// <param name="entities">The Entities to add</param>
        public void Add(params Entity[] entities) {
            Entities.Add(entities);
        }

        /// <summary>
        /// Shortcut function for removing a set of Entities from the Scene's Entities list
        /// </summary>
        /// <param name="entities">The Entities to remove</param>
        public void Remove(params Entity[] entities) {
            Entities.Remove(entities);
        }

        /// <summary>
        /// Returns an enumerator that iterates through all entities in the scene.
        /// </summary>
        /// <returns>An enumerator for the scene's entities.</returns>
        /// <remarks>
        /// This allows you to use foreach loops to iterate through all entities in the scene.
        /// The entities are enumerated in their current depth order.
        /// </remarks>
        public IEnumerator<Entity> GetEnumerator() {
            return Entities.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through all entities in the scene.
        /// </summary>
        /// <returns>An enumerator for the scene's entities.</returns>
        /// <remarks>
        /// This is the non-generic implementation required by IEnumerable.
        /// </remarks>
        IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets all entities that have any of the tags specified in the mask.
        /// </summary>
        /// <param name="mask">The bitmask of tags to search for.</param>
        /// <returns>A list of entities that match the tag mask. The list is never null but may be empty.</returns>
        public List<Entity> GetEntitiesByTagMask(int mask) {
            List<Entity> results = new();
            foreach (var entity in Entities)
                if ((entity.Tag & mask) != 0)
                    results.Add(entity);
            return results;
        }

        /// <summary>
        /// Gets all entities that do not have any of the tags specified in the mask.
        /// </summary>
        /// <param name="mask">The bitmask of tags to exclude.</param>
        /// <returns>A list of entities that do not match the tag mask. The list is never null but may be empty.</returns>
        public List<Entity> GetEntitiesExcludingTagMask(int mask) {
            List<Entity> results = new();
            foreach (var entity in Entities)
                if ((entity.Tag & mask) == 0)
                    results.Add(entity);
            return results;
        }

        #endregion

        #region Renderer Shortcuts

        /// <summary>
        /// Adds a renderer to the scene's renderer list.
        /// </summary>
        /// <param name="renderer">The renderer to add to the scene.</param>
        /// <remarks>
        /// This is a shortcut method that delegates to the RendererList.
        /// The renderer will be automatically managed by the scene and will receive
        /// rendering lifecycle callbacks (BeforeRender, Render, AfterRender).
        /// </remarks>
        public void Add(Renderer renderer) {
            RendererList.Add(renderer);
        }

        /// <summary>
        /// Removes a renderer from the scene's renderer list.
        /// </summary>
        /// <param name="renderer">The renderer to remove from the scene.</param>
        /// <remarks>
        /// This is a shortcut method that delegates to the RendererList.
        /// The renderer will no longer receive rendering lifecycle callbacks from the scene.
        /// </remarks>
        public void Remove(Renderer renderer) {
            RendererList.Remove(renderer);
        }

        #endregion
    }
}
