using Mono.Cecil;

namespace MstatAnalyser.Core;

public class TypeStats
{
    public required TypeReference Type { get; init; }
    public int Size { get; set; }
}
