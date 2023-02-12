using Mono.Cecil;

namespace MstatAnalyzer.Core;

internal static class Extensions
{
    public static MethodDefinition GetTypesInformationContainer(this TypeDefinition globalType)
    {
        return globalType.Methods.First(x => x.Name == "Types");
    }
    public static MethodDefinition GetMethodsInformationContainer(this TypeDefinition globalType)
    {
        return globalType.Methods.First(x => x.Name == "Methods");
    }
    public static MethodDefinition GetBlobsInformationContainer(this TypeDefinition globalType)
    {
        return globalType.Methods.First(x => x.Name == "Blobs");
    }
}
