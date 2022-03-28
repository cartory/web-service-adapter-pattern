using System;
using System.IO;
using System.Web;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace utils.XmlAdapter
{
    public abstract class RequestOptions { }

    public abstract class XmlAdapter
    {
        protected abstract string Fetch(RequestOptions requestOptions);

        public virtual string ToXml(string json)
        {
            Dictionary<string, string> args = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            XmlDocument xml = new XmlDocument();

            XmlDeclaration xmlDeclaration = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            xml.InsertBefore(xmlDeclaration, xml.DocumentElement);

            XmlElement xmlResponse = xml.CreateElement(string.Empty, "XMLResponse", string.Empty);
            xml.AppendChild(xmlResponse);

            Array.ForEach(args.Keys.ToArray(), key => {
                XmlElement tag = xml.CreateElement(string.Empty, key, string.Empty);
                XmlText xmlText = xml.CreateTextNode(args[key]);

                tag.AppendChild(xmlText);
                xmlResponse.AppendChild(tag);
            });

            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xml.WriteTo(xmlWriter);
                    xmlWriter.Flush();

                    return stringWriter.GetStringBuilder().ToString();
                }
            }
        }
    }
}