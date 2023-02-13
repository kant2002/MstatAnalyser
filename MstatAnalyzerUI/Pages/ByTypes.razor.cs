using Microsoft.AspNetCore.Components;
using Mono.Cecil;

namespace MstatAnalyzerUI.Pages;

public partial class ByTypes
{
    [Inject]
    public ApplicationStatsProvider AssemblyStatsProvider { get; set; } = null!;
    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "assembly")]
    public string? Assembly { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "ns")]
    public string? Namespace { get; set; }

    public List<SimpleStat>? Types { get; private set; }

    protected override void OnInitialized()
    {
        if (AssemblyStatsProvider.ApplicationStats == null)
        {
            Navigation.NavigateTo("/");
            return;
        }

        var typeStats = AssemblyStatsProvider.ApplicationStats.TypeStats.AsEnumerable();

        if (Assembly is not (null or "All"))
        {
            typeStats = typeStats.Where(x => x.Type.Scope.Name == Assembly);
        }

        if (Namespace is not (null or "All"))
        {
            typeStats = typeStats.Where(x => FindNamespace(x.Type) == Namespace);
        }

        Types = typeStats
            .GroupBy(x => (x.Type.Namespace, Name: NestedTypeName(x.Type)))
            .Select(x => new SimpleStat { Name = x.Key.Name, Count = x.Count(), Size = x.Sum(x => x.TotalSize) })
            .ToList();

        string NestedTypeName(TypeReference type)
        {
            if (type.DeclaringType != null)
            {
                return NestedTypeName(type.DeclaringType) + "+" + type.Name;
            }
            return type.Name;
        }
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
                    return "<global>";
                }

                current = current.DeclaringType;
            }
        }
    }
}
