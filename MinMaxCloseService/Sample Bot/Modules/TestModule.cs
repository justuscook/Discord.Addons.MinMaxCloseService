using System;
using System.Threading.Tasks;
using Discord.Addons.MinMaxClose;
using Discord.Commands;
using Discord.WebSocket;
using Discord;

namespace Sample_Bot.Modules
{
    public class TestModule : ModuleBase<SocketCommandContext>
    {
		public MinMaxCloseService minMaxCloseService { get; set; }

		[Command("Test1")]
		public async Task Test1Async()
		{
			var msg = await ReplyAsync("This is a test.");
			await minMaxCloseService.SendMMCMessageAsync(Context, msg);
		}

		[Command("Test2")]
		public async Task Test2Async()
		{
			var embed = new EmbedBuilder();
			embed.AddField("Testing is fun", "Testing for embed messages.");
			embed.AddField("Testing is funner", "Testing for embed messages that have multiple fields.");
			embed.AddField("Testing is funnerer", "Testing for embed messages that have multiple fields, and are kinda long :D.");
			var msg = await ReplyAsync("", embed: embed.Build());
			await minMaxCloseService.SendMMCMessageAsync(Context, msg);
		}
	}
}
