using Microsoft.AspNetCore.Components;
using Mono.Cecil;
using MstatAnalyser.Core;

namespace MstatAnalyzerUI.Pages;

public partial class ByMethods
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

    [Parameter]
    [SupplyParameterFromQuery(Name = "type")]
    public string? Type { get; set; }

    public List<SimpleStat>? Methods { get; private set; }

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

        TypeStats? type = null;
        if (Type.EndsWith(">") && Type != "<Module>")
        {
            type = typeStats.Single(x => x.Type.FullName[(x.Type.Namespace.Length + 1)..] == Type);
        }
        else
        {
            type = typeStats.Single(x => NestedTypeName(x.Type) == Type);
        }

        var typePrefix = type.Type.FullName + "::";

        Methods = type.Methods
            .GroupBy(x => x.Method.GetElementMethod().MetadataToken)
            .Select(x => new SimpleStat { Name = x.First().Method.GetElementMethod().FullName.Replace(typePrefix, ""), Count = x.Count(), Size = x.Sum(s => s.TotalSize) })
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
