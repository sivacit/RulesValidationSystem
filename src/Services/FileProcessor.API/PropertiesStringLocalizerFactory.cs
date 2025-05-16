using Microsoft.Extensions.Localization;

public class PropertiesStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly string _resourcesPath;
    public PropertiesStringLocalizerFactory(IWebHostEnvironment env)
    {
        _resourcesPath = Path.Combine(env.ContentRootPath, "Resources");
    }

    public IStringLocalizer Create(Type resourceSource) =>
        new PropertiesStringLocalizer(resourceSource.Name, _resourcesPath);

    public IStringLocalizer Create(string baseName, string location) =>
        new PropertiesStringLocalizer(baseName, _resourcesPath);
}
