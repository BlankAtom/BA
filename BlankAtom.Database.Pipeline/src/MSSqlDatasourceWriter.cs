using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using System.Data;
using System.Text;

namespace BlankAtom.Database.Pipeline;

public class MSSqlDatasourceWriter : IDatasourceWriter
{
    private string connectionString;

    public MSSqlDatasourceWriter(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public void WriteDDL(List<IDDLDetail> targetDdl)
    {
        using (var connection = new SqlConnection(this.connectionString))
        {
            connection.Open();

            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                foreach (var ddl in targetDdl)
                {
                    using (var command = new SqlCommand("Drop Table If Exists " + ddl.TableName, connection))
                    {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                    }

                    // 生成创建表的命令
                    var createTableCommand = GenerateCreateTableCommandByScheme(ddl);
                    using (var command = new SqlCommand(createTableCommand, connection))
                    {
                        try
                        {
                            command.Transaction = transaction;
                            command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error: " + ddl.TableName);
                            throw;
                        }
                    }

                    // using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    // {
                    //     bulkCopy.DestinationTableName = ddl.TableName;
                    //     bulkCopy.WriteToServer(ddl.Schema);
                    // }
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                transaction.Rollback();
            }
        }
    }

    public void WriteData(List<DataTable> targetData)
    {
        using (var connection = new SqlConnection(this.connectionString))
        {
            connection.Open();

            foreach (var data in targetData)
            {
                using (var adapter = new SqlDataAdapter($"SELECT * FROM {data.TableName}", connection))
                {
                    adapter.Update(data);
                }
            }
        }
    }


    /// <summary>
    ///  生成创建表的命令
    /// </summary>
    /// <param name="ddl"></param>
    /// <returns></returns>
    private string GenerateCreateTableCommandByScheme(IDDLDetail ddl)
    {
        var command = new StringBuilder($"CREATE TABLE {ddl.TableName} (");
        DataTable schema = ddl.Schema;
        if (schema.ExtendedProperties["TableType"]!.ToString() != "BASE TABLE")
        {
            return schema.ExtendedProperties["ViewDefinition"]!.ToString()!;
        }
        foreach (DataRow row in schema.Rows)
        {
            // DataColumn? columnName = schema.Columns["ColumnName"];
            ;

            command.Append($"[{row["ColumnName"]}] {row["DataTypeName"]} ");
            if (row["DataType"].ToString()!.Contains("String") && !bool.Parse(row["IsLong"].ToString()!))
            {
                command.Append($"({row["ColumnSize"]}) ");
            }

            if(bool.Parse(row["IsUnique"].ToString()!))
            {
                command.Append("UNIQUE ");
            }
            else if (row["IsKey"] != DBNull.Value && bool.Parse(row["IsKey"].ToString()!))
            {
                command.Append("PRIMARY KEY ");
            }



            command.Append(", ");

        }

        command.Append(");");

        return command.ToString();
    }

    private string GenerateCreateTableCommand(IDDLDetail ddl)
    {
        var command = new StringBuilder($"CREATE TABLE {ddl.TableName} (");

        DataTable schema = ddl.Schema;

        // foreach (DataRow row in schema.Rows)
        // {
        //     // 0 name, 2 size, 5 unique, 6 pk, 13 allow null, 24 type name
        //     row[0]
        // }
        for (int i = 0; i < schema.Columns.Count; i++)
        {
            var column = schema.Columns[i];
            // var row = schema.Rows[i];

            command.Append($"{column.ColumnName} {column.DataType.ToSqlDbType().ToString()} ");

            if(column.DataType == typeof(string))
            {
                command.Append($"({column.MaxLength}) ");
            }
            if (column.Unique)
            {
                command.Append("UNIQUE ");
            }
            if(column.DefaultValue != DBNull.Value)
            {
                if(column.DataType == typeof(string))
                {
                    command.Append($"DEFAULT '{column.DefaultValue}' ");
                }
                else
                {
                    command.Append($"DEFAULT {column.DefaultValue} ");
                }
            }

            if (i < schema.Columns.Count - 1)
            {
                command.Append(", ");
            }
        }

        // 如果有主键列，添加PRIMARY KEY子句
        if (schema.PrimaryKey.Length > 0)
        {
            command.Append($", PRIMARY KEY ({string.Join(", ", schema.PrimaryKey.Select(c => c.ColumnName))})");
        }

        command.Append(");");

        return command.ToString();
    }
}

public static class SqlTypeExtensions
{
    public static SqlDbType ToSqlDbType(this Type dataType)
    {
        var typeMap = new Dictionary<Type, SqlDbType>
        {
            { typeof(string), SqlDbType.NVarChar },
            { typeof(int), SqlDbType.Int },
            { typeof(DateTime), SqlDbType.DateTime },
            { typeof(bool), SqlDbType.Bit },
            { typeof(byte), SqlDbType.TinyInt },
            { typeof(short), SqlDbType.SmallInt },
            { typeof(long), SqlDbType.BigInt },
            { typeof(decimal), SqlDbType.Decimal },
            { typeof(float), SqlDbType.Float },
            { typeof(double), SqlDbType.Real },
            { typeof(byte[]), SqlDbType.Binary }
        };

        if (typeMap.TryGetValue(dataType, out SqlDbType sqlDbType))
        {
            return sqlDbType;
        }

        throw new ArgumentException($"Unsupported data type: {dataType}");
    }
}