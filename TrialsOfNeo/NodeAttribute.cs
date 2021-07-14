namespace TrialsOfNeo
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class NodeAttribute : System.Attribute
    {
        public string Label { get; }

        public NodeAttribute(string label)
        {
            Label = label;
        }
    }
}