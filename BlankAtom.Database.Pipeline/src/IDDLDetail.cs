using System.Data;

namespace BlankAtom.Database.Pipeline;

/// <summary>
///  MySql数据库DDL详情
/// </summary>
public interface IDDLDetail
{
    public string TableName { get; }
    public DataTable Schema { get; }
}


// public interface IColumnDetail
// {
//     public string ColumnName { get; }
//     public string ColumnType { get; }
//     public bool IsPrimary { get; }
//     public bool IsUnique { get; }
// }

// public class SqlServerDDLDetail : IDDLDetail
// {
//     public string TableName { get; }
//     public DataTable Schema { get; }
//
//     public SqlServerDDLDetail(string tableName, DataTable schema)
//     {
//         this.TableName = tableName;
//         this.Schema = schema;
//     }
// }