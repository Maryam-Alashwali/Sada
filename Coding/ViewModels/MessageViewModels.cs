using System.ComponentModel.DataAnnotations;

namespace Sada.ViewModels.Client
{
    public class MessageListViewModel
    {
        public List<ConversationViewModel> Conversations { get; set; } = new();
    }

    public class ConversationViewModel
    {
        public TailorInfoViewModel Tailor { get; set; } = new();
        public DateTime? LastMessageDate { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ConversationDetailViewModel
    {
        public TailorInfoViewModel Tailor { get; set; } = new();
        public List<MessageViewModel> Messages { get; set; } = new();
        public List<OrderSummaryViewModel> Orders { get; set; } = new();
    }

    public class MessageViewModel
    {
        public int MessageId { get; set; }
        public string? MessageText { get; set; }
        public DateTime? SentDate { get; set; }
        public long? SenderId { get; set; }
        public string? SenderType { get; set; }
        public long? ReceiverId { get; set; }
        public string? ReceiverType { get; set; }
        public bool IsRead { get; set; }
        public bool IsSentByCurrentUser { get; set; }
        public string SenderName { get; set; } = string.Empty;
    }

    public class OrderSummaryViewModel
    {
        public int OrderId { get; set; }
        public string? OrderStatus { get; set; }
        public DateTime? DateCreated { get; set; }
        public decimal? TotalPrice { get; set; }
    }

    public class SendMessageViewModel
    {
        public int TailorId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        [Display(Name = "Message")]
        public string MessageText { get; set; } = string.Empty;
    }

    public class SendInitialMessageViewModel
    {
        [Required(ErrorMessage = "Tailor is required")]
        public int TailorId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;
    }
}
