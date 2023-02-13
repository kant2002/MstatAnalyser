using Microsoft.AspNetCore.Components;

namespace MstatAnalyzerUI.Pages;

public partial class ByAssembly
{
    [Inject]
    public ApplicationStatsProvider AssemblyStatsProvider { get; set; } = null!;
    [Inject]
    public NavigationManager Navigation { get; set; } = null!;
    public List<SimpleStat>? SizeByAssembly { get; set; }
    public List<SimpleStat>? BlobStats { get; set; } 

    protected override void OnInitialized()
    {
        if (AssemblyStatsProvider.ApplicationStats == null)
        {
            Navigation.NavigateTo("/");
            return;
        }

        SizeByAssembly = AssemblyStatsProvider.ApplicationStats.TypeStats.GroupBy(x => x.Type.Scope.Name).Select(x => new SimpleStat { Name = x.Key, Size = x.Sum(x => x.TotalSize) }).ToList();
        SizeByAssembly.Add(new() { Name = "All", Size = SizeByAssembly.Sum(x => x.Size) });

        BlobStats = AssemblyStatsProvider.ApplicationStats.BlobStats.Select(x => new SimpleStat { Name = x.Name, Size = x.Size }).ToList();
        BlobStats.Add(new() { Name = "All", Size = BlobStats.Sum(x => x.Size) });
    }
}
