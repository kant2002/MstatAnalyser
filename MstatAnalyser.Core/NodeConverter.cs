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
        foreach (var type in types)
        {
            if (TryConvertNode(type, node, out var convertedNode))
            {
                return convertedNode;
            }
        }

        return node;
    }

    private bool TryConvertNode(TypeReference type, Node node, [NotNullWhen(true)]out Node? convertedNode)
    {
        var assemblyName = type.Scope.Name;
        var typeName = type.FullName;
        if (MangleAssemblyName(assemblyName.Replace(".", "_")) + "_" + MangleTypeName(typeName) == node.Name)
        {
            convertedNode = new TypeNode(node.Index, type);
            return true;
        }

        if (MangleTypeName(typeName) == node.Name)
        {
            convertedNode = new TypeNode(node.Index, type);
            return true;
        }

        if (typeName.Replace("/", "+") == node.Name)
        {
            convertedNode = new TypeNode(node.Index, type);
            return true;
        }

        if ($"[{assemblyName}]{typeName.Replace("/", "+")}" == node.Name)
        {
            convertedNode = new TypeNode(node.Index, type);
            return true;
        }

        if (typeName == node.Name)
        {
            convertedNode = new TypeNode(node.Index, type);
            return true;
        }

        if (type is TypeDefinition typeDefinition)
        {
            foreach (var nestedType in typeDefinition.NestedTypes)
            {
                if (TryConvertNode(nestedType, node, out convertedNode))
                {
                    return true;
                }
            }
        }

        convertedNode = null;
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
