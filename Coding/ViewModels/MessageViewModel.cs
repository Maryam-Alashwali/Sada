using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Tailor
{
    public class MessageListViewModel
    {
        public List<ConversationViewModel> Conversations { get; set; } = new();
    }

    public class ConversationViewModel
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public DateTime? LastMessageDate { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ConversationDetailViewModel
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public List<MessageItemViewModel> Messages { get; set; } = new();
        public List<ClientOrderViewModel> Orders { get; set; } = new();
    }

    public class MessageItemViewModel
    {
        public int MessageId { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public DateTime? SentDate { get; set; }
        public string SenderType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsFromTailor { get; set; }
    }

    public class ClientOrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public decimal? TotalPrice { get; set; }
        public List<string> Services { get; set; } = new();
    }

    public class SendMessageViewModel
    {
        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        [Display(Name = "Message")]
        public string MessageText { get; set; } = string.Empty;

        public int ClientId { get; set; }
    }
}
