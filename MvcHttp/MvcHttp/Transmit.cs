using System;
using System.IO;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace AiLib.MvcHttp
{
    public static class Transmit
    {
        public static FilePathResult FilePathResult(string path, string name)
        {
            var ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".jpg":
                    return new FilePathResultInline(path, "image/jpeg") { FileDownloadName = name };
                //  Content - Disposition: inline; filename = "myfile.txt"
                //  -> HttpResponseBase.TransmitFile
                //  Mvc.Controller -> return base.File(path, "image/jpeg", name); // null or download name

                case ".png":
                    return new FilePathResultInline(path, "image/png") { FileDownloadName = name };
                case ".gif":
                    return new FilePathResultInline(path, "image/gif") { FileDownloadName = name };
                case ".tiff":
                    return new FilePathResultInline(path, "image/tiff") { FileDownloadName = name };
                case ".pdf":
                    return new FilePathResultInline(path, "application/pdf") { FileDownloadName = name };
                case ".xls":
                    return new FilePathResultInline(path, "application/vnd.ms-excel") { FileDownloadName = name };
                case ".xlsx":
                    return new FilePathResultInline(path, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    { FileDownloadName = name };
                case ".doc":
                    return new FilePathResult(path, "application/msword") { FileDownloadName = name };
                case ".docx":
                    return new FilePathResult(path, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                    { FileDownloadName = name };
                default:
                    return new FilePathResult(path, "application/octet-stream") { FileDownloadName = name };
            }
        }
    }

    // http://stackoverflow.com/questions/3724278/asp-net-mvc-how-can-i-get-the-browser-to-open-and-display-a-pdf-instead-of-disp

    public class FilePathResultInline : FilePathResult
    {
        public FilePathResultInline(string path, string contentType) : base(path, contentType)
        {
            this.Inline = true;
        }

        public bool Inline { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = ContentType;
            if (!string.IsNullOrEmpty(FileDownloadName))
            {
                string str = new ContentDisposition { FileName = this.FileDownloadName, Inline = Inline }.ToString();
                context.HttpContext.Response.AddHeader("Content-Disposition", str);
            }

            WriteFile(response);
        }
    }
}