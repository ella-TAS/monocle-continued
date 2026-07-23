using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Xml;

namespace Monocle {
    public static class XmlElementExt {
        #region Attributes

        public static bool HasAttr(this XmlElement xml, string attributeName) {
            return xml.Attributes[attributeName] != null;
        }

        public static string Attr(this XmlElement xml, string attributeName) {
#if DEBUG
            if (!xml.HasAttr(attributeName))
                throw new Exception("Element does not contain the attribute \"" + attributeName + "\"");
#endif
            return xml.Attributes[attributeName]?.InnerText ?? "";
        }

        public static string Attr(this XmlElement xml, string attributeName, string defaultValue) {
            if (!xml.HasAttr(attributeName))
                return defaultValue;
            else
                return xml.Attributes[attributeName]!.InnerText;
        }

        public static int AttrInt(this XmlElement xml, string attributeName) {
#if DEBUG
            if (!xml.HasAttr(attributeName))
                throw new Exception("Element does not contain the attribute \"" + attributeName + "\"");
#endif
            return Convert.ToInt32(xml.Attributes[attributeName]?.InnerText);
        }

        public static int AttrInt(this XmlElement xml, string attributeName, int defaultValue) {
            if (!xml.HasAttr(attributeName))
                return defaultValue;
            else
                return Convert.ToInt32(xml.Attributes[attributeName]!.InnerText);
        }

        public static float AttrFloat(this XmlElement xml, string attributeName) {
#if DEBUG
            if (!xml.HasAttr(attributeName))
                throw new Exception("Element does not contain the attribute \"" + attributeName + "\"");
#endif
            return Convert.ToSingle(xml.Attributes[attributeName]?.InnerText, CultureInfo.InvariantCulture);
        }

        public static float AttrFloat(this XmlElement xml, string attributeName, float defaultValue) {
            if (!xml.HasAttr(attributeName))
                return defaultValue;
            else
                return Convert.ToSingle(xml.Attributes[attributeName]!.InnerText, CultureInfo.InvariantCulture);
        }

        public static Vector3 AttrVector3(this XmlElement xml, string attributeName) {
            var attr = xml.Attr(attributeName).Split(',');
            var x = float.Parse(attr[0].Trim(), CultureInfo.InvariantCulture);
            var y = float.Parse(attr[1].Trim(), CultureInfo.InvariantCulture);
            var z = float.Parse(attr[2].Trim(), CultureInfo.InvariantCulture);

            return new Vector3(x, y, z);
        }

        public static Vector2 AttrVector2(this XmlElement xml, string xAttributeName, string yAttributeName) {
            return new Vector2(xml.AttrFloat(xAttributeName), xml.AttrFloat(yAttributeName));
        }

        public static Vector2 AttrVector2(this XmlElement xml, string xAttributeName, string yAttributeName, Vector2 defaultValue) {
            return new Vector2(xml.AttrFloat(xAttributeName, defaultValue.X), xml.AttrFloat(yAttributeName, defaultValue.Y));
        }

        public static bool AttrBool(this XmlElement xml, string attributeName) {
#if DEBUG
            if (!xml.HasAttr(attributeName))
                throw new Exception("Element does not contain the attribute \"" + attributeName + "\"");
#endif
            return Convert.ToBoolean(xml.Attributes[attributeName]?.InnerText);
        }

        public static bool AttrBool(this XmlElement xml, string attributeName, bool defaultValue) {
            if (!xml.HasAttr(attributeName))
                return defaultValue;
            else
                return AttrBool(xml, attributeName);
        }

        public static char AttrChar(this XmlElement xml, string attributeName) {
#if DEBUG
            if (!xml.HasAttr(attributeName))
                throw new Exception("Element does not contain the attribute \"" + attributeName + "\"");
#endif
            return Convert.ToChar(xml.Attributes[attributeName]?.InnerText ?? "\0");
        }

        public static char AttrChar(this XmlElement xml, string attributeName, char defaultValue) {
            if (!xml.HasAttr(attributeName))
                return defaultValue;
            else
                return AttrChar(xml, attributeName);
        }

        public static T AttrEnum<T>(this XmlElement xml, string attributeName) where T : struct {
            if (!xml.HasAttr(attributeName))
                throw new Exception("Element does not contain the attribute \"" + attributeName + "\"");

            if (Enum.IsDefined(typeof(T), xml.Attributes[attributeName]!.InnerText))
                return Enum.Parse<T>(xml.Attributes[attributeName]!.InnerText);
            else
                throw new Exception("The attribute value cannot be converted to the enum type.");
        }

        public static T AttrEnum<T>(this XmlElement xml, string attributeName, T defaultValue) where T : struct {
            if (!xml.HasAttr(attributeName))
                return defaultValue;
            else
                return xml.AttrEnum<T>(attributeName);
        }

        public static Color AttrHexColor(this XmlElement xml, string attributeName) {
#if DEBUG
            if (!xml.HasAttr(attributeName))
                throw new Exception("Element does not contain the attribute \"" + attributeName + "\"");
#endif
            return Calc.HexToColor(xml.Attr(attributeName));
        }

        public static Color AttrHexColor(this XmlElement xml, string attributeName, Color defaultValue) {
            if (!xml.HasAttr(attributeName))
                return defaultValue;
            else
                return AttrHexColor(xml, attributeName);
        }

        public static Color AttrHexColor(this XmlElement xml, string attributeName, string defaultValue) {
            if (!xml.HasAttr(attributeName))
                return Calc.HexToColor(defaultValue);
            else
                return AttrHexColor(xml, attributeName);
        }

        public static Vector2 Position(this XmlElement xml) {
            return new Vector2(xml.AttrFloat("x"), xml.AttrFloat("y"));
        }

        public static Vector2 Position(this XmlElement xml, Vector2 defaultPosition) {
            return new Vector2(xml.AttrFloat("x", defaultPosition.X), xml.AttrFloat("y", defaultPosition.Y));
        }

        public static int X(this XmlElement xml) {
            return xml.AttrInt("x");
        }

        public static int X(this XmlElement xml, int defaultX) {
            return xml.AttrInt("x", defaultX);
        }

        public static int Y(this XmlElement xml) {
            return xml.AttrInt("y");
        }

        public static int Y(this XmlElement xml, int defaultY) {
            return xml.AttrInt("y", defaultY);
        }

        public static int Width(this XmlElement xml) {
            return xml.AttrInt("width");
        }

        public static int Width(this XmlElement xml, int defaultWidth) {
            return xml.AttrInt("width", defaultWidth);
        }

        public static int Height(this XmlElement xml) {
            return xml.AttrInt("height");
        }

        public static int Height(this XmlElement xml, int defaultHeight) {
            return xml.AttrInt("height", defaultHeight);
        }

        public static Rectangle Rect(this XmlElement xml) {
            return new Rectangle(xml.X(), xml.Y(), xml.Width(), xml.Height());
        }

        public static int ID(this XmlElement xml) {
            return xml.AttrInt("id");
        }

        #endregion

        #region Inner Text

        public static int InnerInt(this XmlElement xml) {
            return Convert.ToInt32(xml.InnerText);
        }

        public static float InnerFloat(this XmlElement xml) {
            return Convert.ToSingle(xml.InnerText, CultureInfo.InvariantCulture);
        }

        public static bool InnerBool(this XmlElement xml) {
            return Convert.ToBoolean(xml.InnerText);
        }

        public static T InnerEnum<T>(this XmlElement xml) where T : struct {
            if (Enum.IsDefined(typeof(T), xml.InnerText))
                return (T) Enum.Parse(typeof(T), xml.InnerText);
            else
                throw new Exception("The attribute value cannot be converted to the enum type.");
        }

        public static Color InnerHexColor(this XmlElement xml) {
            return Calc.HexToColor(xml.InnerText);
        }

        #endregion

        #region Child Inner Text

        public static bool HasChild(this XmlElement xml, string childName) {
            return xml[childName] != null;
        }

        public static string ChildText(this XmlElement xml, string childName) {
#if DEBUG
            if (!xml.HasChild(childName))
                throw new Exception("Cannot find child xml tag with name '" + childName + "'.");
#endif
            return xml[childName]?.InnerText ?? "";
        }

        public static string ChildText(this XmlElement xml, string childName, string defaultValue) {
            if (xml.HasChild(childName))
                return xml[childName]!.InnerText;
            else
                return defaultValue;
        }

        public static int ChildInt(this XmlElement xml, string childName) {
#if DEBUG
            if (!xml.HasChild(childName))
                throw new Exception("Cannot find child xml tag with name '" + childName + "'.");
#endif
            return xml[childName]?.InnerInt() ?? 0;
        }

        public static int ChildInt(this XmlElement xml, string childName, int defaultValue) {
            if (xml.HasChild(childName))
                return xml[childName]!.InnerInt();
            else
                return defaultValue;
        }

        public static float ChildFloat(this XmlElement xml, string childName) {
#if DEBUG
            if (!xml.HasChild(childName))
                throw new Exception("Cannot find child xml tag with name '" + childName + "'.");
#endif
            return xml[childName]?.InnerFloat() ?? 0f;
        }

        public static float ChildFloat(this XmlElement xml, string childName, float defaultValue) {
            if (xml.HasChild(childName))
                return xml[childName]!.InnerFloat();
            else
                return defaultValue;
        }

        public static bool ChildBool(this XmlElement xml, string childName) {
#if DEBUG
            if (!xml.HasChild(childName))
                throw new Exception("Cannot find child xml tag with name '" + childName + "'.");
#endif
            return xml[childName]?.InnerBool() ?? false;
        }

        public static bool ChildBool(this XmlElement xml, string childName, bool defaultValue) {
            if (xml.HasChild(childName))
                return xml[childName]!.InnerBool();
            else
                return defaultValue;
        }

        public static T ChildEnum<T>(this XmlElement xml, string childName) where T : struct {
            if (!xml.HasChild(childName))
                throw new Exception("Cannot find child xml tag with name '" + childName + "'.");

            if (Enum.IsDefined(typeof(T), xml[childName]!.InnerText))
                return Enum.Parse<T>(xml[childName]!.InnerText);
            else
                throw new Exception("The attribute value cannot be converted to the enum type.");
        }

        public static T ChildEnum<T>(this XmlElement xml, string childName, T defaultValue) where T : struct {
            if (xml.HasChild(childName)) {
                if (Enum.IsDefined(typeof(T), xml[childName]!.InnerText))
                    return Enum.Parse<T>(xml[childName]!.InnerText);
                else
                    throw new Exception("The attribute value cannot be converted to the enum type.");
            } else
                return defaultValue;
        }

        public static Color ChildHexColor(this XmlElement xml, string childName) {
#if DEBUG
            if (!xml.HasChild(childName))
                throw new Exception("Cannot find child xml tag with name '" + childName + "'.");
#endif
            return Calc.HexToColor(xml[childName]?.InnerText ?? "");
        }

        public static Color ChildHexColor(this XmlElement xml, string childName, Color defaultValue) {
            if (xml.HasChild(childName))
                return Calc.HexToColor(xml[childName]!.InnerText);
            else
                return defaultValue;
        }

        public static Color ChildHexColor(this XmlElement xml, string childName, string defaultValue) {
            if (xml.HasChild(childName))
                return Calc.HexToColor(xml[childName]!.InnerText);
            else
                return Calc.HexToColor(defaultValue);
        }

        public static Vector2 ChildPosition(this XmlElement xml, string childName) {
#if DEBUG
            if (!xml.HasChild(childName))
                throw new Exception("Cannot find child xml tag with name '" + childName + "'.");
#endif
            return xml[childName]?.Position() ?? Vector2.Zero;
        }

        public static Vector2 ChildPosition(this XmlElement xml, string childName, Vector2 defaultValue) {
            if (xml.HasChild(childName))
                return xml[childName]!.Position(defaultValue);
            else
                return defaultValue;
        }

        #endregion

        #region Ogmo Nodes

        public static Vector2 FirstNode(this XmlElement xml) {
            if (xml["node"] == null)
                return Vector2.Zero;
            else
                return new Vector2((int) xml["node"]!.AttrFloat("x"), (int) xml["node"]!.AttrFloat("y"));
        }

        public static Vector2? FirstNodeNullable(this XmlElement xml) {
            if (xml["node"] == null)
                return null;
            else
                return new Vector2((int) xml["node"]!.AttrFloat("x"), (int) xml["node"]!.AttrFloat("y"));
        }

        public static Vector2? FirstNodeNullable(this XmlElement xml, Vector2 offset) {
            if (xml["node"] == null)
                return null;
            else
                return new Vector2((int) xml["node"]!.AttrFloat("x"), (int) xml["node"]!.AttrFloat("y")) + offset;
        }

        public static Vector2[] Nodes(this XmlElement xml, bool includePosition = false) {
            XmlNodeList nodes = xml.GetElementsByTagName("node");
            if (nodes == null)
                return includePosition ? [xml.Position()] : [];

            Vector2[] ret;
            if (includePosition) {
                ret = new Vector2[nodes.Count + 1];
                ret[0] = xml.Position();
                for (int i = 0; i < nodes.Count; i++)
                    ret[i + 1] = new Vector2(Convert.ToInt32(nodes[i]!.Attributes!["x"]!.InnerText), Convert.ToInt32(nodes[i]!.Attributes!["y"]!.InnerText));
            } else {
                ret = new Vector2[nodes.Count];
                for (int i = 0; i < nodes.Count; i++)
                    ret[i] = new Vector2(Convert.ToInt32(nodes[i]!.Attributes!["x"]!.InnerText), Convert.ToInt32(nodes[i]!.Attributes!["y"]!.InnerText));
            }

            return ret;
        }

        public static Vector2[] Nodes(this XmlElement xml, Vector2 offset, bool includePosition = false) {
            var nodes = Nodes(xml, includePosition);

            for (int i = 0; i < nodes.Length; i++)
                nodes[i] += offset;

            return nodes;
        }

        public static Vector2 GetNode(this XmlElement xml, int nodeNum) {
            return xml.Nodes()[nodeNum];
        }

        public static Vector2? GetNodeNullable(this XmlElement xml, int nodeNum) {
            if (xml.Nodes().Length > nodeNum)
                return xml.Nodes()[nodeNum];
            else
                return null;
        }

        #endregion

        #region Add Stuff

        public static void SetAttr(this XmlElement xml, string attributeName, object setTo) {
            XmlAttribute attr;

            if (xml.HasAttr(attributeName))
                attr = xml.Attributes[attributeName]!;
            else {
                attr = xml.OwnerDocument.CreateAttribute(attributeName);
                xml.Attributes.Append(attr);
            }

            attr.Value = setTo.ToString();
        }

        public static void SetChild(this XmlElement xml, string childName, object setTo) {
            XmlElement ele;

            if (xml.HasChild(childName))
                ele = xml[childName]!;
            else {
                ele = xml.OwnerDocument.CreateElement(null, childName, xml.NamespaceURI);
                xml.AppendChild(ele);
            }

            ele.InnerText = setTo.ToString() ?? "";
        }

        public static XmlElement CreateChild(this XmlDocument doc, string childName) {
            XmlElement ele = doc.CreateElement(null, childName, doc.NamespaceURI);
            doc.AppendChild(ele);
            return ele;
        }

        public static XmlElement CreateChild(this XmlElement xml, string childName) {
            XmlElement ele = xml.OwnerDocument.CreateElement(null, childName, xml.NamespaceURI);
            xml.AppendChild(ele);
            return ele;
        }

        #endregion
    }
}
