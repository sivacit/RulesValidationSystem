namespace RulesValidationSystem.Dtos
{
    public class WorkflowInputDto
    {
        public string WorkflowName { get; set; }
        public List<RuleDto> Rules { get; set; }
    }
}
