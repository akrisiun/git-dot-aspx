using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using Linq = System.Xml.Linq;

namespace AiLib.Web
{
    public static class XPathWeb
    {
        public static void XTransformToWriter(this Linq.XElement xmlDoc, TextWriter writer,
                    Linq.XElement xmlXslt = null, // Action<XslCompiledTransform> onXsltLoad = null, 
                    object xsltExtension = null,
                    string urn = "urn:request-info")
        {
            XslCompiledTransform trans = new XslCompiledTransform();
            //if (onXsltLoad != null)
            //    onXsltLoad(trans);
            //else
                trans.Load(xmlXslt.CreateReader());

            if (xsltExtension != null)
            {
                XsltArgumentList xslArg = new XsltArgumentList();
                xslArg.AddExtensionObject(urn, xsltExtension);

                // XTransformTo(trans, xmlDoc.CreateReader(), xslArg, writer);
                // var results = writer;
                XmlReader input = xmlDoc.CreateReader();
                XmlWriterSettings outputSettings = trans.OutputSettings;
                using (XmlWriter writerXml = XmlWriter.Create(writer, outputSettings))
                {
                    trans.Transform(input, arguments: xslArg, results: writerXml, 
                          documentResolver: XmlNullResolver.Singleton); // XsltConfigSection.CreateDefaultResolver());
                    writerXml.Close();
                }
            }
            else 
            {
                // trans.Transform(xmlDoc.CreateNavigator() as IXPathNavigable, arguments: null, results: writer);
                XTransformTo(trans, xmlDoc.CreateReader(), null, writer);
            }
        }

        public static void XTransformTo(this XslCompiledTransform trans, XmlReader input,
                XsltArgumentList arguments, TextWriter results)
        {
            Guard.CheckArgumentNull(input);
            Guard.CheckArgumentNull(results);

            var outputSettings = trans.OutputSettings;
            using (XmlWriter writer = XmlWriter.Create(results, outputSettings))
            {
                trans.Transform(input, arguments, writer, XmlNullResolver.Singleton); // XsltConfigSection.CreateDefaultResolver());
                writer.Close();
            }
        }
    }

    // http://referencesource.microsoft.com/#System.Xml/System/Xml/XmlNullResolver.cs
    internal class XmlNullResolver : XmlResolver
    {
        public static readonly XmlNullResolver Singleton = new XmlNullResolver();

        // Private constructor ensures existing only one instance of XmlNullResolver
        private XmlNullResolver() {}

        public override Object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            System.Diagnostics.Debugger.Break();
            throw new XmlException("GetEntity error");
        }

        public override System.Net.ICredentials Credentials
        {
            set { throw new NotImplementedException(); }
        }
    }

}
