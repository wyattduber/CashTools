using System.Text.Json;
using System.Text.Json.Nodes;
using CashTools.ClassGenerator.Enum;
using Newtonsoft.Json.Linq;

namespace CashTools.ClassGenerator.Helpers;

internal abstract class Helper
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

    internal static string UpperCaseFirstLetter(string str) => char.ToUpper(str[0]) + str[1..];
    

    internal static string ConvertAccessModifier(AccessModifierOptions accessModifier)
    {
        return accessModifier switch
        {
            AccessModifierOptions.@private => "private",
            AccessModifierOptions.@protected => "protected",
            AccessModifierOptions.@internal => "internal",
            AccessModifierOptions.@public => "public",
            AccessModifierOptions.@protectedInternal => "protected internal",
            AccessModifierOptions.@privateProtected => "private protected",
            _ => throw new ArgumentOutOfRangeException(nameof(accessModifier), accessModifier, null),
        };
    }
}
