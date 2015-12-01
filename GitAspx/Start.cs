using System;
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
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");

            routes.MapRoute("DirectoryList", "", new { controller = "DirectoryList", action = "Index" });
            routes.MapRoute("DirectoryListCreate", "Create", new { controller = "DirectoryList", action = "Create" });

            routes.MapRoute("info-refs", "{project}/info/refs",
                            new { controller = "InfoRefs", action = "Execute" },
                            new { method = new HttpMethodConstraint("GET") });

            routes.MapRoute("upload-pack", "{project}/git-upload-pack",
                            new { controller = "Rpc", action = "UploadPack" },
                            new { method = new HttpMethodConstraint("POST") });


            routes.MapRoute("receive-pack", "{project}/git-receive-pack",
                            new { controller = "Rpc", action = "ReceivePack" },
                            new { method = new HttpMethodConstraint("POST") });

            // Dumb protocol
            //routes.MapRoute("info-refs-dumb", "dumb/{project}/info/refs", new {controller = "Dumb", action = "InfoRefs"});
            routes.MapRoute("get-text-file", "{project}/HEAD", new { controller = "Dumb", action = "GetTextFile" });
            routes.MapRoute("get-text-file2", "{project}/objects/info/alternates", new { controller = "Dumb", action = "GetTextFile" });
            routes.MapRoute("get-text-file3", "{project}/objects/info/http-alternates", new { controller = "Dumb", action = "GetTextFile" });

            routes.MapRoute("get-info-packs", "{project}/info/packs", new { controller = "Dumb", action = "GetInfoPacks" });

            routes.MapRoute("get-text-file4", "{project}/objects/info/{something}", new { controller = "Dumb", action = "GetTextFile" });

            routes.MapRoute("get-loose-object", "{project}/objects/{segment1}/{segment2}",
                new { controller = "Dumb", action = "GetLooseObject" });

            routes.MapRoute("get-pack-file", "{project}/objects/pack/pack-{filename}.pack",
                new { controller = "Dumb", action = "GetPackFile" });

            routes.MapRoute("get-idx-file", "{project}/objects/pack/pack-{filename}.idx",
                new { controller = "Dumb", action = "GetIdxFile" });

            // routes.MapRoute("project", "{project}");
            routes.MapRoute("project", "{project}", new { controller = "DotGit", action = "Index" });
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