namespace MstatAnalyser.Core;

public class AssemblyStats
{
    public required string AssemblyName { get; set; }

    public int TypesSize { get; set; }
    public int MethodsSize { get; set; }
    public int TotalSize => TypesSize + MethodsSize;
}
