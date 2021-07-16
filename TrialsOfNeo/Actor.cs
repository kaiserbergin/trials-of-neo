namespace TrialsOfNeo
{
    [Node("Person")]
    public class Actor 
    {
        [NeoProperty("name")]
        public string Name { get; set; }
        [NeoProperty("born")]
        public long Born { get; set; }
    }
}