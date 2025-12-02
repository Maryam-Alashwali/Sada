using System;
using System.Collections.Generic;

namespace Sada.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public string? MessageText { get; set; }

    public DateTime? SentDate { get; set; }

    public long? SenderId { get; set; }

    public string? SenderType { get; set; }

    public long? ReceiverId { get; set; }

    public string? ReceiverType { get; set; }

    public bool IsRead { get; set; }
}
