using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AiLib.Report
{
    public static class ExcelResponse
    {
        public const string ContentExcel = "application/vnd.ms-excel";

        public static bool Render(System.Web.HttpResponseBase Response, 
            Excel.IWorkbookBase workbook, string fileName)
        {
            // AiLib.Report.Excel.IWorkbookBase workbook 

            // http://dotnetslackers.com/articles/aspnet/Create-Excel-Spreadsheets-Using-NPOI.aspx
            // Save the Excel spreadsheet to a MemoryStream and return it to the client

            using (var exportData = new MemoryStream())
            {
                workbook.Write(exportData);

                string saveAsFileName = fileName;
                Response.ContentType = ContentExcel;
                Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", saveAsFileName));
                Response.Clear();
                Response.BinaryWrite(exportData.GetBuffer());
                Response.End();
            }

            return true;
        }

        public static bool SaveExcel(Excel.IWorkbookBase workbook, string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fs);
                return true;
            }
        }

    }

}