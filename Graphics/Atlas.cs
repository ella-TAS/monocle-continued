using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Monocle {
    public class Atlas {
        public List<Texture2D> Sources = [];
        private Dictionary<string, MTexture> textures = new Dictionary<string, MTexture>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, List<MTexture>> orderedTexturesCache = new Dictionary<string, List<MTexture>>();

        public enum AtlasDataFormat {
            TexturePacker_Sparrow,
            CrunchXml,
            CrunchBinary,
            CrunchXmlOrBinary,
            CrunchBinaryNoAtlas,
            Packer,
            PackerNoAtlas
        };

        public static Atlas FromAtlas(string path, AtlasDataFormat format) {
            var atlas = new Atlas();
            atlas.Sources = new List<Texture2D>();
            ReadAtlasData(atlas, path, format);
            return atlas;
        }

        private static void ReadAtlasData(Atlas atlas, string path, AtlasDataFormat format) {
            switch (format) {
            case AtlasDataFormat.TexturePacker_Sparrow: {
                    XmlDocument xml = Calc.LoadContentXML(path);
                    XmlElement at = xml["TextureAtlas"]!;

                    string texturePath = at.Attr("imagePath", "");
                    Stream fileStream = TitleContainer.OpenStream(Path.Combine(Engine.ContentDirectory, Path.GetDirectoryName(path) ?? "", texturePath));
                    var texture = Texture2D.FromStream(Engine.Instance!.GraphicsDevice, fileStream);
                    fileStream.Close();

                    var mTexture = new MTexture(texture);
                    atlas.Sources.Add(texture);

                    XmlNodeList subtextures = at.GetElementsByTagName("SubTexture");

                    foreach (XmlElement sub in subtextures) {
                        string name = sub.Attr("name");
                        Rectangle clipRect = sub.Rect();
                        if (sub.HasAttr("frameX"))
                            atlas.textures[name] = new MTexture(mTexture, name, clipRect, new Vector2(-sub.AttrInt("frameX"), -sub.AttrInt("frameY")), sub.AttrInt("frameWidth"), sub.AttrInt("frameHeight"));
                        else
                            atlas.textures[name] = new MTexture(mTexture, name, clipRect);
                    }
                }
                break;
            case AtlasDataFormat.CrunchXml: {
                    XmlDocument xml = Calc.LoadContentXML(path);
                    XmlElement at = xml["atlas"]!;

                    foreach (XmlElement tex in at) {
                        string texturePath = tex.Attr("n", "");
                        Stream fileStream = TitleContainer.OpenStream(Path.Combine(Engine.ContentDirectory, Path.GetDirectoryName(path) ?? "", texturePath + ".png"));
                        var texture = Texture2D.FromStream(Engine.Instance!.GraphicsDevice, fileStream);
                        fileStream.Close();

                        var mTexture = new MTexture(texture);
                        atlas.Sources.Add(texture);

                        foreach (XmlElement sub in tex) {
                            string name = sub.Attr("n");
                            var clipRect = new Rectangle(sub.AttrInt("x"), sub.AttrInt("y"), sub.AttrInt("w"), sub.AttrInt("h"));
                            if (sub.HasAttr("fx"))
                                atlas.textures[name] = new MTexture(mTexture, name, clipRect, new Vector2(-sub.AttrInt("fx"), -sub.AttrInt("fy")), sub.AttrInt("fw"), sub.AttrInt("fh"));
                            else
                                atlas.textures[name] = new MTexture(mTexture, name, clipRect);
                        }
                    }
                }
                break;

            case AtlasDataFormat.CrunchBinary:
                using (Stream stream = TitleContainer.OpenStream(Path.Combine(Engine.ContentDirectory, path))) {
                    var reader = new BinaryReader(stream);
                    short textures = reader.ReadInt16();

                    for (int i = 0; i < textures; i++) {
                        string textureName = reader.ReadNullTerminatedString();
                        string texturePath = Path.Combine(Engine.ContentDirectory, Path.GetDirectoryName(path) ?? "", textureName + ".png");
                        Stream fileStream = TitleContainer.OpenStream(texturePath);
                        var texture = Texture2D.FromStream(Engine.Instance!.GraphicsDevice, fileStream);
                        fileStream.Close();

                        atlas.Sources.Add(texture);

                        var mTexture = new MTexture(texture);
                        short subtextures = reader.ReadInt16();
                        for (int j = 0; j < subtextures; j++) {
                            string name = reader.ReadNullTerminatedString();
                            short x = reader.ReadInt16();
                            short y = reader.ReadInt16();
                            short w = reader.ReadInt16();
                            short h = reader.ReadInt16();
                            short fx = reader.ReadInt16();
                            short fy = reader.ReadInt16();
                            short fw = reader.ReadInt16();
                            short fh = reader.ReadInt16();

                            atlas.textures[name] = new MTexture(mTexture, name, new Rectangle(x, y, w, h), new Vector2(-fx, -fy), fw, fh);
                        }
                    }
                }
                break;

            case AtlasDataFormat.CrunchBinaryNoAtlas:
                using (Stream stream = TitleContainer.OpenStream(Path.Combine(Engine.ContentDirectory, path + ".bin"))) {
                    var reader = new BinaryReader(stream);
                    short folders = reader.ReadInt16();

                    for (int i = 0; i < folders; i++) {
                        string folderName = reader.ReadNullTerminatedString();
                        string folderPath = Path.Combine(Engine.ContentDirectory, Path.GetDirectoryName(path) ?? "", folderName);

                        short subtextures = reader.ReadInt16();
                        for (int j = 0; j < subtextures; j++) {
                            string name = reader.ReadNullTerminatedString();
                            short x = reader.ReadInt16();
                            short y = reader.ReadInt16();
                            short w = reader.ReadInt16();
                            short h = reader.ReadInt16();
                            short fx = reader.ReadInt16();
                            short fy = reader.ReadInt16();
                            short fw = reader.ReadInt16();
                            short fh = reader.ReadInt16();

                            Stream fileStream = TitleContainer.OpenStream(Path.Combine(folderPath, name + ".png"));
                            var texture = Texture2D.FromStream(Engine.Instance!.GraphicsDevice, fileStream);
                            fileStream.Close();

                            atlas.Sources.Add(texture);
                            atlas.textures[name] = new MTexture(texture, new Vector2(-fx, -fy), fw, fh);
                        }
                    }
                }
                break;

            case AtlasDataFormat.Packer:
                using (Stream stream = TitleContainer.OpenStream(Path.Combine(Engine.ContentDirectory, path + ".meta"))) {
                    var reader = new BinaryReader(stream);
                    reader.ReadInt32(); // version
                    reader.ReadString(); // args
                    reader.ReadInt32(); // hash

                    short textures = reader.ReadInt16();
                    for (int i = 0; i < textures; i++) {
                        string textureName = reader.ReadString();
                        string texturePath = Path.Combine(Engine.ContentDirectory, Path.GetDirectoryName(path) ?? "", textureName + ".data");
                        Stream fileStream = TitleContainer.OpenStream(texturePath);
                        var texture = Texture2D.FromStream(Engine.Instance!.GraphicsDevice, fileStream);
                        fileStream.Close();

                        atlas.Sources.Add(texture);

                        var mTexture = new MTexture(texture);
                        short subtextures = reader.ReadInt16();
                        for (int j = 0; j < subtextures; j++) {
                            string name = reader.ReadString().Replace('\\', '/');
                            short x = reader.ReadInt16();
                            short y = reader.ReadInt16();
                            short w = reader.ReadInt16();
                            short h = reader.ReadInt16();
                            short fx = reader.ReadInt16();
                            short fy = reader.ReadInt16();
                            short fw = reader.ReadInt16();
                            short fh = reader.ReadInt16();

                            atlas.textures[name] = new MTexture(mTexture, name, new Rectangle(x, y, w, h), new Vector2(-fx, -fy), fw, fh);
                        }
                    }
                }

                break;

            case AtlasDataFormat.PackerNoAtlas:
                using (Stream stream = TitleContainer.OpenStream(Path.Combine(Engine.ContentDirectory, path + ".meta"))) {
                    var reader = new BinaryReader(stream);
                    reader.ReadInt32(); // version
                    reader.ReadString(); // args
                    reader.ReadInt32(); // hash

                    short folders = reader.ReadInt16();
                    for (int i = 0; i < folders; i++) {
                        string folderName = reader.ReadString();
                        string folderPath = Path.Combine(Engine.ContentDirectory, Path.GetDirectoryName(path) ?? "", folderName);

                        short subtextures = reader.ReadInt16();
                        for (int j = 0; j < subtextures; j++) {
                            string name = reader.ReadString().Replace('\\', '/');
                            short x = reader.ReadInt16();
                            short y = reader.ReadInt16();
                            short w = reader.ReadInt16();
                            short h = reader.ReadInt16();
                            short fx = reader.ReadInt16();
                            short fy = reader.ReadInt16();
                            short fw = reader.ReadInt16();
                            short fh = reader.ReadInt16();

                            Stream fileStream = TitleContainer.OpenStream(Path.Combine(folderPath, name + ".data"));
                            var texture = Texture2D.FromStream(Engine.Instance!.GraphicsDevice, fileStream);
                            fileStream.Close();

                            atlas.Sources.Add(texture);
                            atlas.textures[name] = new MTexture(texture, new Vector2(-fx, -fy), fw, fh);
                        }
                    }
                }
                break;

            case AtlasDataFormat.CrunchXmlOrBinary:

                if (File.Exists(Path.Combine(Engine.ContentDirectory, path + ".bin")))
                    ReadAtlasData(atlas, path + ".bin", AtlasDataFormat.CrunchBinary);
                else
                    ReadAtlasData(atlas, path + ".xml", AtlasDataFormat.CrunchXml);

                break;


            default:
                throw new NotImplementedException();
            }
        }

        public static Atlas FromMultiAtlas(string rootPath, string[] dataPath, AtlasDataFormat format) {
            var atlas = new Atlas();
            atlas.Sources = new List<Texture2D>();

            for (int i = 0; i < dataPath.Length; i++)
                ReadAtlasData(atlas, Path.Combine(rootPath, dataPath[i]), format);

            return atlas;
        }

        public static Atlas FromMultiAtlas(string rootPath, string filename, AtlasDataFormat format) {
            var atlas = new Atlas();
            atlas.Sources = new List<Texture2D>();

            int index = 0;
            while (true) {
                string dataPath = Path.Combine(rootPath, filename + index.ToString() + ".xml");

                if (!File.Exists(Path.Combine(Engine.ContentDirectory, dataPath)))
                    break;

                ReadAtlasData(atlas, dataPath, format);
                index++;
            }

            return atlas;
        }

        public static Atlas FromDirectory(string path) {
            var atlas = new Atlas();
            atlas.Sources = new List<Texture2D>();

            string contentDirectory = Engine.ContentDirectory;
            int contentDirectoryLength = contentDirectory.Length;
            string contentPath = Path.Combine(contentDirectory, path);
            int contentPathLength = contentPath.Length;

            foreach (string file in Directory.GetFiles(contentPath, "*", SearchOption.AllDirectories)) {
                string ext = Path.GetExtension(file);
                if (ext != ".png" && ext != ".xnb")
                    continue;

                // get path and load
                Stream fileStream = TitleContainer.OpenStream(file.Substring(contentDirectoryLength + 1));
                var texture = Texture2D.FromStream(Engine.Instance!.GraphicsDevice, fileStream);
                fileStream.Close();

                atlas.Sources.Add(texture);

                // make nice for dictionary
                string filepath = file.Substring(contentPathLength + 1);
                filepath = filepath.Substring(0, filepath.Length - 4);
                filepath = filepath.Replace('\\', '/');

                // load
                atlas.textures.Add(filepath, new MTexture(texture));
            }

            return atlas;
        }

        public MTexture this[string id] {
            get { return textures[id]; }
            set { textures[id] = value; }
        }

        public bool Has(string id) {
            return textures.ContainsKey(id);
        }

        public MTexture GetOrDefault(string id, MTexture defaultTexture) {
            if (string.IsNullOrEmpty(id) || !Has(id))
                return defaultTexture;
            return textures[id];
        }

        public List<MTexture> GetAtlasSubtextures(string key) {
            if (!orderedTexturesCache.TryGetValue(key, out List<MTexture>? list)) {
                list = new List<MTexture>();

                int index = 0;
                while (true) {
                    MTexture? texture = GetAtlasSubtextureFromAtlasAt(key, index);
                    if (texture != null)
                        list.Add(texture);
                    else
                        break;
                    index++;
                }

                orderedTexturesCache.Add(key, list);
            }

            return list;
        }

        private MTexture GetAtlasSubtextureFromCacheAt(string key, int index) {
            return orderedTexturesCache[key][index];
        }

        private MTexture? GetAtlasSubtextureFromAtlasAt(string key, int index) {
            if (index == 0 && textures.ContainsKey(key))
                return textures[key];

            string indexString = index.ToString();
            int startLength = indexString.Length;
            while (indexString.Length < startLength + 6) {
                if (textures.TryGetValue(key + indexString, out MTexture? result))
                    return result;
                indexString = "0" + indexString;
            }

            return null;
        }

        public MTexture? GetAtlasSubtexturesAt(string key, int index) {
            if (orderedTexturesCache.TryGetValue(key, out List<MTexture>? list))
                return list[index];
            else
                return GetAtlasSubtextureFromAtlasAt(key, index);
        }

        public void Dispose() {
            foreach (Texture2D texture in Sources)
                texture.Dispose();
            Sources.Clear();
            textures.Clear();
        }


    }
}
