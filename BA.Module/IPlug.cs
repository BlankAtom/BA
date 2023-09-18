namespace libplugtool;

public class IPlug
{
    public string name    { get; private set; }
    public string version { get; private set; }

    public IPlug(string _name, string _version)
    {
        name    = _name;
        version = _version;
    }
}