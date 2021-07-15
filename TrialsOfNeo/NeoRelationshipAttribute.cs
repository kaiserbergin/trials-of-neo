namespace TrialsOfNeo
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class NeoRelationshipAttribute : System.Attribute
    {
        public string Type { get; set; }
        public RelationshipDirection Direction { get; set; }

        public NeoRelationshipAttribute(string type, RelationshipDirection direction)
        {
            Type = type;
            Direction = direction;
        }
    }
}