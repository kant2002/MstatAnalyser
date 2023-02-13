using Mono.Cecil;
using System.Text.RegularExpressions;

namespace MstatAnalyser.Core;

public class StatsFilter
{
    public string? AssemblyFilter { get; set; }
    public string[]? ExcludedAssemblies { get; set; }

    public IList<TypeStats> FilterTypes(IList<TypeStats> typeStats)
    {
        return typeStats.Where(type => IsTypeFiltered(type, AssemblyFilter)
            && (ExcludedAssemblies is null || !IsTypeFiltered(type, ExcludedAssemblies))).ToList();
    }

    public IList<MethodStats> FilterMethods(IList<MethodStats> methodStats)
    {
        return methodStats.Where(methodStats => IsMethodFiltered(methodStats, AssemblyFilter)
            && (ExcludedAssemblies is null || !IsMethodFiltered(methodStats, ExcludedAssemblies))).ToList();
    }

    private static string WildCardToRegular(string value)
    {
        return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
    }

    private static bool IsTypeFiltered(TypeReference type, string[] assemblyFilter)
    {
        return assemblyFilter.Any(f => IsTypeFiltered(type, f));
    }

    private static bool IsTypeFiltered(TypeStats type, string[] assemblyFilter)
    {
        return assemblyFilter.Any(f => IsTypeFiltered(type, f));
    }

    private static bool IsTypeFiltered(TypeStats type, string? assemblyFilter)
    {
        if (assemblyFilter is null)
        {
            return true;
        }

        return type.RelatedAssemblies.Any(assemblyName => Regex.IsMatch(assemblyName, WildCardToRegular(assemblyFilter)));
    }

    private static bool IsMethodFiltered(MethodStats method, string[] assemblyFilter)
    {
        return assemblyFilter.Any(f => IsMethodFiltered(method, f));
    }

    private static bool IsMethodFiltered(MethodStats method, string? assemblyFilter)
    {
        if (assemblyFilter is null)
        {
            return true;
        }

        return method.RelatedAssemblies.Any(assemblyName => Regex.IsMatch(assemblyName, WildCardToRegular(assemblyFilter)));
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
}
