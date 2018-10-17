using System.Collections.Generic;
using System.Linq;
using QuakeTeamsWebHook;

namespace QuakeTeamsWebHook
{
    public class Game
    {
        public bool Finished;
        public string EndReason = "";
        public string MapName = "";
        public List<Scorecard> Scorecard = new List<Scorecard>();
        public List<Player> Players = new List<Player>();

        public Game()
        {
        }

        public void Reset()
        {
            Finished = false;
            Scorecard = new List<Scorecard>();
            Players = new List<Player>();
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
        public void AddPlayer(string id, string name = "")
        {
            var player = new Player(id, name);
            Players.Add(player);
        }
        public void EditPlayer(string id, string name = "")
        {
            var player = Players.FirstOrDefault(x => x.Id == id);
            if (player == null) { AddPlayer(id, name); }
            else { player.Name = name; }
        }
    }
}