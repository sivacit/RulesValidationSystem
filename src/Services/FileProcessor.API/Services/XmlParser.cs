// File: Services/XmlParser.cs

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FileProcessor.API.Services
{
    public class XmlParser : IFileParser
    {
        public async Task<IEnumerable<FileRecord>> ParseAsync(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            var xmlContent = await reader.ReadToEndAsync();

            var records = new List<FileRecord>();

            var document = XDocument.Parse(xmlContent);

            // Assumes structure like: <Root><Record><Field1>...</Field1></Record></Root>
            var root = document.Root;
            if (root == null)
                throw new InvalidDataException("Invalid XML format: missing root node.");

            var recordElements = root.Elements();

            foreach (var record in recordElements)
            {
                var fields = record.Elements()
                    .ToDictionary(el => el.Name.LocalName, el => el.Value);

                records.Add(new FileRecord { Fields = fields });
            }

            return records;
        }
    }
}
