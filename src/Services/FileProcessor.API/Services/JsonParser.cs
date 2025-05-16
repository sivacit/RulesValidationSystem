using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileProcessor.API.Services
{
    public class JsonParser : IFileParser
    {
        public async Task<IEnumerable<FileRecord>> ParseAsync(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            var content = await reader.ReadToEndAsync();

            var records = new List<FileRecord>();

            var jsonArray = JArray.Parse(content); // assumes top-level array of objects

            foreach (JObject obj in jsonArray)
            {
                var dict = obj.Properties()
                              .ToDictionary(p => p.Name, p => p.Value.ToString());

                records.Add(new FileRecord { Fields = dict });
            }

            return records;
        }
    }
}
