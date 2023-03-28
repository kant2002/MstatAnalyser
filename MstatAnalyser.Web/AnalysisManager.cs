using MstatAnalyser.Core;
using Mono.Cecil;

namespace MstatAnalyser.Web;

public class AnalysisManager
{
    public ApplicationStats? ApplicationStats { get; private set; }
    public string? FileName { get; private set; }

    public event Action<ApplicationStats?>? ApplicationStatsChanged;

    public void OpenFile(string fileName, Stream data)
    {
        this.FileName = fileName;
        this.ApplicationStats = new ApplicationStats(data);
        this.ApplicationStatsChanged?.Invoke(ApplicationStats);
    }
}
