namespace RulesValidationSystem.Dtos
{
    public class RuleDto
    {
        public string RuleName { get; set; }
        public string RuleExpressionType { get; set; }
        public string Expression { get; set; }
        public string ErrorMessage { get; set; }

        public string SuccessEvent { get; set; }

        public bool Enabled { get; set; }
    }
}
