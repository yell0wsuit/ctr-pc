using CutTheRope.game;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CutTheRope.ios
{
    internal sealed class XMLNode
    {
        // (get) Token: 0x060000FB RID: 251 RVA: 0x000057CA File Offset: 0x000039CA
        public string Name { get; private set; }

        // (get) Token: 0x060000FC RID: 252 RVA: 0x000057D2 File Offset: 0x000039D2
        public NSString Data { get; private set; }

        public NSString this[string key] => !attributes_.TryGetValue(key, out string rhs) ? new NSString("") : new NSString(rhs);

        public XMLNode()
        {
            parent = null;
            childs_ = [];
            attributes_ = [];
        }

        public bool Attributes()
        {
            return attributes_ != null && attributes_.Count > 0;
        }

        public List<XMLNode> Childs()
        {
            return childs_;
        }

        public XMLNode FindChildWithTagNameAndAttributeNameValueRecursively(string tag, string attrName, string attrVal, bool recursively)
        {
            if (Childs() == null)
            {
                return null;
            }
            foreach (XMLNode item in childs_)
            {
                if (item.Name == tag && item.Attributes() && item.attributes_.TryGetValue(attrName, out string text) && text == attrVal)
                {
                    return item;
                }
                if (recursively && item.Childs() != null)
                {
                    XMLNode xMLNode = item.FindChildWithTagNameRecursively(tag, recursively);
                    if (xMLNode != null)
                    {
                        return xMLNode;
                    }
                }
            }
            return null;
        }

        public XMLNode FindChildWithTagNameRecursively(NSString tag, bool recursively)
        {
            return FindChildWithTagNameRecursively(tag.ToString(), recursively);
        }

        public XMLNode FindChildWithTagNameRecursively(string tag, bool recursively)
        {
            if (Childs() == null)
            {
                return null;
            }
            foreach (XMLNode item in childs_)
            {
                if (item.Name == tag)
                {
                    return item;
                }
                if (recursively && item.Childs() != null)
                {
                    XMLNode xMLNode = item.FindChildWithTagNameRecursively(tag, recursively);
                    if (xMLNode != null)
                    {
                        return xMLNode;
                    }
                }
            }
            return null;
        }

        private static XMLNode ReadNode(XmlReader textReader, XMLNode parent)
        {
            while (textReader.NodeType != XmlNodeType.Element && textReader.Read())
            {
            }
            if (textReader.NodeType != XmlNodeType.Element)
            {
                return null;
            }
            XMLNode xMLNode = new();
            if (parent != null)
            {
                xMLNode.parent = parent;
                parent.childs_.Add(xMLNode);
            }
            xMLNode.Name = textReader.Name;
            xMLNode.depth = textReader.Depth;
            if (textReader.HasAttributes)
            {
                while (textReader.MoveToNextAttribute())
                {
                    xMLNode.attributes_.Add(textReader.Name, textReader.Value);
                }
                _ = textReader.MoveToElement();
            }
            bool flag = false;
            try
            {
                xMLNode.Data = new NSString(textReader.ReadElementContentAsString());
                goto IL_00A3;
            }
            catch (Exception)
            {
                flag = true;
                goto IL_00A3;
            }
        IL_009B:
            _ = ReadNode(textReader, xMLNode);
        IL_00A3:
            if ((!flag && !textReader.Read()) || textReader.Depth <= xMLNode.depth)
            {
                return xMLNode;
            }
            goto IL_009B;
        }

        public static XMLNode ParseXML(string fileName)
        {
            return ParseLINQ(fileName);
        }

        private static XMLNode ReadNodeLINQ(XElement nodeLinq, XMLNode parent)
        {
            XMLNode xMLNode = new();
            if (parent != null)
            {
                xMLNode.parent = parent;
                parent.childs_.Add(xMLNode);
            }
            xMLNode.Name = nodeLinq.Name.ToString();
            string text = (string)nodeLinq;
            if (text != null)
            {
                xMLNode.Data = new NSString(text);
            }
            foreach (XAttribute item in nodeLinq.Attributes())
            {
                xMLNode.attributes_.Add(item.Name.ToString(), item.Value);
            }
            foreach (XElement xelement in nodeLinq.Elements())
            {
                _ = ReadNodeLINQ(xelement, xMLNode);
            }
            return xMLNode;
        }

        private static XMLNode ParseLINQ(string fileName)
        {
            XDocument xDocument = null;
            try
            {
                Stream stream = TitleContainer.OpenStream("content/" + ResDataPhoneFull.ContentFolder + fileName);
                xDocument = XDocument.Load(stream);
                stream.Dispose();
            }
            catch (Exception)
            {
            }
            xDocument ??= XDocument.Parse(ResDataPhoneFull.GetXml(fileName));
            return ReadNodeLINQ(xDocument.Elements().First(), null);
        }

        private int depth;

        private XMLNode parent;

        private readonly List<XMLNode> childs_;
        private readonly Dictionary<string, string> attributes_;
    }
}
