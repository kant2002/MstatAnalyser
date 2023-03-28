using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Mono.Cecil;
using MstatAnalyser.Core;

namespace MstatAnalyzerUI.Pages;

public partial class Index
{
    [Inject]
    public ApplicationStatsProvider AssemblyStatsProvider { get; set; }
    [Inject]
    public NavigationManager Navigation { get; set; }

    public async Task FileUploaded(InputFileChangeEventArgs args)
    {
        var ms = new MemoryStream((int)args.File.Size);
        using (var stream = args.File.OpenReadStream(maxAllowedSize: 500_000_000))
        {
            await stream.CopyToAsync(ms);
        }
        ms.Position = 0;

        AssemblyStatsProvider.ApplicationStats = new ApplicationStats(ms);
        Navigation.NavigateTo("ByAssembly");
    }
}
