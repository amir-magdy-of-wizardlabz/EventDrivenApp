namespace NotificationService.Core.Interfaces
{
    public interface INotificationService
    {
        void Notify(string toAddress, string subject, string body);
    }
}
