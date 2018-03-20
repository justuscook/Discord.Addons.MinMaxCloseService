using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Discord.Addons.MinMaxClose
{
	public class MinMaxCloseService
	{
		private DiscordSocketClient Client { get; }
		private static List<MinMaxCloseMessage> messages { get; set; }
		Emoji stop = new Emoji("\ud83c\uddfd");
		Emoji minimize = new Emoji("\u2B06");
		Emoji maximize = new Emoji("\u2195");
		Emoji info = new Emoji("\u2139");

		public MinMaxCloseService(DiscordSocketClient client)
		{
			Client = client;
			messages = new List<MinMaxCloseMessage>();
			client.ReactionAdded += MinMaxCloseReaction;
			client.MessageDeleted += MinMaxCloseRemoveMessage;
		}

		public async Task MinMaxCloseRemoveMessage(Cacheable<IMessage, ulong> cacheable, ISocketMessageChannel channel)
		{
			var message = await cacheable.GetOrDownloadAsync();
			if (messages.FirstOrDefault(x => x.Message == message).Message == message) messages.Remove(messages.FirstOrDefault(x => x.Message == message));
			else return;
		}

		public async Task MinMaxCloseReaction(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel messageChannel, SocketReaction reaction)
		{
			var message = await cacheable.GetOrDownloadAsync();
			if (reaction.UserId == Client.CurrentUser.Id) return;
			MinMaxCloseMessage minMaxCloseMessage = null;
			if (messages.Any(x => x.Message.Id == message.Id)) minMaxCloseMessage = messages.FirstOrDefault(x => x.Message.Id == message.Id);
			else return;

			switch (reaction.Emote.Name)
			{
				case "\ud83c\uddfd"://delete
					if (minMaxCloseMessage.UserID != reaction.UserId) return;
					await minMaxCloseMessage.Message.DeleteAsync();
					break;
				case "\u2B06"://minimize
					await message.RemoveReactionAsync(minimize, reaction.User.GetValueOrDefault());
					if (!minMaxCloseMessage.Maximized) return;
					minMaxCloseMessage.Maximized = false;
					if (message.Content != "") await message.ModifyAsync(x => x.Content = minMaxCloseMessage.ShortMessage);
					else
					{
						var embed = new EmbedBuilder();
						embed.AddField(DateTime.UtcNow.ToString() + " UTC.", minMaxCloseMessage.ShortMessage);
						await message.ModifyAsync(x => x.Embed = embed.Build());
					}

					break;
				case "\u2195"://maximize
					await message.RemoveReactionAsync(maximize, reaction.User.GetValueOrDefault());
					if (minMaxCloseMessage.Maximized) return;
					minMaxCloseMessage.Maximized = true;
					if (minMaxCloseMessage.Message.Content != "") await message.ModifyAsync(x => x.Content = minMaxCloseMessage.Message.Content);
					else
					{
						await message.ModifyAsync(x => x.Embed = minMaxCloseMessage.Message.Embeds.FirstOrDefault() as Embed);
						await message.ModifyAsync(x => x.Content = "");
					}
					break;
				case "\u2139"://info
					await message.RemoveReactionAsync(info, reaction.User.GetValueOrDefault());
					var msg = await message.Channel.SendMessageAsync("Use :regional_indicator_x: to delete the message, can only be done by the user that called the command, :arrow_up_down: to maximize the message,:arrow_up: to minimize the message.");
					_ = Task.Run(() => DelayDeleteAsync(message: msg as IMessage));
					break;
			}
		}

		public async Task SendMMCMessageAsync(SocketCommandContext context, IUserMessage message)
		{
			var msg = message;
			var minMaxCloseMessage = new MinMaxCloseMessage(msg, context);
			messages.Add(minMaxCloseMessage);
			await msg.AddReactionAsync(stop);
			await msg.AddReactionAsync(minimize);
			await msg.AddReactionAsync(maximize);
			await msg.AddReactionAsync(info);

			_ = Task.Run(() => DelayDeleteAsync(context, msg, 300));

		}


		public async Task DelayDeleteAsync(SocketCommandContext context = null, IMessage message = null, int? timeDelay = null)
		{
			if (context != null)
			{
				if (context.Channel is SocketDMChannel) return;
			}
			if (timeDelay == null) timeDelay = 15;
			await Task.Delay(timeDelay.Value * 1000);
			if (message.Channel.GetMessageAsync(message.Id) != null) await message.DeleteAsync();
		}
	}
}
