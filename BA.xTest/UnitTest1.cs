using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using BA.DB;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Versioning;
using Xunit.Abstractions;

namespace BA.xTest;

public class UnitTest1
{
    private readonly ITestOutputHelper _test_output_helper;
    private          int               count = 0;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        this._test_output_helper = testOutputHelper;
    }

    [Fact]
    public void Test2()
    {
        // List<string> result = GetDepen2("Microsoft.EntityFrameworkCore.SqlServer", "7.0.16").Result;

        // foreach (string s in result)
        // {
            // _test_output_helper.WriteLine(s);
        // }
    }




    private static async Task<List<string>> GetDepen(string id, string version)
    {
        List<string> list = new List<string>();

        // Firstly, Find the nuget package path by default.
        ISettings load_default_settings = Settings.LoadDefaultSettings(root: null);
        string global_packages_folder = SettingsUtility.GetGlobalPackagesFolder(settings: load_default_settings);

        list.Add(global_packages_folder);

        // Secondly, Query the framework description.
        string framework_description = RuntimeInformation.FrameworkDescription;
        string tag, tbg;

        tag = framework_description.Split(' ')[0].ToLower();
        tag = tag.TrimStart('.');

        tbg = framework_description.Split(' ')[1];


        list.Add(framework_description);


        CancellationToken cancellationToken = new CancellationToken();
        using PackageArchiveReader packageReader = new PackageArchiveReader("C:\\Users\\Atomic\\.nuget\\packages\\newtonsoft.json\\13.0.3\\newtonsoft.json.13.0.3.nupkg", new DefaultFrameworkNameProvider());
        NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(cancellationToken);

        string project_url = nuspecReader.GetProjectUrl();
        list.Add(project_url);
        return list;
    }

    private static string similarPath()
    {
        return ":";
    }
    [Fact]
    public void Test1()
    {
        Type            type        = typeof(UserModel);
        FieldInfo?      field_info  = type.GetField("age");
        FieldInfo?      field_info1  = type.GetField("age1");
        FieldInfo?      field_info2  = type.GetField("age2");
        FieldInfo?      field_info3  = type.GetField("age3");
        List<UserModel> user_models = new List<UserModel>();

        var sw = new Stopwatch();
        sw.Start();
        long s1 = sw.ElapsedMilliseconds;
        for (int i = 0; i < 1000000; i++)
        {
            object? instance = Activator.CreateInstance(type);

            field_info.SetValue(instance, i);
            field_info1.SetValue(instance, i);
            field_info2.SetValue(instance, i);
            field_info3.SetValue(instance, i);
        }

        _test_output_helper.WriteLine((sw.ElapsedMilliseconds - s1).ToString());
        s1 = sw.ElapsedMilliseconds;

        for (int i = 0; i < 1000000; i++)
        {
            UserModel instance = new UserModel();

            field_info.SetValue(instance, i);
            field_info1.SetValue(instance, i);
            field_info2.SetValue(instance, i);
            field_info3.SetValue(instance, i);
        }

        _test_output_helper.WriteLine((sw.ElapsedMilliseconds - s1).ToString());
    }
}