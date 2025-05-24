using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

public class ExcelParser : IFileParser
{
    public async Task<IEnumerable<FileRecord>> ParseAsync(Stream fileStream)
    {
        using var doc = SpreadsheetDocument.Open(fileStream, false);
        var workbookPart = doc.WorkbookPart!;
        var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

        var sheet = workbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
        var worksheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id!)).Worksheet;
        var rows = worksheet.GetFirstChild<SheetData>()!.Elements<Row>().ToList();

        var headers = rows.First().Elements<Cell>().Select(c => GetCellValue(c, sharedStrings)).ToList();
        var records = new List<FileRecord>();

        foreach (var row in rows.Skip(1))
        {
            var cells = row.Elements<Cell>().ToList();
            var record = new FileRecord { Fields = new Dictionary<string, string>() };

            for (int i = 0; i < headers.Count; i++)
            {
                string value = i < cells.Count ? GetCellValue(cells[i], sharedStrings) : "";
                record.Fields[headers[i]] = value;
            }

            records.Add(record);
        }

        return records;
    }

    private string GetCellValue(Cell cell, SharedStringTable? sharedStrings)
    {
        if (cell == null || cell.CellValue == null) return "";

        string value = cell.CellValue.InnerText;

        if (cell.DataType != null && cell.DataType == CellValues.SharedString && sharedStrings != null)
        {
            if (int.TryParse(value, out int index) && index < sharedStrings.Count())
            {
                return sharedStrings.ElementAt(index).InnerText;
            }
        }

        return value;
    }
}
