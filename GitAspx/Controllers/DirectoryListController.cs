#region License

// Copyright 2010 Jeremy Skinner (http://www.jeremyskinner.co.uk)
//  
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://github.com/JeremySkinner/git-dot-aspx

#endregion

namespace GitAspx.Controllers
{
    using System.Web.Mvc;
    using GitAspx.Lib;
    using GitAspx.ViewModels;
    using System.Linq;
    using System;
    using StructureMap;
    using System.Web;
    using System.IO;
    using System.Threading;

    public class DirectoryListController : Controller
    {
        public static object Settings()
        {
            return AppSettings.FromAppConfig();
        }

        public static void RenderIndex()
        {
            Type controllerType = typeof(DirectoryListController);
            var controller = ObjectFactory.GetInstance(controllerType) as DirectoryListController;
            var repositories = controller.repositories;

            HttpContext context = System.Web.HttpContext.Current;
            try
            {
                var settings = RepositoryService.AppSettings;
                var model = new DirectoryListViewModel
                  {
                      RepositoriesDirectory = repositories.GetRepositoriesDirectory().FullName,
                      Repositories = repositories.GetAllRepositories(null, null)
                        .Select(x => new RepositoryViewModel(x))
                  };
                var view = controller.Index(null, null);

                var baseCtx = context.Request.RequestContext.HttpContext as HttpContextBase;
                var routeData = new System.Web.Routing.RouteData();
                routeData.Values.Add("Action", "Index");
                routeData.Values.Add("Controller", "DirectoryList");
                var mvcCtx = new ControllerContext(baseCtx, routeData, controller);
                view.ExecuteResult(mvcCtx);
            }
            catch (Exception ex)
            {
                context.Response.Write("<b>Error</b><br/> " + ex.ToString().Replace(Environment.NewLine, "<br/>"));
            }
        }

        readonly RepositoryService repositories;

        public DirectoryListController(RepositoryService repositories)
        {
            this.repositories = repositories;
        }

        public ActionResult Index(string cat, string subcat)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = this.GetWebBrowsingSettings().CultureObject;

            return View(new DirectoryListViewModel
            {
                RepositoryLevel = repositories.RepositoryLevel,
                RepositoriesDirectory = repositories.GetRepositoriesDirectory().FullName,
                RepositoryCategory = repositories.CombineRepositoryCat(cat, subcat),
                Repositories = repositories.GetAllRepositories(cat, subcat).Select(x => new RepositoryViewModel(x))
            });
        }

        [HttpPost]
        public ActionResult CreateRepository(string cat, string subcat, string project)
        {
            if (!string.IsNullOrEmpty(project))
                repositories.CreateRepository(cat, subcat, project);

            return RedirectToAction("Index", new { cat, subcat });
        }

        public ActionResult Cat(string cat)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = this.GetWebBrowsingSettings().CultureObject;

            string lsRoot = repositories.GetRepositoriesDirectory().FullName;
            string lsDir = string.IsNullOrEmpty(cat) ? lsRoot : Path.Combine(lsRoot, cat);
            string[] lsaGitDirs = Array.FindAll(Array.ConvertAll(Directory.GetDirectories(lsDir), a => Path.GetFileName(a)), b => !b.EndsWith(".git"));

            var lqGetRepo = string.IsNullOrEmpty(cat)
                ? lsaGitDirs.Select(a => new { Category = a, Repositories = repositories.GetAllRepositories(a, null) })
                : lsaGitDirs.Select(a => new { Category = a, Repositories = repositories.GetAllRepositories(cat, a) });

            var lqCatgories = lqGetRepo.Select(a => new
                    {
                        a.Category,
                        Repository = a.Repositories.Select(b => new { b.Name, Commit = b.GetLatestCommit() })
                            .OrderByDescending(b => b.Commit != null ? b.Commit.Date : DateTime.MinValue).FirstOrDefault()
                    }
                ).Select(c => new CatViewModel.CatInfo
                    {
                        CatName = c.Category,
                        LatestRepositoryName = c.Repository != null ? c.Repository.Name : null,
                        LatestCommitInfo = c.Repository != null && c.Repository.Commit != null ? c.Repository.Commit.Message + " - " + c.Repository.Commit.Date.ToPrettyDateString() : null
                    }
                );


            return View(new CatViewModel { RepositoryCategory = cat, Categories = lqCatgories });
        }

        [HttpPost]
        public ActionResult CreateCategory(string cat, string newcat)
        {
            string lsDir = repositories.GetRepositoriesDirectory().FullName;
            if (!string.IsNullOrEmpty(cat))
            {
                if (!Directory.Exists(Path.Combine(lsDir, cat, newcat)))
                    Directory.CreateDirectory(Path.Combine(lsDir, cat, newcat));
            }
            else
            {
                if (!Directory.Exists(Path.Combine(lsDir, newcat)))
                    Directory.CreateDirectory(Path.Combine(lsDir, newcat));
            }

            return RedirectToAction("Cat", new { cat = cat });
        }
    }
}