using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AiLib.Render
{
    public class XsltIncludeResolver : XmlUrlResolver
    {
        public XsltIncludeResolver(string serverUrl)
        {
            this.serverUri = new Uri(serverUrl);
        }
        private Uri serverUri;

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            if (baseUri != null)
                return base.ResolveUri(baseUri, relativeUri);
            else
                return base.ResolveUri(serverUri, relativeUri);
        }
    }
}
