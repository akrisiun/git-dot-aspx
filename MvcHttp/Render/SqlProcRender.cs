// SqlProcRender.cs Puslapio "gabaliuko" gamyba, gaunat XML is Sql proceduros/XSLT transformacija i HTML

using System;
using System.Net;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using AiLib.Render;
using AiLib.Entity;

#if !NET40 && !NET45
    using prek.data;
#endif
#if WEB
    using System.Web;
    using System.Web.UI;
using AiLib.Web;
    // using System.Web.UI.Design;     // System.Design.dll
#else
    using AiLib.IISHost;
    using RazorEngine.Text;
#endif

namespace AiLib.Render
{
    public struct Bin
    {
        public const string Version = "2.0.1";
    }

    /// <summary>
    ///		Summary description for sqlxml.
    /// </summary>
#if WEB
    [ToolboxData("<{0}:SqlProcRender runat=\"server\" />")]
#endif
    // [Designer(typeof(SqlProcRenderDesigner))]
    public class SqlProcRender : RenderXsltBase, IRenderXslt, IRenderBase, ILastError
    {

        #region Params_Section                    // SqlXml params

        protected string dirXslt = "";
        public String DirXslt          // Xlst subdirectory value
        {
            set
            {
                TraceWrite("sqlxml", "set DirXslt: value=" + value);
                dirXslt = value;
            }
            get { return dirXslt; }
        }

        protected string sqlproc = "";
        public new string SqlProc
        {
            set { sqlproc = value; }
            get { return sqlproc; }
        }

        protected string sqlParam = "";
        public string SqlParam
        {
            get { return sqlParam; }
            set { sqlParam = value; }
        }

        #endregion

        public new int UserID
        {
            get
            {
#if !NET40 && !NET45
                if (this.Context != null)
                {
                    try
                    {
                        UserInfo user = this.Context.Session[UserInfo.SESSION_KEY] as prek.data.UserInfo;
                        if (user != null) return user.UserId;
                    }
                    catch (Exception) { }
                }
#endif
                return 0;
            }
        }

        public virtual string GetParamSql(string key)
        {
            if (SqlParamDict == null) return null;
            string val = "";
            if (SqlParamDict.TryGetValue(key, out val))
                return val;
            return "";
        }

        public Dictionary<string, string> SqlParamDict { get; set; }

        public virtual XPathDocument SqlProcParameters(List<SqlParameter> listSqlParam)
        {
            XPathDocument xmlDoc = SqlXmlDoc.SqlProcParameters(this, listSqlParam, "Root");
            return xmlDoc;
        }

        private string xsltFileFull = null;

        public override string Render(TextWriter writer)
        {
            RenderHtml(writer, Prepare());
            return string.Empty;
        }

        public virtual void RenderHtml(TextWriter writer, XPathDocument xmlDoc)
        {
            // Transform XLST validation
            xsltFileFull = Xslt;
            if (!xsltFileFull.Contains(@":\") && Context != null)
                xsltFileFull = Context.Server.MapPath(dirXslt + Xslt);

            Trace.Write("Transform ID=" + this.ID, "xslt=" + xsltFileFull);

            XsltArgumentList xslArg = new XsltArgumentList();
            XslCompiledTransform trans = new XslCompiledTransform();

            if (Context != null && Xslt.Length > 0 && File.Exists(xsltFileFull))
            {
#if WEB
                string serverUrl = Context.Request.Url.Scheme + "://" + Context.Request.Url.Authority + "/";
#else
                string serverUrl = Context.Request.Url.Scheme + "://" + Context.Request.Url.Authority + "/";
                // serverUrl = Context.Request.QueryString;
#endif
                XsltIncludeResolver resolver = new XsltIncludeResolver(serverUrl);  // for <xsl:include>
                trans.Load(xsltFileFull, XsltSettings.TrustedXslt, resolver as XmlUrlResolver);
            }
            else
            {
                var reader = XmlReader.Create(System.IO.File.OpenRead(xsltFileFull));
                trans.Load(stylesheet: reader);
            }

            Trace.Write("Transform ID=" + this.ID, "XSLT Parse Test OK");

            // Add an object to convert
            RequestInfo info = null;
            if (Context != null)
                info = new RequestInfo(this, Context.Request);
            else
                info = new RequestInfo(this, null);

            xslArg.AddExtensionObject("urn:request-info", info);

            if (isDebug <= 1)
                trans.Transform(xmlDoc, xslArg, writer);
        }

#if WEB // using System.Web

        protected override void Render(HtmlTextWriter writer)
        {
            TraceWrite(string.Format("V{0} sqlxml.ID={1}", Bin.Version, this.ID)
                      , string.Format("proc={0} xslt={1}", this.SqlProc, this.Xslt));

            HttpRequest Request = Context.Request;
            if (Request == null)
            {
                TraceWrite("sqlxml=" + ID, "<no Request>");
                writer.Write("Error: No request");
                return;
            }

            TraceContext Trace = Context.Trace;

            // General client request info values 
            Trace.Write("IPAddress", Request.UserHostAddress);
            if (Request.UrlReferrer != null) Trace.Write("Referrer", Request.UrlReferrer.ToString());
            Trace.Write("Url", Request.Url.ToString());

            DateTime tStart = DateTime.Now;
            // Create argument list to pass to XSLT

            XPathDocument xmlDoc = null;
            try
            {
                xmlDoc = Prepare();
                RenderHtml(writer, xmlDoc);
                // Debug: xmlDoc.CreateNavigator().OuterXml

            }
            catch (Exception exp)
            {
                Log.Write("SqlProcRender." + Bin.Version + " exception : " + exp.Message
                        + "\n Url=" + Request.Url + "\n ip=" + Request.UserHostAddress
                        + " sqlxml.id=" + this.ID + " XSLT=" + dirXslt + Xslt);
                var message = exp.Message;
                var stack = exp.StackTrace.PadRight(400).TrimEnd();
                if (exp.InnerException != null)
                {
                    var ex2 = exp.InnerException;
                    message += " | " + ex2.Message;
                    stack = ex2.StackTrace.PadRight(400).TrimEnd();
                    if (ex2.InnerException != null)
                        message += " | " + ex2.InnerException.Message;
                }

                Trace.Write("XSLT ID=" + this.ID, "Error: " + message + "\n" + stack + "\n"
                        + " XSLT=" + dirXslt + Xslt + " )");
                writer.Write("</br><span class=\"error\">Render." + Bin.Version
                        + " Xml Error: " + message.Replace("|", "<br/>")
                        + " (ID=" + this.ID + " XSLT=" + dirXslt + Xslt + " )</span>");
                return;
            }

            if (xmlDoc != null)
            {
                if (isDebug > 0)
                {
                    writer.Write("<br/>");
                    writer.Write("<br/>xmlDoc<br/><code><pre>");
                    if (isDebug == 3)      // for Chrome/Mozilla browser output
                        writer.Write(xmlDoc.CreateNavigator().OuterXml);
                    else                   // IE debug output
                        writer.Write(HttpUtility.HtmlEncode(xmlDoc.CreateNavigator().OuterXml));

                    writer.Write("</pre></code>");
                    writer.Write("<br/>xsltFileFull=" + xsltFileFull + "<br/><code><pre>");
                    if (isDebug == 3)
                        writer.Write(File.ReadAllText(xsltFileFull).ToString());
                    else                   // IE debug output 
                        writer.Write(HttpUtility.HtmlEncode(File.ReadAllText(xsltFileFull).ToString()));
                    writer.Write("</code></pre>");
                }

                Trace.Write("V" + Bin.Version + " SqmlXml.ID=" + this.ID, "Finish Render");
            }

        }

#endif
        #region Prepare XmlDoc

        public virtual XPathDocument Prepare(bool isClear = true)
        {
            if (isClear && keyParam != null) keyParam.Clear();
            return XmlDoc;
        }

        public virtual void ParseSqlParam(SqlParameter prm)
        {
            if (prm.ParameterName.Equals("site", StringComparison.OrdinalIgnoreCase)
                && String.IsNullOrWhiteSpace(prm.Value.ToString()))
                prm.Value = AiLib.Web.Segment.Instance.Site;
            if (prm.ParameterName.Equals("Lang", StringComparison.OrdinalIgnoreCase)
                && String.IsNullOrWhiteSpace(prm.Value.ToString()))
                prm.Value = AiLib.Web.Segment.Instance.Lang;
        }

        public XPathNavigator Navigator() { return XmlDoc.CreateNavigator(); }
        public XPathNavigator SelectSingleNode(string xpath)
        {
            return XmlDoc == null ? null : Navigator().SelectSingleNode(xpath);
        }
        public string OuterXml { get { return xmlDoc == null ? null : Navigator().OuterXml; } }

        public Exception LastError { get; set; }

        protected XPathDocument xmlDoc = null;
        public virtual XPathDocument XmlDoc
        {
            get
            {
                if (xmlDoc == null)
                {
                    var sqlParam = this.SqlParam;
                    xmlDoc = SqlXmlDoc.SqlProcExecute(this,
                           sqlParam, listParam, listParamNum, prmAdd, prmApp
                           , formParam, formParamNum, "Root"
                           , (prm) => ParseSqlParam(prm));
                }
                return xmlDoc;
            }
            set { xmlDoc = value as XPathDocument; }
        }

        public void SetXmlDoc(XDocument doc)
        {
            xmlDoc = new XPathDocument(doc.CreateReader());
        }

        #endregion

#if WEB
        public virtual string GetHtml()
        {
            Prepare();
            using (var writer = new HtmlTextWriter(new StringWriter()))
            {
                if (Context != null)
                    Render(writer);
                else
                {
                    RenderHtml(writer, XmlDoc);
                }
                return writer.InnerWriter.ToString();
            }
        }
#else
        public virtual RazorEngine.Text.RawString GetHtml()
        {
            var writer = new StringWriter();  // new StreamWriter(
            Prepare();
            RenderHtml(writer, XmlDoc);
            
            //var task = AiLib.IISHost.HeliosApplication.Current.RenderHtml(writer, null);
            //task.Wait();
            return new RawString(writer.ToString());
        }
#endif

    }

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

#if !NET40 && !NET45
    public class SqlProcRenderDesigner : System.Web.UI.Design.ControlDesigner
    {

        public override string GetDesignTimeHtml()
        {
            string str = "<div style=\"border: 1px solid silver;\">[" + Bin.Version + " SqlXml]";

            SqlProcRender con = base.Component as SqlProcRender;
            if (con != null)
                str += string.Format(".ID={0}</br><nobr>", con.ID)
                        + string.Format("SqlProc={0} ", con.SqlProc)
                        + string.Format("Xslt=<a href=\"{0}\">{0}</a>", con.Xslt)
                        + "</nobr>";
            str += "</div>";
            return str;
        }

        public override void Initialize(IComponent component)
        {
            // if (!(component is SqlProcRender)) throw new ArgumentException();
            base.Initialize(component);
        }

    }
#endif

}
