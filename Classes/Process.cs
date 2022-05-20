using TL;
using WTelegram;
namespace CliBotWinForm.Classes
{
    public class Process
    {
        public List<ChatModel> chats;
        public Client Client { private get; set; }
        public Process(Client client) => Client = client;


        public async Task<List<ChatModel>> GetAllChatsAsync()
        {
            chats = new List<ChatModel>();
            var channelAndGroupChats = await Client.Messages_GetAllChats();
            var privateChats = await Client.Messages_GetAllDialogs();

            foreach (var chat in channelAndGroupChats.chats)
            {
                if (chat.Value as Channel != null)
                {
                    chats.Add(new ChatModel
                    {
                        Id = chat.Value.ID,
                        Title = chat.Value.Title,
                        TypeChat = (chat.Value as Channel).IsChannel ? TypeChat.Channel : TypeChat.Group
                    });
                }
            }
            foreach (var chat in privateChats.users)
            {
                if (chat.Value as User != null)
                {
                    chats.Add(new ChatModel
                    {
                        Id = chat.Value.ID,
                        Title = $"{chat.Value.first_name} {chat.Value.last_name}",
                        TypeChat = TypeChat.Private
                    });
                }
            }
            return chats;
        }

        public async Task SendMessageAsync(string? filePath, string caption, List<long> users)
        {
            foreach (var user in users)
            {
                var aceesHash = Client.GetAccessHashFor<Channel>(user);

                if (filePath != null)
                {
                    var file = await Client.UploadFileAsync(filePath);
                    await Client.SendMediaAsync(new InputPeerUser(user, long.Parse(Properties.Settings.Default.ApiId)), caption, file);
                }
                else
                    await Client.SendMessageAsync(new InputUser(user, aceesHash), caption);
            }
        }
    };

    public class ChatModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public TypeChat TypeChat { get; set; }
    }

    public enum TypeChat
    {
        Channel,
        Private,
        Group
    }
}
