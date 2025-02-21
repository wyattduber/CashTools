using DevToys.Api;
using static DevToys.Api.GUI;

namespace CashTools.ClassGenerator.Helpers;

internal class UIHelper
{
    internal static IUIInfoBar GetGeneralErrorInfoBar(string error) =>
        GetErrorInfoBar(CashTools.GeneralError, error);

    internal static IUIInfoBar GetErrorInfoBar(string title, string error) =>
        InfoBar().ShowIcon().Title(title).Description(error).NonClosable().Error().Open();
}
