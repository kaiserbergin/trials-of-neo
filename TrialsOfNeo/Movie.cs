namespace TrialsOfNeo
{
    [Node("Movie")]
    public class Movie
    {
        [NeoProperty("tagline")]
        public string Description { get; set; }
        
        [NeoProperty("title")]
        public string Title { get; set; }

        [NeoProperty("released")]
        public int Released { get; set; }
    }
}