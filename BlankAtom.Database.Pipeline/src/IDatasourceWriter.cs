using System.Data;
using System.Runtime.CompilerServices;

namespace BlankAtom.Database.Pipeline;

public interface IDatasourceWriter
{
    void WriteDDL(List<IDDLDetail> targetDdl);
    void WriteData(List<DataTable> targetData);
}

// Sql server