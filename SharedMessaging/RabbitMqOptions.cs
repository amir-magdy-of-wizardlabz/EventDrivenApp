using System.ComponentModel.DataAnnotations;

namespace SharedMessaging
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMQ";

        [Required]
        public string HostName { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
