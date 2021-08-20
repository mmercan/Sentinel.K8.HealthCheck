namespace Sentinel.K8s.DotnetKubernetesClient.LabelSelectors
{
    public record ExistsSelector : ILabelSelector
    {
        public ExistsSelector(string label) => Label = label;

        public string Label { get; }

        public string ToExpression() => $"{Label}";
    }
}