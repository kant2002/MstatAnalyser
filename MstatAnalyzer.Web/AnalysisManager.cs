using MstatAnalyzer.Core;
using Mono.Cecil;

namespace MstatAnalyzer.Web;

public class AnalysisManager
{
    public AssemblyStats? AssemblyStats { get; private set; }
    public string? FileName { get; private set; }

    public void OpenFile(string fileName, Stream data)
    {
        this.FileName = fileName;
        var asm = AssemblyDefinition.ReadAssembly(data);
        this.AssemblyStats = new AssemblyStats(asm);
    }
}
