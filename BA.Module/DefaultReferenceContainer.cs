namespace libplugtool;

public class DefaultReferenceContainer : IReferenceContainer
{
    private static Dictionary<CachedItemKey, Stream> _cachedStream = new Dictionary<CachedItemKey, Stream>();

    public List<CachedItemKey> GetAll()
        => _cachedStream.Keys.ToList();

    public bool Exist(string name, string version)
        => _cachedStream.ContainsKey(new CachedItemKey(name, version));

    public void SaveStream(string name, string version, Stream stream)
    {
        if (Exist(name: name, version))
            return;

        _cachedStream.Add(new CachedItemKey(name, version), stream);
    }

    public Stream GetStream(string name, string version)
    {
        if (!Exist(name, version))
            return null;

        var key = new CachedItemKey(name, version);
        _cachedStream[key].Position = 0;
        return _cachedStream[key];
    }
}