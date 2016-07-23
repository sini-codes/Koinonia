namespace Koinonia
{
    public interface IPathConfiguration
    {
        string RootPath { get; }
        string InstallRegistryPath { get; set; }
        string DefaultMappingPathAbsolute { get; }
        string DefaultMappingPathRelative { get; }
        string PackageConfigurationPath { get; }
        string RepositoryCachePath { get; }
    }
}