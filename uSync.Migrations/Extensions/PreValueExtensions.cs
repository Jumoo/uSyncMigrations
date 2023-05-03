using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core;
using Umbraco.Extensions;

using uSync.Migrations.Models;


namespace uSync.Migrations.Extensions;

public static class PreValueExtensions
{
    public static void AddIntPreValue(this JObject item, IEnumerable<PreValue>? preValues, string alias)
    {
        if (preValues == null) return;

        var value = preValues.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        if (value != null)
        {
            var attempt = value.Value.TryConvertTo<int>();
            if (attempt.Success)
                item.Add(alias, attempt.Result);
        }
    }

    public static void AddDecimalPreValue(this JObject item, IEnumerable<PreValue>? preValues, string alias)
    {
        if (preValues == null) return;

        var value = preValues.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        if (value != null)
        {
            var attempt = value.Value.TryConvertTo<decimal>();
            if (attempt.Success)
            {
                item.Add(alias, attempt.Result);
            }
        }
    }

    public static object? MapPreValues(this object? config, IEnumerable<PreValue>? preValues)
    {
        if (config == null) return null;
        if (preValues == null) return config;

        // generic mapping of aliases to properties ?
        var properties = config.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = preValues?.FirstOrDefault(x => x.Alias.InvariantEquals(property.Name));
            if (value != null)
            {
                var attempt = ConvertValue(value.Value, property.PropertyType);
                if (attempt.Success)
                {
                    property.SetValue(config, attempt.Result);
                }
            }
        }

        return config;
    }

    public static object MapPreValues(this object config, IEnumerable<PreValue>? preValues, Dictionary<string, string> mappings)
    {
        if (preValues == null) return config;

        var properties = config.GetType().GetProperties();
        foreach (var value in preValues)
        {
            if (mappings.ContainsKey(value.Alias))
            {
                var propertyName = mappings[value.Alias];
                var property = properties.FirstOrDefault(x => x.Name == propertyName);
                if (property != null)
                {
                    var attempt = ConvertValue(value.Value, property.PropertyType);
                    if (attempt.Success)
                    {
                        property.SetValue(config, attempt.Result);
                    }
                }
            }
        }

        return config;
    }    
    public static object ConvertPreValuesToJson(this IEnumerable<PreValue>? preValues, bool uppercase)
    {
        return preValues.ConvertPreValuesToJson(uppercase, null);
    }
    public static object ConvertPreValuesToJson(this IEnumerable<PreValue>? preValues, bool uppercase, Dictionary<string, string>? mappings)
    {
        var config = new JObject();
        if (preValues == null) return config;

        foreach (var preValue in preValues)
        {
            var alias = preValue.Alias;
            // look for mappings
            if (mappings != null && mappings.Count > 0)
            {
                // then we need to ignore any properties that aren't in the mappings
                if (mappings.ContainsKey(alias))
                {
                    var newMappedAlias = mappings[alias];
                    if (String.IsNullOrWhiteSpace(newMappedAlias))
                    {
                        // no value has been supplied for this property, skip it
                        continue;
                    }
                    alias = newMappedAlias;
                }
                else
                {
                    //no mapping for this property, skip it
                    continue;
                }
            }
            
            if (uppercase && alias.Length > 1)
            {
                alias = alias[0].ToString().ToUpper() + alias.Substring(1);
            }

            try
            {
                if (preValue.Value != null)
                {
                    if (preValue.Value.DetectIsJson())
                    {
                        config.Add(alias, JToken.Parse(preValue.Value));
                    }
                    else
                    {
                        config.Add(alias, new JValue(preValue.Value));
                    }
                }

            }
            catch
            {
                // here - something went wrong ?
            }
        }

        return config;
    }
    private static Attempt<object?> ConvertValue(string value, Type type)
    {
        if (value.DetectIsJson())
        {
            try
            {
                var json = JsonConvert.DeserializeObject(value, type);
                return Attempt.Succeed(json);
            }
            catch
            {
                return Attempt.Fail<object?>();
            }
        }
        else
        {
            return value.TryConvertTo(type);
        }
    }

    public static TResult GetPreValueOrDefault<TResult>(this IEnumerable<PreValue>? preValues, string alias, TResult defaultValue)
    {
        if (preValues == null) return defaultValue;

        var preValue = preValues.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        if (preValue == null) return defaultValue;

        var attempt = preValue.Value.TryConvertTo<TResult>();
        if (attempt.Success)
            return attempt.Result ?? defaultValue;

        return defaultValue;

    }
}
