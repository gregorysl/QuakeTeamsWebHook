﻿using System.Collections.Generic;
using System.Linq;
using QuakeTeamsWebHook;

namespace QuakeTeamsWebHook
{
    public class Game
    {
        public bool Finished= false;
        public string EndReason = "";
        public string MapName = "";
        public List<Scorecard> Scorecard = new List<Scorecard>();
        public List<Player> Players = new List<Player>();
        public List<Kill> Kills = new List<Kill>();

        public Game()
        {
        }

        public void Reset()
        {
            Finished = false;
            Scorecard = new List<Scorecard>();
            Players = new List<Player>();
            Kills = new List<Kill>();
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
        public void AddKill(int killer, int killed, int means)
        {
            Kills.Add(new Kill { Killer = killer, Killed = killed, Means = means });
        }
    }
}