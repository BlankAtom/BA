using MySql.Data.MySqlClient;
using System.Data;
using System.Text;

namespace BlankAtom.Database.Pipeline;
// MYSQL
public class MySqlDatasourceWriter : IDatasourceWriter
{
    private string connectionString;

    public MySqlDatasourceWriter(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public void WriteDDL(List<IDDLDetail> targetDdl)
    {
        using (var connection = new MySqlConnection(this.connectionString))
        {
            connection.Open();

            foreach (var ddl in targetDdl)
            {
                using(var command = new MySqlCommand("Drop Table If Exists " + ddl.TableName, connection))
                {
                    command.ExecuteNonQuery();
                }

                // 生成创建表的命令
                var createTableCommand = GenerateCreateTableCommand(ddl);
                using (var command = new MySqlCommand(createTableCommand, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    private string GenerateCreateTableCommand(IDDLDetail ddl)
    {
        var command = new StringBuilder($"CREATE TABLE {ddl.TableName} (");

        for (int i = 0; i < ddl.Schema.Columns.Count; i++)
        {
            var column = ddl.Schema.Columns[i];
            command.Append($"{column.ColumnName} {column.DataType}");

            if (i < ddl.Schema.Columns.Count - 1)
            {
                command.Append(", ");
            }
        }

        command.Append(");");

        return command.ToString();
    }

    public void WriteData(List<DataTable> targetData)
    {
        using (var connection = new MySqlConnection(this.connectionString))
        {
            connection.Open();

            foreach (var data in targetData)
            {
                using (var adapter = new MySqlDataAdapter($"SELECT * FROM {data.TableName}", connection))
                {
                    adapter.Update(data);
                }
            }
        }
    }
}
