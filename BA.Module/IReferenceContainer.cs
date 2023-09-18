namespace libplugtool;

/// <summary>
/// Reference assembly buffer container.
/// </summary>
public interface IReferenceContainer
{
    /// <summary>
    /// Get all assembly of system.
    /// </summary>
    /// <returns></returns>
    List<CachedItemKey> GetAll();

    /// <summary>
    /// Determine the plugin is exists or not.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    bool Exist(string name, string version);

    /// <summary>
    /// Save the file stream of plugin.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <param name="stream"></param>
    void SaveStream(string name, string version, Stream stream);

    /// <summary>
    /// Get file stream of plugin.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    Stream GetStream(string name, string version);
}
