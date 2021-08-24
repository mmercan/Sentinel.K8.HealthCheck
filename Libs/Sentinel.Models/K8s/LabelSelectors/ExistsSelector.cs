namespace Sentinel.Models.K8s.LabelSelectors
{
    public record ExistsSelector : ILabelSelector
    {
        public ExistsSelector(string label) => Label = label;

        public string Label { get; }

        public string ToExpression() => $"{Label}";
    }
}