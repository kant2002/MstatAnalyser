using System.CommandLine;
using Mono.Cecil;
using MstatAnalyser.Core;

namespace MstatAnalyser;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var fileOption = new Option<FileInfo?>(
            name: "--file",
            description: "The file to read and display on the console.");
        var dgmlOption = new Option<FileInfo?>(
            name: "--dgml",
            description: "The DGML file to read and display on the console.");
        var assemblyFilterOption = new Option<string?>(
            name: "--assembly",
            description: "Filter types and methods to one which reference assembly with given name.");
        var excludeAssemblyFilterOption = new Option<string[]>(
            name: "--exclude-assembly",
            description: "Filter types and methods to one which reference assembly with given name.");
        var detailedOption = new Option<bool>(
            name: "--detailed",
            description: "List types and methods instead of providing only summary stats.");

        var rootCommand = new RootCommand("Sample app for System.CommandLine");
        rootCommand.AddOption(fileOption);
        rootCommand.AddOption(dgmlOption);
        rootCommand.AddOption(assemblyFilterOption);
        rootCommand.AddOption(detailedOption);
        rootCommand.AddOption(excludeAssemblyFilterOption);
        rootCommand.SetHandler((file, dgml, assemblyFilter, detailed, excludeAssemblyFilter) => 
            {
                if (file is null)
                {
                    Console.WriteLine($"File for analysis does not specified.");
                    return;
                }

                var fileName = file!.FullName;
                if (!file.Exists)
                {
                    if (Directory.Exists(fileName))
                    {
                        var files = Directory.GetFiles(fileName, "*.mstat", new EnumerationOptions() { RecurseSubdirectories = true });
                        if (files.Length == 1)
                        {
                            fileName = files[0];
                        }
                        else
                        {
                            Console.WriteLine($"There more then one mstat file inside directory {fileName}.");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"File {fileName} does not exists");
                        return;
                    }
                }

                ReadFile(fileName, dgml?.FullName, assemblyFilter, detailed, excludeAssemblyFilter);
            },
            fileOption, dgmlOption, assemblyFilterOption, detailedOption, excludeAssemblyFilterOption);
        return await rootCommand.InvokeAsync(args);       
    }

    private static void ReadFile(string file, string? dgmlFile, string? assemblyFilter, bool detailed, string[] excludeAssemblyFilter)
    {
        var asm = AssemblyDefinition.ReadAssembly(file);
        var applicationStats = new ApplicationStats(asm);
        var statsFilter = new StatsFilter
        {
            AssemblyFilter = assemblyFilter,
            ExcludedAssemblies = excludeAssemblyFilter,
        };
        IList<TypeStats> typeStats = applicationStats.TypeStats;
        typeStats = statsFilter.FilterTypes(typeStats);
        PrintTypesStatistics(typeStats, assemblyFilter, detailed);

        if (dgmlFile is not null)
        {
            var processing = new DgmlGraphProcessing();
            var graphResult = processing.ParseXml(dgmlFile, typeStats.Select(_ => _.Type).ToArray(), File.OpenRead(dgmlFile), out var g);
            Console.WriteLine($"Graph parsing: {graphResult}. Nodes: {g.Nodes.Count}");
        }

        Console.WriteLine();
        IList<MethodStats> methodStats = applicationStats.MethodStats;
        methodStats = statsFilter.FilterMethods(methodStats);
        PrintMethodsStatistics(methodStats, detailed);

        Console.WriteLine();

        bool printByNamespaces = assemblyFilter is null 
            && (excludeAssemblyFilter is null || excludeAssemblyFilter.Length == 0);
        if (printByNamespaces)
        {
            PrintMethodsStatistics(typeStats, methodStats);
        }

        if (string.IsNullOrWhiteSpace(assemblyFilter))
        {
            var blobStats = applicationStats.BlobStats;
            PrintBlobStatistics(blobStats);
        }
    }

    private static void PrintMethodsStatistics(IList<TypeStats> typeStats, IList<MethodStats> methodStats)
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
        string FindNamespace(TypeReference type)
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
    }

    private static void PrintBlobStatistics(IList<BlobStats> blobStats)
    {
        var blobSize = blobStats.Sum(x => x.Size);
        Console.WriteLine($"// ********** Blobs Total Size {blobSize:n0}");
        foreach (var m in blobStats.OrderByDescending(x => x.Size))
        {
            Console.WriteLine($"{m.Name,-40} {m.Size,7:n0}");
        }
        Console.WriteLine($"// **********");
    }

    private static void PrintMethodsStatistics(IList<MethodStats> methodStats, bool detailed)
    {
        var methodSize = methodStats.Sum(x => x.TotalSize);
        Console.WriteLine($"// ********** Methods Total Size {methodSize:n0}");
        if (detailed)
        {
            foreach (var m in methodStats.OrderByDescending(x => x.Method.FullName))
            {
                Console.WriteLine($"{m.Method.FullName,-40} {m.TotalSize,7:n0}");
            }
        }
        else
        {
            var methodsByModules = methodStats.GroupBy(x => x.Method.DeclaringType.Scope).Select(x => new { x.Key.Name, Sum = x.Sum(x => x.TotalSize) }).ToList();
            foreach (var m in methodsByModules.OrderByDescending(x => x.Sum))
            {
                Console.WriteLine($"{m.Name,-40} {m.Sum,7:n0}");
            }
        }

        Console.WriteLine($"// **********");
    }

    private static void PrintTypesStatistics(IList<TypeStats> typeStats, string? assemblyFilter, bool detailed)
    {
        var typeSize = typeStats.Sum(x => x.Size);
        var filterText = assemblyFilter is null ? null : $"assembly {assemblyFilter} ";
        Console.WriteLine($"// ********** Types {filterText} Total Size {typeSize:n0}");

        if (detailed)
        {
            foreach (var m in typeStats.OrderBy(x => x.Type.FullName))
            {
                Console.WriteLine($"{m.Type.FullName,-40} {m.Size,7:n0}");
            }
        }
        else
        {
            var typesByModules = typeStats.GroupBy(x => x.Type.Scope).Select(x => new { x.Key.Name, Sum = x.Sum(x => x.Size) }).ToList();
            foreach (var m in typesByModules.OrderByDescending(x => x.Sum))
            {
                Console.WriteLine($"{m.Name,-40} {m.Sum,7:n0}");
            }
        }

        Console.WriteLine($"// **********");
    }
}
