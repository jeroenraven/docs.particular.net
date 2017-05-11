using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

public static class Connections
{
    static string ConnectionString(string catalog) => $@"Data Source=.\SQLEXPRESS;Initial Catalog={catalog};Integrated Security=True;Max Pool Size=100;Min Pool Size=10";

    static string Sales => ConnectionString("sales");
    static string Shipping => ConnectionString("shipping");
    static string Adapter => ConnectionString("adapter");

    public static async Task<SqlConnection> GetConnection(string destination)
    {
        var connectionString = GetConnectionString(destination);

        var connection = new SqlConnection(connectionString);

        await connection.OpenAsync()
            .ConfigureAwait(false);

        return connection;
    }

    static string GetConnectionString(string destination)
    {
        if (destination.StartsWith("Samples.ServiceControl.SqlServerTransportAdapter.Sales"))
        {
            return Sales;
        }
        if (destination.StartsWith("Samples.ServiceControl.SqlServerTransportAdapter.Shipping"))
        {
            return Shipping;
        }
        if (destination == "audit" || destination == "error" || destination == "Particular.ServiceControl" || destination == "poison")
        {
            return Adapter;
        }
        throw new Exception("Unknown destination: " + destination);
    }
}
