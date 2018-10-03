using System.Text.RegularExpressions;

namespace QuakeTeamsWebHook
{
    public class NewLog
    {
        private readonly Regex _actionsRegex;
        private readonly Regex _scorecardRegex;
        private readonly Regex _mapNameRegex;

        public Game Game { get; }

        public NewLog()
        {
            _actionsRegex = new Regex($"({Consts.ShutdownGame}|{Consts.Score}|{Consts.Exit}|{Consts.InitGame})");
            _scorecardRegex = new Regex(@"score: (\d+)  ping: \d+  client: \d+ (.+)");
            _mapNameRegex = new Regex(@"mapname\\(.+?)\\");
            Game = new Game();
        }

        public void ProcessTheRows(string line)
        {
            var action = _actionsRegex.Match(line);

            switch (action.Value)
            {
                case Consts.ShutdownGame:
                    Game.Finished = true;
                    break;
                case Consts.Exit:
                    const string findText = " Exit: ";
                    Game.EndReason = line.Substring(line.IndexOf(findText) + findText.Length);
                    break;
                case Consts.Score:
                    var matches = _scorecardRegex.Match(line);
                    if (matches.Groups.Count == 3)
                    {
                        var s = int.Parse(matches.Groups[1].Value);
                        var card = new Scorecard(s, matches.Groups[2].Value);
                        Game.Scorecard.Add(card);
                    }
                    break;
                case Consts.InitGame:
                    var nameMatch = _mapNameRegex.Match(line);
                    Game.MapName = nameMatch.Groups[1].Value;
                    break;
                default:
                    break;
            }
        }
    }
}
