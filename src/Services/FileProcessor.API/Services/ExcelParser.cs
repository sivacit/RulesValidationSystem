// Services/ExcelParser.cs
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

public class ExcelParser : IFileParser
{
    public async Task<IEnumerable<FileRecord>> ParseAsync(Stream fileStream)
    {
        using var doc = SpreadsheetDocument.Open(fileStream, false);
        var sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
        var worksheet = ((WorksheetPart)doc.WorkbookPart.GetPartById(sheet.Id)).Worksheet;
        var rows = worksheet.GetFirstChild<SheetData>().Elements<Row>().ToList();

        var headers = rows.First().Elements<Cell>().Select(c => c.InnerText).ToList();
        var records = new List<FileRecord>();

        foreach (var row in rows.Skip(1))
        {
            var cells = row.Elements<Cell>().ToList();
            var record = new FileRecord { Fields = new Dictionary<string, string>() };
            for (int i = 0; i < headers.Count; i++)
            {
                record.Fields[headers[i]] = i < cells.Count ? cells[i].InnerText : "";
            }
            records.Add(record);
        }

        return records;
    }
}