namespace NotificationService.Core.Interfaces
{
    public interface INotificationService
    {
        Task NotifyAsync(string toAddress, string subject, string body, CancellationToken cancellationToken);
    }
}
