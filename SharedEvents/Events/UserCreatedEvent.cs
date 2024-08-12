namespace SharedEvents.Events
{
    public class UserCreatedEvent : EventBase
    {
        private const string _version = "1.0";
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public override bool IsValidVersion()
        {
            return Version == _version;
        }
    }
}