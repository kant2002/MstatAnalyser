using Mono.Cecil;
using System.Text.RegularExpressions;

namespace MstatAnalyzer;

internal class StatsFilter
{
    public string? AssemblyFilter { get; set; }

    public IList<TypeStats> FilterTypes(IList<TypeStats> typeStats)
    {
        return typeStats.Where(type => IsTypeFiltered(type.Type, AssemblyFilter)).ToList();
    }

    public IList<MethodStats> FilterMethods(IList<MethodStats> methodStats)
    {
        return methodStats.Where(methodStats => IsTypeFiltered(methodStats.Method.DeclaringType, AssemblyFilter)).ToList();
    }

    private static string WildCardToRegular(string value)
    {
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
}
