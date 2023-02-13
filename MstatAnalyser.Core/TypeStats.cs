using Mono.Cecil;

namespace MstatAnalyser.Core;

public class TypeStats
{
    private List<string>? relatedAssemblies;

    public required TypeReference Type { get; init; }
    public int Size { get; set; }

    public string PrimaryAssembly => Type.Scope.Name;
    public List<string> RelatedAssemblies
    {
        get
        {
            if (relatedAssemblies is null)
            {
                relatedAssemblies = new List<string>(GetTypeAssemblies(Type).Distinct());
            }

            return relatedAssemblies;
        }
    }

    internal static IEnumerable<string> GetTypeAssemblies(TypeReference type)
    {
        yield return type.Scope.Name;
        if (type.IsGenericInstance && type is GenericInstanceType genericInstanceType)
        {
            foreach (var subArgument in genericInstanceType.GenericArguments)
            {
                foreach (var subAssembly in GetTypeAssemblies(subArgument))
                {
                    yield return subAssembly;
                }
            }
        }
    }
}
