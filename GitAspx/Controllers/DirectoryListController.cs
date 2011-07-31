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
    using System.Linq;
    using System.Threading;
    using System.Web.Mvc;
    using GitAspx.Lib;
    using GitAspx.ViewModels;

    public class DirectoryListController : Controller
    {
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
                RepositoriesDirectory = repositories.GetRepositoriesDirectory().FullName,
                RepositoryCategory = repositories.CombineRepositoryCat(cat, subcat),
                Repositories = repositories.GetAllRepositories(cat, subcat).Select(x => new RepositoryViewModel(x))
            });
        }

        [HttpPost]
        public ActionResult Create(string cat, string subcat, string project)
        {
            if (!string.IsNullOrEmpty(project))
                repositories.CreateRepository(cat, subcat, project);

            return RedirectToAction("Index");
        }
    }
}