using System.Reflection;
using Services.Constants;

namespace Services.Configurations;
public class ModuleConfiguration
{
    public CacheDefintion? Cache { internal get; set; } = new MemoryCacheDefintion();

    public bool DisableMediaR { internal get; set; } = false;

    public Assembly[] Assemblies { internal get; set; } = [];
}