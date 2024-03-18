using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace BA.DB;

public abstract class Repository<T> where T: class, IGetRowObjectArray
{
    public void test()
    {
        Type            cur_type        = typeof(T);
        TableAttribute? table_attribute = cur_type.GetCustomAttribute<TableAttribute>();
        this.table_name = table_attribute?.tableName ?? "";
        List<ColumnStruct> column_structs = new();

        foreach (FieldInfo field_info in cur_type.GetFields().Where(t=>t.IsDefined(typeof(ColumnAttribute),false)))
        {
            ColumnAttribute? custom_attribute = field_info.GetCustomAttribute<ColumnAttribute>();
            ColumnStruct column_struct = new ColumnStruct()
            {
                name = this.table_name,
                type = custom_attribute!.columnType ?? field_info.FieldType,
                info = field_info
            };
            // DataColumn       column_struct    = new DataColumn(custom_attribute!.columnName);
            // column_struct.DataType = custom_attribute.columnType ?? field_info.FieldType;


            column_structs.Add(column_struct);
        }

        this.structs = column_structs.ToArray();
    }

    private string         table_name;
    private ColumnStruct[] structs;

    public abstract DataTable GetDataTable(T[] data);

    public void insert(T[] data)
    {
        string connectionString = "";

        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();

        using SqlTransaction transaction = connection.BeginTransaction();
        using (SqlBulkCopy bulk_copy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
        {
            bulk_copy.DestinationTableName = this.table_name;
            bulk_copy.BatchSize            = 500;
            bulk_copy.BulkCopyTimeout      = 30;

            DataTable data_table = new DataTable();
            data_table.Columns.AddRange(this.structs.Select(t => new DataColumn(t.name, t.type)).ToArray());

            foreach (T x1 in data)
            {
                data_table.Rows.Add(this.structs.Select(t => t.info.GetValue(x1)).ToArray());
            }

            data_table.Rows.Add();
            bulk_copy.WriteToServer(data_table);
        }

        transaction.Commit();
    }

    public void Select()
    {
        string connectionString = "Server=localhost;Database=your_database_name;User=root;Password=your_password;";

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT * FROM user";

            using (var command = new MySqlCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            // 读取每一行的数据
                            int     id    = reader.GetInt32("Id");
                            string  name  = reader.GetString("Name");
                            decimal price = reader.GetDecimal("Price");
                            // reader.GetD
                            // 进行数据操作，例如将数据添加到列表中
                            Console.WriteLine($"ID: {id}, Name: {name}, Price: {price}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("没有数据行.");
                    }
                }
            }
        }
    }

}