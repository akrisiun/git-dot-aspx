using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using AiLib.Web.Reflection;
using System.Collections.Specialized;
#if NET45
using AiLib.Web;
using System.Security.Claims;
#endif

namespace AiLib.Render
{

#if IISHOST
    using Microsoft.AspNet.Loader.IIS;
    using AiLib.IISHost;
    using AiLib.IIS;
#endif

    //Convert the book price to a new price using the conversion factor.
    public class RequestInfo
    {
        private IRenderXsltBase ctrl;
        // private IUserPrincipal user;

        private HttpRequest req;
        private NameValueCollection httpParams;

        public RequestInfo(IRenderXsltBase _ctrl, HttpRequest _req)
        {
            this.ctrl = _ctrl;
            this.req = _req;
            //this.user = HttpStatic.UserClaim;

            if (this.req != null)
                httpParams = this.req.Params;
            else
                httpParams = HttpStatic.ParseQueryString();
        }

        #region UserInfo

        //public UserInfo User
        //{
        //    get
        //    {
        //        return (user as UserInfoPrincipal).UserInfo;
        //    }
        //}

        public string getParam(string name)
        {
            if (req == null) return string.Empty;
            string val = req.Params[name];
            return val == null ? string.Empty : val;
        }

        public bool isParam(string name, string val)
        {
            return getParam(name).Equals(val);
        }

        /// <summary>
        /// TRUE if user is logged in
        /// </summary>
        /// <returns></returns>
        //public bool isLoggedIn()
        //{
        //    var userClaim = user as UserInfoPrincipal;
        //    return userClaim != null && userClaim.isLoggedIn;       // TODO Principal
        //}

        /// <summary>
        /// User login name (TUSERID)
        /// </summary>
        /// <returns></returns>
        //public string getLoginName()
        //{
        //    var userClaim = this.user as UserInfoPrincipal;
        //    return userClaim == null ? "" : userClaim.LoginName;
        //}

        public string UserZLogin()
        {
            var context = HttpContext.Current;
            if (context == null) return string.Empty;
            string z = req.Params["z"];
            // User login

            if (context.Trace != null)
                context.Trace.Write("login", "action z= " + z);

            if (z != null && z.Equals("1"))
            {
                string u = req.Params["u"], p = req.Params["p"];
                string ip = req.UserHostAddress.ToString();
                if (context.Trace != null)
                    context.Trace.Write("login", "u=" + u + " ip=" + ip);

                if (u != null && p != null
                    && u.Length > 0 && p.Length > 0)
                {
                    string err = string.Empty;
                    //try
                    //{
                    //    Claim.UserLogin(context, u, p, ip);
                    //}
                    //catch (Exception e)
                    //{
                    //    err = e.Message;
                    //}
                    //if (!err.Equals(string.Empty) && context.Trace != null)
                    //    context.Trace.Write("login", "exception " + err);

                    //if (User.LoggedIn)
                    //{
                    //    if (context.Trace != null)
                    //        context.Trace.Write("login", "Login OK " + u);
                    //    return User.LoginName;
                    //}
                }

                if (context.Trace != null)
                    context.Trace.Write("login", "Login FAILED");
            }
            return "";
        }

        public string UserZLogout()
        {
            if (HttpContext.Current == null) return string.Empty;
            string z = req.Params["z"];
            // User logout
            var context = HttpContext.Current;
            if (context.Trace != null)
                context.Trace.Write("logout", "action z= " + z);

            //if (z != null && z.Equals("2"))
            //{
            //    Claim.UserLogout(HttpContext.Current);
            //}
            return "";
        }

        /// <summary>
        /// First name of logged in user
        /// </summary>
        /// <returns></returns>
        public string getFirstName()
        {
            //if (User == null) 
                return "";
            //return User.FirstName;
        }

        /// <summary>
        /// Last name of logged in user
        /// </summary>
        /// <returns></returns>
        //public string getLastName()
        //{
        //    if (User == null) return "";
        //    return User.LastName;
        //}

        /// <summary>
        /// Full user name: first name and last name
        /// </summary>
        /// <returns></returns>
        //public string getUserName()
        //{
        //    if (User == null) return "guest";
        //    return User.UserName;
        //}

        public bool isAction(string val)
        {
            if (req == null) return false;
            string action = req.Params["action"];
            return action != null && action.Equals(val);
        }

        /// <summary>
        /// TUSER.USERID
        /// </summary>
        /// <returns></returns>
        //public short getUserId()
        //{
        //    if (User == null) return 0;
        //    return User.UserId;
        //}

        /// <summary>
        /// TUSER.ISADMIN
        /// </summary>
        /// <returns></returns>
        //public bool isAdmin()
        //{
        //    if (User == null) return false;
        //    return User.IsAdmin;
        //}

        #endregion

        // Translate
        public string Tr(string text)
        {
            return AiLib.Trans.Tr(text);
        }

        public string Lang()
        {
            return StringEq.ToUpperNull(Trans.Lang)
                   // SegmentLang.Instance.Lang
                   ?? (StringEq.ToUpperNull(ConfigurationManager.AppSettings.Get("dir.lang"))
                       ?? string.Empty);
        }

        public string DataLang()
        {
            return AiLib.Trans.TrLang 
                ?? (ConfigurationManager.AppSettings.Get("tr.lang")
                    ?? string.Empty);
        }

        // Http params
        public string Param(string key)
        {
            var val = HttpStatic.Get<string>(httpParams, key);
            var res = val;
            if (val.ContainsCase("\"") || val.ContainsCase("Ė"))
                res = HttpUtility.HtmlEncode(val);

            return res ?? string.Empty;
        }

        // Http params
        public int ParamInt(string key)
        {
            var value = httpParams[key];
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            int num = 0;
            if (Int32.TryParse(value, out num))
                return num;

            return 0;
        }

        public string getAppSetting(string key)
        {
            List<string> list = new List<string>(ConfigurationManager.AppSettings.AllKeys);
            if (!list.Contains(key)) return string.Empty;
            return ConfigurationManager.AppSettings[key];
        }
        public string getIP()
        {
            if (req == null) return string.Empty;
            return req.UserHostAddress;
        }
        public string getReferer()
        {
            if (req == null) return string.Empty;
            return (req.UrlReferrer == null) ? "" : req.UrlReferrer.ToString();
        }

        public bool isPageHasReferer()
        {
            if (req == null) return false;
            bool hasRefer = false;
#if WEB
            try
            {
                hasRefer = (req.UrlReferrer.PathAndQuery.ToString().Length > 0);
            }
            catch (Exception) { }
#endif
            return hasRefer;
        }

        // html escape text
        public string getHtmlEncode(string str)
        {
#if IISHOST
            return System.Xml.XmlConvert.EncodeName(str);
#else
            return HttpUtility.HtmlEncode(str);
#endif
        }

        // html unescape
        public string getHtmlDecode(string str)
        {
            // not works with HttpUtility.HtmlDecode(str);
            return System.Xml.XmlConvert.DecodeName(str);
        }

        // parametras is SqlXml.PrmAdd()
        public string getParam2(string prm)
        {
            string val = ctrl == null ? null : ctrl.GetParam(prm);
            if ((val == null || val.Length == 0) && req != null)
                val = req.Params[prm];

            return val == null ? "" : val;
        }

        // Sql
        public string getParamSql(string key)
        {
            string val = ctrl == null ? null : ctrl.GetParamSql(key);
            return val ?? String.Empty;
        }

        // parametro reiksmes palyginimas is SqlXml.PrmAdd() 
        public bool isParam2(string prm, string valEq)
        {
            string val = ctrl == null ? null : ctrl.GetParam(prm);
            if ((val == null || val.Length == 0) && req != null)
                val = req.Params[prm];

            return val != null && val.Equals(valEq);
        }

    }

}