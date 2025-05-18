using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RulesValidationSystem.Data;
using RulesValidationSystem.Model;
using System.IO;
using System.Threading.Tasks;

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

        [HttpGet("Configure")]
        public async Task<IActionResult> Configure(string workflowName = "DefaultWorkflow")
        {
            var workflow = await _context.RulesWorkflows
                .FirstOrDefaultAsync(w => w.WorkflowName == workflowName);

            if (workflow == null && workflowName == "DefaultWorkflow")
            {
                var filePath = Path.Combine(_dataDirectory, "DefaultWorkflow.json");
                if (System.IO.File.Exists(filePath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(filePath);
                    workflow = new RulesWorkflow
                    {
                        WorkflowName = "DefaultWorkflow",
                        RulesJson = json
                    };
                    return Content(json, "application/json");
                }

                return NotFound("DefaultWorkflow not found in database or file system.");
            }

            return Content(workflow.RulesJson, "application/json");
        }
    }
}