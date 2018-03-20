using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Discord.Addons.MinMaxClose;
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Sample_Bot
{
	class Program
	{
		static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		private DiscordSocketClient client;
		private CommandService commands;
		private IServiceProvider services;

		public async Task MainAsync()
		{
			var jsonStr = File.ReadAllText("token.json");
			var jobj = JObject.Parse(jsonStr);
			var token = jobj.SelectToken("token").ToString();

			client = new DiscordSocketClient();

			client.Log += log =>
			{
				Console.WriteLine(log.ToString());
				return Task.CompletedTask;
			};

			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			services = new ServiceCollection()
				.AddSingleton(client)
				.AddSingleton<MinMaxCloseService>()
				.BuildServiceProvider();

			commands = new CommandService();
			await commands.AddModulesAsync(Assembly.GetEntryAssembly());

			client.MessageReceived += HandleCommandAsync;

			await Task.Delay(-1);
		}

		public async Task HandleCommandAsync(SocketMessage m)
		{
			if (!(m is SocketUserMessage msg)) return;
			if (msg.Author.IsBot) return;

			int argPos = 0;
			if (!(msg.HasStringPrefix("mmc.", ref argPos))) return;

			var context = new SocketCommandContext(client, msg);
			await commands.ExecuteAsync(context, argPos, services);
		}
	}
}