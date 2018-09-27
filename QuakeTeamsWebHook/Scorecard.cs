namespace QuakeTeamsWebHook
{
    public class Scorecard
    {
        public int Score { get; set; }
        public string Name { get; protected set; }

        public Scorecard(int score, string name)
        {
            Name = name;
            Score = score;
        }

        public override string ToString()
        {
            return $"{Score} {Name}";
        }
    }
}
