#nullable enable
using System;
using System.Collections.Generic;

namespace Monocle {
    /// <summary>
    /// Represents a bit-based tag system for efficiently categorizing and identifying game objects.
    /// Each BitTag is assigned a unique bit position allowing for fast bitwise operations.
    /// Maximum of 32 tags are supported due to int bit limitations.
    /// </summary>
    public sealed class BitTag {
        internal static int TotalTags = 0;
        internal static BitTag[] byID = new BitTag[32];
        private static Dictionary<string, BitTag> byName = new Dictionary<string, BitTag>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a BitTag by its name.
        /// </summary>
        /// <param name="name">The name of the tag to retrieve.</param>
        /// <returns>The BitTag with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
        /// <exception cref="Exception">Thrown when no tag with the specified name exists (debug only).</exception>
        public static BitTag Get(string name) {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

#if DEBUG
            if (!byName.ContainsKey(name))
                throw new Exception($"No tag with the name '{name}' has been defined!");
#endif
            return byName[name];
        }

        /// <summary>
        /// Attempts to get a BitTag by its name without throwing exceptions.
        /// </summary>
        /// <param name="name">The name of the tag to retrieve.</param>
        /// <param name="tag">When this method returns, contains the BitTag with the specified name, if found; otherwise, null.</param>
        /// <returns>true if a tag with the specified name was found; otherwise, false.</returns>
        public static bool TryGet(string name, out BitTag? tag) {
            if (name == null) {
                tag = null;
                return false;
            }
            return byName.TryGetValue(name, out tag);
        }

        /// <summary>
        /// The unique identifier for this tag (0-31).
        /// </summary>
        public int ID;

        /// <summary>
        /// The bit value for this tag (2^ID).
        /// </summary>
        public int Value;

        /// <summary>
        /// The name of this tag.
        /// </summary>
        public string Name;

        /// <summary>
        /// Creates a new BitTag with the specified name.
        /// </summary>
        /// <param name="name">The name for this tag. Must be unique and not null.</param>
        /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
        /// <exception cref="ArgumentException">Thrown when name is empty or whitespace.</exception>
        /// <exception cref="Exception">Thrown when the maximum tag limit of 32 is exceeded or when a tag with the same name already exists (debug only).</exception>
        public BitTag(string name) {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name cannot be empty or whitespace.", nameof(name));

#if DEBUG
            if (TotalTags >= 32)
                throw new Exception("Maximum tag limit of 32 exceeded!");
            if (byName.ContainsKey(name))
                throw new Exception($"Two tags defined with the same name: '{name}'!");
#endif

            ID = TotalTags;
            Value = 1 << TotalTags;
            Name = name;

            byID[ID] = this;
            byName[name] = this;

            TotalTags++;
        }

        /// <summary>
        /// Implicitly converts a BitTag to its integer value.
        /// </summary>
        /// <param name="tag">The BitTag to convert.</param>
        /// <returns>The bit value of the tag.</returns>
        public static implicit operator int(BitTag tag) => tag?.Value ?? 0;

        /// <summary>
        /// Returns the name of this BitTag.
        /// </summary>
        /// <returns>The tag name.</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Determines whether the specified object is equal to this BitTag.
        /// </summary>
        /// <param name="obj">The object to compare with this BitTag.</param>
        /// <returns>true if the specified object is a BitTag with the same ID; otherwise, false.</returns>
        public override bool Equals(object? obj) => obj is BitTag other && ID == other.ID;

        /// <summary>
        /// Returns a hash code for this BitTag based on its ID.
        /// </summary>
        /// <returns>A hash code for this BitTag.</returns>
        public override int GetHashCode() => ID.GetHashCode();

        /// <summary>
        /// Determines whether two BitTag instances are equal.
        /// </summary>
        /// <param name="left">The first BitTag to compare.</param>
        /// <param name="right">The second BitTag to compare.</param>
        /// <returns>true if the BitTags are equal; otherwise, false.</returns>
        public static bool operator ==(BitTag? left, BitTag? right) {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.ID == right.ID;
        }

        /// <summary>
        /// Determines whether two BitTag instances are not equal.
        /// </summary>
        /// <param name="left">The first BitTag to compare.</param>
        /// <param name="right">The second BitTag to compare.</param>
        /// <returns>true if the BitTags are not equal; otherwise, false.</returns>
        public static bool operator !=(BitTag? left, BitTag? right) => !(left == right);
    }
}
