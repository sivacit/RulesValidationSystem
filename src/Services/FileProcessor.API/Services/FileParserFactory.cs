using FileProcessor.API.Services;

public class FileParserFactory
{
    private readonly IServiceProvider _provider;
    public FileParserFactory(IServiceProvider provider) => _provider = provider;

    public IFileParser GetParser(string contentType, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLower();
        return ext switch
        {
            ".csv"  => _provider.GetRequiredService<CsvParser>(),
            ".json" => _provider.GetRequiredService<JsonParser>(),
            ".xml"  => _provider.GetRequiredService<XmlParser>(),
            ".xlsx" => _provider.GetRequiredService<ExcelParser>(),
            _       => throw new NotSupportedException("Unsupported file type")
        };
    }
}
