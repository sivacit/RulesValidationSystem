using Microsoft.Extensions.Logging;

using RulesEngine.Models;
using RulesEngine;
using System.Text.Json;
using RulesValidationSystem.Model;


namespace RulesValidationSystem.Services
{
    public class RulesService
    {
        private readonly RulesEngine.RulesEngine _rulesEngine;
        private readonly ILogger<RulesService> _logger;


        public RulesService(IWebHostEnvironment env, ILogger<RulesService> logger)
        {
            var rulesFilePath = Path.Combine(env.ContentRootPath, "rules.json");
            var json = File.ReadAllText(rulesFilePath);
            var workflows = JsonSerializer.Deserialize<Workflow[]>(json); // ✅ Fixed
            _rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray(), null);
            _logger = logger;
            _logger.LogInformation("RulesService constructor started");
        }

        public async Task<string> EvaluateCustomerAsync(Customer customer)
        {
            _logger.LogInformation($"Evaluating customer: {customer.Name}, {customer.TotalPurchases}");

            var inputs = new RuleParameter[] { new RuleParameter("input1", customer) };
            var results = await _rulesEngine.ExecuteAllRulesAsync("DiscountWorkflow", inputs);

            foreach (var result in results)
            {
                _logger.LogInformation($"Rule: {result.Rule.RuleName}, Success: {result.IsSuccess}, Exception: {result.ExceptionMessage}");
            }

            var first = results.FirstOrDefault();
            if (first?.IsSuccess == true)
                return "Rule Passed";
            else
                return first?.ExceptionMessage ?? "Rule Failed";
        }

    }
}
