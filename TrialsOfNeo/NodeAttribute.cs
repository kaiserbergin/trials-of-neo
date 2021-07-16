using System;

namespace TrialsOfNeo
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NodeAttribute : Attribute
    {
        public string Label { get; }

        public NodeAttribute(string label)
        {
            Label = label;
        }
    }
}