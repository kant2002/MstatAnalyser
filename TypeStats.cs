using Mono.Cecil;

namespace MstatAnalyzer;

public class TypeStats
{
    public required TypeReference Type { get; init; }
    public int Size { get; set; }
}
