using DevToys.Api;
using System.ComponentModel.Composition;

namespace CashTools;

[Export(typeof(GuiToolGroup))]
[Name("CashTools.NET")]
[Order(Before = PredefinedCommonToolGroupNames.Converters)]
internal class CashToolsGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal CashToolsGroup()
    {
        IconFontName = "FluentSystemIcons";
        IconGlyph = '\uE670';
        DisplayTitle = "CashTools.NET";
        AccessibleName = "CashTools.NET";
    }
}
