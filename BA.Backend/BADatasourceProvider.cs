using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BA.Backend2;

namespace BA.Backend;
public record BATableDescription(string ColumnName, string DataType, bool IsPrimaryKey, bool IsUniqueKey, bool IsNullable, int DefaultConstraintID, string DefaultValue)
{
    public override string ToString()
    {
        return $"{{ ColumnName = {ColumnName}, DataType = {DataType}, IsPrimaryKey = {IsPrimaryKey}, IsUniqueKey = {IsUniqueKey}, IsNullable = {IsNullable}, DefaultConstraintID = {DefaultConstraintID}, DefaultValue = {DefaultValue} }}";
    }
}
public abstract class BADatasourceProvider
{
    // TODO 获取数据库结构
    public  Dictionary<string, BATableDescription> tableDescriptions;

    private DbConnection _dbConnection;

    public BADatasourceProvider(string connectionString, DatasourceType type = DatasourceType.MSSQL)
    {
        var dbConnection = ConnectionHelpers.GetConnection();
        this._dbConnection = dbConnection;

        dbConnection.ConnectionString = connectionString;
    }

    /// <summary>
    /// 连接字符串
    /// </summary>
    protected string? ConnectionString { get; init; }

    /// <summary>
    /// 默认端口
    /// </summary>
    protected string? DefaultPort { get; set; }

    /// <summary>
    /// 查询表信息语句，包含一个占位符，数据库名
    /// </summary>
    protected string? SelectTableI8n { get; init; }

    protected string[]? SelectTableParameters;

    /// <summary>
    /// 查询字段信息语句，占位符0：数据库名，占位符1：表名
    /// </summary>
    public string? SelectFieldsI8n { get; set; }

    public string[] SelectFieldsParameters;

    /// <summary>
    /// 插入语句，占位符: 0-表名、1-属性名（带括号）、2-值（全部的值的字符串）
    /// </summary>
    public string InsertString { get; set; }

    /// <summary>
    /// 删除语句，占位符： 0-表明
    /// </summary>
    public string DeleteString { get; set; }

    /// <summary>
    /// 查询表数据
    /// </summary>
    public string SelectTableString { get; set; }

    /// <summary>
    ///  查询主键
    /// </summary>
    public string SelectPrimaryKeyString { get; set; }

    //public string
    protected string database;
    protected string username;
    protected string password;
    protected string url;
    protected string port;

    public abstract Dictionary<string, Dictionary<string, string>> GetAllTableAndFields();

    // public abstract Dictionary<string, BGDataTable> GetData(Dictionary<string, List<string>> property, Action<int, string> updateProgress = null);
    //
    // public abstract Dictionary<string, List<string>> GetAllTablePrimaryKeys();
    //
    // public abstract void SetData(Dictionary<string, BGDataTable> data, Action<int, string> updateProgress = null);
    //
    // protected abstract List<Dictionary<string, BGDataColumn>> SelectQuery(string sql);

    protected abstract void InsertQuery(string sql);


    protected DbConnection? Connection;

    protected List<string> GetTablesNames()
    {
        //
        // 当语句为空则结束
        //
        if (SelectTableI8n == null || SelectTableI8n == "")
        {
            return new List<string>();
        }

        // 
        // 当数据库为空则结束
        // 
        if (database == null || database == "")
        {
            return new List<string>();
        }

        List<string> tables = new List<string>();

        //
        // 构造sql语句
        // 
        string sql = String.Format(SelectTableI8n, database);
        //
        // 执行sql
        //
        var ret = SelectQuery(sql);
        //
        // 遍历结果
        // 
        foreach (var r in ret)
        foreach (var c in r)
        {
            tables.Add(c.Value.Value);
            break;
        }

        return tables;
    }


    protected Dictionary<string, string> GetFieldNamesByTableName(string tableName)
    {
        if (tableName == null || tableName == "")
            return new Dictionary<string, string>();

        if (SelectFieldsI8n == null || SelectFieldsI8n == "")
            return new Dictionary<string, string>();

        if (database == null || database == "")
            return new Dictionary<string, string>();

        Dictionary<string, string> fields = new Dictionary<string, string>();

        string sql = String.Format(SelectFieldsI8n, database, tableName);
        var ret = SelectQuery(sql);

        foreach (var r in ret)
        {
            string fname = r[SelectFieldsParameters[0]].Value;
            string ftype = r[SelectFieldsParameters[1]].Value;

            fields.Add(fname, ftype);
        }

        return fields;
    }

    /// <summary>
    /// 判断该列是否为文本列
    /// </summary>
    /// <param name="col">该列属性</param>
    /// <returns>true：是文本列</returns>
    protected bool IsText(BGDataColumn col)
    {
        if (col.ColumnType.ToLower().Contains("char") || col.ColumnType.ToLower().Contains("text") || col.ColumnType.ToLower().Contains("blob"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 使用逗号合并字符串数组，并用括号括起来
    /// </summary>
    /// <param name="strs">待合并数组</param>
    /// <returns>合并后结果</returns>
    protected string JoinWithParenthese(string[] strs)
    {
        return "(" + JoinWithNonParenthese(strs) + ")";
    }

    /// <summary>
    /// 使用逗号合并字符串数组
    /// </summary>
    /// <param name="strs">待合并数组</param>
    /// <returns>合并后结果</returns>
    protected string JoinWithNonParenthese(string[] strs)
    {
        return String.Join(",", strs);
    }

    /// <summary>
    /// 基于属性<see cref="InsertString"/>构造插入字符串
    /// </summary>
    /// <param name="table">表名</param>
    /// <returns>sql字符串</returns>
    protected string CreateInsertString(BGDataTable table)
    {
        if (table.Rows.Count == 0)
            return "";

        List<string> columnNames = new List<string>();
        foreach (var c in table.Rows[0])
        {
            columnNames.Add(c.Value.ColumnName);
        }

        List<string> values = new List<string>();
        foreach (var r in table.Rows)
        {
            List<string> value = new List<string>();
            foreach (var c in r)
            {
                value.Add(IsText(c.Value) ? "'" + c.Value.Value + "'" : c.Value.Value);
            }

            string oneValue = JoinWithParenthese(value.ToArray());
            values.Add(oneValue);
        }

        string fieldsString = JoinWithNonParenthese(columnNames.ToArray());
        string valuesString = JoinWithNonParenthese(values.ToArray());

        string sql = String.Format(DeleteString, table.TableName) +
                     String.Format(InsertString, table.TableName, fieldsString, valuesString);
        return sql;
    }

    /// <summary>
    /// 获取单表数据
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="fieldNames">要查询的字段名</param>
    /// <returns>带数据的<see cref="BGDataTable"/></returns>
    protected BGDataTable? GetTableData(string tableName, List<string> fieldNames)
    {
        if (SelectTableString == null || SelectTableString == "")
            return null;

        string fields = JoinWithNonParenthese(fieldNames.ToArray());
        string sql = String.Format(SelectTableString, tableName, fields);

        var ret = SelectQuery(sql);

        var bt = new BGDataTable(tableName);
        bt.Rows = ret;

        return bt;
    }







}