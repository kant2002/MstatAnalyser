using Mono.Cecil;
using System.Text.RegularExpressions;

namespace MstatAnalyzer;

internal class StatsFilter
{
    public string? AssemblyFilter { get; set; }
    public string[]? ExcludedAssemblies { get; set; }

    public IList<TypeStats> FilterTypes(IList<TypeStats> typeStats)
    {
        return typeStats.Where(type => IsTypeFiltered(type.Type, AssemblyFilter)
            && (ExcludedAssemblies is null || !IsTypeFiltered(type.Type, ExcludedAssemblies))).ToList();
    }

    public IList<MethodStats> FilterMethods(IList<MethodStats> methodStats)
    {
        return methodStats.Where(methodStats => IsTypeFiltered(methodStats.Method.DeclaringType, AssemblyFilter)
            && (ExcludedAssemblies is null || !IsTypeFiltered(methodStats.Method.DeclaringType, ExcludedAssemblies))).ToList();
    }

    private static string WildCardToRegular(string value)
    {
        return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
    }

    private static bool IsTypeFiltered(TypeReference type, string[] assemblyFilter)
    {
        return assemblyFilter.Any(f => IsTypeFiltered(type, f));
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
