using DevToys.Api;
using System.ComponentModel.Composition;

namespace CashTools;

[Export(typeof(GuiToolGroup))]
[Name(nameof(CashTools.BaseGroupName))]
[Order(Before = PredefinedCommonToolGroupNames.Converters)]
internal sealed class CashToolsGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal CashToolsGroup()
    {
        IconFontName = "FluentSystemIcons";
        IconGlyph = '\uE670';
        DisplayTitle = CashTools.BaseGroupName;
        AccessibleName = CashTools.BaseGroupName;
    }
}
