using System;

namespace SharedEvents.Events
{
    public class OrderCreatedEvent : EventBase
    {
        private const string _version = "1.0";

        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }

        public override bool IsValidVersion()
        {
            return Version == _version;
        }
    }
}
