using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

class OrderAcceptedHandler : IHandleMessages<OrderAccepted>
{
    static ILog log = LogManager.GetLogger<OrderAcceptedHandler>();
    Database db;

    public OrderAcceptedHandler(Database db)
    {
        this.db = db;
    }

    public async Task Handle(OrderAccepted message, IMessageHandlerContext context)
    {
        log.Info($"Shipping order {message.OrderId} for {message.Value}");
        await db.Store();
        await context.Publish(new OrderShipped
        {
            OrderId = message.OrderId,
            Value = message.Value
        });
    }
}