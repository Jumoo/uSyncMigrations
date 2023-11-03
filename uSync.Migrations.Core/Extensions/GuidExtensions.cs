using System.Text.RegularExpressions;

namespace uSync.Migrations.Core.Extensions;

/// <summary>
///  Extensions for Guids
/// </summary>
/// <remarks>
///  in general GUIDs are just unique numbers.
///  
///  but there are times where we want the unique numbers to 
///  always be the same between migrations (e.g new GUIDs for new properties)
///  
///  this way if you re-migrate the content stays the same between migrations
///  makes migrations quicker. and more repeatable.
/// </remarks>

public static class GuidExtensions
{
    private static readonly int[] _guidByteOrder = new[] { 15, 14, 13, 12, 11, 10, 9, 8, 6, 7, 4, 5, 0, 1, 2, 3 };

    // https://stackoverflow.com/q/1383030
    public static Guid Combine(this Guid guid1, Guid guid2)
    {
        var x = guid1.ToByteArray();
        var y = guid2.ToByteArray();

        var b = new byte[x.Length];

        for (var i = 0; i < b.Length; i++)
        {
            b[i] = (byte)(x[i] ^ y[i]);
        }

        return new Guid(b);
    }

    public static Guid Increment(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        var carry = true;

        for (int i = 0; i < _guidByteOrder.Length && carry; i++)
        {
            int index = _guidByteOrder[i];
            byte oldValue = bytes[index]++;
            carry = oldValue > bytes[index];
        }

        return new Guid(bytes);
    }

    internal static Guid Int2Guid(this int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    /// <summary>
    /// Utility function to wrap guids inside double quotes to ensure Json strings are valid
    /// </summary>
    /// <param name="value">the Json string to be parsed</param>
    /// <param name="regex">The regex string</param>
    /// <param name="group">The regex group position, 0 = whole match > 1 each individual group match</param>
    /// <returns>string</returns>
    public static string WrapGuidsWithQuotes(string value, string regex, int group)
    {
        string guidRegEx = regex;

        HashSet<string> uniqueMatches = new HashSet<string>();

        foreach (Match m in Regex.Matches(value, guidRegEx))
        {
            uniqueMatches.Add(m.Groups[group].Value);
        }

        foreach (var guid in uniqueMatches)
        {
            value = value.Replace(guid, "\"" + guid + "\"")
                .Replace("\"\"", "\"");
        }
        return value;
    }

    public static string LocalLink2Udi(string value)
    {
        string guidRegEx = @"(href|data-id)=""/?({|%7B)?(localLink:)?\b[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12}\b(}|%7D)?""";

        HashSet<string> uniqueMatches = new HashSet<string>();

        foreach (Match m in Regex.Matches(value, guidRegEx))
        {
            uniqueMatches.Add(m.Value);
        }

        foreach (var guid in uniqueMatches)
        {
            if (guid.Contains("data-id"))
            {
                var newValue = guid.Replace("data-id", "data-udi").Insert(10, "umb://document/");
                value = value.Replace(guid, newValue);
            }
            else if (guid.Contains("href"))
            {
                var colonPos = guid.IndexOf(':');
                var newValue = guid.Insert(colonPos + 1, "umb://document/");
                value = value.Replace(guid, newValue);
            }
        }
        return value;
    }
}
