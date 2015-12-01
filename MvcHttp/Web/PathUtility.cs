using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace AiLib.Web
{
    // VirtualPathUtility wrap
    public static class PathUtility
    {
        public static string MapPath(string path)
        {
            var virtPath = HostingEnvironment.ApplicationVirtualPath;
            if (path.StartsWith("~"))
                path = path.Replace("~", "");
            // var result = Combine(AppendTrailingSlash(virtPath), path);
            string result = String.Join("", AppendTrailingSlash(virtPath), path);
            if (result.Contains("\\") || result.Contains("//"))
                result = result.Replace("\\", "/").Replace("//", "/");
            return result;
        }

        //     Combines a base path and a relative path.
        public static string Combine(string basePath, string relativePath) { return VirtualPathUtility.Combine(basePath, relativePath); }

        //     Retrieves the extension of the file that is referenced in the virtual path.
        //     The file name extension string literal, including the period (.), null, or
        public static string GetExtension(string virtualPath) { return VirtualPathUtility.GetExtension(virtualPath); }
        
        //     Retrieves the file name of the file that is referenced in the virtual path.
        public static string GetFileName(string virtualPath) { return VirtualPathUtility.GetFileName(virtualPath); }


        public static string AppendTrailingSlash(string virtualPath) { return VirtualPathUtility.AppendTrailingSlash(virtualPath); }

        //     Returns the directory portion of a virtual path.
        public static string GetDirectory(string virtualPath) { return VirtualPathUtility.GetDirectory(virtualPath); }
        
        
        //     Returns a Boolean value indicating whether the specified virtual path is
        //     absolute; that is, it starts with a literal slash mark (/).
        public static bool IsAbsolute(string virtualPath) { return VirtualPathUtility.IsAbsolute(virtualPath); }
        
        //     Returns a Boolean value indicating whether the specified virtual path is
        //     relative to the application.
        public static bool IsAppRelative(string virtualPath) { return VirtualPathUtility.IsAbsolute(virtualPath); }
        
        //     Returns the relative virtual path from one virtual path containing the root
        //     operator (the tilde [~]) to another.
        public static string MakeRelative(string fromPath, string toPath) { return VirtualPathUtility.MakeRelative(fromPath, toPath); }
        
        //     Removes a trailing slash mark (/) from a virtual path.
        // Returns:
        //     A virtual path without a trailing slash mark, if the virtual path is not
        //     already the root directory ("/"); otherwise, null.
        public static string RemoveTrailingSlash(string virtualPath) { return VirtualPathUtility.RemoveTrailingSlash(virtualPath); }
    }
}
