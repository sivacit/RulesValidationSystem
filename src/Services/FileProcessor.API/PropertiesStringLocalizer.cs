using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Collections.Concurrent;

public class PropertiesStringLocalizer : IStringLocalizer
{
    private readonly string _resourceName;
    private readonly string _resourcePath;
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache = new();

    public PropertiesStringLocalizer(string resourceName, string resourcePath)
    {
        _resourceName = resourceName;
        _resourcePath = resourcePath;
    }

    private Dictionary<string, string> LoadProperties(CultureInfo culture)
    {
        var filePath = Path.Combine(_resourcePath, $"{_resourceName}.{culture.Name}.properties");
        if (!File.Exists(filePath)) return new Dictionary<string, string>();

        var lines = File.ReadAllLines(filePath);
        var dict = new Dictionary<string, string>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
                dict[parts[0].Trim()] = parts[1].Trim();
        }
        return dict;
    }

    private Dictionary<string, string> GetCultureStrings(CultureInfo culture)
    {
        return _cache.GetOrAdd(culture.Name, _ => LoadProperties(culture));
    }

    public LocalizedString this[string name]
    {
        get
        {
            var culture = CultureInfo.CurrentUICulture;
            var dict = GetCultureStrings(culture);
            return new LocalizedString(name, dict.TryGetValue(name, out var value) ? value : name, resourceNotFound: !dict.ContainsKey(name));
        }
    }

    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(this[name].Value, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = CultureInfo.CurrentUICulture;
        var dict = GetCultureStrings(culture);
        return dict.Select(kv => new LocalizedString(kv.Key, kv.Value, resourceNotFound: false));
    }
}
