using System.Reflection.Emit;
using Neo4j.Driver;

namespace TrialsOfNeo
{
    [Node(label: "Person")]
    public class Actor 
    {
        [NeoProperty("name")]
        public string Name { get; set; }
        [NeoProperty("born")]
        public long Born { get; set; }
    }
}