using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace MstatAnalyser.Core;

public class ApplicationStats
{
    private readonly AssemblyDefinition assemblyDefinition;
    private TypeDefinition? globalType;
    private TypeStats[]? typeStats;
    private MethodStats[]? methodStats;
    private BlobStats[]? blobStats;
    private IList<AssemblyStats>? assemblyStats;

    public ApplicationStats(AssemblyDefinition assemblyDefinition)
    {
        this.assemblyDefinition = assemblyDefinition;
    }

    public TypeDefinition GlobalType => globalType ??= (TypeDefinition)assemblyDefinition.MainModule.LookupToken(0x02000001);
    public TypeStats[] TypeStats => typeStats ??= GetTypes(GlobalType.GetTypesInformationContainer());
    public MethodStats[] MethodStats => methodStats ??= GetMethods(GlobalType.GetMethodsInformationContainer());
    public BlobStats[] BlobStats => blobStats ??= GetBlobs(GlobalType.GetBlobsInformationContainer());

    public IList<AssemblyStats> AssemblyStats
    {
        get
        {
            if (assemblyStats == null)
            {
                assemblyStats = TypeStats.Select(_ => new { _.PrimaryAssembly, TypeSize = _.Size, MethodSize = 0 })
                    .Concat(MethodStats.Select(_ => new { _.PrimaryAssembly, TypeSize = 0, MethodSize = _.TotalSize }))
                    .GroupBy(x => x.PrimaryAssembly)
                    .Select(x => new AssemblyStats { AssemblyName = x.Key, TypesSize = x.Sum(x => x.TypeSize), MethodsSize = x.Sum(x => x.MethodSize) })
                    .ToList();
            }

            return assemblyStats;
        }
    }

    public IList<TypeStats> GetAssemblyTypes(string assemblyName)
    {
        return TypeStats.Where(_ => _.PrimaryAssembly == assemblyName).ToList();
    }

    private TypeStats[] GetTypes(MethodDefinition types)
    {
        types.Body.SimplifyMacros();
        var il = types.Body.Instructions;
        var result = new List<TypeStats>(il.Count / 2);
        for (int i = 0; i + 2 <= il.Count; i += 2)
        {
            var type = (TypeReference)il[i + 0].Operand;
            var size = (int)il[i + 1].Operand;
            result.Add(new TypeStats
            {
                Type = type,
                Size = size
            });
        }
        var dic = result.ToDictionary(x => x.Type.MetadataToken);
        foreach (var m in MethodStats)
        {
            if (!dic.TryGetValue(m.Method.DeclaringType.MetadataToken, out var type))
            {
                type = new TypeStats { Type = m.Method.DeclaringType };
                dic.Add(type.Type.MetadataToken, type);
                result.Add(type);
            }
            type.Methods.Add(m);
        }
        return result.ToArray();
    }

    private MethodStats[] GetMethods(MethodDefinition methods)
    {
        methods.Body.SimplifyMacros();
        var il = methods.Body.Instructions;
        var result = new MethodStats[il.Count / 4];
        var resultI = 0;
        for (int i = 0; i + 4 <= il.Count; i += 4)
        {
            var method = (MethodReference)il[i + 0].Operand;
            var size = (int)il[i + 1].Operand;
            var gcInfoSize = (int)il[i + 2].Operand;
            var ehInfoSize = (int)il[i + 3].Operand;
            result[resultI++] = new MethodStats
            {
                Method = method,
                Size = size,
                GcInfoSize = gcInfoSize,
                EhInfoSize = ehInfoSize
            };
        }
        return result;
    }

    private BlobStats[] GetBlobs(MethodDefinition blobs)
    {
        blobs.Body.SimplifyMacros();
        var il = blobs.Body.Instructions;
        var result = new BlobStats[il.Count / 2];
        var resultI = 0;
        for (int i = 0; i + 2 <= il.Count; i += 2)
        {
            var name = (string)il[i + 0].Operand;
            var size = (int)il[i + 1].Operand;
            result[resultI++] = new BlobStats
            {
                Name = name,
                Size = size
            };
        }
        return result;
    }
}
