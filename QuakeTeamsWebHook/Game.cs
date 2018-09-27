using System.Collections.Generic;
using QuakeTeamsWebHook;

namespace QuakeTeamsWebHook
{
    public class Game
    {
        public bool Finished;
        public string EndReason = "";
        public string MapName = "";
        public List<Scorecard> Scorecard = new List<Scorecard>();

        public Game()
        {
        }

        public void Reset()
        {
            Finished = false;
            Scorecard = new List<Scorecard>();
            EndReason = "";
            MapName = "";
        }

        public string ScorecardJson()
        {
            var s = "";
            foreach (var score in Scorecard)
            {
                s += $"{{name: \"{score.Score}\",value: \"{score.Name}\" }},";
            }

            return s.TrimEnd(',');
        }
    }
}