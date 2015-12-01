using System;
using System.Data.SqlClient;
using System.IO;
using System.Xml.XPath;

namespace AiLib.Render
{

    // <snx:AspxPageMenu ID = "pagetopx" runat="server" PrmAdd="link=asortx" />

    // <snx:AspxProcRender ID = "pagetopx" runat="server" IsDebug="0"
    //    SqlProc='dbo.jmenu_top' PrmAdd="link=asortx"
    //    PrmListNum="userid" Xslt="../pagemenux.xslt" Timeout="2000" />

    public class AspxPageMenu : AspxProcRender
    {
        public AspxPageMenu()
        {
            SqlProc = "dbo.jmenu_top";
            PrmListNum = "userid";
            SqlParam = "@site = 'PRK', @userid = @userid, @link = @link, @FIRMID = @FirmID";
            Xslt = "../pagemenux.xslt";
            Timeout = 2000;
            VarcharAdd = "15";
        }

        public string link { get; set; }

        public override void RenderHtml(TextWriter writer, XPathDocument xmlDoc)
        {
            if (isDebug > 0)
                writer.WriteLine("[PageMenu=" + this.PrmAdd ?? "" + "]");
            if (this.PrmAdd != null && PrmAdd.Contains("link="))
                link = PrmAdd.Substring(5);

            base.RenderHtml(writer, xmlDoc);
        }

        public override void ParseSqlParam(SqlParameter prm)
        {
            base.ParseSqlParam(prm);

            if (prm.ParameterName.Equals("@link", StringComparison.InvariantCultureIgnoreCase) && link != null)
                prm.Value = link;
        }
    }
}
