using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RulesValidationSystem.Data;
using RulesValidationSystem.Dtos;
using RulesValidationSystem.Model;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;


namespace RulesValidationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RulesController : ControllerBase
    {
        private readonly RulesDbContext _context;
        private readonly string _dataDirectory;

        public RulesController(RulesDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _dataDirectory = Path.Combine(env.ContentRootPath, "Data");
        }

        /// <summary>
        /// Saves or updates a workflow configuration with the specified rules.
        /// </summary>
        /// <param name="input">
        /// The <see cref="WorkflowInputDto"/> containing the workflow name and rules to be saved.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the operation:
        /// - <c>BadRequest</c>
        /// <response code="200">Returns the workflow configuration and rules.</response>
        /// <response code="404">If the workflow is not found in the database.</response>
        [HttpGet("Configure")]
        public async Task<IActionResult> Configure(string workflowName = "CSVWorkflow")
        {
            Console.WriteLine($"------------- {workflowName}");

            var workflow = await _context.RulesWorkflows
                .FirstOrDefaultAsync(w => w.WorkflowName == workflowName);

            if (workflow == null)
            {
                return NotFound($"{workflowName} not found in database.");
            }

            // Define minimal rule structure to exclude unnecessary fields
            var allRules = JsonSerializer.Deserialize<List<RuleDto>>(workflow.RulesJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Filter out rules with missing RuleName or Expression
            var validRules = allRules
                .Where(r => !string.IsNullOrWhiteSpace(r.RuleName) && !string.IsNullOrWhiteSpace(r.Expression))
                .Select(r => new
                {
                    r.RuleName,
                    r.RuleExpressionType,
                    r.Expression,
                    r.ErrorMessage,
                    r.SuccessEvent
                })
                .ToList();

            var response = new
            {
                WorkflowName = workflow.WorkflowName,
                Rules = validRules
            };

            var jsonOutput = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return Content(jsonOutput, "application/json");
        }



        /// <summary>
        /// Saves or updates a workflow configuration with the specified rules.
        /// </summary>
        /// <param name="input">The workflow input containing the workflow name and rules.</param>
        /// <returns>A message indicating the result of the operation.</returns>
        /// <response code="200">Workflow saved successfully.</response>
        /// <response code="400">If the input is invalid.</response>
        [HttpPost("Configure")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SaveWorkflow([FromBody] WorkflowInputDto input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.WorkflowName) || input.Rules == null)
                return BadRequest("WorkflowName and Rules are required.");

            var rulesJson = JsonSerializer.Serialize(input.Rules, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var existing = await _context.RulesWorkflows
            .FirstOrDefaultAsync(w => w.WorkflowName == input.WorkflowName);
            Console.WriteLine($"Already existing workflow {existing}  --- {input.WorkflowName}");
            if (existing != null)
            {
                existing.RulesJson = rulesJson;
                _context.RulesWorkflows.Update(existing);
            }
            else
            {
                _context.RulesWorkflows.Add(new RulesWorkflow
                {
                    WorkflowName = input.WorkflowName,
                    RulesJson = rulesJson
                });
            }

            await _context.SaveChangesAsync();

            // Optional: Save to file if it's DefaultWorkflow or for all workflows
            var filePath = Path.Combine(_dataDirectory, $"{input.WorkflowName}.json");
            await System.IO.File.WriteAllTextAsync(filePath, rulesJson);

            return Ok(new { message = $"Workflow '{input.WorkflowName}' saved successfully." });
        }
    }
}