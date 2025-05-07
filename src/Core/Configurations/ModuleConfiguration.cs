using System.Reflection;
using Core.Constants;

namespace Core.Configurations;
public class ModuleConfiguration
{
    internal static string? AppName;
    public CacheDefintion? Cache { get; set; } = new MemoryCacheDefintion();

    public bool DisableMediaR { internal get; set; } = false;

    public Assembly[] Assemblies { internal get; set; } = [];
}