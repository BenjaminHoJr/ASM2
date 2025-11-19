namespace WebApplication1.Models
{
    public class EmailSetting
    {
        public int EmailSettingId { get; set; }
        public string SmtpServer { get; set; } = default!;
        public int SmtpPort { get; set; }
        public string SenderName { get; set; } = default!;
        public string SenderEmail { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}