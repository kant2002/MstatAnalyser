using System.CommandLine;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NativeAOTSizeAnalyzer;

internal class Program
{
    static string FindNamespace(TypeReference type)
    {
        var current = type;
        while (true)
        {
            if (!string.IsNullOrEmpty(current.Namespace))
            {
                return current.Namespace;
            }

            if (current.DeclaringType == null)
            {
                return current.Name;
            }

            current = current.DeclaringType;
        }
    }

    static async Task<int> Main(string[] args)
    {
        var fileOption = new Option<FileInfo?>(
            name: "--file",
            description: "The file to read and display on the console.");
        var assemblyFilterOption = new Option<string?>(
            name: "--assembly",
            description: "Filter types and methods to one which reference assembly with given name.");

        var rootCommand = new RootCommand("Sample app for System.CommandLine");
        rootCommand.AddOption(fileOption);
        rootCommand.AddOption(assemblyFilterOption);
        rootCommand.SetHandler((file, assemblyFilter) => 
            { 
                ReadFile(file!, assemblyFilter);
            },
            fileOption, assemblyFilterOption);
        return await rootCommand.InvokeAsync(args);       
    }

    private static void ReadFile(FileInfo file, string? assemblyFilter)
    {
        var asm = AssemblyDefinition.ReadAssembly(file.FullName);
        var globalType = (TypeDefinition)asm.MainModule.LookupToken(0x02000001);
        var types = globalType.GetTypesInformationContainer();
        var typeStats = GetTypes(types).ToList();
        PrintTypesStatistics(typeStats, assemblyFilter);

        Console.WriteLine();
        var methods = globalType.GetMethodsInformationContainer();
        var methodStats = GetMethods(methods).ToList();
        PrintMethodsStatistics(methodStats, assemblyFilter);

        Console.WriteLine();

        bool printByNamespaces = assemblyFilter is null;
        if (printByNamespaces)
        {
            var methodsByNamespace = methodStats.Select(x => new TypeStats { Type = x.Method.DeclaringType, Size = x.Size + x.GcInfoSize + x.EhInfoSize })
                .Concat(typeStats).GroupBy(x => FindNamespace(x.Type)).Select(x => new { x.Key, Sum = x.Sum(x => x.Size) }).ToList();
            Console.WriteLine($"// ********** Size By Namespace");
            foreach (var m in methodsByNamespace.OrderByDescending(x => x.Sum))
            {
                Console.WriteLine($"{m.Key,-40} {m.Sum,7:n0}");
            }
            Console.WriteLine($"// **********");

            Console.WriteLine();
        }

        if (string.IsNullOrWhiteSpace(assemblyFilter))
        {
            var blobs = globalType.GetBlobsInformationContainer();
            var blobStats = GetBlobs(blobs).ToList();
            PrintBlobStatistics(blobStats);
        }
    }

    private static void PrintBlobStatistics(List<BlobStats> blobStats)
    {
        var blobSize = blobStats.Sum(x => x.Size);
        Console.WriteLine($"// ********** Blobs Total Size {blobSize:n0}");
        foreach (var m in blobStats.OrderByDescending(x => x.Size))
        {
            Console.WriteLine($"{m.Name,-40} {m.Size,7:n0}");
        }
        Console.WriteLine($"// **********");
    }

    private static void PrintMethodsStatistics(List<MethodStats> methodStats, string? assemblyFilter)
    {
        methodStats = methodStats.Where(methodStats => IsTypeFiltered(methodStats.Method.DeclaringType, assemblyFilter)).ToList();
        var methodSize = methodStats.Sum(x => x.TotalSize);
        var methodsByModules = methodStats.GroupBy(x => x.Method.DeclaringType.Scope).Select(x => new { x.Key.Name, Sum = x.Sum(x => x.TotalSize) }).ToList();
        Console.WriteLine($"// ********** Methods Total Size {methodSize:n0}");
        foreach (var m in methodsByModules.OrderByDescending(x => x.Sum))
        {
            Console.WriteLine($"{m.Name,-40} {m.Sum,7:n0}");
        }
        Console.WriteLine($"// **********");
    }

    private static string WildCardToRegular(string value) {
        return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$"; 
    }

    private static bool IsTypeFiltered(TypeReference type, string? assemblyFilter)
    {
        if (assemblyFilter is null)
        {
            return true;
        }
        
        if (Regex.IsMatch(type.Scope.Name, WildCardToRegular(assemblyFilter)))
        {
            return true;
        }

        if (type.IsGenericInstance && type is GenericInstanceType genericInstanceType)
        {
            return genericInstanceType.GenericArguments.Any(gp => IsTypeFiltered(gp, assemblyFilter));
        }

        return false;
    }

    private static void PrintTypesStatistics(List<TypeStats> typeStats, string? assemblyFilter)
    {
        typeStats = typeStats.Where(type => IsTypeFiltered(type.Type, assemblyFilter)).ToList();
        var typeSize = typeStats.Sum(x => x.Size);
        var typesByModules = typeStats.GroupBy(x => x.Type.Scope).Select(x => new { x.Key.Name, Sum = x.Sum(x => x.Size) }).ToList();
        if (assemblyFilter is not null)
        {
            Console.WriteLine($"// ********** Types in assembly {assemblyFilter} Total Size {typeSize:n0}");
        }
        else
        {
            Console.WriteLine($"// ********** Types Total Size {typeSize:n0}");
        }

        foreach (var m in typesByModules.OrderByDescending(x => x.Sum))
        {
            Console.WriteLine($"{m.Name,-40} {m.Sum,7:n0}");
        }
        Console.WriteLine($"// **********");
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
