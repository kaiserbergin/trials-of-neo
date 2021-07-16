namespace TrialsOfNeo
{
    public class ActorWithSingleMovie : Actor
    {
        [NeoRelationship(type: "ACTED_IN", direction: RelationshipDirection.Outgoing)]
        public Movie Movie { get; set; }
    }
}