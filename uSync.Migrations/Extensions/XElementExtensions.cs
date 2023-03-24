using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace uSync.Migrations.Extensions;
internal static class XElementExtensions
{

    /// <summary>
    ///  clone the XElement into a new orphaned XElement object
    /// </summary>
    public static XElement? Clone(this XElement element)
    {
        try
        {
            return XElement.Parse(element.ToString());
        }
        catch { return null; }
    }
}
