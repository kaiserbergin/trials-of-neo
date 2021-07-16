using System;

namespace TrialsOfNeo
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NeoPropertyAttribute : Attribute
    {
        public string Name { get; set; }

        public NeoPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}