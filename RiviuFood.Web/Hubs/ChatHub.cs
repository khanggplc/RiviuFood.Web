using Microsoft.AspNetCore.SignalR;

namespace RiviuFood.Web.Hubs;

public class ChatHub : Hub
{
    // Hàm này để các máy khách (Client) gọi lên khi có bình luận mới
    public async Task SendComment(string user, string message, int postId)
    {
        
        await Clients.All.SendAsync("ReceiveComment", user, message, postId);
    }
    public async Task DeleteComment(int commentId)
    {
        await Clients.All.SendAsync("CommentDeleted", commentId);
    }

    public async Task EditComment(int commentId, string newContent)
    {
        await Clients.All.SendAsync("CommentEdited", commentId, newContent);
    }
}