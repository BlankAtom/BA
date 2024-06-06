using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;

namespace BA.Backend2;

public class ConnectionHelpers
{
    static ConnectionHelpers()
    {
        DbProviderFactories.RegisterFactory("MSSQL", SqlClientFactory.Instance);
    }
    public static DbConnection? GetConnection(DatasourceType type = DatasourceType.MSSQL)
    {
        if (DbProviderFactories.TryGetFactory("MSSQL", out var factory))
        {
            return factory.CreateConnection();
        }

        return null;
    }
    public static dynamic[] GetTableFields(ConnectionConfiguration configuration, string tableNameFilter = "")
    {
        string cs = GetConnectionString(configuration);

        using var connection = new SqlConnection(cs);
        using var command = connection.CreateCommand();

        connection.Open();
        command.CommandText =
            $"""
            select name
            from sys.objects
            where type = 'U' and name like '%{tableNameFilter}%'
            """;

        IAsyncResult asyncResult = command.BeginExecuteReader();

        var list = new List<object>();
        using var reader = command.EndExecuteReader(asyncResult);
        while (reader.Read())
        {
            string tableName = reader.GetString(0);
            // string fieldType = reader.GetString(1);

            list.Add(new { tableName });
        }

        reader.Close();
        connection.Close();

        return list.ToArray();
    }

    private static string GetConnectionString(ConnectionConfiguration info)
    {
        var builder = new SqlConnectionStringBuilder();
        builder.DataSource = string.IsNullOrEmpty(info.port) ? info.ip : info.ip + "," + info.port;
        builder.InitialCatalog = info.catalog;
        builder.UserID = info.user;
        builder.Password = info.word;
        builder.PersistSecurityInfo = true;
        builder.TrustServerCertificate = true;
        return builder.ToString();
    }
}