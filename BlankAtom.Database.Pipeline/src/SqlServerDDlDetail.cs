using System.Data;

namespace BlankAtom.Database.Pipeline;

public class SqlServerDDLDetail : IDDLDetail
{
    /// <inheritdoc />
    public string TableName { get; }

    /// <inheritdoc />
    public DataTable Schema { get; }

    public SqlServerDDLDetail(string tableName, DataTable schema)
    {
        this.TableName = tableName;
        this.Schema = schema;
    }
}
