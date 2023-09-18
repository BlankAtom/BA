using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace libplugtool;

public class DefaultReferenceLoader : IReferenceLoader
{
    private IReferenceContainer _referenceContainer = null;

    private readonly ILogger<DefaultReferenceLoader> _logger = null;

    public DefaultReferenceLoader(IReferenceContainer referenceContainer, ILogger<DefaultReferenceLoader> logger)
    {
        _referenceContainer = referenceContainer;
        _logger             = logger;
    }

    public void LoadStreamsIntoContext(AssemblyLoadContext context, string moduleFolder, Assembly assembly)
    {
        var references = assembly.GetReferencedAssemblies();

        foreach (var item in references)
        {
            var name = item.Name;

            var version = item.Version.ToString();

            var stream = _referenceContainer.GetStream(name, version);

            if (stream != null)
            {
                _logger.LogDebug($"Found the cached reference '{name}' v.{version}");
                context.LoadFromStream(stream);
            }
            else
            {

                if (IsSharedFramework(name))
                {
                    continue;
                }

                var dllName  = $"{name}.dll";
                var filePath = $"{moduleFolder}\\{dllName}";

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning($"The package '{dllName}' is missing.");
                    continue;
                }

                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    var referenceAssembly = context.LoadFromStream(fs);

                    var memoryStream = new MemoryStream();

                    fs.Position = 0;
                    fs.CopyTo(memoryStream);
                    fs.Position           = 0;
                    memoryStream.Position = 0;
                    _referenceContainer.SaveStream(name, version, memoryStream);

                    LoadStreamsIntoContext(context, moduleFolder, referenceAssembly);
                }
            }
        }
    }

    private bool IsSharedFramework(string name)
    {
        return SharedFrameworkConst.IsFrameWork(name);
    }
}

internal static class SharedFrameworkConst
{
    public static List<string> SharedFrameWorksMatches = new()
    {
        "Microsoft.AspNetCore.*",
        "System.*",
        "api-ms-win-*",
        "clrcompression",
        "clrjit",
        "coreclr",
        "dbgshim",
        "hostpolicy",
        "Microsoft.CSharp",
        "Microsoft.DiaSymReader.*",
        "Microsoft.NETCore.*",
        "Microsoft.Win32.*",
        "Microsoft.VisualBasic.*",
        "mscordaccore",
        "mscordaccore_amd64*",
        "mscordbi",
        "mscorlib",
        "mscorrc.*",
        "mscorrc",
        "netstandard",
        "ucrtbase",
        "WindowsBase",
    };

    public static bool IsFrameWork(string nameWithoutExtension)
    {
        throw new NotImplementedException();
    }
}