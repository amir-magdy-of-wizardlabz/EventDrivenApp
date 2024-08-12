namespace SharedEvents.Events
{
    public class UserDeletedEvent : EventBase
    {
        public int Id { get; set; }

        public override bool IsValidVersion()
        {
            return Version == "1.0";
        }
    }
}