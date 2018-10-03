namespace QuakeTeamsWebHook
{
    public class Scorecard
    {
        public int Score { get; set; }
        public string Name { get; protected set; }

        public Scorecard(int score, string name)
        {
            var asaa = new[] { "^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9", "^0" };
            foreach(var i in asaa)
            {
                name = name.Replace(i, "");
            }
            Name = name;
            Score = score;
        }

        public override string ToString()
        {
            return $"{Score} {Name}";
        }
    }
}
