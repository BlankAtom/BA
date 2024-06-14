using MySql.Data.MySqlClient;
using System.Data;

namespace BlankAtom.Database.Pipeline;

public class MySqlDatasourceReader : IDatasourceReader
{
    private string connectionString;

    public MySqlDatasourceReader(string connectionString)
    {
        this.connectionString = connectionString;
    }

    List<IDDLDetail> details;
    /// <inheritdoc/>
    public List<IDDLDetail> ReadToDDL()
    {
        details = new List<IDDLDetail>();

        using (var connection = new MySqlConnection(this.connectionString))
        {
            connection.Open();

            List<string> list = new List<string>();
            var command = new MySqlCommand("SHOW TABLES", connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var tableName = reader.GetString(0);
                    list.Add(tableName);
                }
            }

            foreach (var tableName in list)
            {
                details.Add(new MySqlDDLDetail(tableName, GetTableSchema(connection, tableName)));
            }
        }

        return details;
    }

    private DataTable GetTableSchema(MySqlConnection connection, string tableName)
    {
        using (var command = new MySqlCommand($"SELECT * FROM {tableName} LIMIT 0", connection))
        {
            using (var reader = command.ExecuteReader())
            {
                return reader.GetSchemaTable()!;
            }
        }
    }

    public List<DataTable> GetTableData()
    {
        var connection = new MySqlConnection(this.connectionString);

        if (connection.State != ConnectionState.Open)
            connection.Open();

        foreach (IDDLDetail detail in details)
        {
            using (var command = new MySqlCommand($"SELECT * FROM {detail.TableName}", connection))
            {
                new MySqlDataAdapter(command)
                    .Fill(detail.Schema);
            }
        }

        return new List<DataTable>();
    }

    private DataTable GetTableData(MySqlConnection connection, string tableName)
    {
        var data = new DataTable();

        using (var command = new MySqlCommand($"SELECT * FROM {tableName}", connection))
        {
            using (var reader = command.ExecuteReader())
            {
                data.Load(reader);
            }
        }

        return data;
    }
}
