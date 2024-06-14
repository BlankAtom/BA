using System.Data;
using System.Data.Common;

namespace BlankAtom.Database.Pipeline;

public interface IDatasourceReader
{
    /// <summary>
    /// 从数据库读取DDL信息，并缓存返回
    /// </summary>
    /// <param name="???"></param>
    List<IDDLDetail> ReadToDDL();

    /// <summary>
    ///  从数据库读取数据，并缓存返回
    /// </summary>
    /// <returns></returns>
    List<DataTable> GetTableData();
}
