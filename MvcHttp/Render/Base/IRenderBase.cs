using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiLib.Web
{
    public interface IRenderBase
    {
        string Render(TextWriter writer);
    }
}
