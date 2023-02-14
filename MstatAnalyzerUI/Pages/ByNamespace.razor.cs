using Microsoft.AspNetCore.Components;
using Mono.Cecil;

namespace MstatAnalyzerUI.Pages;

public partial class ByNamespace
{
    [Inject]
    public ApplicationStatsProvider AssemblyStatsProvider { get; set; } = null!;
    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "assembly")]
    public string? Assembly { get; set; }

    public List<SimpleStat>? MethodsByNamespace { get; private set; }

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

        MethodsByNamespace = typeStats
            .GroupBy(x => FindNamespace(x.Type))
            .Select(x => new SimpleStat { Name = x.Key, Size = x.Sum(x => x.TotalSize) })
            .ToList();

        MethodsByNamespace.Add(new() { Name = "All", Size = MethodsByNamespace.Sum(x => x.Size) });

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
