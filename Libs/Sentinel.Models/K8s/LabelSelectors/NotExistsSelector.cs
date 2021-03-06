namespace Sentinel.Models.K8s.LabelSelectors
{
    public record NotExistsSelector : ILabelSelector
    {
        public NotExistsSelector(string label) => Label = label;

        public string Label { get; }

        public string ToExpression() => $"!{Label}";
    }
}