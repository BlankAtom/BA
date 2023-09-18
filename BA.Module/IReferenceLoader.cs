using System.Reflection;
using System.Runtime.Loader;

namespace libplugtool;

public interface IReferenceLoader
{
    void LoadStreamsIntoContext(AssemblyLoadContext context, string folder, Assembly assembly);
}
