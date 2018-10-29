using NLog;
using System.Text.RegularExpressions;

namespace QuakeTeamsWebHook
{
    public class NewLog
    {
        public readonly Regex _actionsRegex;
        private readonly Regex _scorecardRegex;
        private readonly Regex _mapNameRegex;
        private readonly Regex _userInfoChangedRegex;
        private readonly Regex _clientConnectRegex;
        private readonly Regex _killRegex;

        public int killCount { get; set; }
        public Game Game { get; }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public NewLog()
        {
            _actionsRegex = new Regex($"({Consts.ShutdownGame}|{Consts.Score}|{Consts.Exit}|{Consts.InitGame}|{Consts.ClientConnect}|{Consts.ClientUserinfoChanged}|{Consts.Kill})", RegexOptions.Compiled);
            _scorecardRegex = new Regex(@"score: (\d+)  ping: \d+  client: \d+ (.+)", RegexOptions.Compiled);
            _mapNameRegex = new Regex(@"mapname\\(.+?)\\", RegexOptions.Compiled);
            _userInfoChangedRegex = new Regex(@"(\d+) n\\(.+?)\\t\\0\\", RegexOptions.Compiled);
            _clientConnectRegex = new Regex($@"{Consts.ClientConnect} (\d+)", RegexOptions.Compiled);
            _killRegex = new Regex($@".+{Consts.Kill} (\d+) (\d+) (\d+):.+(MOD.+)", RegexOptions.Compiled);
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
                    logger.Info(line);
                    Exit(line);
                    break;
                case Consts.Score:
                    logger.Info(line);
                    Score(line);
                    break;
                case Consts.InitGame:
                    logger.Info(line);
                    Init(line);
                    break;
                case Consts.ClientConnect:
                    logger.Info(line);
                    Connect(line);
                    break;
                case Consts.ClientUserinfoChanged:
                    logger.Info(line);
                    UserInfoChanged(line);
                    break;
                case Consts.Kill:
                    logger.Info(line);
                    Kill(line);
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
        private void UserInfoChanged(string line)
        {
            var playerMatch = _userInfoChangedRegex.Match(line);
            var id = GetMatchValue(playerMatch, 1);
            var name = GetMatchValue(playerMatch, 2);
            Game.EditPlayer(id, name);

        }
        private void Connect(string line)
        {
            var connectedMatch = _clientConnectRegex.Match(line);
            var connectedId = GetMatchValue(connectedMatch, 1);
            Game.AddPlayer(connectedId);
        }
        private void Init(string line)
        {
            var mapMatch = _mapNameRegex.Match(line);
            Game.MapName = GetMatchValue(mapMatch, 1);
        }
        private void Score(string line)
        {
            var matches = _scorecardRegex.Match(line);
            if (matches.Groups.Count == 3)
            {
                var s = int.Parse(GetMatchValue(matches, 1));
                var card = new Scorecard(s, GetMatchValue(matches, 2));
                Game.Scorecard.Add(card);
            }
        }
        private void Exit(string line)
        {
            const string findText = " Exit: ";
            Game.EndReason = line.Substring(line.IndexOf(findText) + findText.Length);
        }
        private void Kill(string line)
        {
            var killMatch = _killRegex.Match(line);
            var killer = int.Parse(GetMatchValue(killMatch, 1));
            var killed = int.Parse(GetMatchValue(killMatch, 2));
            var means = int.Parse(GetMatchValue(killMatch, 3));
            Game.AddKill(killer, killed, means);
        }
    }
}
