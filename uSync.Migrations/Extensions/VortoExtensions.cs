using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Org.BouncyCastle.Bcpg.OpenPgp;

using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace uSync.Migrations.Extensions;

/// <summary>
///  vorto manipulation 
/// </summary>
/// <remarks>
///  vorto is special we keep vorto in the core. 
/// </remarks>
internal static class VortoExtensions
{
    public static bool IsVortoEditorAlias(this string editorAlias)
        => editorAlias.InvariantEquals("Our.Umbraco.Vorto");

    public static Attempt<VortoValue> ConvertToVortoValue(this string value)
    {
        try
        {
            var vorto = JsonConvert.DeserializeObject<VortoValue>(value);
            return vorto != null 
                ? Attempt.Succeed(vorto) 
                : Attempt<VortoValue>.Fail(new ArgumentNullException("Null value in vorto"));
        }
        catch(Exception ex)
        {
            return Attempt<VortoValue>.Fail(ex);
        }
    }
}

public class VortoValue
{
    public Guid DtdGuid { get; set; }

    public Dictionary<string, string> Values { get; set; }
        = new Dictionary<string, string>();
}
