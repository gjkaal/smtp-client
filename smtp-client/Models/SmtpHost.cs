
namespace Nice2Experience.Smtp.Models
{
    internal class SmtpHost
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
    }

    internal class SmtpSettings
    {
        public string Sender { get; set; }
        public string FriendlyName { get; set; }

        public bool SendActive { get; set; }
        public SmtpHost Host { get; set; }

        public bool DebugActive { get; set; }
        public string DebugTag { get; set; }
        public SmtpHost DebugHost { get; set; }

        public int MaxMessageSizeInMb { get; set; }
    }
}
