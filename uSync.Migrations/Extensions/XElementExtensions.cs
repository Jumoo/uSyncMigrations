using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Umbraco.Extensions;

using uSync.Core;

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

    /// <summary>
    ///  will extract the property values by culture from a content property element.
    /// </summary>
    public static Dictionary<string, string>? GetPropertyValueByCultures(this XElement property)
    {
        if (property == null) return null;
        if (!property.HasElements) return null;

        var propertiesByCulture = new Dictionary<string, string>();

        foreach (var value in property.Elements())
        {
            var culture = value.Attribute("Culture").ValueOrDefault(string.Empty);
            propertiesByCulture[culture] = value.ValueOrDefault(string.Empty);
        }

        return propertiesByCulture;
    }

    public static XElement CreateValueElement(this string value, string culture)
    {
        // create the value element for a cultuer. 
        var valueElement = new XElement("Value");
        if (!string.IsNullOrEmpty(culture))
            valueElement.Add(new XAttribute("Culture", culture));
        valueElement.Add(value);

        return valueElement; 
    }

    public static void RemoveByName(this XElement parent, IEnumerable<string> names)
    {
        if (parent == null || names == null) return;

        foreach(var element in parent.Elements().ToList())
        {
            if (names.InvariantContains(element.Name.LocalName))
                element.Remove();
        }
    }
}
