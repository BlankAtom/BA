using System.Runtime.InteropServices;
using System.Xml.Linq;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;

namespace BA.Loader;

public class Helpers
{
    public static List<string> GetDependencyFiles(string id, string version, string? framework = null)
    {
        List<string> list = new List<string>();
        // NuGetFramework targetFramework = NuGetFramework.Parse(framework);
        NuGetFramework targetFramework = string.IsNullOrEmpty(framework)
            ? GetNuGetFrameworkFromCurrent()
            : NuGetFramework.Parse(framework);

        if (targetFramework == NuGetFramework.UnsupportedFramework)
        {
            throw new InvalidOperationException($"Cannot resolving this framework: {framework}");
        }

        // Firstly, Find the nuget package path by default.
        ISettings settings = NuGet.Configuration.Settings.LoadDefaultSettings(root: null);
        string packagesFolder = NuGet.Configuration.SettingsUtility.GetGlobalPackagesFolder(settings: settings);

        string combine = Path.Combine(packagesFolder, id, version);
        if (!Directory.Exists(combine))
            throw new Exception("Cannot find the path with id&version.");


        // Read the nuspec to check reference about current package.
        var specPath = Directory.GetFiles(combine, "*.nuspec", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (specPath == null)
            throw new Exception("Cannot find the .nuspec with id&version.");

        var reducer = new FrameworkReducer();

        var nuspecXml = XDocument.Load(specPath);

        // Get the root namespace
        XNamespace ns = nuspecXml.Root.GetDefaultNamespace();

        var dependencies = nuspecXml.Descendants(ns + "dependencies");

        var dictionary = new Dictionary<NuGetFramework, List<(string id, string version)>>();

        foreach (var elementFramework in dependencies.Elements(ns + "group"))
        {
            string? targetFrameworkAttribute = elementFramework.Attribute("targetFramework")?.Value;

            NuGetFramework nuget_framework = NuGetFramework.Parse(targetFrameworkAttribute);
            dictionary[nuget_framework] = new List<(string id, string version)>();

            foreach (var dependency in elementFramework.Elements(ns + "dependency"))
            {
                string targetId = dependency.Attribute("id")?.Value;
                string targetVersion = dependency.Attribute("version")?.Value;

                // Catch the framework, packageId, packageVersion
                dictionary[nuget_framework].Add((targetId, targetVersion));
            }
        }

        var nearest = reducer.GetNearest(targetFramework, dictionary.Keys);

        if (nearest == null)
            return list;

        foreach (var tuple in dictionary[nearest])
        {
            List<string> package_items_path = GetPackageItemsPath(tuple.id, tuple.version, nearest.DotNetFrameworkName);

            list.AddRange(package_items_path);
        }

        return list;
    }

    public static List<string> GetPackageItemsPath(string id, string version, string? framework = null, NuGetFramework? specialFramework = null)
    {
        // NuGetFramework targetFramework = NuGetFramework.Parse(framework);
        NuGetFramework targetFramework = specialFramework != null
            ? specialFramework
            : string.IsNullOrEmpty(framework)
                ? GetNuGetFrameworkFromCurrent()
                : NuGetFramework.ParseFrameworkName(framework, DefaultFrameworkNameProvider.Instance);

        if (targetFramework == NuGetFramework.UnsupportedFramework)
        {
            throw new InvalidOperationException($"Cannot resolving this framework: {framework}");
        }

        // Firstly, Find the nuget package path by default.
        ISettings settings = NuGet.Configuration.Settings.LoadDefaultSettings(root: null);
        string packagesFolder = NuGet.Configuration.SettingsUtility.GetGlobalPackagesFolder(settings: settings);

        string combine = Path.Combine(packagesFolder, id, version);
        var packagePath = Directory.GetFiles(combine, "*.nupkg", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (string.IsNullOrEmpty(packagePath) || !File.Exists(packagePath))
        {
            throw new Exception("This package file not found");
        }


        List<string> list = new List<string>();
        using var packageStream = File.OpenRead(packagePath);
        var reader = new PackageArchiveReader(packageStream);

        // Get supported framework list
        var frameworks = reader.GetSupportedFrameworks().ToList();
        var frameworkItems = reader.GetLibItems().ToList();

        var reducer = new FrameworkReducer();

        // Find the nearest framework
        var nearest = reducer.GetNearest(targetFramework, frameworks.Select(x => x));
        var items = frameworkItems.Where(x => x.TargetFramework.Equals(nearest));

        foreach (var item in items)
        {
            foreach (var file in item.Items)
            {
                if (file.EndsWith(".dll"))
                {
                    // check file in local nuget path
                    string existPath = Path.Combine(combine, file);
                    if (File.Exists(existPath))
                        list.Add(existPath);
                }
            }
        }

        return list;
    }


    public static NuGetFramework GetNuGetFrameworkFromCurrent()
    {
        // Get current framework of execute app
        var frameworkDescription = RuntimeInformation.FrameworkDescription;

        //
        string identifier;
        string versionString;

        if (frameworkDescription.StartsWith(".NET Core"))
        {
            identifier = ".NETCoreApp";
            versionString = frameworkDescription.Substring(".NET Core ".Length);
        }
        else if (frameworkDescription.StartsWith(".NET Framework"))
        {
            identifier = ".NETFramework";
            versionString = frameworkDescription.Substring(".NET Framework ".Length);
        }
        else if (frameworkDescription.StartsWith(".NET "))
        {
            identifier = ".NET";
            versionString = frameworkDescription.Substring(".NET ".Length);
        }
        else if (frameworkDescription.StartsWith(".NET Standard "))
        {
            identifier = ".NETStandard";
            versionString = frameworkDescription.Substring(".NET Standard ".Length);
        }
        else
        {
            throw new InvalidOperationException($"Cannot resolving this framework description: {frameworkDescription}");
        }

        // Major.Min
        var versionParts = versionString.Split('.');
        versionString = string.Join(".", versionParts[0], versionParts[1]);

        // Create framework object
        var nugetFramework = NuGetFramework.Parse($"{identifier}{versionString}");

        return nugetFramework;
    }
}