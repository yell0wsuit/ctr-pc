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
    // Token: 0x0200001C RID: 28
    internal class XMLNode
    {
        // Token: 0x1700001D RID: 29
        // (get) Token: 0x060000FB RID: 251 RVA: 0x000057CA File Offset: 0x000039CA
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        // Token: 0x1700001E RID: 30
        // (get) Token: 0x060000FC RID: 252 RVA: 0x000057D2 File Offset: 0x000039D2
        public NSString data
        {
            get
            {
                return this.value;
            }
        }

        // Token: 0x1700001F RID: 31
        public NSString this[string key]
        {
            get
            {
                string rhs = null;
                if (!this.attributes_.TryGetValue(key, out rhs))
                {
                    return new NSString("");
                }
                return new NSString(rhs);
            }
        }

        // Token: 0x060000FE RID: 254 RVA: 0x0000580C File Offset: 0x00003A0C
        public XMLNode()
        {
            this.parent = null;
            this.childs_ = new List<XMLNode>();
            this.attributes_ = new Dictionary<string, string>();
        }

        // Token: 0x060000FF RID: 255 RVA: 0x00005831 File Offset: 0x00003A31
        public bool attributes()
        {
            return this.attributes_ != null && this.attributes_.Count > 0;
        }

        // Token: 0x06000100 RID: 256 RVA: 0x0000584B File Offset: 0x00003A4B
        public List<XMLNode> childs()
        {
            return this.childs_;
        }

        // Token: 0x06000101 RID: 257 RVA: 0x00005854 File Offset: 0x00003A54
        public XMLNode findChildWithTagNameAndAttributeNameValueRecursively(string tag, string attrName, string attrVal, bool recursively)
        {
            if (this.childs() == null)
            {
                return null;
            }
            foreach (XMLNode item in this.childs_)
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

        // Token: 0x06000102 RID: 258 RVA: 0x00005900 File Offset: 0x00003B00
        public XMLNode findChildWithTagNameRecursively(NSString tag, bool recursively)
        {
            return this.findChildWithTagNameRecursively(tag.ToString(), recursively);
        }

        // Token: 0x06000103 RID: 259 RVA: 0x00005910 File Offset: 0x00003B10
        public XMLNode findChildWithTagNameRecursively(string tag, bool recursively)
        {
            if (this.childs() == null)
            {
                return null;
            }
            foreach (XMLNode item in this.childs_)
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

        // Token: 0x06000104 RID: 260 RVA: 0x00005998 File Offset: 0x00003B98
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
            XMLNode.ReadNode(textReader, xMLNode);
        IL_00A3:
            if ((!flag && !textReader.Read()) || textReader.Depth <= xMLNode.depth)
            {
                return xMLNode;
            }
            goto IL_009B;
        }

        // Token: 0x06000105 RID: 261 RVA: 0x00005A74 File Offset: 0x00003C74
        public static XMLNode parseXML(string fileName)
        {
            return XMLNode.ParseLINQ(fileName);
        }

        // Token: 0x06000106 RID: 262 RVA: 0x00005A7C File Offset: 0x00003C7C
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
                XMLNode.ReadNodeLINQ(xelement, xMLNode);
            }
            return xMLNode;
        }

        // Token: 0x06000107 RID: 263 RVA: 0x00005B64 File Offset: 0x00003D64
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
            return XMLNode.ReadNodeLINQ(xDocument.Elements().First<XElement>(), null);
        }

        // Token: 0x04000096 RID: 150
        private int depth;

        // Token: 0x04000097 RID: 151
        private XMLNode parent;

        // Token: 0x04000098 RID: 152
        private List<XMLNode> childs_;

        // Token: 0x04000099 RID: 153
        private string name;

        // Token: 0x0400009A RID: 154
        private NSString value;

        // Token: 0x0400009B RID: 155
        private Dictionary<string, string> attributes_;
    }
}
