<%@ Page Language="C#" %>
<%
    Response.Write("Version: " + System.Environment.Version.ToString());

    Response.Write("<br/>UsingIntegratedPipeline=" + System.Web.HttpRuntime.UsingIntegratedPipeline.ToString());
    Response.Write(", IISVersion=" + System.Web.HttpRuntime.IISVersion.ToString());
    Response.Write("<br/>AspInstallDirectory=" + System.Web.HttpRuntime.AspInstallDirectory.ToString());
    Response.Write("<br/>AppDomain.BaseDirectory=" + System.AppDomain.CurrentDomain.BaseDirectory);

    AiLib.RazorGenerator.Mvc.EngineDebug.Output(Response);

    Response.Write("<br/>Assemblies=" + System.AppDomain.CurrentDomain.GetAssemblies().Length.ToString());
    foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
    {
        try
        {
            Response.Write("<br/>" + asm.CodeBase.Replace("file:///", ""));
        }
        catch
        {
            // case: Anonymously Hosted DynamicMethods Assembly
            Response.Write("<br/>" + asm.FullName);
        }
    }

/*	
GitAspx/bin/DiffieHellman.DLL
GitAspx/bin/GitAspx.DLL
GitAspx/bin/GitSharp.Core.DLL
GitAspx/bin/GitSharp.DLL
GitAspx/bin/ICSharpCode.SharpZipLib.DLL
GitAspx/bin/Org.Mentalis.Security.DLL
GitAspx/bin/StructureMap.DLL
GitAspx/bin/Tamir.SharpSSH.DLL
GitAspx/bin/Winterdom.IO.FileMap.DLL
*/
        
%>