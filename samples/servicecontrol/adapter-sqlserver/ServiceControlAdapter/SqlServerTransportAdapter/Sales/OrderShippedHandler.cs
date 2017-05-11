using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

class OrderShippedHandler : IHandleMessages<OrderShipped>
{
    static ILog log = LogManager.GetLogger<OrderShippedHandler>();
    Database db;

    public OrderShippedHandler(Database db)
    {
        this.db = db;
    }

    public Task Handle(OrderShipped message, IMessageHandlerContext context)
    {
        log.Info($"Completing order {message.OrderId} worth {message.Value}");
        return db.Store();
    }
}