using System.Reflection;

namespace BA.DB;

public class TestOfResposity
{
    
}

public interface IGetRowObjectArray
{
    public object[] GetRowObjectArray();
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TableAttribute : Attribute
{
    public string tableName;

    public TableAttribute(string table_name)
    {
        this.tableName = table_name;
    }
}
[AttributeUsage(AttributeTargets.Field)]
public class ColumnAttribute : Attribute
{
    public string columnName;
    public Type?  columnType;

    public ColumnAttribute(string columnName, Type? column_type = null)
    {
        this.columnName = columnName;
        this.columnType = column_type;
    }
}

[Table(table_name: "user")]
public class User
{
    [Column(columnName: "name")]
    public string name;
}

internal struct ColumnStruct
{
    public string    name;
    public Type      type;
    public FieldInfo info;
}
