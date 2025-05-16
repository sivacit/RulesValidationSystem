using System.Globalization;
using CsvHelper;

public class CsvParser : IFileParser
{
    public async Task<IEnumerable<FileRecord>> ParseAsync(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var records = new List<FileRecord>();
        await foreach (var row in csv.GetRecordsAsync<dynamic>())
        {
            var record = new FileRecord { Fields = ((IDictionary<string, object>)row).ToDictionary(k => k.Key, k => k.Value?.ToString()) };
            records.Add(record);
        }
        return records;
    }
}
