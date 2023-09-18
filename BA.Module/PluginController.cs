using System.Runtime.Loader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace libplugtool;

[ApiController]
public class PluginController : ControllerBase
{
    private readonly PluginManager _pluginManager;

    public PluginController(PluginManager pluginManager)
    {
        this._pluginManager = pluginManager;
    }

    public IActionResult Enable(Guid id)
    {
        IPlug plug = this._pluginManager.GetPlugin(id);
        if (!PluginsLoadContexts.Any(plug.name))
        {
            var context = new CollectibleAssemblyLoadContext();

            var plugName = plug.name;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default", $"{plugName}.dll");

            var assembly = context.LoadFromAssemblyPath(path);

            var part =new AssemblyPart(assembly);

            this._pluginManager.EnablePlugin(id);
            this._pluginManager.AddApplicationParts(part);
            this._pluginManager.Refresh();

            PluginsLoadContexts.AddPlugin(plugName, context);
        }
        else
        {
            var context = PluginsLoadContexts.GetContext(plug.name);
            var part    = new AssemblyPart(context.Assemblies.First());

            this._pluginManager.EnablePlugin(id);
            this._pluginManager.AddApplicationParts(part);
            this._pluginManager.Refresh();
        }

        return Ok();
    }

    public IActionResult Delete(Guid id)
    {
        var plug = this._pluginManager.GetPlugin(id);
        this._pluginManager.DisablePlugin(id);
        this._pluginManager.DeletePlugin(id);

        var pluginName = plug.name;

        var match = this._pluginManager.FindApplicationParts(pluginName);

        if (match != null)
        {
            this._pluginManager.RemoveApplicationParts(match);
            match = null;
        }

        this._pluginManager.Refresh();
        PluginsLoadContexts.RemovePlugin(pluginName);

        return Ok();
    }
}