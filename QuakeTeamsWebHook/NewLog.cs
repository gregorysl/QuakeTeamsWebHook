using NLog;
using System.Text.RegularExpressions;

namespace QuakeTeamsWebHook
{
    public class NewLog
    {
        private readonly Regex _actionsRegex;
        private readonly Regex _scorecardRegex;
        private readonly Regex _mapNameRegex;
        private readonly Regex _userInfoChangedRegex;
        private readonly Regex _clientConnectRegex;

        public Game Game { get; }

        public NewLog()
        {

            _actionsRegex = new Regex($"({Consts.ShutdownGame}|{Consts.Score}|{Consts.Exit}|{Consts.InitGame}|{Consts.ClientConnect}|{Consts.ClientUserinfoChanged})");
            _scorecardRegex = new Regex(@"score: (\d+)  ping: \d+  client: \d+ (.+)");
            _mapNameRegex = new Regex(@"mapname\\(.+?)\\");
            _userInfoChangedRegex = new Regex(@"(\d+) n\\(.+?)\\t\\0\\");
            _clientConnectRegex = new Regex($@"{Consts.ClientConnect} (\d+)");
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
                        var s = int.Parse(GetMatchValue(matches, 1));
                        var card = new Scorecard(s, GetMatchValue(matches, 2));
                        Game.Scorecard.Add(card);
                    }
                    logger.Info("parsedScores");
                    break;
                case Consts.InitGame:
                    var mapMatch = _mapNameRegex.Match(line);
                    Game.MapName = GetMatchValue(mapMatch, 1);
                    break;
                case Consts.ClientConnect:
                    var connectedMatch = _clientConnectRegex.Match(line);
                    var connectedId = GetMatchValue(connectedMatch, 1);
                    Game.AddPlayer(connectedId);
                    break;
                case Consts.ClientUserinfoChanged:
                    var playerMatch = _userInfoChangedRegex.Match(line);
                    var id = GetMatchValue(playerMatch, 1);
                    var name = GetMatchValue(playerMatch, 2);
                    Game.EditPlayer(id, name);
                    break;
                default:
                    break;
            }
            if (Game.Players.Count > 0 && Game.Scorecard.Count == Game.Players.Count)
            {
                Game.Finished = true;
            }
        }
        private string GetMatchValue(Match match, int i)
        {
            return match.Groups[i].Value;
        }
    }
}
