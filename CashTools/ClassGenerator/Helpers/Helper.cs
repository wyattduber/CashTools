using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace CashTools.ClassGenerator.Helpers;

internal partial class Helper
{
    
    /**
         Prettify the JSON string if it is valid JSON, otherwise return the string as is.
    */
    internal static string PrettifyAsJsonOrDoNothing(string str)
    {
        try
        {
            var prettified = JsonNode.Parse(str);
            return prettified!.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return str;
        }
    }

    /**
        Get the error message if the JSON is invalid, otherwise return an empty string.
    */
    internal static string GetJsonParseError(string json)
    {
        try
        {
            JsonNode.Parse(json);
            return string.Empty;
        }
        catch (JsonException e)
        {
            return e.Message;
        }
    }

    internal static string GetCSharpType(JTokenType type)
    {
        return type switch
        {
            JTokenType.Integer => "int",
            JTokenType.Float => "double",
            JTokenType.Boolean => "bool",
            JTokenType.Array => "List<object>",
            JTokenType.Object => "object",
            JTokenType.Null => "object",
            _ => "string",
        };
    }

    internal static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove non-alphanumeric characters and split words
        string[] words = MyRegex().Split(input);

        TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
        return string.Concat(Array.ConvertAll(words, word => textInfo.ToTitleCase(word.ToLower())));
    }

    internal static string ToCamelCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove non-alphanumeric characters and split words
        string[] words = MyRegex().Split(input);

        if (words.Length == 0)
            return string.Empty;

        TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;

        // Convert first word to lowercase, others to PascalCase
        for (int i = 1; i < words.Length; i++)
        {
            words[i] = textInfo.ToTitleCase(words[i].ToLower());
        }

        return words[0].ToLower() + string.Concat(words[1..]); // Join with first word in lowercase
    }

    [GeneratedRegex(@"[^a-zA-Z0-9]+")]
    private static partial Regex MyRegex();
}
