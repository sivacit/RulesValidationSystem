using System;
using System.ComponentModel.DataAnnotations;

namespace RulesValidationSystem.Model
{
    public class RulesWorkflow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string WorkflowName { get; set; } = string.Empty;

        [Required]
        public string RulesJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
