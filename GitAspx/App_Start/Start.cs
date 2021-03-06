﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GitAspx.Lib;
using StructureMap;
using StructureMap.Configuration.DSL;

[assembly: System.Web.PreApplicationStartMethod(typeof(GitAspx.Start), "Application_Start")]

namespace GitAspx
{
    public static class Start
    {
        public static void Application_Start()
        {
            // AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            ObjectFactory.Initialize(cfg => cfg.AddRegistry(new AppRegistry()));
            ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {

            // Dumb protocol
            //routes.MapRoute("info-refs-dumb", "dumb/{project}/info/refs", new {controller = "Dumb", action = "InfoRefs"});

            RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            RouteTable.Routes.IgnoreRoute("favicon.ico");

            MapSimpleRoute("settings", "settings/{key}/{value}", "WebBrowsingSettings", "Index");

            string lsPath = GetPathPattern();
            string lsPathSlash = lsPath.Length > 0 ? lsPath + "/" : lsPath;
            string lsCat = GetCatPattern();
            string lsCatSlash = lsCat.Length > 0 ? lsCat + "/" : lsCat;
            if (lsPath.Length > 0)
            {
                string[] lsaCat = lsCat.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (lsaCat.Length >= 1)
                {
                    MapSimpleRoute("CatList1", "", "DirectoryList", "Cat");
                    MapSimpleRoute("CatListCreate1", "create", "DirectoryList", "CreateCategory");
                }
                if (lsaCat.Length == 2)
                {
                    MapSimpleRoute("CatList2", lsaCat[0], "DirectoryList", "Cat");
                    MapSimpleRoute("CatListCreate2", lsaCat[0] + "/create", "DirectoryList", "CreateCategory");
                }

                MapSimpleRoute("DirectoryList", lsCat, "DirectoryList", "Index");
                MapSimpleRoute("DirectoryListCreate", lsCatSlash + "create", "DirectoryList", "CreateRepository");
            }

            MapSimpleRouteGetOnly("info-refs", lsPath + "/info/refs", "InfoRefs", "Execute");

            MapSimpleRoutePostOnly("upload-pack", lsPath + "/git-upload-pack", "Rpc", "UploadPack");
            MapSimpleRoutePostOnly("receive-pack", lsPath + "/git-receive-pack", "Rpc", "ReceivePack");

            MapSimpleRoute("get-info-packs", lsPath + ".git/info/packs", "Dumb", "GetInfoPacks");
            MapSimpleRoute("get-text-file", lsPath + ".git/HEAD", "Dumb", "GetHead");
            MapSimpleRoute("get-text-file2", lsPath + ".git/objects/info/alternates", "Dumb", "GetAlternates");
            MapSimpleRoute("get-text-file3", lsPath + ".git/objects/info/http-alternates", "Dumb", "GetHttpAlternates");
            MapSimpleRoute("get-text-file4", lsPath + ".git/objects/info/{something}", "Dumb", "GetOtherInfo");
            MapSimpleRoute("get-loose-object", lsPath + ".git/objects/{segment1}/{segment2}", "Dumb", "GetLooseObject");
            MapSimpleRoute("get-pack-file", lsPath + ".git/objects/pack/pack-{filename}.pack", "Dumb", "GetPackFile");
            MapSimpleRoute("get-idx-file", lsPath + ".git/objects/pack/pack-{filename}.idx", "Dumb", "GetIdxFile");

            MapSimpleRoute("giturl", lsPath + ".git");

            MapSimpleRoute("repo-home", lsPath, "TreeView", "Index");
            MapSimpleRoute("tree-home", lsPathSlash + "tree", "TreeView", "Index");
            MapSimpleRoute("tree-commit", lsPathSlash + "tree/{tree}", "TreeView", "Index");
            MapSimpleRoute("tree", lsPathSlash + "tree/{tree}/{*path}", "TreeView", "Index");
            MapSimpleRoute("blob", lsPathSlash + "blob/{tree}/{*path}", "BlobView", "Index");
            MapSimpleRoute("download-commit", lsPathSlash + "download/{tree}", "DownloadView", "Index");
            MapSimpleRoute("download", lsPathSlash + "download/{tree}/{*path}", "DownloadView", "Index");

            routes.MapRoute("project", "{project}");
            //routes.MapRoute("project", "{project}",
            //    new { controller = "DotGit", action = "Index" });

        }

        static void MapSimpleRoute(string asName, string asUrl)
        {
            RouteTable.Routes.MapRoute(asName, asUrl);
        }

        static void MapSimpleRoute(string asName, string asUrl, string asController, string asAction)
        {
            RouteTable.Routes.MapRoute(asName, asUrl, new { controller = asController, action = asAction });
        }

        static void MapSimpleRouteGetOnly(string asName, string asUrl, string asController, string asAction)
        {
            RouteTable.Routes.MapRoute(asName, asUrl, new { controller = asController, action = asAction }, new { method = new HttpMethodConstraint("GET") });
        }

        static void MapSimpleRoutePostOnly(string asName, string asUrl, string asController, string asAction)
        {
            RouteTable.Routes.MapRoute(asName, asUrl, new { controller = asController, action = asAction }, new { method = new HttpMethodConstraint("POST") });
        }

        static string GetCatPattern()
        {
            int liLevel = AppSettings.FromAppConfig().RepositoryLevel;
            if (liLevel == 0 || liLevel == 1)
                return "";
            else if (liLevel == 2)
                return "{cat}";
            else if (liLevel == 3)
                return "{cat}/{subcat}";

            throw new NotSupportedException(string.Format("RepositoryLevel {0} not supported", liLevel));
        }

        static string GetPathPattern()
        {
            int liLevel = AppSettings.FromAppConfig().RepositoryLevel;
            if (liLevel == 0)
                return "";
            else if (liLevel == 1)
                return "{project}";
            else if (liLevel == 2)
                return "{cat}/{project}";
            else if (liLevel == 3)
                return "{cat}/{subcat}/{project}";

            throw new NotSupportedException(string.Format("RepositoryLevel {0} not supported", liLevel));
        }

        class AppRegistry : Registry
        {
            public AppRegistry()
            {
                For<AppSettings>()
                    .Singleton()
                    .Use(AppSettings.FromAppConfig);
            }
        }
    }
}