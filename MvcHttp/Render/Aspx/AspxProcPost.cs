using System;
using System.Net;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
// using System.Web.UI.Design;     // System.Design.dll
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using AiLib.Render;
#if !NET40 && !NET45
using prek.data;
#endif

namespace AiLib.Render.sqlproc
{
    [ToolboxData("<{0}:AspxProcPost runat=\"server\" SqlProc=\"\" />")]
    // [Designer(typeof(SqlProcRenderDesigner))]
    public class AspxProcPost : AspxProcRender
    {
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

            if (keyParam != null) keyParam.Clear();
            XPathDocument xmlDoc = SqlXmlDoc.SqlProcExecute(this, this.SqlParam
                                        , listParam, listParamNum, prmAdd, prmApp
                                        , formParam, formParamNum
                                        , "Root");
            // Debug: xmlDoc.CreateNavigator().OuterXml

            DateTime tStart = DateTime.Now;

            // Create argument list to pass to XSLT
            XsltArgumentList xslArg = new XsltArgumentList();
            XslCompiledTransform trans = new XslCompiledTransform();

            // Transform XLST validation
            string xsltFileFull = Context.Server.MapPath(dirXslt + Xslt);

            try
            {
                Trace.Write("Transform ID=" + this.ID, "xslt=" + xsltFileFull);

                if (Xslt.Length > 0 && File.Exists(xsltFileFull))
                {
                    string serverUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/";
                    XsltIncludeResolver resolver = new XsltIncludeResolver(serverUrl);  // for <xsl:include>
                    trans.Load(xsltFileFull, XsltSettings.TrustedXslt, resolver as XmlUrlResolver);
                }

                Trace.Write("Transform ID=" + this.ID, "XSLT Parse Test OK");
            }
            catch (Exception exp)
            {
                Log.Write("SqlProcRender." + Bin.Version + " exception : " + exp.Message
                        + "\n Url=" + Request.Url + "\n ip=" + Request.UserHostAddress
                        + " sqlxml.id=" + this.ID + " XSLT=" + dirXslt + Xslt);
                Trace.Write("XSLT ID=" + this.ID, "Error: " + exp.Message
                        + " XSLT=" + dirXslt + Xslt + " )");
                writer.Write("</br><span class=\"error\">Render." + Bin.Version
                        + " Xml Error: " + exp.Message
                        + " (ID=" + this.ID + " XSLT=" + dirXslt + Xslt + " )</span>");
                return;
            }

            if (xmlDoc != null && trans != null)
            {
                try
                {
                    // Add an object to convert
                    RequestInfo info = new RequestInfo(this, Request);

                    xslArg.AddExtensionObject("urn:request-info", info);
                    if (isDebug <= 1)
                        trans.Transform(xmlDoc, xslArg, writer);
                }
                catch (Exception exp)
                {
                    Trace.Write("Render." + Bin.Version
                            , " Transform error: " + exp.Message);
                    Log.Write("sqlxml transform : " + exp.Message
                            + "\n Url=" + Request.Url + "\n ip=" + Request.UserHostAddress
                            + " sqlxml.id=" + this.ID + " XSLT=" + dirXslt + Xslt);
                    writer.Write("</br><span class=\"error\">Render." + Bin.Version
                            + " Transform error: " + exp.Message);
                    return;
                }


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

    }
}