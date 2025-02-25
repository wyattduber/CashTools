using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CashTools.ClassGenerator.Enum;
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

    internal static string GetCSharpType(JToken token, string propName, StringBuilder subClasses)
    {
        return token.Type switch
        {
            JTokenType.Integer => "int",
            JTokenType.Float => "decimal",
            JTokenType.Boolean => "bool",
            JTokenType.Guid => "Guid",
            JTokenType.Date => "DateTime",
            JTokenType.String => TryParseSpecialTypes(token.ToString()),
            JTokenType.Array => GetListType(token, propName, subClasses),
            JTokenType.Object => CreateSubClass(token, propName, subClasses),
            JTokenType.Null => "object",
            _ => "string",
        };
    }

    private static string TryParseSpecialTypes(string value)
    {
        if (Guid.TryParse(value, out _)) return "Guid";
        if (DateTime.TryParse(value, out _)) return "DateTime";
        if (DateOnly.TryParse(value, out _)) return "DateOnly";
        if (decimal.TryParse(value, out _)) return "decimal";
        return "string";
    }

    private static string GetListType(JToken token, string propName, StringBuilder subClasses)
    {
        var arrayItems = token.Children<JToken>().FirstOrDefault();
        if (arrayItems != null)
        {
            string itemType = GetCSharpType(arrayItems, propName + "Item", subClasses);
            return $"List<{itemType}>";
        }
        return "List<object>";
    }

    private static string CreateSubClass(JToken token, string propName, StringBuilder subClasses)
    {
        string className = UpperCaseFirstLetter(propName);
        subClasses.AppendLine($"public class {className}");
        subClasses.AppendLine("{");
        
        foreach (var subProperty in token.Children<JProperty>())
        {
            string subPropName = subProperty.Name;
            string subPropType = GetCSharpType(subProperty.Value, subPropName, subClasses);
            subClasses.AppendLine($"    public {subPropType} {UpperCaseFirstLetter(subPropName)} {{ get; set; }}");
        }
        
        subClasses.AppendLine("}");
        subClasses.AppendLine();
        
        return className;
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

    [GeneratedRegex(@"[^a-zA-Z0-9]+")]
    private static partial Regex MyRegex();
}
