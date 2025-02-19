using DevToys.Api;
using System.ComponentModel.Composition;

namespace CashTools;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(CashToolsResourceAssemblyIdentifier))]
internal sealed class CashToolsResourceAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}
