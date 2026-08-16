using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Monocle {
    public static class MonocleSettings {
        public static bool ImageSnapDefault = true;

        public static List<JsonConverter> JsonConverters { get; } = [];
        public static bool JsonPrettyPrint = false;
    }
}
