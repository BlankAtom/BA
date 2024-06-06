using System.Data;
using System.Data.Common;

namespace BlankAtom.Database.Pipeline;

public class DatasourceReader
{
    /// <summary>
    /// 从数据库读取DDL信息，并缓存返回
    /// </summary>
    /// <param name="???"></param>
    public static List<IDDLDetail> ReadToDDL(DbConnection connection)
    {
        // 打开连接，并读取目标数据库的DDL信息
connection.Open();

        throw new NotImplementedException();
    }
}

public class SqlDatasourceReaderCommand
{
    public List<IDDLDetail> ReadToDDL(DbConnection connection)
    {
        connection.Open();
        var details = new Dictionary<string, SqlServerDDLDetail>();


        var command = connection.CreateCommand();
        command.CommandText = """
                              select
                                  c.name as data_name,
                                  t.name as data_type,
                                  st.name as table_name,
                                  
                              from
                                  sys.columns c left join sys.tables st on c.object_id = st.object_id
                              left join sys.types t on c.user_type_id = t.user_type_id
                              where st.type = 'U';
                              """;
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string column_name = reader.GetString(0);
            string type_name = reader.GetString(1);
            string table_name = reader.GetString(2);

            if (!details.ContainsKey(table_name))
            {
                details.Add(table_name, new SqlServerDDLDetail(table_name));
            }

            details[table_name].TableDetails.Add(new SqlServerTableDDLDetail(column_name, type_name,));

        }
    }
}

public class SqlServerDDLDetail(string name) : IDDLDetail
{
    public List<SqlServerTableDDLDetail> TableDetails { get; set; }
    = new List<SqlServerTableDDLDetail>();
    public string GetFieldName()
    {
        throw new NotImplementedException();
    }

    public DbType GetFieldType()
    {
        throw new NotImplementedException();
    }
}

public class SqlServerTableDDLDetail(string name, string type, bool isPrimary, bool isUnique) : IDDLDetail
{
    public string GetFieldName()
    {
        throw new NotImplementedException();
    }

    public DbType GetFieldType()
    {
        throw new NotImplementedException();
    }
}

public interface IDDLDetail
{
    string GetFieldName();

    DbType GetFieldType();
}