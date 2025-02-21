using System.ComponentModel.Composition;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CommunityToolkit.Diagnostics;
using DevToys.Api;
using Newtonsoft.Json.Linq;
using static DevToys.Api.GUI;

namespace CashTools.ClassGenerator;

[Export(typeof(IGuiTool))]
[Name("CashToolsClassGenerator")] // A unique, internal name of the tool.
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0108', 
    GroupName = nameof(CashTools.BaseGroupName), 
    ResourceManagerAssemblyIdentifier = nameof(CashToolsResourceAssemblyIdentifier),
    ResourceManagerBaseName = "CashTools.CashTools",
    ShortDisplayTitleResourceName = nameof(CashTools.ShortDisplayTitle), 
    LongDisplayTitleResourceName = nameof(CashTools.LongDisplayTitle),
    DescriptionResourceName = nameof(CashTools.Description),
    AccessibleNameResourceName = nameof(CashTools.AccessibleName)
)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Json)]
internal sealed class JsonToolsClassGeneratorGui : IGuiTool
{
    private UIToolView? _view;

    private CancellationTokenSource _cancellationTokenSource = new();

    public JsonToolsClassGeneratorGui()
    {
        _input = MultiLineTextInput()
            .Title(CashTools.Input)
            .Language("json")
            .Extendable()
            .OnTextChanged(TriggerValidation);

        _output = MultiLineTextInput()
            .Title(CashTools.ClassOutput)
            .Language("csharp")
            .Extendable()
            .ReadOnly();

        _inputFile = FileSelector("input-file")
            .CanSelectOneFile()
            .OnFilesSelected(OnInputFileSelected)
            .LimitFileTypesTo(".json");

        _defaultError = GetGeneralErrorInfoBar(CashTools.JsonRequiredError);
    }

    #region enums

    enum GridRows
    {
        ConfigRow,
        UploadRow,
        InputRow,
        ErrorsRow
    }

    private enum GridColumns
    {
        Stretch
    }

    #endregion

    #region events

    private async ValueTask OnFileSelected(IUIMultiLineTextInput input, SandboxedFileReader[] files)
    {
        Guard.HasSizeEqualTo(files, 1);
        using var memStream = new MemoryStream();
        using var file = files[0];

        await file.CopyFileContentToAsync(memStream, CancellationToken.None);
        var str = Encoding.UTF8.GetString(memStream.ToArray());
        input.Text(PrettifyAsJsonOrDoNothing(str));
    }

    private async ValueTask TriggerValidation(string arg)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            await Task.Delay(500, token);
            if (!token.IsCancellationRequested) GenerateCSharpClass("MyClass", "MyNamespace", true, true); // TODO Add settings for these
        }
        catch (TaskCanceledException) { }
    }

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        _input.Text(PrettifyAsJsonOrDoNothing(parsedData!.ToString()!));
    }

    private async void OnInputFileSelected(SandboxedFileReader[] files)
    {
        await OnFileSelected(_input, files);
    }

    #endregion


    #region UIElements

    private readonly IUIMultiLineTextInput _input;
    private readonly IUIMultiLineTextInput _output;

    private readonly IUIFileSelector _inputFile;

    private readonly IUIStack _errorsStack = Stack().Vertical();

    private readonly IUIInfoBar _defaultError;

    #endregion

    #region methods

    private void GenerateCSharpClass
    (
        string className, 
        string namespaceName,
        bool includeJsonPropertyAttribute,
        bool useNullableReferenceTypes
    )
    {
        var obj = ValidateAndParseSchema();
        if (obj == null) return;
        StringBuilder sb = new();

        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();

        sb.AppendLine("public class " + className);
        sb.AppendLine("{");

        foreach (var property in obj!.Properties())
        {
            string propName = property.Name;
            string propType = GetCSharpType(property.Value.Type);
            if (includeJsonPropertyAttribute) sb.AppendLine($"    [JsonProperty(\"{ToCamelCase(propName)}\")]");
            sb.AppendLine($"    public {propType}{(useNullableReferenceTypes ? "?" : "")} {ToPascalCase(propName)} {{ get; set; }}");
            if (includeJsonPropertyAttribute) sb.AppendLine();
        }

        sb.AppendLine("}");
        _output.Text(sb.ToString());
    }

    private static string GetCSharpType(JTokenType type)
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

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove non-alphanumeric characters and split words
        string[] words = Regex.Split(input, @"[^a-zA-Z0-9]+");

        TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
        return string.Concat(Array.ConvertAll(words, word => textInfo.ToTitleCase(word.ToLower())));
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove non-alphanumeric characters and split words
        string[] words = Regex.Split(input, @"[^a-zA-Z0-9]+");

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

    /**
         Prettify the JSON string if it is valid JSON, otherwise return the string as is.
    */
    private string PrettifyAsJsonOrDoNothing(string str)
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
    private string GetJsonParseError(string json)
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

    private JObject? ValidateAndParseSchema()
    {
        if (string.IsNullOrWhiteSpace(_input.Text))
        {
            _errorsStack.WithChildren([_defaultError]);
        }
        else
        {
            var jsonError = GetJsonParseError(_input.Text);
            if (!string.IsNullOrEmpty(jsonError))
            {
                _errorsStack.WithChildren(
                    GetErrorInfoBar(CashTools.InputError, jsonError)
                );
            }
            else
            {
                JObject? jObject = null;
                try
                {
                    jObject = JObject.Parse(_input.Text);
                }
                catch (Exception e)
                {
                    _errorsStack.WithChildren(
                        GetErrorInfoBar(CashTools.SchemaError, e.Message)
                    );
                }
                _errorsStack.WithChildren([]);
                return jObject;
            }
        }

        return null;
    }

    private static IUIInfoBar GetGeneralErrorInfoBar(string error)
    {
        return GetErrorInfoBar(CashTools.GeneralError, error);
    }

    private static IUIInfoBar GetErrorInfoBar(string title, string error)
    {
        return InfoBar().ShowIcon().Title(title).Description(error).NonClosable().Error().Open();
    }

    #endregion

    public UIToolView View
    {
        get
        {
            _view ??= new UIToolView(
                Grid()
                    .Rows(
                        (GridRows.ConfigRow, Auto),
                        (GridRows.UploadRow, Auto),
                        (GridRows.InputRow, new UIGridLength(1, UIGridUnitType.Fraction)),
                        (GridRows.ErrorsRow, Auto)
                    )
                    .Columns((GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))
                    .Cells(
                        Cell(GridRows.UploadRow, GridColumns.Stretch, _inputFile),
                        Cell(
                            GridRows.InputRow,
                            GridColumns.Stretch,
                            SplitGrid()
                                .Vertical()
                                .WithLeftPaneChild(_input)
                                .WithRightPaneChild(_output)
                        ),
                        Cell(
                            GridRows.ErrorsRow,
                            GridColumns.Stretch,
                            _errorsStack.WithChildren([_defaultError])
                        )
                    )
            );
            return _view;
        }
    }

}

