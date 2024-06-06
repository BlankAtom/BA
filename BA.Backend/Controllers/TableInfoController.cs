using BA.Backend2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace BA.Backend.Controllers;

[ApiController]
[Route("connection")]
public class TableInfoController(IMemoryCache cache) : Controller
{
    [HttpPost("tables")]
    public IActionResult GetTableArray([FromBody] ConnectionConfiguration info, [FromQuery] string filter)
    {
        string key = $"tables:filter:{filter}";
        if (cache.TryGetValue<dynamic[]>(key, out var value))
        {
            return Ok(value);
        }

        dynamic[] fields = ConnectionHelpers.GetTableFields(info, filter);
        cache.Set(key, fields, new TimeSpan(0, 0, 30, 0));

        return Ok(fields);
    }




    [HttpPost("{tableName}/columns")]
    public IActionResult GetTableFieldInfo([FromRoute] string tableName, [FromBody] ConnectionConfiguration info)
    {
        var connectionBuilder = new SqlConnectionStringBuilder();
        connectionBuilder.DataSource = string.IsNullOrEmpty(info.port) ? info.ip : info.ip + "," + info.port;
        connectionBuilder.InitialCatalog = info.catalog;
        connectionBuilder.UserID = info.user;
        connectionBuilder.Password = info.word;
        connectionBuilder.PersistSecurityInfo = true;
        connectionBuilder.TrustServerCertificate = true;
        string connectionString = connectionBuilder.ToString();

        using var connection = new SqlConnection(connectionString);
        using var command = connection.CreateCommand();

        connection.Open();
        command.CommandText =
            """
            SELECT
                c.name AS ColumnName,
                t.name AS DataType,
                CASE WHEN pk.name IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey,
                CASE WHEN uq.name IS NOT NULL THEN 1 ELSE 0 END AS IsUniqueKey,
                CASE WHEN c.is_nullable = 1 THEN 1 ELSE 0 END AS IsNullable,
                c.default_object_id AS DefaultConstraintID,
                IsNull(dc.definition, '') AS DefaultValue
            FROM sys.columns c
                     INNER JOIN sys.tables t ON c.object_id = t.object_id
                     LEFT JOIN sys.index_columns ic ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                     LEFT JOIN sys.indexes pk ON ic.object_id = pk.object_id AND ic.index_id = pk.index_id AND pk.is_primary_key = 1
                     LEFT JOIN sys.indexes uq ON ic.object_id = uq.object_id AND ic.index_id = uq.index_id AND uq.is_unique_constraint = 1
                     LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
            WHERE t.name = @table_name
            """;

        command.Parameters.AddWithValue("@table_name", tableName);

        IAsyncResult asyncResult = command.BeginExecuteReader();

        var list = new List<object>();
        using var reader = command.EndExecuteReader(asyncResult);
        while (reader.Read())
        {
            string ColumnName = reader.GetString(0);
            string DataType = reader.GetString(1);
            int primaryKey = reader.GetInt32(2);
            int UniqueKey = reader.GetInt32(3);
            int Nullable = reader.GetInt32(4);
            int DefaultConstraintID = reader.GetInt32(5);
            string DefaultValue = reader.GetString(6);

            list.Add(new BATableDescription(ColumnName, DataType, primaryKey == 1, UniqueKey == 1, Nullable == 1, DefaultConstraintID, DefaultValue));
        }

        reader.Close();
        connection.Close();

        return Ok(list);
    }
}