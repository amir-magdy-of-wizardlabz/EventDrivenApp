
namespace SharedEvents.Events
{
    public abstract class EventBase
    {
        public required string Version { get;  set; }      
        public abstract Boolean IsValidVersion ();
    }
}