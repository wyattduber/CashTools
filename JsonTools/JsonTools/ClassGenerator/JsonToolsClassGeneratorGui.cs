using DevToys.Api;
using static DevToys.Api.GUI;
using System.ComponentModel.Composition;
using System.Text.Json.Nodes;
using System.Text.Json;
using CommunityToolkit.Diagnostics;
using System.Text;

namespace JsonTools;

[Export(typeof(IGuiTool))]
[Name("JsonToolsClassGenerator")] // A unique, internal name of the tool.
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0108', 
    GroupName = "CashTools", 
    ResourceManagerAssemblyIdentifier = nameof(JsonToolsResourceAssemblyIdentifier),
    ResourceManagerBaseName = "JsonTools.ClassGenerator.JsonToolsClassGenerator",
    ShortDisplayTitleResourceName = nameof(JsonToolsClassGenerator.ShortDisplayTitle), 
    LongDisplayTitleResourceName = nameof(JsonToolsClassGenerator.LongDisplayTitle),
    DescriptionResourceName = nameof(JsonToolsClassGenerator.Description),
    AccessibleNameResourceName = nameof(JsonToolsClassGenerator.AccessibleName)
)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Json)]
internal sealed class JsonToolsClassGeneratorGui : IGuiTool
{
    private UIToolView? _view;

    private CancellationTokenSource _cancellationTokenSource = new();

    [Import]
    private ISettingsProvider _settingsProvider = null!;

    #region UIElements
    private readonly IUIMultiLineTextInput _input;
    private readonly IUIMultiLineTextInput _schemaOutput;
    private readonly IUIMultiLineTextInput _classOutput;
    private readonly IUIFileSelector _inputFile;
    private readonly IUIStack _errorStack = Stack().Vertical();
    private readonly IUIInfoBar _defaultError;
    #endregion


    public JsonToolsClassGeneratorGui()
    {
        _input = MultiLineTextInput()
            .Title(JsonToolsClassGenerator.Input)
            .Language("json")
            .OnTextChanged(TriggerValidation);

        _schemaOutput = MultiLineTextInput()
            .Title(JsonToolsClassGenerator.SchemaOutput)
            .Language("json")
            .ReadOnly();

        _classOutput = MultiLineTextInput()
            .Title(JsonToolsClassGenerator.ClassOutput)
            .Language("csharp")
            .ReadOnly();

        _inputFile = FileSelector("input-file")
            .CanSelectOneFile()
            .OnFilesSelected(OnInputFileSelected)
            .LimitFileTypesTo(".json");

        _defaultError = GetGeneralErrorInfoBar(JsonToolsClassGenerator.JsonRequiredError);
    }

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        _input.Text(PrettifyJsonInput(parsedData!.ToString()!));
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

    #region Json Handling

    private string PrettifyJsonInput(string str) 
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

    private void GenerateClass() 
    {

    }

    #endregion

    #region Input File Logic

    private async void OnInputFileSelected(SandboxedFileReader[] files) => await  OnFileSelected(_input, files);

    private async ValueTask OnFileSelected(IUIMultiLineTextInput input, SandboxedFileReader[] files)
    {
        Guard.HasSizeEqualTo(files, 1);
        using var memStream = new MemoryStream();
        using var file = files[0];

        await file.CopyFileContentToAsync(memStream, CancellationToken.None);
        var str = Encoding.UTF8.GetString(memStream.ToArray());
        input.Text(PrettifyJsonInput(str));
    }    

    #endregion

    #region Error Handling

    private static IUIInfoBar GetGeneralErrorInfoBar(string error)
    {
        return GetErrorInfoBar(JsonToolsClassGenerator.GeneralError, error);
    }

    private static IUIInfoBar GetErrorInfoBar(string title, string error)
    {
        return InfoBar().ShowIcon().Title(title).Description(error).NonClosable().Error().Open();
    }

    #endregion

    #region Generate View
    public UIToolView View
    {
        get => _view!;
        set => _view = value;
    }
    #endregion

}

