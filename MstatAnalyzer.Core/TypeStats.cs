using Mono.Cecil;

namespace MstatAnalyzer.Core;

public class TypeStats
{
    public required TypeReference Type { get; init; }
    public int Size { get; set; }
}
