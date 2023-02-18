using Mono.Cecil;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace MstatAnalyser.Core;

public class NodeConverter
{
    private readonly TypeReference[] types;

    public NodeConverter(TypeReference[] types)
    {
        this.types = types;
    }

    public Node Convert(Node node)
    {
        if (TryMatchType(node.Name, out var position, out var resolvedType))
        {
            if (position == node.Name.Length)
            {
                return new TypeNode(node.Index, resolvedType);
            }
        }

        return node;
    }

    private IEnumerable<TypeReference> GetTypes()
    {
        foreach (var type in types)
        {
            foreach (var nestedType in GetTypes(type))
            {
                yield return nestedType;
            }
        }
    }

    private IEnumerable<TypeReference> GetTypes(TypeReference type)
    {
        yield return type;
        if (type is TypeDefinition typeDefinition)
        {
            foreach (var nestedType in typeDefinition.NestedTypes)
            {
                foreach (var tt in GetTypes(nestedType))
                {
                    yield return tt;
                }
            }
        }
    }

    public bool TryMatchType(ReadOnlySpan<char> nodeName, out int position, [NotNullWhen(true)]out TypeReference? resolvedType)
    {
        var candidates = new List<(int Position, TypeReference candidate)>();
        foreach (var type in GetTypes())
        {
            if (TryMatchType(type, nodeName, out position, out resolvedType))
            {
                candidates.Add((position, resolvedType));
                //return true;
            }
        }

        candidates.Sort((x, y) => y.Position - x.Position);
        if (candidates.Count > 0)
        {
            (position, resolvedType) = candidates[0];
            return true;
        }

        resolvedType = null;
        position = 0;
        return false;
    }

    private bool IsMatch(ReadOnlySpan<char> candidateName, ReadOnlySpan<char> nodeName, out int position)
    {
        if (nodeName.StartsWith(candidateName))
        {
            position = candidateName.Length;
            return true;
        }

        position = 0;
        return false;
    }

    private IEnumerable<string> GenerateTypeVariants(TypeReference type)
    {
        var assemblyName = type.Scope.Name.Replace("System.Private.", "S.P.");
        var typeName = type.FullName;
        yield return MangleAssemblyName(assemblyName) + "_" + MangleTypeName(typeName);
        yield return MangleTypeName(typeName);
        yield return typeName.Replace("/", "+");
        yield return $"[{assemblyName}]{typeName.Replace("/", "+")}";
        yield return typeName;
    }

    private bool TryMatchType(TypeReference type, ReadOnlySpan<char> nodeName, out int position, [NotNullWhen(true)]out TypeReference? resolvedType)
    {
        foreach (var variant in GenerateTypeVariants(type))
        {
            if (IsMatch(variant, nodeName, out var matchPosition))
            {
                if (matchPosition == nodeName.Length || !type.HasGenericParameters)
                {
                    resolvedType = type;
                    position = matchPosition;
                    return true;
                }
                else
                {
                    if (type.HasGenericParameters)
                    {
                        if (nodeName[matchPosition] == '<' || nodeName[matchPosition] == '_')
                        {
                            char endGenericsList = nodeName[matchPosition] == '<' ? '>' : '_';
                            char separatorGenericsList = nodeName[matchPosition] == '<' ? ',' : '_';
                            var gt = new GenericInstanceType(type);
                            var expectedGenericParameters = type.GenericParameters.Count;
                            matchPosition++;
                            ReadOnlySpan<char> leftover = nodeName[matchPosition..];
                            for (var currentGenericArgument = 0; currentGenericArgument < type.GenericParameters.Count; currentGenericArgument++)
                            {
                                if (!TryMatchType(leftover, out int genericParameterPosition, out var genericParameter))
                                {
                                    goto end_generic_lookup;
                                }

                                if (currentGenericArgument == type.GenericParameters.Count - 1 && leftover[genericParameterPosition] == endGenericsList)
                                {
                                    gt.GenericArguments.Add(genericParameter);
                                    matchPosition += genericParameterPosition + 1;
                                    leftover = nodeName[matchPosition..];

                                    resolvedType = gt;
                                    position = matchPosition;
                                    return true;
                                }

                                if (currentGenericArgument != type.GenericParameters.Count - 1 && leftover[genericParameterPosition] == separatorGenericsList)
                                {
                                    gt.GenericArguments.Add(genericParameter);
                                    matchPosition += genericParameterPosition + 1;
                                    leftover = nodeName[matchPosition..];
                                    continue;
                                }

                                goto end_generic_lookup;
                            }
                        }
                    }
                }

            end_generic_lookup:
                ;
            }
        }

        resolvedType = null;
        position = 0;
        return false;
    }

    private static string MangleTypeName(string originalType)
    {
        return originalType.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("`", "_").Replace("/", "_");
    }

    private static string MangleAssemblyName(string originalAssembly)
    {
        return originalAssembly.Replace(".", "_");
    }
}

public class TypeNode : Node
{
    internal TypeNode(int id, TypeReference typeReference)
        : base(id, typeReference.FullName)
    {
        TypeReference = typeReference;
    }

    public TypeReference TypeReference { get; }
}
