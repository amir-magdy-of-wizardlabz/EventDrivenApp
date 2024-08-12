namespace SharedEvents.Events
{
    public class UserUpdatedEvent : EventBase
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }

        public override bool IsValidVersion()
        {
            return Version == "1.0";
        }
    }
}