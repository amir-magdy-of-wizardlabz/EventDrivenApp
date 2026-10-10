namespace SharedEvents
{
    // Exchange names and routing keys are part of the contract between services,
    // so they live next to the events instead of in each service's configuration.
    public static class Exchanges
    {
        public const string Users = "user-events";
        public const string Orders = "order-events";
    }

    public static class RoutingKeys
    {
        public const string UserCreated = "user.created";
        public const string UserUpdated = "user.updated";
        public const string UserDeleted = "user.deleted";
        public const string OrderCreated = "order.created";
    }
}
