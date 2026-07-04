using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Xml;

namespace Monocle {
    /// <summary>
    /// Save and Load objects to files.
    /// Currently supported modes: Json, JsonObfuscated, XML.
    /// Binary format is no longer supported due to a deprecation.
    /// </summary>
    public static class SaveLoad {
        public enum SerializeMode {
            Json,
            JsonObfuscated,
            XML
        }

        private static readonly JsonSerializerOptions JsonOptions = new() {
            WriteIndented = true,
            IncludeFields = true
        };

        // maybe different on a platform basis, works on linux/win
        public static string GetSaveFolder(string folderName = "Saves")
            => Path.Combine(AppContext.BaseDirectory, folderName);

        public static string GetSavePath(string fileName, string folderName = "Saves")
            => Path.Combine(GetSaveFolder(folderName), fileName);

        private static void EnsureFolder(string folder) {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        public static void Save<T>(T data, string fileName, SerializeMode mode, string folderName = "Saves")
            where T : class {
            ArgumentNullException.ThrowIfNull(data);

            string folder = GetSaveFolder(folderName);
            string path = GetSavePath(fileName, folderName);
            EnsureFolder(folder);

            using FileStream stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );

            switch (mode) {
            case SerializeMode.Json:
                JsonSerializer.Serialize(stream, data, JsonOptions);
                break;

            case SerializeMode.JsonObfuscated:
                byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions);
                foreach (byte b in bytes) {
                    stream.WriteByte(ObfuscateByte(b));
                }
                break;

            case SerializeMode.XML: {
                    using var writer = XmlWriter.Create(stream, new XmlWriterSettings() {
                        Indent = true
                    });
                    var serializer = new DataContractSerializer(typeof(T));
                    serializer.WriteObject(writer, data);
                    break;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            Logger.Release("SaveLoad", $"File {folderName}/{fileName} saved");
        }

        public static bool SafeSave<T>(T data, string fileName, SerializeMode mode, string folderName = "Saves")
            where T : class {
#if DEBUG
            Save(data, fileName, mode, folderName);
            return true;
#else
            try {
                Save(data, fileName, mode, folderName);
                return true;
            } catch (Exception e) {
                ErrorLog.Write($"Exception while saving {folderName}/{fileName} in {mode} mode:\n" + e);
                return false;
            }
#endif
        }

        public static T Load<T>(string fileName, SerializeMode mode, string folderName = "Saves")
                where T : class {
            string path = GetSavePath(fileName, folderName);

            if (!File.Exists(path)) {
                Logger.Release("SaveLoad", $"File {folderName}/{fileName} not found");
                return null;
            }

            using FileStream stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            T result;

            switch (mode) {
            case SerializeMode.Json:
                result = JsonSerializer.Deserialize<T>(stream, JsonOptions);
                break;

            case SerializeMode.JsonObfuscated: {
                    using var output = new MemoryStream();
                    int b;
                    while ((b = stream.ReadByte()) != -1) {
                        output.WriteByte(ObfuscateByte((byte) b));
                    }

                    output.Position = 0;
                    result = JsonSerializer.Deserialize<T>(output, JsonOptions);
                    break;
                }

            case SerializeMode.XML:
                var serializer = new DataContractSerializer(typeof(T));
                result = (T) serializer.ReadObject(stream);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            Logger.Release("SaveLoad", $"File {folderName}/{fileName} loaded");

            return result;
        }

        public static T SafeLoad<T>(string fileName, SerializeMode mode, string folderName = "Saves")
            where T : class {
#if DEBUG
            return Load<T>(fileName, mode, folderName);
#else
            try {
                return Load<T>(fileName, mode, folderName);
            } catch (Exception e) {
                ErrorLog.Write($"Exception while loading {folderName}/{fileName} in {mode} mode:\n" + e);
                return null;
            }
#endif
        }

        public static byte ObfuscateByte(byte input) {
            return (byte) (input ^ 0xAA);
        }
    }
}
