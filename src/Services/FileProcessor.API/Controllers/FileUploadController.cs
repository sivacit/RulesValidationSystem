using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.IO;


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

    public FileUploadController(FileParserFactory factory, IRedisRepository redis, IStringLocalizerFactory localizerFactory)
    {
        _factory = factory;
        _redis = redis;
        _localizer = localizerFactory.Create("lang", "FileProcessor.API");
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

    /// <summary>
    /// Uploads a file (.csv, .json, .xml, .xlsx) and stores validated records in Redis.
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <returns>Success message and Redis key</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "lang.en.json");
        Console.WriteLine($"---------------- File Path {filePath}");

        if (file == null || file.Length == 0)
            return BadRequest(_localizer["FileEmpty"]);

        try
        {
            var parser = _factory.GetParser(file.ContentType, file.FileName);
            using var stream = file.OpenReadStream();
            var records = await parser.ParseAsync(stream);

            var key = $"upload:{Guid.NewGuid()}";
            await _redis.StoreAsync(key, records);

            return Ok(new { message = _localizer["FileUploaded"], redisKey = key });
        }
        catch (NotSupportedException)
        {
            return BadRequest(_localizer["FileTypeInvalid"]);
        }
    }
}
