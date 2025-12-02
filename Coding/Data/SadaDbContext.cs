using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sada.Models;

namespace Sada.Data;

public partial class SadaDbContext : DbContext
{
    public SadaDbContext()
    {
    }

    public SadaDbContext(DbContextOptions<SadaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Advertisement> Advertisements { get; set; }

    public virtual DbSet<Availability> Availabilities { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Measurement> Measurements { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderService> OrderServices { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Tailor> Tailors { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-15N93HR\\SQLEXPRESS;Database=sada;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admin__43AA4141795D12F3");

            entity.ToTable("Admin");

            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.AdminName)
                .HasMaxLength(100)
                .HasColumnName("admin_name");
            entity.Property(e => e.AdminRole)
                .HasMaxLength(50)
                .HasColumnName("admin_role");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Admins)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Admin__user_id__72C60C4A");
        });

        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasKey(e => e.AdsId).HasName("PK__Advertis__DF721008EA94D5AC");

            entity.Property(e => e.AdsId).HasColumnName("ads_id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.Content)
                .HasMaxLength(255)
                .HasColumnName("content");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .HasColumnName("image");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.TailorId).HasColumnName("tailor_id");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");

            entity.HasOne(d => d.Admin).WithMany(p => p.Advertisements)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Advertise__admin__73BA3083");

            entity.HasOne(d => d.Tailor).WithMany(p => p.Advertisements)
                .HasForeignKey(d => d.TailorId)
                .HasConstraintName("FK__Advertise__tailo__74AE54BC");
        });

        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasKey(e => e.AvailabilityId).HasName("PK__Availabi__86E3A801923263F7");

            entity.ToTable("Availability");

            entity.Property(e => e.AvailabilityId).HasColumnName("availability_id");
            entity.Property(e => e.DayOfWeek)
                .HasMaxLength(20)
                .HasColumnName("day_of_week");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true)
                .HasColumnName("is_available");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.TailorId).HasColumnName("tailor_id");

            entity.HasOne(d => d.Tailor).WithMany(p => p.Availabilities)
                .HasForeignKey(d => d.TailorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Availabil__tailo__75A278F5");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__D54EE9B4B82D0830");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.Category1)
                .HasMaxLength(100)
                .HasColumnName("category");

            entity.HasOne(d => d.Admin).WithMany(p => p.Categories)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Category__admin___76969D2E");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PK__Client__BF21A424B676DA2E");

            entity.ToTable("Client");

            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ClientAddress)
                .HasMaxLength(255)
                .HasColumnName("client_address");
            entity.Property(e => e.ClientFirstName)
                .HasMaxLength(100)
                .HasColumnName("client_first_name");
            entity.Property(e => e.ClientLastName)
                .HasMaxLength(100)
                .HasColumnName("client_last_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Clients)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Client__user_id__778AC167");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoice__F58DFD49427ECB53");

            entity.ToTable("Invoice");

            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.InvoiceTotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("invoice_total_amount");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasColumnName("payment_status");

            entity.HasOne(d => d.Client).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoice__client___787EE5A0");

            entity.HasOne(d => d.Order).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoice__order_i__797309D9");

            entity.HasOne(d => d.Payment).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK__Invoice__payment__7A672E12");
        });

        modelBuilder.Entity<Measurement>(entity =>
        {
            entity.HasKey(e => e.MeasurementId).HasName("PK__Measurem__E3D1E1C1C813C894");

            entity.ToTable("Measurement");

            entity.Property(e => e.MeasurementId).HasColumnName("measurement_id");
            entity.Property(e => e.Chest)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("chest");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.Hips)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("hips");
            entity.Property(e => e.Length)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("length");
            entity.Property(e => e.OtherNotes)
                .HasMaxLength(255)
                .HasColumnName("other_notes");
            entity.Property(e => e.SleeveLength)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("sleeve_length");
            entity.Property(e => e.Waist)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("waist");

            entity.HasOne(d => d.Client).WithMany(p => p.Measurements)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Measureme__clien__7B5B524B");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Message__0BBF6EE6349009D0");

            entity.ToTable("Message");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.MessageText)
                .HasMaxLength(255)
                .HasColumnName("message_text");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.ReceiverType)
                .HasMaxLength(50)
                .HasColumnName("receiver_type");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SenderType)
                .HasMaxLength(50)
                .HasColumnName("sender_type");
            entity.Property(e => e.SentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("sent_date");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842FAB0AB634");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .HasColumnName("message");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TailorId).HasColumnName("tailor_id");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");

            entity.HasOne(d => d.Admin).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK__Notificat__admin__7C4F7684");

            entity.HasOne(d => d.Client).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK__Notificat__clien__7D439ABD");

            entity.HasOne(d => d.Tailor).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.TailorId)
                .HasConstraintName("FK__Notificat__tailo__7E37BEF6");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__46596229BF93C5DA");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ClientNotes)
                .HasMaxLength(255)
                .HasColumnName("client_notes");
            entity.Property(e => e.ClientUploadedImage)
                .HasMaxLength(255)
                .HasColumnName("client_uploaded_image");
            entity.Property(e => e.CompletionDate)
                .HasColumnType("datetime")
                .HasColumnName("completion_date");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.OrderAddress)
                .HasMaxLength(255)
                .HasColumnName("order_address");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("order_status");
            entity.Property(e => e.PlatformCommission)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("platform_commission");
            entity.Property(e => e.ScheduledPick)
                .HasColumnType("datetime")
                .HasColumnName("scheduled_pick");
            entity.Property(e => e.ScheduledVisitDate)
                .HasColumnType("datetime")
                .HasColumnName("scheduled_visit_date");
            entity.Property(e => e.ServiceType)
                .HasMaxLength(50)
                .HasColumnName("service_type");
            entity.Property(e => e.TailorId).HasColumnName("tailor_id");
            entity.Property(e => e.TailorPayout)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("tailor_payout");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_price");

            entity.HasOne(d => d.Client).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__client_id__7F2BE32F");

            entity.HasOne(d => d.Tailor).WithMany(p => p.Orders)
                .HasForeignKey(d => d.TailorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__tailor_id__00200768");
        });

        modelBuilder.Entity<OrderService>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ServiceId }).HasName("PK__Order_Se__A5B9B9A38660957B");

            entity.ToTable("Order_Service");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .HasColumnName("image");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .HasColumnName("note");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderServices)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__Order_Ser__order__01142BA1");

            entity.HasOne(d => d.Service).WithMany(p => p.OrderServices)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__Order_Ser__servi__02084FDA");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__ED1FC9EA3512FD5B");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("payment_amount");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentQuantity).HasColumnName("payment_quantity");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasColumnName("payment_status");
            entity.Property(e => e.PaymentTime).HasColumnName("payment_time");
            entity.Property(e => e.PaymentTransactionId)
                .HasMaxLength(100)
                .HasColumnName("payment_transaction_id");

            entity.HasOne(d => d.Client).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__client___02FC7413");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__order_i__03F0984C");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Review__60883D9016CCD3DC");

            entity.ToTable("Review");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewCreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("review_created_at");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");

            entity.HasOne(d => d.Admin).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__admin_id__04E4BC85");

            entity.HasOne(d => d.Client).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__client_i__05D8E0BE");

            entity.HasOne(d => d.Order).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__order_id__06CD04F7");

            entity.HasOne(d => d.Service).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__service___07C12930");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Service__3E0DB8AF81513E4B");

            entity.ToTable("Service");

            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.BasePrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("base_price");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.ServiceDescription)
                .HasMaxLength(255)
                .HasColumnName("service_description");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .HasColumnName("service_name");
            entity.Property(e => e.TailorId).HasColumnName("tailor_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Services)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Service__categor__08B54D69");

            entity.HasOne(d => d.Tailor).WithMany(p => p.Services)
                .HasForeignKey(d => d.TailorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Service__tailor___09A971A2");
        });

        modelBuilder.Entity<Tailor>(entity =>
        {
            entity.HasKey(e => e.TailorId).HasName("PK__Tailor__1CE3EC8546A03378");

            entity.ToTable("Tailor");

            entity.Property(e => e.TailorId).HasColumnName("tailor_id");
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false)
                .HasColumnName("is_approved");
            entity.Property(e => e.TailorAddress)
                .HasMaxLength(255)
                .HasColumnName("tailor_address");
            entity.Property(e => e.TailorFirstName)
                .HasMaxLength(100)
                .HasColumnName("tailor_first_name");
            entity.Property(e => e.TailorLastName)
                .HasMaxLength(100)
                .HasColumnName("tailor_last_name");
            entity.Property(e => e.TailorPhone)
                .HasMaxLength(20)
                .HasColumnName("tailor_phone");
            entity.Property(e => e.TailorProfilePicture)
                .HasMaxLength(255)
                .HasColumnName("tailor_profile_picture");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Tailors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Tailor__user_id__0A9D95DB");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370F639D0CD0");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__AB6E61646884440D").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
