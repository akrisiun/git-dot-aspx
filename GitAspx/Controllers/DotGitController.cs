using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GitAspx.Controllers
{
    public class DotGitController : Controller
    {
        public DotGitController()
        {
        }

        protected override void HandleUnknownAction(string actionName)
        {
            base.HandleUnknownAction(actionName);
        }

        protected override HttpNotFoundResult HttpNotFound(string statusDescription)
        {
            return base.HttpNotFound(statusDescription);
        }

        // GET: DotGit
        public ActionResult Index()
        {
            var method = Request.HttpMethod;
            Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
            Response.Write("OK " + Request.Url.PathAndQuery);
            return new EmptyResult();
        }
    }
}