using System.ComponentModel.Composition;
using System.Text;
using CommunityToolkit.Diagnostics;
using DevToys.Api;
using Newtonsoft.Json.Linq;
using CashTools.ClassGenerator.Enum;
using static DevToys.Api.GUI;
using CashTools.ClassGenerator.Helpers;

namespace CashTools.ClassGenerator;

[Export(typeof(IGuiTool))]
[Name("CashToolsClassGenerator")] // A unique, internal name of the tool.
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0108', 
    GroupName = nameof(CashTools.BaseGroupName), 
    ResourceManagerAssemblyIdentifier = nameof(CashToolsResourceAssemblyIdentifier),
    ResourceManagerBaseName = "CashTools.ClassGenerator.CashToolsClassGenerator",
    ShortDisplayTitleResourceName = nameof(CashToolsClassGenerator.ShortDisplayTitle), 
    LongDisplayTitleResourceName = nameof(CashToolsClassGenerator.LongDisplayTitle),
    DescriptionResourceName = nameof(CashToolsClassGenerator.Description),
    AccessibleNameResourceName = nameof(CashToolsClassGenerator.AccessibleName)
)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Json)]
internal sealed class JsonToolsClassGeneratorGui : IGuiTool
{
    private UIToolView? _view;
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly IUIMultiLineTextInput _input;
    private readonly IUIMultiLineTextInput _output;
    private readonly IUIFileSelector _inputFile;
    private readonly IUIStack _errorsStack = Stack().Vertical();
    private readonly IUIInfoBar _defaultError;

    [Import]
    private ISettingsProvider _settingsProvider = null!;

    #region Settings

    private static readonly SettingDefinition<bool> isClassStatic = new(name: $"{CashToolsClassGenerator.MakeStaticClass}", defaultValue: false);
    private static readonly SettingDefinition<AccessModifierOptions> classAccessModifier = new(name: $"{CashToolsClassGenerator.ClassAccessModifier}", defaultValue: AccessModifierOptions.@public);

    #endregion

    public JsonToolsClassGeneratorGui()
    {
        _input = MultiLineTextInput()
            .Title(CashToolsClassGenerator.Input)
            .Language("json")
            .Extendable()
            .OnTextChanged(TriggerValidation);

        _output = MultiLineTextInput()
            .Title(CashToolsClassGenerator.ClassOutput)
            .Language("csharp")
            .Extendable()
            .ReadOnly();

        _inputFile = FileSelector("input-file")
            .CanSelectOneFile()
            .OnFilesSelected(OnInputFileSelected)
            .LimitFileTypesTo(".json");

        _defaultError = UIHelper.GetGeneralErrorInfoBar(CashToolsClassGenerator.JsonRequiredError);
    }

    private static async ValueTask OnFileSelected(IUIMultiLineTextInput input, SandboxedFileReader[] files)
    {
        Guard.HasSizeEqualTo(files, 1);
        using var memStream = new MemoryStream();
        using var file = files[0];

        await file.CopyFileContentToAsync(memStream, CancellationToken.None);
        var str = Encoding.UTF8.GetString(memStream.ToArray());
        input.Text(Helper.PrettifyAsJsonOrDoNothing(str));
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
        var sb = new StringBuilder();

        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();

        sb.AppendLine("public class " + className);
        sb.AppendLine("{");

        foreach (var property in obj!.Properties())
        {
            string propName = property.Name;
            string propType = Helper.GetCSharpType(property.Value.Type);
            if (includeJsonPropertyAttribute) sb.AppendLine($"    [JsonProperty(\"{Helper.ToCamelCase(propName)}\")]");
            sb.AppendLine($"    public {propType}{(useNullableReferenceTypes ? "?" : "")} {Helper.ToPascalCase(propName)} {{ get; set; }}");
            if (includeJsonPropertyAttribute) sb.AppendLine();
        }

        sb.AppendLine("}");
        _output.Text(sb.ToString());
    }

    private JObject? ValidateAndParseSchema()
    {
        if (string.IsNullOrWhiteSpace(_input.Text))
        {
            _errorsStack.WithChildren([_defaultError]);
        }
        else
        {
            var jsonError = Helper.GetJsonParseError(_input.Text);
            if (!string.IsNullOrEmpty(jsonError))
            {
                _errorsStack.WithChildren(
                    UIHelper.GetErrorInfoBar(CashToolsClassGenerator.InputError, jsonError)
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
                        UIHelper.GetErrorInfoBar(CashToolsClassGenerator.SchemaError, e.Message)
                    );
                }
                _errorsStack.WithChildren([]);
                return jObject;
            }
        }

        return null;
    }

    public void OnDataReceived(string dataTypeName, object? parsedData) => 
        _input.Text(Helper.PrettifyAsJsonOrDoNothing(parsedData!.ToString()!));
    

    private async void OnInputFileSelected(SandboxedFileReader[] files) =>
        await OnFileSelected(_input, files);
    

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