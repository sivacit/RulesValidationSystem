using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using RulesEngine.Models;

using Microsoft.Extensions.Localization;
using System;
using System.IO;
using RuleValidationSystem.Models;
using System.Text.Json;
using System.Dynamic;



/// <summary>
/// Handles file uploads and validation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IStringLocalizer _localizer;
    private readonly FileParserFactory _factory;
    private readonly IRedisRepository _redis;

    private readonly IHttpClientFactory _httpClientFactory;


    private readonly IWebHostEnvironment _env;

    public static ExpandoObject ToExpando<T>(T obj)
    {
        var expando = new ExpandoObject();
        var dict = (IDictionary<string, object>)expando;

        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            dict[prop.Name] = prop.GetValue(obj);
        }

        return expando;
    }

    public FileUploadController(FileParserFactory factory, IRedisRepository redis, IStringLocalizerFactory localizerFactory, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
    {
        this._factory = factory;
        this._redis = redis;
        this._localizer = localizerFactory.Create("lang", "FileProcessor.API");
        this._env = env;
        this._httpClientFactory = httpClientFactory;

        Console.WriteLine("Assembly: " + Assembly.GetExecutingAssembly().GetName().Name);
        Console.WriteLine($"Using localizer: lang, assembly: {Assembly.GetExecutingAssembly().GetName().Name}");

    }

    [HttpGet("dump-localization")]
    public IActionResult Dump()
    {
        var all = _localizer.GetAllStrings(true)
            .Select(x => new { x.Name, x.Value });
        return Ok(all);
    }

    public static Dictionary<string, string> ToFlatDictionary(object obj, string prefix = "")
    {
        var result = new Dictionary<string, string>();

        if (obj is ExpandoObject expando)
        {
            foreach (var kvp in (IDictionary<string, object>)expando)
            {
                // If prefix is "Fields", drop it
                string newPrefix = (prefix == "Fields") ? kvp.Key :
                                   string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

                foreach (var nested in ToFlatDictionary(kvp.Value, newPrefix))
                {
                    result[nested.Key] = nested.Value;
                }
            }
        }
        else if (obj is IDictionary<string, string> dict)
        {
            foreach (var kvp in dict)
            {
                string key = (prefix == "Fields") ? kvp.Key :
                             string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
                result[key] = kvp.Value;
            }
        }
        else if (obj is IEnumerable<ExpandoObject> list)
        {
            int i = 0;
            foreach (var item in list)
            {
                foreach (var nested in ToFlatDictionary(item, $"{prefix}[{i}]"))
                {
                    result[nested.Key] = nested.Value;
                }
                i++;
            }
        }
        else
        {
            result[prefix] = obj?.ToString() ?? "";
        }

        return result;
    }


    /// <summary>
    /// Uploads a file (.csv, .json, .xml, .xlsx) and stores validated records in Redis.
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <returns>Success message and Redis key</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "lang.en.json");
        if (file == null || file.Length == 0)
            return BadRequest(_localizer["FileEmpty"]);

        var httpClient = _httpClientFactory.CreateClient("RuleEngine");
        var response = await httpClient.GetAsync("/api/Rules/Configure?workflowName=CSVWorkflow");

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
        var workflowJson = await response.Content.ReadAsStringAsync();
        Console.WriteLine(workflowJson);

        // Load rules and create the RulesEngine instance
        // var rulesPath = Path.Combine(_env.ContentRootPath, "rules.json");
        // var rulesJson = System.IO.File.ReadAllText(rulesPath);
        var workflow = JsonSerializer.Deserialize<Workflow>(workflowJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var workflows = new[] { workflow! }; // Wrap into array
        var rulesEngine = new RulesEngine.RulesEngine(workflows, null); // ✅ use this instead of _rulesEngine



        try
        {
            var parser = _factory.GetParser(file.ContentType, file.FileName);
            using var stream = file.OpenReadStream();
            var records = await parser.ParseAsync(stream);

            // Limit to first 100 records
            var limitedRecords = records?.Take(100).ToList();

            // Store validated records for response
            var validatedRecords = new List<ExpandoObject>();

            foreach (var record in limitedRecords)
            {
                var expando = ToExpando(record); // Convert FileRecord to ExpandoObject
                var rowDict = ToFlatDictionary(expando);
                foreach (var kvp in rowDict)
                {
                    Console.WriteLine($"{kvp.Key} = {kvp.Value}");
                }

                var dict = (IDictionary<string, object>)expando;
                var errors = new List<string>();

                var input = new RuleParameter("input1", rowDict);
                var ruleResults = await rulesEngine.ExecuteAllRulesAsync("CSVWorkflow", new[] { input });

                Console.WriteLine($" {dict} --------------- {ruleResults}");
                foreach (var result in ruleResults)
                {
                    if (!result.IsSuccess)
                    {
                        errors.Add(result.ExceptionMessage ?? result.Rule?.ErrorMessage ?? "Validation failed.");
                    }
                }

                dict["ValidationErrors"] = errors;
                validatedRecords.Add(expando);
            }

            Console.WriteLine(validatedRecords);

            // Store full original records in Redis
            var key = $"upload:{Guid.NewGuid()}";
            await _redis.StoreAsync(key, records);
            Console.WriteLine(validatedRecords);
            return Ok(new
            {
                message = _localizer["FileUploaded"],
                redisKey = key,
                data = validatedRecords // return validated 100 records with ValidationErrors
            });
        }
        catch (NotSupportedException)
        {
            return BadRequest(_localizer["FileTypeInvalid"]);
        }
    }
}
