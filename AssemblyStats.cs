using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace MstatAnalyzer;

internal class AssemblyStats
{
    private readonly AssemblyDefinition assemblyDefinition;
    private TypeDefinition? globalType;
    private IList<TypeStats>? typeStats;
    private IList<MethodStats>? methodStats;
    private IList<BlobStats>? blobStats;

    public AssemblyStats(AssemblyDefinition assemblyDefinition)
    {
        this.assemblyDefinition = assemblyDefinition;
    }

    public TypeDefinition GlobalType
    {
        get
        {
            if (globalType == null)
            {
                globalType = (TypeDefinition)assemblyDefinition.MainModule.LookupToken(0x02000001);
            }

            return globalType;
        }
    }

    public IList<TypeStats> TypeStats
    {
        get
        {
            if (typeStats == null)
            {
                var types = GlobalType.GetTypesInformationContainer();
                typeStats = GetTypes(types).ToList();
            }

            return typeStats;
        }
    }

    public IList<MethodStats> MethodStats
    {
        get
        {
            if (methodStats == null)
            {
                var methods = GlobalType.GetMethodsInformationContainer();
                methodStats = GetMethods(methods).ToList();
            }

            return methodStats;
        }
    }

    public IList<BlobStats> BlobStats
    {
        get
        {
            if (blobStats == null)
            {
                var methods = GlobalType.GetBlobsInformationContainer();
                blobStats = GetBlobs(methods).ToList();
            }

            return blobStats;
        }
    }

    public static IEnumerable<TypeStats> GetTypes(MethodDefinition types)
    {
        types.Body.SimplifyMacros();
        var il = types.Body.Instructions;
        for (int i = 0; i + 2 < il.Count; i += 2)
        {
            var type = (TypeReference)il[i + 0].Operand;
            var size = (int)il[i + 1].Operand;
            yield return new TypeStats
            {
                Type = type,
                Size = size
            };
        }
    }

    public static IEnumerable<MethodStats> GetMethods(MethodDefinition methods)
    {
        methods.Body.SimplifyMacros();
        var il = methods.Body.Instructions;
        for (int i = 0; i + 4 < il.Count; i += 4)
        {
            var method = (MethodReference)il[i + 0].Operand;
            var size = (int)il[i + 1].Operand;
            var gcInfoSize = (int)il[i + 2].Operand;
            var ehInfoSize = (int)il[i + 3].Operand;
            yield return new MethodStats
            {
                Method = method,
                Size = size,
                GcInfoSize = gcInfoSize,
                EhInfoSize = ehInfoSize
            };
        }
    }

    public static IEnumerable<BlobStats> GetBlobs(MethodDefinition blobs)
    {
        blobs.Body.SimplifyMacros();
        var il = blobs.Body.Instructions;
        for (int i = 0; i + 2 < il.Count; i += 2)
        {
            var name = (string)il[i + 0].Operand;
            var size = (int)il[i + 1].Operand;
            yield return new BlobStats
            {
                Name = name,
                Size = size
            };
        }
    }
}
