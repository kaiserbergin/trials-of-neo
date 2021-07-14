namespace TrialsOfNeo
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class NeoPropertyAttribute : System.Attribute
    {
        public string Name { get; set; }

        public NeoPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}