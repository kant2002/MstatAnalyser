using Mono.Cecil;

namespace MstatAnalyser.Core;

public class MethodStats
{
    private List<string>? relatedAssemblies;
    public required MethodReference Method { get; init; }
    public int Size { get; set; }
    public int GcInfoSize { get; set; }
    public int EhInfoSize { get; set; }

    public int TotalSize => Size + GcInfoSize + EhInfoSize;

    public string PrimaryAssembly => Method.DeclaringType.Scope.Name;
    public List<string> RelatedAssemblies
    {
        get
        {
            if (relatedAssemblies is null)
            {
                relatedAssemblies = new List<string>(GetTypeAssemblies(Method.DeclaringType).Distinct());
                if (Method.IsGenericInstance && Method is GenericInstanceMethod genericInstanceMethod)
                {
                    foreach (var ga in genericInstanceMethod.GenericArguments)
                    {
                        relatedAssemblies.AddRange(GetTypeAssemblies(ga));
                    }

                    relatedAssemblies = relatedAssemblies.Distinct().ToList();
                }
            }

            return relatedAssemblies;
        }
    }

    private IEnumerable<string> GetTypeAssemblies(TypeReference type)
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
