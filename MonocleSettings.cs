using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Monocle {
    public static class MonocleSettings {
        public static bool ImageSnapDefault = true;

        public static List<JsonConverter> JsonConverters = [new JsonStringEnumConverter()];
        public static bool JsonPrettyPrint = false;
    }
}
