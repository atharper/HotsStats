using Heroes.ReplayParser;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace StatsFetcher
{
  public class ProfileFetcher
	{
		private readonly Game _game;
		private readonly HttpClient _web;

		public ProfileFetcher(Game game)
		{
			_game = game;
			_web = new HttpClient();
		}

		public async Task FetchBasicProfiles()
		{
			var tasks = new List<Task>();

			// start all requests in parallel
			foreach (var p in _game.Players) {
				tasks.Add(FetchBasicProfile(p));
			}

			foreach (var task in tasks) {
				await task;
			}
		}

		public async Task FetchFullProfiles()
		{
			var tasks = new List<Task>();

			// start all requests in parallel
			foreach (var p in _game.Players) {
				tasks.Add(FetchFullProfile(p));
			}

			foreach (var task in tasks) {
				await task;
			}
		}

		private async Task FetchBasicProfile(PlayerProfile p)
		{
			var url = $"https://www.hotslogs.com/API/Players/{(int)_game.Region}/{p.BattleTag.Replace('#', '_')}";
			var str = await _web.GetStringAsync(url);
			if (string.IsNullOrWhiteSpace(str) || str == "null")
				return;
			try {
				dynamic json = JObject.Parse(str);
				p.HotslogsId = json.PlayerID;
				foreach (var r in json.LeaderboardRankings) {
					var mode = (GameMode)Enum.Parse(typeof(GameMode), (string)r.GameMode);
					p.Ranks[mode] = new PlayerProfile.MmrValue(mode, (int)r.CurrentMMR, (PlayerProfile.League?)(int?)r.LeagueID, (int?)r.LeagueRank);
				}
			}
			catch (Exception e) { /* some dirty exception swallow */ }
		}

		private async Task FetchFullProfile(PlayerProfile p)
		{
			if (p.HotslogsId == null)
				return;
			var url = $"http://www.hotslogs.com/Player/Profile?PlayerID={p.HotslogsId}";
			var str = await _web.GetStringAsync(url);
			try {
				var doc = new HtmlDocument();
				doc.LoadHtml(str);
				p.HotsLogsProfile = doc;
			}
			catch (Exception e) { /* some dirty exception swallow */ }
		}
	}
}
