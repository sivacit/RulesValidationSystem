using System;
using System.Collections.Generic;
using System.Linq;

namespace RulesValidationSystem.Services
{
    public static class CsvParserService
    {
        public static List<Dictionary<string, string>> Parse(string csv)
        {
            var lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (lines.Count < 2)
                return new List<Dictionary<string, string>>();

            var headers = lines[0].Split(',').Select(h => h.Trim()).ToList();
            var rows = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Count; i++)
            {
                var values = lines[i].Split(',');
                var row = new Dictionary<string, string>();

                for (int j = 0; j < headers.Count; j++)
                {
                    string value = j < values.Length ? values[j].Trim() : "";
                    row[headers[j]] = value;
                }

                rows.Add(row);
            }

            return rows;
        }
    }
}
