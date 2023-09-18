using System.Runtime.Loader;

namespace libplugtool;

/// <summary>
/// 将当前的插件的程序集放到字典中，当移除时，需要Unload来释放上下文
/// </summary>
public static class PluginsLoadContexts
{
    private static Dictionary<string, CollectibleAssemblyLoadContext>
        _pluginContexts;

    static PluginsLoadContexts()
    {
        _pluginContexts = new Dictionary<string, CollectibleAssemblyLoadContext>();
    }

    /// <summary>
    /// Determine the existence of a plugin based on its name.
    /// </summary>
    /// <param name="pluginName">name of plugin without extension name.</param>
    /// <returns>true: exists, false: not exists</returns>
    public static bool Any(string pluginName) => _pluginContexts.ContainsKey(pluginName);

    /// <summary>
    /// Add plugin with name and context.
    /// </summary>
    /// <param name="pluginName">name of plugin</param>
    /// <param name="context">context of target</param>
    public static void AddPlugin(string pluginName, CollectibleAssemblyLoadContext context)
    {
        _pluginContexts.Add(pluginName, context);
    }

    /// <summary>
    /// Remove plugin and unload target context.
    /// </summary>
    /// <param name="pluginName">name of plugin</param>
    public static void RemovePlugin(string pluginName)
    {
        if (!Any(pluginName))
            return;

        _pluginContexts[pluginName].Unload();
        _pluginContexts.Remove(pluginName);
    }

    /// <summary>
    /// Obtain the target named pluginName.
    /// </summary>
    /// <param name="pluginName">plugin named</param>
    /// <returns></returns>
    public static AssemblyLoadContext GetContext(string pluginName) => _pluginContexts[pluginName];
}