using Mono.Cecil;
using System.Diagnostics.CodeAnalysis;

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

            if (node.Name[position] == '.' || node.Name[position] == '_')
            {
                var leftover = node.Name[position] == '.'
                    ? node.Name.AsSpan()[(position + 1)..]
                    : node.Name.AsSpan()[(position + 2)..];
                if (TryMatchMember(resolvedType, leftover, out var memberPosition, out var memberReference))
                {
                    if (memberReference is FieldReference fieldReference && memberPosition == leftover.Length)
                    {
                        return new FieldNode(node.Index, fieldReference);
                    }

                    if (memberReference is MethodReference methodReference && memberPosition == leftover.Length)
                    {
                        return new MethodNode(node.Index, methodReference);
                    }
                }
            }
        }

        return node;
    }

    private bool TryMatchMember(TypeReference type, ReadOnlySpan<char> content, out int position, [NotNullWhen(true)]out MemberReference? memberReference)
    {
        var candidates = new List<(int Position, MemberReference candidate)>();
        if (type is TypeDefinition typeDefinition)
        {
            foreach (var field in typeDefinition.Fields)
            {
                if (content.StartsWith(field.Name))
                {
                    candidates.Add((field.Name.Length, field));
                }
            }

            foreach (var field in typeDefinition.Methods)
            {
                if (content.StartsWith(field.Name) || content.StartsWith(field.Name.Replace('.', '_')))
                {
                    if (content.Length == field.Name.Length)
                    {
                        candidates.Add((field.Name.Length, field));
                    }
                    else
                    {
                        if (content[field.Name.Length] == '(')
                        {
                            candidates.Add((content.IndexOf(')') + 1, field));
                        }
                    }
                }
            }
        }

        candidates.Sort((x, y) => y.Position - x.Position);
        if (candidates.Count > 0)
        {
            (position, memberReference) = candidates[0];
            return true;
        }

        position = 0;
        memberReference = null;
        return false;
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

    private static string GetFullName(TypeReference type)
    {
        var fullName = string.IsNullOrEmpty(type.Namespace)
            ? type.Name
            : type.Namespace + '.' + type.Name;

        if (type.IsNested)
            fullName = GetFullName(type.DeclaringType) + "+" + fullName;

        return fullName;
    }

    private IEnumerable<string> GenerateTypeVariants(TypeReference type, bool bracketed)
    {
        if (bracketed)
        {
            var typeName = GetFullName(type);
            var assemblyName = type.Scope.Name.Replace("System.Private.", "S.P.");
            yield return $"[{assemblyName}]{typeName}";
        }
        else
        {
            var typeName = GetFullName(type);
            yield return typeName;
            var mangledTypeName = MangleTypeName(typeName);
            yield return mangledTypeName;
            var assemblyName = type.Scope.Name.Replace("System.Private.", "S.P.");
            yield return MangleAssemblyName(assemblyName) + "_" + mangledTypeName;
        }
    }

    private bool TryMatchType(TypeReference type, ReadOnlySpan<char> nodeName, out int position, [NotNullWhen(true)]out TypeReference? resolvedType)
    {
        foreach (var variant in GenerateTypeVariants(type, nodeName[0] == '['))
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
        return originalType.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("`", "_").Replace("+", "_");
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

public class FieldNode : Node
{
    internal FieldNode(int id, FieldReference fieldReference)
        : base(id, fieldReference.FullName)
    {
        FieldReference = fieldReference;
    }

    public FieldReference FieldReference { get; }
}

public class MethodNode : Node
{
    internal MethodNode(int id, MethodReference methodReference)
        : base(id, methodReference.FullName)
    {
        MethodReference = methodReference;
    }

    public MethodReference MethodReference { get; }
}
