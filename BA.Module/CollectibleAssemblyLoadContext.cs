using System.Reflection;
using System.Runtime.Loader;

namespace libplugtool;

/// <summary>
/// 每个插件都使用单独的该对象，但是每个该对象都放在 PluginsLoad Context中
/// </summary>
/// <remarks>
/// Each plugin used its own instance of that object,
/// but each instance of that object is places within the `PluginLoad Context` Class.
/// Each plugin has its own program domain and does not conflict with other plugins.
/// </remarks>
public class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    public CollectibleAssemblyLoadContext() : base(isCollectible: true)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return base.Load(assemblyName);
    }
}