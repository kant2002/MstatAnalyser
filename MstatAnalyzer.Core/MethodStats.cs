using Mono.Cecil;

namespace MstatAnalyzer.Core;

public class MethodStats
{
    public required MethodReference Method { get; init; }
    public int Size { get; set; }
    public int GcInfoSize { get; set; }
    public int EhInfoSize { get; set; }

    public int TotalSize => Size + GcInfoSize + EhInfoSize;
}
