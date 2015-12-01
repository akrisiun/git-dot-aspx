using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiLib.Web
{
    public interface ILastWebError
    {
        Exception LastError { get; set; }
    }
}
