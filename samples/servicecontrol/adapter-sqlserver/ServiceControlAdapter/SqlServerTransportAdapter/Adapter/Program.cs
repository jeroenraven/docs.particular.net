using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Routing;
using NServiceBus.Transport.SQLServer;
using ServiceControl.TransportAdapter;

namespace Adapter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Samples.ServiceControl.SqlServerTransportAdapter.Adapter";
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            var adapterConfig = new TransportAdapterConfig<SqlServerTransport, SqlServerTransport>("ServiceControl.SqlServer.Adapter");

            adapterConfig.CustomizeEndpointTransport(transport =>
            {
                transport.EnableLegacyMultiInstanceMode(Connections.GetConnection);
                //HACK: SQLServer expects this
                transport.GetSettings().Set<EndpointInstances>(new EndpointInstances());
            });

            adapterConfig.CustomizeServiceControlTransport(transport =>
            {
                transport.ConnectionString(
                    @"Data Source=.\SQLEXPRESS;Initial Catalog=ServiceControl;Integrated Security=True;Max Pool Size=100;Min Pool Size=10");
                //HACK: SQLServer expects this
                transport.GetSettings().Set<EndpointInstances>(new EndpointInstances());
            });
            
            adapterConfig.ServiceControlSideControlQueue = "Particular.ServiceControl.SQL";

            var adapter = TransportAdapter.Create(adapterConfig);

            await adapter.Start();

            Console.WriteLine("Press <enter> to shutdown adapter.");
            Console.ReadLine();

            await adapter.Stop();
        }
    }
}
