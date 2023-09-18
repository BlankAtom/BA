using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace libplugtool;

public class PluginManager
{
    public static PluginManager instance
    {
        get;
    } = new();

    private Dictionary<Guid, IPlug> _plugs;

    public PluginManager()
    {
        this._plugs = new();
    }

    public IPlug GetPlugin(Guid id)
    {
        return this._plugs[id];
    }

    public void EnablePlugin(Guid id)
    {
        throw new NotImplementedException();
    }

    public void AddApplicationParts(AssemblyPart part)
    {
        throw new NotImplementedException();
    }

    public void Refresh()
    {
        throw new NotImplementedException();
    }

    public void DisablePlugin(Guid id)
    {
        throw new NotImplementedException();
    }

    public void DeletePlugin(Guid id)
    {
        throw new NotImplementedException();
    }

    public void RemoveApplicationParts(object match)
    {
        throw new NotImplementedException();
    }

    public ApplicationPart FindApplicationParts(string plugin_name)
    {
        throw new NotImplementedException();
    }
}