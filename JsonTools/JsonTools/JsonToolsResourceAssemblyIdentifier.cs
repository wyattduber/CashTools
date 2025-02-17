using DevToys.Api;
using System.ComponentModel.Composition;

namespace JsonTools;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(JsonToolsResourceAssemblyIdentifier))]
internal sealed class JsonToolsResourceAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}
