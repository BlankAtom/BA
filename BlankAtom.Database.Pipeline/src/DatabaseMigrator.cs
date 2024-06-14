using System.Data;

namespace BlankAtom.Database.Pipeline;

public class DatabaseMigrator
{
    private IDatasourceReader sourceReader;
    private IDatasourceWriter targetWriter;

    public DatabaseMigrator(IDatasourceReader sourceReader, IDatasourceWriter targetWriter)
    {
        this.sourceReader = sourceReader;
        this.targetWriter = targetWriter;
    }

    public void Migrate()
    {
        // 读取源数据库的DDL和数据
        var ddlDetails = this.sourceReader.ReadToDDL();
        var data = this.sourceReader.GetTableData();

        // 将DDL和数据转换为目标数据库可以理解的格式
        var targetDDL = ConvertDDL(ddlDetails);
        var targetData = ConvertData(data);

        // 在目标数据库中创建相应的结构并插入数据
        this.targetWriter.WriteDDL(targetDDL);
        this.targetWriter.WriteData(data);
    }

    private List<IDDLDetail> ConvertDDL(List<IDDLDetail> ddlDetails)
    {
        // 这里需要根据目标数据库的特性来转换DDL
        return ddlDetails;
    }

    private List<DataTable> ConvertData(List<DataTable> data)
    {
        // 这里需要根据目标数据库的特性来转换数据
        return data;
    }
}