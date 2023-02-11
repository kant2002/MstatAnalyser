using Mono.Cecil;

namespace NativeAOTSizeAnalyzer;

public class TypeStats
{
    public required TypeReference Type { get; init; }
    public int Size { get; set; }
}
