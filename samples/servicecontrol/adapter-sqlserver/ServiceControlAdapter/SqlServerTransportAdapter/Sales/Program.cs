﻿using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Transport.SQLServer;

class Program
{
    static void Main()
    {
        AsyncMain().GetAwaiter().GetResult();
    }

    static async Task AsyncMain()
    {
        Console.Title = "Samples.ServiceControl.SqlServerTransportAdapter.Sales";
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
        var random = new Random();
        var endpointConfiguration = new EndpointConfiguration(
            "Samples.ServiceControl.SqlServerTransportAdapter.Sales");

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.Routing().RegisterPublisher(typeof(OrderShipped),
            "Samples.ServiceControl.SqlServerTransportAdapter.Shipping");
        transport.EnableLegacyMultiInstanceMode(Connections.GetConnection);

        endpointConfiguration.UsePersistence<InMemoryPersistence>();

        var database = new Database();
        endpointConfiguration.RegisterComponents(c =>
        {
            c.ConfigureComponent(() => database, DependencyLifecycle.SingleInstance);
        });

        endpointConfiguration.Recoverability()
            .Immediate(immediate => immediate.NumberOfRetries(0))
            .Delayed(delayed => delayed.NumberOfRetries(0))
            .DisableLegacyRetriesSatellite();

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");
        endpointConfiguration.EnableInstallers();

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        Console.WriteLine("Press enter to exit");
        Console.WriteLine("Press 'o' to generate order");
        Console.WriteLine("Press 'f' to toggle simulating of database failure");
        while (true)
        {
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }
            var lowerInvariant = char.ToLowerInvariant(key.KeyChar);
            if (lowerInvariant == 'o')
            {
                var orderId = new string(Enumerable.Range(0, 4).Select(x => letters[random.Next(letters.Length)]).ToArray());
                var orderSubmitted = new OrderAccepted
                {
                    OrderId = orderId,
                    Value = random.Next(100)
                };
                await endpointInstance.Publish(orderSubmitted)
                    .ConfigureAwait(false);
            }
            if (lowerInvariant == 'f')
            {
                database.IsFailing = !database.IsFailing;
                Console.WriteLine("Database failure simulation is now turned " + (database.IsFailing ? "on" : "off"));
            }
        }
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}