using Microsoft.AspNetCore.Mvc;
using RulesEngine.Models;
using RulesEngine;
using System.Text.Json;
using RulesValidationSystem.Model;
using RulesValidationSystem.Services;

namespace RulesValidationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValidationController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ValidationController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
                return BadRequest("Upload failed.");

            var rulesPath = Path.Combine(_env.ContentRootPath, "rules.json");
            var rulesJson = System.IO.File.ReadAllText(rulesPath);
            var workflows = JsonSerializer.Deserialize<Workflow[]>(rulesJson);
            var engine = new RulesEngine.RulesEngine(workflows, null);

            using var reader = new StreamReader(csvFile.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            var rows = CsvParserService.Parse(content); // returns List<dynamic>

            var results = new List<object>();

            foreach (var row in rows)
            {
                var input = new RuleParameter("input1", row);
                var outcome = await engine.ExecuteAllRulesAsync("CSVWorkflow", new[] { input });

                results.Add(new
                {
                    Row = row,
                    Outcome = outcome.Select(r => new
                    {
                        r.Rule.RuleName,
                        r.IsSuccess,
                        r.ExceptionMessage
                    })
                });
            }

            return Json(results);
        }
    }
}