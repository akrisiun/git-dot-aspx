using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AiLib.Web
{
    public interface IRenderTable : IRenderBase 
    {
        object TableHeader();
        IEnumerable ReadData();
    }

    public abstract class RenderTable : IRenderTable 
    {
        public virtual string Render(TextWriter writer)
        {
            Table(writer);
            return String.Empty;
        }

        public virtual void Table(TextWriter writer)
        {
            var tab = new XElement("table"
                , new XAttribute("id", "postGrid")
                , new XAttribute("class", "list"));

            var th = TableHeader();
            tab.Add(th);

            foreach (object row in ReadData())
                tab.Add(row);

            tab.WriteTo(XmlWriter.Create(writer));
        }

        public virtual object TableHeader()
        {
            var th = new XElement("tr");
            th.Add(new XElement("th", "#"));

            for (int i = 0; i < Header.Length; i++)
            {
                string cellTH = Header[i] != null ? Header[i].ToString() : null;

                if (string.IsNullOrWhiteSpace(cellTH))
                    th.Add(new XElement("th"));
                else
                    th.Add(new XElement("th"
                        , new XElement("div"
                              // , new XAttribute("style", "min-width: 200px;")
                              , cellTH))
                              );
            }
            return th;
        }

        public object[] Header { get; set; }
        public abstract IEnumerable ReadData();

     }

}
