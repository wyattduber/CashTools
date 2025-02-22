# Class Generator
This extension accepts a JSON payload and generates a C# class.

> NOTE: This extension does not yet support the ability to generate sub-classes from complex JSON objects. That is in development and will come in an upcoming version.

There are options available to customize the class that is generated.

Options:
- Custom Class Name
- Custom Namespace
- JsonProperty Attribute Generation
    - Ability to add \[JsonProperty()\] attributes above each member of the generated class, part of the [Json.NET (Newtonsoft.Json)](https://github.com/JamesNK/Newtonsoft.Json) package.
- Class Modifiers
    - Access Modifiers 
        - `private`,
        - `protected`,
        - `internal`,
        - `public`,
        - `protected internal`,
        - `private protected`
    - Static Modifier
        - `public static class ...`
- Nullable Types
    - Ex: `string` -> `string?`
- Pascal Case Member Names
    - Ex: `myFirstProperty` -> `MyFirstProperty`

## Example Usage

Input Body:
```json
{
    "id": "340beb75-cd5a-ed11-8c36-000d3a9ce7e3",
    "timestamp": 8172319,
    "name": "MyObject",
    "displayName": "My Display Name",
    "businessProfileId": ""
}
```

Generated Class:
```csharp
namespace MyNamespace;

public class MyClass
{
    public string id { get; set; }
    public int timestamp { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string businessProfileId { get; set; }
}
```

> NOTE: Advanced types, such as `Guid` are still being added.