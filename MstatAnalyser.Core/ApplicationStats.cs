using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Reflection.PortableExecutable;

namespace MstatAnalyser.Core;

public class ApplicationStats
{
    private readonly AssemblyDefinition assemblyDefinition;

    private TypeDefinition? globalType;
    private TypeStats[]? typeStats;
    private MethodStats[]? methodStats;
    private BlobStats[]? blobStats;
    private IList<AssemblyStats>? assemblyStats;
    private readonly List<string> mangledNames = new();

    public ApplicationStats(Stream assemblyStream)
    {
        var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyStream);
        this.assemblyDefinition = assemblyDefinition;
        this.Version = assemblyDefinition.Name.Version;
        if (HasMangledName)
        {
            LoadMangledNames(assemblyStream);
        }
    }

    public ApplicationStats(string assemblyFile)
    {
        var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyFile);
        this.assemblyDefinition = assemblyDefinition;
        this.Version = assemblyDefinition.Name.Version;
        if (HasMangledName)
        {
            using (var stream = File.OpenRead(assemblyFile))
            {
                LoadMangledNames(stream);
            }
        }
    }

    private void LoadMangledNames(Stream stream)
    {
        var peReader = new PEReader(stream);
        var namesSection = peReader.GetSectionData(".names");
        var blobReader = namesSection.GetReader();

        while (blobReader.RemainingBytes > 0)
        {
            mangledNames.Add(blobReader.ReadSerializedString() ?? "<null>");
        }
    }

    public Version Version { get; }

    public bool HasMangledName => Version >= new Version(2, 0);

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
        var entrySize = Version.Major == 1 ? 2 : 3;
        var result = new List<TypeStats>(il.Count / entrySize);
        for (int i = 0; i + entrySize <= il.Count; i += entrySize)
        {
            var type = (TypeReference)il[i + 0].Operand;
            var size = (int)il[i + 1].Operand;
            if (HasMangledName)
            {
                var mangledNameIndex = (int)il[i + 2].Operand;
            }

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
        var entrySize = Version.Major == 1 ? 4 : 5;
        var result = new MethodStats[il.Count / entrySize];
        var resultI = 0;
        for (int i = 0; i + entrySize <= il.Count; i += entrySize)
        {
            var method = (MethodReference)il[i + 0].Operand;
            var size = (int)il[i + 1].Operand;
            var gcInfoSize = (int)il[i + 2].Operand;
            var ehInfoSize = (int)il[i + 3].Operand;
            if (HasMangledName)
            {
                var mangledNameIndex = (int)il[i + 4].Operand;
            }

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
