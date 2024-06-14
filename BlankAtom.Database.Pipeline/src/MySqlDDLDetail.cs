using System.Data;

namespace BlankAtom.Database.Pipeline;

/// <summary>
///  MySql数据库DDL详情
/// </summary>
public class MySqlDDLDetail : IDDLDetail
{
    /// <inheritdoc />
    public string TableName { get; }

    /// <inheritdoc />
    public DataTable Schema { get; }

    public MySqlDDLDetail(string tableName, DataTable schema)
    {
        this.TableName = tableName;
        this.Schema = schema;
    }
}
