using System;
using System.IO;
using System.Text.Json;

namespace Monocle;

/// <summary>
/// Class for handling data saving and loading in JSON.
/// Create a Singleton class with only the attributes to be saved/loaded and annotate it with [Serializable].
/// </summary>
public static class UserIO {
    public static string GetSaveFolder(string folderName = "Saves") {
        return Path.Combine(AppContext.BaseDirectory, folderName);
    }

    public static string GetSavePath(string fileName, string folderName = "Saves") {
        return Path.Combine(GetSaveFolder(folderName), fileName);
    }

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true,
        IncludeFields = true
    };

    public static void Save<T>(T data, string fileName, string folderName = "Saves") where T : class {
        string folder = GetSaveFolder(folderName);
        string file = GetSavePath(fileName, folderName);

        ArgumentNullException.ThrowIfNull(data);

        Console.WriteLine("Saving save data");
        Directory.CreateDirectory(folder);

        string json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(file, json);
        Console.WriteLine("Saved save data");
    }

    public static T Load<T>(string fileName, string folderName = "Saves") where T : class {
        string file = GetSavePath(fileName, folderName);

        if (!File.Exists(file)) {
            Console.WriteLine("Found no save file");
            return null;
        }

        Console.WriteLine("Loading save data");
        string json = File.ReadAllText(file);
        T data = JsonSerializer.Deserialize<T>(json, JsonOptions);
        Console.WriteLine("Loaded save data");
        return data;
    }
}
