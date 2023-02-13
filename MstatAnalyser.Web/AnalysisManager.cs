using MstatAnalyser.Core;
using Mono.Cecil;

namespace MstatAnalyser.Web;

public class AnalysisManager
{
    public ApplicationStats? ApplicationStats { get; private set; }
    public string? FileName { get; private set; }

    public void OpenFile(string fileName, Stream data)
    {
        this.FileName = fileName;
        var asm = AssemblyDefinition.ReadAssembly(data);
        this.ApplicationStats = new ApplicationStats(asm);
    }
}
