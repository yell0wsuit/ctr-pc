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
    internal class XMLNode
    {
        // (get) Token: 0x060000FB RID: 251 RVA: 0x000057CA File Offset: 0x000039CA
        public string Name
        {
            get
            {
                return name;
            }
        }

        // (get) Token: 0x060000FC RID: 252 RVA: 0x000057D2 File Offset: 0x000039D2
        public NSString data
        {
            get
            {
                return value;
            }
        }

        public NSString this[string key]
        {
            get
            {
                string rhs = null;
                if (!attributes_.TryGetValue(key, out rhs))
                {
                    return new NSString("");
                }
                return new NSString(rhs);
            }
        }

        public XMLNode()
        {
            parent = null;
            childs_ = new List<XMLNode>();
            attributes_ = new Dictionary<string, string>();
        }

        public bool attributes()
        {
            return attributes_ != null && attributes_.Count > 0;
        }

        public List<XMLNode> childs()
        {
            return childs_;
        }

        public XMLNode findChildWithTagNameAndAttributeNameValueRecursively(string tag, string attrName, string attrVal, bool recursively)
        {
            if (childs() == null)
            {
                return null;
            }
            foreach (XMLNode item in childs_)
            {
                string text;
                if (item.name == tag && item.attributes() && item.attributes_.TryGetValue(attrName, out text) && text == attrVal)
                {
                    return item;
                }
                if (recursively && item.childs() != null)
                {
                    XMLNode xMLNode = item.findChildWithTagNameRecursively(tag, recursively);
                    if (xMLNode != null)
                    {
                        return xMLNode;
                    }
                }
            }
            return null;
        }

        public XMLNode findChildWithTagNameRecursively(NSString tag, bool recursively)
        {
            return findChildWithTagNameRecursively(tag.ToString(), recursively);
        }

        public XMLNode findChildWithTagNameRecursively(string tag, bool recursively)
        {
            if (childs() == null)
            {
                return null;
            }
            foreach (XMLNode item in childs_)
            {
                if (item.name == tag)
                {
                    return item;
                }
                if (recursively && item.childs() != null)
                {
                    XMLNode xMLNode = item.findChildWithTagNameRecursively(tag, recursively);
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
            xMLNode.name = textReader.Name;
            xMLNode.depth = textReader.Depth;
            if (textReader.HasAttributes)
            {
                while (textReader.MoveToNextAttribute())
                {
                    xMLNode.attributes_.Add(textReader.Name, textReader.Value);
                }
                textReader.MoveToElement();
            }
            bool flag = false;
            try
            {
                xMLNode.value = new NSString(textReader.ReadElementContentAsString());
                goto IL_00A3;
            }
            catch (Exception)
            {
                flag = true;
                goto IL_00A3;
            }
        IL_009B:
            ReadNode(textReader, xMLNode);
        IL_00A3:
            if ((!flag && !textReader.Read()) || textReader.Depth <= xMLNode.depth)
            {
                return xMLNode;
            }
            goto IL_009B;
        }

        public static XMLNode parseXML(string fileName)
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
            xMLNode.name = nodeLinq.Name.ToString();
            string text = (string)nodeLinq;
            if (text != null)
            {
                xMLNode.value = new NSString(text);
            }
            foreach (XAttribute item in nodeLinq.Attributes())
            {
                xMLNode.attributes_.Add(item.Name.ToString(), item.Value);
            }
            foreach (XElement xelement in nodeLinq.Elements())
            {
                ReadNodeLINQ(xelement, xMLNode);
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
            if (xDocument == null)
            {
                xDocument = XDocument.Parse(ResDataPhoneFull.GetXml(fileName));
            }
            return ReadNodeLINQ(xDocument.Elements().First(), null);
        }

        private int depth;

        private XMLNode parent;

        private List<XMLNode> childs_;

        private string name;

        private NSString value;

        private Dictionary<string, string> attributes_;
    }
}
