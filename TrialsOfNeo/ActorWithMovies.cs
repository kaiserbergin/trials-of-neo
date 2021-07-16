using System.Collections.Generic;

namespace TrialsOfNeo
{
    public class ActorWithMovies : Actor
    {
        [NeoRelationship(type: "ACTED_IN", direction: RelationshipDirection.Outgoing)]
        public IEnumerable<Movie> Movie { get; set; }
    }
}