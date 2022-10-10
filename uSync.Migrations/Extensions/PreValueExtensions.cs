using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lucene.Net.Queries.Function.ValueSources;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core;
using Umbraco.Extensions;

using uSync.Migrations.Models;

namespace uSync.Migrations.Extensions;
public static class PreValueExtensions
{
    public static void AddIntPreValue(this JObject item, IList<PreValue> preValues, string alias)
    {
        var value = preValues.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        if (value != null)
        {
            var attempt = value.Value.TryConvertTo<int>();
            if (attempt.Success)
                item.Add(alias, attempt.Result);
        }
    }

    public static void AddDecimalPreValue(this JObject item, IList<PreValue> preValues, string alias)
    {
        var value = preValues.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        if (value != null)
        {
            var attempt = value.Value.TryConvertTo<Decimal>();
            if (attempt.Success)
                item.Add(alias, attempt.Result);
        }
    }

    public static object MapPreValues(this SyncDataTypeInfo dataTypeInfo, object config)
    {
        // generic mapping of aliases to properties ? 
        var properties = config.GetType().GetProperties();
        foreach (var property in properties)
        {
            var value = dataTypeInfo.PreValues?.FirstOrDefault(x => x.Alias.InvariantEquals(property.Name));
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

    public static object MapPreValues(this SyncDataTypeInfo dataTypeInfo, object config, Dictionary<string ,string> mappings)
    {
        var properties = config.GetType().GetProperties();

        foreach (var value in dataTypeInfo.PreValues)
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

    public static object ConvertPreValuesToJson(this SyncDataTypeInfo dataTypeInfo, bool uppercase)
    {
        var config = new JObject();
        foreach (var property in dataTypeInfo.PreValues)
        {
            var alias = property.Alias;
            if (uppercase && alias.Length > 1) {
                alias = alias[0].ToString().ToUpper() + alias.Substring(1);
            }

            try
            {
                if (property.Value != null)
                {
                    if (property.Value.DetectIsJson())
                    {
                        config.Add(alias, JToken.Parse(property.Value));
                    }
                    else
                    {
                        config.Add(alias, new JValue(property.Value));
                    }
                }

            }
            catch (Exception ex)
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

    public static TResult GetPreValueOrDefault<TResult>(this SyncDataTypeInfo dataTypeInfo, string alias, TResult defaultValue)
    {
        if (dataTypeInfo?.PreValues == null) return defaultValue;

        var preValue = dataTypeInfo.PreValues.FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        if (preValue == null) return defaultValue;

        var attempt = preValue.Value.TryConvertTo<TResult>();
        if (attempt.Success)
            return attempt.Result ?? defaultValue;

        return defaultValue;

    }
}
