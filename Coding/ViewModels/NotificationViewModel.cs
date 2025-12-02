namespace Sada.ViewModels.Tailor
{
    public class NotificationListViewModel
    {
        public List<NotificationItemViewModel> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 20;
    }

    public class NotificationItemViewModel
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
    }
}
