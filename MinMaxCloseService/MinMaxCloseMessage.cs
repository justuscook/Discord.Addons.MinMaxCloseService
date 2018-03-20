using Discord.Commands;

namespace Discord.Addons.MinMaxClose
{
    public class MinMaxCloseMessage
    {
		public IUserMessage Message { get; set; }
		public string ShortMessage { get; set; }
		public ulong UserID { get; set; }
		public bool CanOthersClose { get; set; }
		public bool Maximized { get; set; }

		public MinMaxCloseMessage(IUserMessage message, SocketCommandContext context, bool canOthersClose = false, bool maximized = true)
		{
			Message = message;
			ShortMessage = $"**{context.Message.Content}** called by {context.User.Mention} use reactions to delete or expand this message.";
			UserID = context.User.Id;
			CanOthersClose = canOthersClose;
			Maximized = maximized;
		}
	}	
}
