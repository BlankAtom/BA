// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using BA.Loader;
using NuGet.Configuration;

// Console.WriteLine("Hello, World!");

AssemblyLoadContext context = new AssemblyLoadContext("Test");
Assembly assembly = context.LoadFromAssemblyPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Microsoft.EntityFrameworkCore.SqlServer.dll"));
context.LoadFromAssemblyPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Microsoft.EntityFrameworkCore.dll"));
context.LoadFromAssemblyPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Microsoft.EntityFrameworkCore.Relational.dll"));
context.LoadFromAssemblyPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Microsoft.EntityFrameworkCore.Abstractions.dll"));
context.LoadFromAssemblyPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Microsoft.Extensions.DependencyInjection.Abstractions.dll"));
context.LoadFromAssemblyPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Microsoft.Extensions.Logging.Abstractions.dll"));
Assembly assembly2 = context.LoadFromAssemblyPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Microsoft.Data.SqlClient.dll"));
Version? version = assembly2.GetName().Version;

string ver = $"{version.Major}.{version.Minor}.{version.Build}";
List<string> dependency_files = Helpers.GetDependencyFiles(assembly2.GetName().Name, "5.1.4");
AssemblyName[] referenced_assemblies = assembly.GetReferencedAssemblies();

// Helpers.GetPackageItemsPath(assembly.GetName().Name, ver);

dependency_files.ForEach(t =>
{
    Console.WriteLine(t);
} );

assembly.GetTypes();

Console.WriteLine(referenced_assemblies.Length);
