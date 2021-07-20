using System.Collections.Generic;

namespace TrialsOfNeo
{
    public class MovieWithRelationships : Movie
    {
        // We need to ensure that we don't infinite loop on self ref's
        // [NeoRelationship(type: "ACTED_IN", direction: RelationshipDirection.Incoming)]
        // public IEnumerable<Person> Actors { get; set; }
        [NeoRelationship(type: "REVIEWED", direction: RelationshipDirection.Incoming)]
        public IEnumerable<Person> Reviewers { get; set; }
    }
}