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
internal sealed class CashToolsClassGeneratorGui : IGuiTool
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly IUIMultiLineTextInput _input;
    private readonly IUIMultiLineTextInput _output;
    private readonly IUIFileSelector _inputFile;
    private readonly IUIStack _errorsStack = Stack().Vertical();
    private readonly IUIInfoBar _defaultError;
    private readonly IUISingleLineTextInput _classNameInput = SingleLineTextInput();
    private readonly IUISingleLineTextInput _namespaceNameInput = SingleLineTextInput();

    [Import]
    private ISettingsProvider _settingsProvider = null!;

    #region Settings

    private static readonly SettingDefinition<string> _classNameSetting = new(name: $"{CashToolsClassGenerator.ClassName}", defaultValue: "MyClass");
    private static readonly SettingDefinition<string> _namespaceNameSetting = new(name: $"{CashToolsClassGenerator.NamespaceName}", defaultValue: "MyNamespace");
    private static readonly SettingDefinition<bool> _includeJsonPropertyAttributeSetting = new(name: $"{CashToolsClassGenerator.IncludeJsonPropertyAttribute}", defaultValue: false);
    private static readonly SettingDefinition<bool> _isClassStaticSetting = new(name: $"{CashToolsClassGenerator.MakeStaticClass}", defaultValue: false);
    private static readonly SettingDefinition<AccessModifierOptions> _classAccessModifierSetting = new(name: $"{CashToolsClassGenerator.ClassAccessModifier}", defaultValue: AccessModifierOptions.@public);
    private static readonly SettingDefinition<bool> _useNullableTypesSetting = new(name: $"{CashToolsClassGenerator.UseNullableTypes}", defaultValue: false);
    private static readonly SettingDefinition<bool> _usePascalCaseSetting = new(name: $"{CashToolsClassGenerator.UsePascalCase}", defaultValue: true);

    #endregion

    public CashToolsClassGeneratorGui()
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
            if (!token.IsCancellationRequested) GenerateCSharpClass(); // TODO Add settings for these
        }
        catch (TaskCanceledException) { }
    }

    private void GenerateCSharpClass()
    {
        var obj = ValidateAndParseSchema();
        if (obj == null) return;

        // Get Settings
        var className = _settingsProvider.GetSetting(_classNameSetting);
        if (string.IsNullOrEmpty(className)) className = "MyClass";
        var namespaceName = _settingsProvider.GetSetting(_namespaceNameSetting);
        if (string.IsNullOrEmpty(namespaceName)) namespaceName = "MyNamespace";
        var includeJsonPropertyAttribute = _settingsProvider.GetSetting(_includeJsonPropertyAttributeSetting);
        var isClassStatic = _settingsProvider.GetSetting(_isClassStaticSetting);
        var classAccessModifier = _settingsProvider.GetSetting(_classAccessModifierSetting);
        var useNullableReferenceTypes = _settingsProvider.GetSetting(_useNullableTypesSetting);
        var usePascalCase = _settingsProvider.GetSetting(_usePascalCaseSetting);

        var sb = new StringBuilder();

        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();

        sb.AppendLine($"{Helper.ConvertAccessModifier(classAccessModifier)}{(isClassStatic ? " static" : "")} class {className}");
        sb.AppendLine("{");

        foreach (var property in obj!.Properties())
        {
            string propName = property.Name;
            string propType = Helper.GetCSharpType(property.Value.Type);
            if (includeJsonPropertyAttribute) sb.AppendLine($"    [JsonProperty(\"{propName}\")]");
            sb.AppendLine($"    public {propType}{(useNullableReferenceTypes ? "?" : "")} {(usePascalCase ? Helper.UpperCaseFirstLetter(propName) : propName)} {{ get; set; }}");

            // Determine if we are on the last property
            var lastProperty = obj.Properties().Last();
            if (includeJsonPropertyAttribute && property != lastProperty) sb.AppendLine();
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
        => new (
            isScrollable: true,
                Grid()
                .Rows(
                    (GridRows.ConfigRow, Auto),
                    (GridRows.UploadRow, Auto),
                    (GridRows.InputRow, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (GridRows.ErrorsRow, Auto)
                )
                .Columns((GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))
                .Cells(
                    Cell(
                        GridRows.ConfigRow,
                        GridColumns.Stretch,
                        Stack()
                            .Vertical()
                            .WithChildren(
                                SettingGroup()
                                    .Title("Settings")
                                    .Icon("FluentSystemIcons", '\uE670')
                                    .WithChildren(
                                        SettingGroup()
                                            .Title("Use Custom Class Name")
                                            .Description("Customize the class name")
                                            .WithChildren(
                                                _classNameInput
                                                .Title("Class Name")
                                                .OnTextChanged(OnClassNameSettingChanged)
                                            ),
                                        SettingGroup()
                                            .Title("Use Custom Namespace")
                                            .Description("Customize the namespace")
                                            .WithChildren(
                                                _namespaceNameInput
                                                .Title("Namespace")
                                                .OnTextChanged(OnNamespaceNamechanged)
                                            ),
                                        Setting()
                                            .Title(CashToolsClassGenerator.IncludeJsonPropertyAttribute)
                                            .Handle(
                                                _settingsProvider,
                                                _includeJsonPropertyAttributeSetting,
                                                OnIncludeJsonPropertyAttributeChanged
                                            ),
                                        Setting()
                                            .Title(CashToolsClassGenerator.MakeStaticClass)
                                            .Handle(
                                                _settingsProvider,
                                                _isClassStaticSetting,
                                                OnIsClassStaticChanged
                                            ),
                                        Setting()
                                            .Title(CashToolsClassGenerator.ClassAccessModifier)
                                            .Handle(
                                                _settingsProvider,
                                                _classAccessModifierSetting,
                                                OnClassAccessModifierChanged,
                                                Item("private", AccessModifierOptions.@private),
                                                Item("protected", AccessModifierOptions.@protected),
                                                Item("internal", AccessModifierOptions.@internal),
                                                Item("public", AccessModifierOptions.@public),
                                                Item("protected internal", AccessModifierOptions.@protectedInternal),
                                                Item("private protected", AccessModifierOptions.@privateProtected)
                                            ),
                                        Setting()
                                            .Title(CashToolsClassGenerator.UseNullableTypes)
                                            .Handle(
                                                _settingsProvider,
                                                _useNullableTypesSetting,
                                                OnUseNullableTypesChanged
                                            ),
                                        Setting()
                                            .Title(CashToolsClassGenerator.UsePascalCase)
                                            .Handle(
                                                _settingsProvider,
                                                _usePascalCaseSetting,
                                                OnUsePascalCaseChanged
                                            )
                                    )
                            )
                    ),
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
        
    private async void OnClassNameSettingChanged(string value) 
    {
        var settingValue = _settingsProvider.GetSetting(_classNameSetting);
        if (settingValue != value) _settingsProvider.SetSetting(_classNameSetting, value);
        await TriggerValidation(_input.Text);
    }

    public async void OnNamespaceNamechanged(string value)
    {
        var settingValue = _settingsProvider.GetSetting(_namespaceNameSetting);
        if (settingValue != value) _settingsProvider.SetSetting(_namespaceNameSetting, value);
        await TriggerValidation(_input.Text);
    }
    
    public async void OnIncludeJsonPropertyAttributeChanged(bool value)
    {
        var settingValue = _settingsProvider.GetSetting(_includeJsonPropertyAttributeSetting);
        if (settingValue != value) _settingsProvider.SetSetting(_includeJsonPropertyAttributeSetting, value);
        await TriggerValidation(_input.Text);
    }

    public async void OnIsClassStaticChanged(bool value)
    {
        var settingValue = _settingsProvider.GetSetting(_isClassStaticSetting);
        if (settingValue != value) _settingsProvider.SetSetting(_isClassStaticSetting, value);
        await TriggerValidation(_input.Text);
    }

    public async void OnClassAccessModifierChanged(AccessModifierOptions value)
    {
        var settingValue = _settingsProvider.GetSetting(_classAccessModifierSetting);
        if (settingValue != value) _settingsProvider.SetSetting(_classAccessModifierSetting, value);
        await TriggerValidation(_input.Text);
    }

    public async void OnUseNullableTypesChanged(bool value)
    {
        var settingValue = _settingsProvider.GetSetting(_useNullableTypesSetting);
        if (settingValue != value) _settingsProvider.SetSetting(_useNullableTypesSetting, value);
        await TriggerValidation(_input.Text);
    }

    public async void OnUsePascalCaseChanged(bool value)
    {
        var settingValue = _settingsProvider.GetSetting(_usePascalCaseSetting);
        if (settingValue != value) _settingsProvider.SetSetting(_usePascalCaseSetting, value);
        await TriggerValidation(_input.Text);
    }
}