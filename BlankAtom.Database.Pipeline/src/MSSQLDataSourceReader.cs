using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BlankAtom.Database.Pipeline;

public class MSSQLDataSourceReader : IDatasourceReader
{
    // private readonly SqlConnection connection;
    private readonly string connectionString;
    public MSSQLDataSourceReader(string connectionString)
    {
        // this.connection = new SqlConnection();
        this.connectionString = connectionString;
    }

    List<IDDLDetail> details;

    public List<IDDLDetail> ReadToDDL()
    {
        var connection = new SqlConnection(connectionString);

        connection.ConnectionString = connectionString;
        connection.Open();
        details = new List<IDDLDetail>();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.Tables";
        List<string> list = new List<string>();
        List<string> list2 = new List<string>();
        using( var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string table_name = reader.GetString(0);
                string table_type = reader.GetString(1);

                if(table_type == "BASE TABLE")
                    list.Add(table_name);
                else
                    list2.Add(table_name);
            }
        }

        foreach (var TableName in list)
        {
            DataTable tableSchema = GetTableSchema(connection, TableName);
            tableSchema.ExtendedProperties.Add("TableType", "BASE TABLE");
            details.Add(new SqlServerDDLDetail(TableName, tableSchema));
        }

        command.CommandText = $"SELECT VIEW_DEFINITION FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = @viewName";
        command.Parameters.Add("@viewName", SqlDbType.NVarChar);
        foreach (var TableName in list2)
        {
            DataTable tableSchema = GetTableSchema(connection, TableName);
            tableSchema.ExtendedProperties.Add("TableType", "VIEW");

            command.Parameters["@viewName"].Value = TableName;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    tableSchema.ExtendedProperties.Add("ViewDefinition", reader.GetString(0));
                }
            }

            details.Add(new SqlServerDDLDetail(TableName, tableSchema));
        }

        connection.Close();
        return details;
    }


    private DataTable GetTableSchema(SqlConnection connection, string tableName)
    {
        DataTable table = new DataTable();
        using (var command = new SqlCommand($"SELECT * FROM {tableName}", connection))
        {
            // using SqlDataAdapter adapter = new SqlDataAdapter(command);
            // adapter.Fill(table);
            // return table;
            using (var reader = command.ExecuteReader())
            {
                // reader.GetSchemaTable()
                return reader.GetSchemaTable()!;
            }
        }
    }


    /// <inheritdoc />
    public List<DataTable> GetTableData()
    {
        using var connection = new SqlConnection(connectionString);
        if(connection.State != ConnectionState.Open)
            connection.Open();

        List<DataTable> data = new List<DataTable>();
        foreach (IDDLDetail detail in this.details)
        {
            using (var command = new SqlCommand($"SELECT * FROM {detail.TableName}", connection))
            {
                DataTable table = new DataTable();
                new SqlDataAdapter(command)
                    .Fill(table);
                data.Add(table);
            }
        }

        return data;
    }
}
