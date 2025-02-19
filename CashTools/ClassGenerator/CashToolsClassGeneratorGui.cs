using System.ComponentModel.Composition;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CommunityToolkit.Diagnostics;
using DevToys.Api;
using NJsonSchema;
using static DevToys.Api.GUI;

namespace CashTools.ClassGenerator;

[Export(typeof(IGuiTool))]
[Name("CashToolsClassGenerator")] // A unique, internal name of the tool.
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0108', 
    GroupName = nameof(CashTools.BaseGroupName), 
    ResourceManagerAssemblyIdentifier = nameof(CashToolsResourceAssemblyIdentifier),
    ResourceManagerBaseName = "JsonTools.ClassGenerator.JsonToolsClassGenerator",
    ShortDisplayTitleResourceName = nameof(CashToolsClassGenerator.ShortDisplayTitle), 
    LongDisplayTitleResourceName = nameof(CashToolsClassGenerator.LongDisplayTitle),
    DescriptionResourceName = nameof(CashToolsClassGenerator.Description),
    AccessibleNameResourceName = nameof(CashToolsClassGenerator.AccessibleName)
)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Json)]
internal sealed class JsonToolsClassGeneratorGui : IGuiTool
{
    private UIToolView? _view;

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    [Import]
    private ISettingsProvider _settingsProvider = null!;

    public JsonToolsClassGeneratorGui()
    {
        _input = MultiLineTextInput()
            .Title(CashToolsClassGenerator.Input)
            .Language("json")
            .OnTextChanged(TriggerValidation);

        _output = MultiLineTextInput()
            .Title(CashToolsClassGenerator.ClassOutput)
            .Language("typescript")
            .ReadOnly();

        _inputFile = FileSelector("input-file")
            .CanSelectOneFile()
            .OnFilesSelected(OnInputFileSelected)
            .LimitFileTypesTo(".json");

        _defaultError = GetGeneralErrorInfoBar(CashToolsClassGenerator.JsonRequiredError);
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
            if (!token.IsCancellationRequested)
            {
                GenerateClass();
            }
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
    private void GenerateClass()
    {
        var schema = ValidateAndParseSchema();
        if (schema == null)
        {
            return;
        }

        var csharpGenerator = new NJsonSchema.CodeGeneration.CSharp.CSharpGenerator(schema);
        _output.Text(csharpGenerator.GenerateFile());
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

    private JsonSchema? ValidateAndParseSchema()
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
                    GetErrorInfoBar(CashToolsClassGenerator.InputError, jsonError)
                );
            }
            else
            {
                JsonSchema? schema = null;
                try
                {
                    schema = JsonSchema.FromJsonAsync(_input.Text).Result;
                }
                catch (Exception e)
                {
                    _errorsStack.WithChildren(
                        GetErrorInfoBar(CashToolsClassGenerator.SchemaError, e.Message)
                    );
                }
                _errorsStack.WithChildren(
                    [
                        InfoBar()
                            .ShowIcon()
                            .Title(CashToolsClassGenerator.Success)
                            .Description(CashToolsClassGenerator.ClassGenerated)
                            .Success()
                            .Open()
                    ]
                );
                return schema;
            }
        }

        return null;
    }

    private static IUIInfoBar GetGeneralErrorInfoBar(string error)
    {
        return GetErrorInfoBar(CashToolsClassGenerator.GeneralError, error);
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

