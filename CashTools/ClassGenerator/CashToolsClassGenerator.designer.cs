using System;

namespace CashTools.ClassGenerator;

internal class CashToolsClassGenerator {

    private static global::System.Resources.ResourceManager resourceMan;

    private static global::System.Globalization.CultureInfo resourceCulture;

    [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    internal CashToolsClassGenerator() {
    }

    /// <summary>
    ///   Returns the cached ResourceManager instance used by this class.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
    internal static global::System.Resources.ResourceManager ResourceManager {
        get {
            if (object.ReferenceEquals(resourceMan, null)) {
                global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CashTools.ClassGenerator.CashToolsClassGenerator", typeof(CashToolsClassGenerator).Assembly);
                resourceMan = temp;
            }
            return resourceMan;
        }
    }

    /// <summary>
    ///   Overrides the current thread's CurrentUICulture property for all
    ///   resource lookups using this strongly typed resource class.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
    internal static global::System.Globalization.CultureInfo Culture {
        get {
            return resourceCulture;
        }
        set {
            resourceCulture = value;
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to A &quot;Hello World&quot; extension for DevToys.
    /// </summary>
    internal static string AccessibleName {
        get {
            return ResourceManager.GetString("AccessibleName", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to A sample extension.
    /// </summary>
    internal static string Description {
        get {
            return ResourceManager.GetString("Description", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to My Awesome Extension.
    /// </summary>
    internal static string LongDisplayTitle {
        get {
            return ResourceManager.GetString("LongDisplayTitle", resourceCulture);
        }
    }

    /// <summary>
    ///   Looks up a localized string similar to My Extension.
    /// </summary>
    internal static string ShortDisplayTitle {
        get {
            return ResourceManager.GetString("ShortDisplayTitle", resourceCulture);
        }
    }

    /// <summary>
    ///  Looks up a localized string similar to Input.
    ///  </summary>
    internal static string Input {
        get {
            return ResourceManager.GetString("Input", resourceCulture);
        }
    }

    /// <summary>
    ///  Looks up a localized string similar to Output.
    ///  </summary>
    internal static string SchemaOutput {
        get {
            return ResourceManager.GetString("SchemaOutput", resourceCulture);
        }
    }

    /// <summary>
    /// Looks up a localized string similar to Class Output.
    /// </summary>
    internal static string ClassOutput {
        get {
            return ResourceManager.GetString("ClassOutput", resourceCulture);
        }
    }

    /// <summary>
    /// Looks up a localized string similar to a generalized error message
    /// </summary>
    internal static string GeneralError {
        get {
            return ResourceManager.GetString("GeneralError", resourceCulture);
        }
    }

    /// <summary>
    /// Looks up a localized string similar to The input file is required.
    /// </summary>
    internal static string JsonRequiredError {
        get {
            return ResourceManager.GetString("JsonRequiredError", resourceCulture);
        }
    }

    /// <summary>
    /// Looks up a localized string similar to a generalized error message
    /// </summary>
    internal static string InputError {
        get {
            return ResourceManager.GetString("InputError", resourceCulture);
        }
    }

    /// <summary>
    /// Looks up a localized string similar to The input file is required.
    /// </summary>
    internal static string Success {
        get {
            return ResourceManager.GetString("Success", resourceCulture);
        }
    }

    /// <summary>
    /// Looks up a localized string similar to The input file is required.
    /// </summary>
    internal static string ClassGenerated {
        get {
            return ResourceManager.GetString("ClassGenerated", resourceCulture);
        }
    }

    /// <summary>
    /// Looks up a localized string similar to The input file is required.
    /// </summary>
    internal static string SchemaError {
        get {
            return ResourceManager.GetString("SchemaError", resourceCulture);
        }
    }
}
