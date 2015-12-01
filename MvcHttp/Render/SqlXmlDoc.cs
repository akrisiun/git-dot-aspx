using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.ComponentModel;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Configuration;
using System.Xml.Linq;

namespace AiLib.Render
{
    /// <summary>
    /// Summary description for SqlXmlDoc
    /// </summary>
    public class SqlXmlDoc
    {
        public static string SqlXmlDoc_ConnKey = "SNTXCCConnectionString";     // is Web.Config

        public static string ConnectionString {
            get 
            { 
                return ConfigurationManager.ConnectionStrings[SqlXmlDoc_ConnKey].ConnectionString;
            }
        }

        public static SqlCommand InitCmd(IRenderXslt ctrl)
        {
            SqlCommand cmd = null;
            SqlConnection conn = null;
            ctrl.LastErrorStr = "";

            try
            {
#if !NET40 && !NET45
                conn = prek.data.Sql.GetOpenConnection();
#else
                var connStr = new SqlConnectionStringBuilder(ConnectionString).ConnectionString;
                conn = new SqlConnection(connStr);
                conn.Open();
#endif
                conn.ChangeDatabase(ctrl.SqlDb);
                cmd = new SqlCommand(ctrl.SqlProc, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = ctrl.Timeout;
            }
            catch (Exception exp)
            {
                ctrl.TraceWrite(ctrl.ID, string.Format("SqlCommand={0} error<br/>{1}", ctrl.SqlProc, exp.Message));
                ctrl.LastError = exp;
                ctrl.LastErrorStr = exp.Message + (exp.InnerException != null ? exp.InnerException.Message : String.Empty);
                cmd = null;
            }
            return cmd;
        }


        public static XPathDocument SqlProcExecute(IRenderXslt ctrl
                    , string sqlParamPass
                    , string listParam, string listParamNum, string prmAdd, string prmApp
                    , string formParam, string formParamNum
                    , string rootNode
                    , Action<SqlParameter> parsePrm = null)
        {
            SqlCommand cmd = InitCmd(ctrl);
            if (cmd == null)
                return XPathError(ctrl.ID, ctrl.LastErrorStr);

            // HttpUtility.ParseQueryString(Request.QueryString.ToString(), Encoding.UTF8);
            System.Collections.Specialized.NameValueCollection collQuery = ctrl.QueryString;

            List<SqlParameter> listSqlParam = new List<SqlParameter>();

            if (!listParamNum.Equals(string.Empty))
                ParseParam(ctrl, listSqlParam, listParamNum, collQuery, SqlDbType.Int, 0);
            if (!listParam.Equals(string.Empty))
                ParseParam(ctrl, listSqlParam, listParam, collQuery
                        , SqlDbType.VarChar, Int32.Parse(ctrl.VarcharPrm));
            if (!prmAdd.Equals(string.Empty))
                ParseParam(ctrl, listSqlParam, prmAdd, null
                        , SqlDbType.VarChar, Int32.Parse(ctrl.VarcharAdd));

            if (!string.IsNullOrEmpty(prmApp))
            {
                string value = System.Configuration.ConfigurationManager.AppSettings.Get(prmApp);
                ctrl.TraceWrite(ctrl.ID, "appConfig key=" + prmApp + " value=" + value);
                listSqlParam.Add(new SqlParameter("@" + prmApp, value));
            }

#if WEB
            NameValueCollection collForm = ctrl.Context == null ? null : ctrl.Context.Request.Form;
            if (collForm != null && collForm.Count > 0 && (!formParam.Equals(string.Empty) || !formParamNum.Equals(string.Empty)))
            {
                ParseParam(ctrl, listSqlParam, formParamNum, collForm, SqlDbType.Int, 0);
                ParseParam(ctrl, listSqlParam, formParam, collForm, SqlDbType.Int, 0);
            }
#endif
            if (listSqlParam.Count > 0 && string.IsNullOrEmpty(sqlParamPass))
            {
                SqlParameter[] sqlParam = new SqlParameter[listSqlParam.Count];
                listSqlParam.CopyTo(sqlParam);
                cmd.Parameters.AddRange(sqlParam);

                ctrl.TraceWrite(ctrl.ID, "sqlParam:");
                foreach (var prm in sqlParam)
                    if (!prm.ParameterName.Equals("@p") && !prm.ParameterName.Equals("@passwd"))
                        ctrl.TraceWrite(ctrl.ID, prm.ParameterName + "=" + prm.SqlValue);

            }
            else if (!string.IsNullOrEmpty(sqlParamPass))
            {
                SqlParameter[] sqlParamTrans = ParseParamPass(sqlParamPass, listSqlParam, ctrl);
                if (sqlParamTrans != null && sqlParamTrans.Length > 0)
                    cmd.Parameters.AddRange(sqlParamTrans);

                ctrl.TraceWrite(ctrl.ID, "sqlParamTrans:");
                foreach (var prm in sqlParamTrans)
                    if (!prm.ParameterName.Equals("@p") && !prm.ParameterName.Equals("@passwd"))
                        ctrl.TraceWrite(ctrl.ID, prm.ParameterName + "=" + prm.SqlValue);
            }

            ctrl.SqlParamDict = new Dictionary<string,string>();
            foreach (SqlParameter prm in cmd.Parameters)
            {
                if (parsePrm != null)
                    parsePrm(prm);
                ctrl.SqlParamDict.Add(prm.ParameterName, prm.Value.ToString());
            }

            XPathDocument retXml = ExecCmd(ctrl, cmd, rootNode);
            // debug:  retXml.CreateNavigator().OuterXml
            return retXml;
        }

        public static XPathDocument SqlProcParameters(IRenderXslt ctrl
                    , List<SqlParameter> listSqlParam
                    , string rootNode)
        {
            SqlCommand cmd = InitCmd(ctrl);
            if (cmd == null)
                return XPathError(ctrl.ID, ctrl.LastErrorStr);

            if (listSqlParam != null && listSqlParam.Count > 0)
            {
                SqlParameter[] sqlParam = new SqlParameter[listSqlParam.Count];
                listSqlParam.CopyTo(sqlParam);
                cmd.Parameters.AddRange(sqlParam);
            }

            XPathDocument retXml = ExecCmd(ctrl, cmd, rootNode);
            // debug:  retXml.CreateNavigator().OuterXml
            return retXml;
        }


        public static XPathDocument ExecCmd(IRenderXslt ctrl, SqlCommand cmd, string rootNode)
        {

            XPathDocument retXml = null;
            try
            {
                using (XmlReader xmlRead = cmd.ExecuteXmlReader())
                {
                    retXml = new XPathDocument(xmlRead);
                }
            }
            catch (Exception ex)
            {
                ctrl.LastError = ex;
                var mergeMsg = ex.Message;
                if (ex.InnerException != null)
                    mergeMsg += Environment.NewLine + ex.InnerException.Message;
                ctrl.TraceWrite(ctrl.ID, mergeMsg);
                return XPathError(ctrl.ID, mergeMsg);
            }

            if (rootNode.Length == 0)
                return retXml;

            XmlDocument rootX = new XmlDocument();
            XmlDeclaration dec = rootX.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlNode xnode = rootX.CreateElement(rootNode);
            rootX.AppendChild(xnode);

            // Copy XPath to XPath: msdn.microsoft.com/en-us/library/5x8bxy86(v=vs.110).aspx
            XPathNavigator rootNav = rootX.CreateNavigator();
            rootNav.MoveToChild(rootNode, "");

            var sqlNav = retXml.CreateNavigator();
            foreach (XPathNavigator nav in sqlNav.Select("*"))
            {
                rootNav.AppendChild(nav);
            }

            XPathDocument retRoot = new XPathDocument(new XmlNodeReader(rootX));
            return retRoot;     // debug:  retRoot.CreateNavigator().OuterXml
        }


        private static XPathDocument XPathError(string id, string message)
        {
            //XmlDocument xmlErr = new XmlDocument();
            //xmlErr.LoadXml(String.Format("<Error ID=\"{0}\" message=\"{1}\" />", id, message));
            // XmlNodeReader read1 = new XmlNodeReader(xmlErr); // .SelectSingleNode("*"));
            var xmlErr = new XElement("Error", new XAttribute("ID", id ?? "-"),
                    XElement.Parse("<span>" + message.Replace("\n", "<br/>").Replace("&#39;", "") // .Replace("&quot;", "") 
                                 + "</span>")
                    );
            return new XPathDocument(xmlErr.CreateReader());
        }

        private static void ParseParam(IRenderXslt ctrl, List<SqlParameter> listSqlParam
                , string listPrm, NameValueCollection collQuery, SqlDbType sqlTypeDef, int varMax)
        {

            if (listPrm == null || listPrm.Equals(String.Empty))
                return;

            char[] separator = { ',' };
            string[] list = listPrm.Split(separator);

            foreach (string strItem in list)
            {
                string strKey = strItem.Trim();
                if (strKey.Length < 1) continue;
                object value = null;
                string str1 = strKey;
                SqlDbType sqlType = sqlTypeDef;

                string strSql = "@" + str1;
                if (str1.Contains("@"))
                {
                    string[] parse1 = str1.Split(new char[] { '@', ':', '=' });
                    strKey = parse1[0];
                    if (strKey.Equals(string.Empty))
                        strKey = parse1[1];
                    strSql = "@" + parse1[1];

                    string[] parse2 = str1.Split(new char[] { '@' });
                    str1 = parse2[1];
                }

                var prmKey = strKey.ToLower();
                if (collQuery == null || prmKey.Equals("userid") || prmKey.Equals("chuserid")
                     || prmKey.Equals("ip") || prmKey.Equals("site"))
                {
                    // Assert.IsTrue(str1.Contains("=") || str1.Contains("userid"));

                    string[] parse0 = str1.Split(new char[] { ':', '=' });
                    if (parse0[0].Contains("@"))
                        strSql = parse0[0];
                    if (strKey.Contains("="))
                        strKey = parse0[0].ToLower();

                    if (strKey.Equals("userid") || strKey.Equals("chuserid"))
                    {
                        sqlType = SqlDbType.SmallInt;
                        value = ctrl.UserID;
                    }
                    else if (strKey.Equals("site"))
                    {
                        sqlType = SqlDbType.VarChar;
                        // param from web.config
                        value = ConfigurationManager.AppSettings[strKey];
                    }
#if WEB
                    else if (strKey.Equals("ip") && ctrl != null && ctrl.Context != null)
                    {
                        sqlType = SqlDbType.VarChar;
                        value =  ctrl.Context.Request.UserHostAddress.ToString();
                    }
#endif
                    else
                        if (strKey.Length > 0)
                        {
                            string findVal = ctrl.GetParam(strKey);
                            if (findVal.Length == 0)     // not jet assigned
                            {
                                sqlType = SqlDbType.VarChar;
                                if (str1.Contains(":"))
                                {
                                    if (str1.Contains(":int"))
                                        sqlType = SqlDbType.Int;
                                    parse0[1] = parse0[2];
                                }
                                if (!strSql.Contains("@") || strSql.Contains("="))
                                    strSql = "@" + strKey;
                                if (parse0.Length > 1)
                                    value = parse0[1];

                                if (value is string && !string.IsNullOrEmpty(value as string))
                                {
                                    string strTrim = (value as string).Trim();
                                    if (strTrim.StartsWith("\'") && strTrim.EndsWith("\'"))
                                        value = strTrim.Length <= 2 ? string.Empty :
                                                strTrim.Substring(1, strTrim.Length - 2);
                                }
                            }
                        }

                }
                else if (ContainsKey(collQuery, strKey))
                {
                    value = collQuery[strKey];
                }

                SqlParameter item = null;
                if (value != null)
                {
                    // Assert.IsTrue(strSql.StartsWith("@"));

                    item = new SqlParameter(strSql, sqlType);
                    if (varMax > 0)
                    {
                        item.Size = varMax;
                        if (sqlType == SqlDbType.VarChar && value.ToString().Length > varMax)
                        {
                            ctrl.TraceWrite(ctrl.ID, "trim param=" + strKey + 
                                " value=" + value + " to Length=" + varMax);

                            value = value.ToString().Substring(0, varMax);
                        }
                    }

                    if (value is string && item.SqlDbType == SqlDbType.SmallInt)
                        item.Value = string.IsNullOrEmpty(value as string) ? 0 : Int16.Parse(value as string);
                    else
                        if (value is string && item.SqlDbType == SqlDbType.Int)
                            item.Value = string.IsNullOrEmpty(value as string) ? 0 : Int32.Parse(value as string);
                        else
                            item.Value = value;
                }

                if (item != null && !item.Value.ToString().Equals(string.Empty))
                {
                    ctrl.SetParam(strKey, item.Value.ToString());
                    listSqlParam.Add(item);

                    if (!strKey.Equals("p") && !strKey.Equals("passwd"))
                        ctrl.TraceWrite("param " + strKey, string.Format("{0}={1}", strSql, value.ToString()));
                }
            }

        }

        private static SqlParameter[] ParseParamPass(string sqlParamPass, List<SqlParameter> sqlParam, IRenderXslt ctrl)
        {
            var coll = new List<SqlParameter>();

            string str = sqlParamPass.Replace('\n', ' ').Replace('\r', ' ');
            string[] span = str.Split(new char[] { ',' });
            foreach (string item in span)
            {
                string[] eq = item.Trim().Split(new char[] { '=' });
                string sqlParamName = eq[0].Trim();
                object val = null;

                string localVar = eq.Length > 1 ? eq[1].Trim() : string.Empty;
                if (localVar.StartsWith("@"))
                {
                    // predicate find:  Predicate<T> match)
                    SqlParameter find = sqlParam.Find(x => { return x.ParameterName == localVar; });
                    if (find != null)
                        val = find.Value;
                    else
                        if (eq.Length >= 3)
                            localVar = eq[2].Trim();

                    if (val == null)
                        val = ctrl.GetParam(localVar.Substring(1));
                }

                if (!localVar.StartsWith("@"))
                {
                    int intVal = 0;
                    if (int.TryParse(localVar, out intVal))
                        val = intVal;
                    else
                    {
                        if (localVar.Equals(@"''"))
                            val = string.Empty;
                        else
                            val = localVar;

                        if (val is string && !string.IsNullOrEmpty(val as string))
                        {
                            string strTrim = (val as string).Trim();
                            if (strTrim.StartsWith("\'") && strTrim.EndsWith("\'"))
                                val = strTrim.Length <= 2 ? string.Empty :
                                      strTrim.Substring(1, strTrim.Length - 2);
                        }
                    }
                }

                if (val == null || string.IsNullOrEmpty(sqlParamName))
                    continue;

                if (!sqlParamName.Equals("p") && !sqlParamName.Equals("passwd"))
                    ctrl.TraceWrite(ctrl.ID + " pass", sqlParamName + " = " + val.ToString());
                SqlParameter prm = new SqlParameter(sqlParamName, val);
                coll.Add(prm);
            }

            var rez = Array.CreateInstance(typeof(SqlParameter), coll.Count) as SqlParameter[];
            coll.CopyTo(rez, 0);
            return rez;
        }

        //  System.Collections.Generic : public bool Contains (T item)
        private static bool ContainsKey(NameValueCollection collection, string key)
        {
            if (collection.Get(key) == null)
            {
                List<string> list = new List<string>(collection.AllKeys);
                if (!list.Contains(key))
                    return false;
            }
            return true;
        }

    }

}
