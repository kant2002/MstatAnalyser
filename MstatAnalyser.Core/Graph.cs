﻿using System.Xml;

namespace MstatAnalyser.Core;

public class Node
{
    public readonly int Index;
    public readonly string Name;
    public readonly Dictionary<Node, List<string>> Targets = new Dictionary<Node, List<string>>();
    public readonly Dictionary<Node, List<string>> Sources = new Dictionary<Node, List<string>>();

    public Node(int index, string name)
    {
        Index = index;
        Name = name;
    }

    public override string ToString()
    {
        return $"Index: {Index}, Name: {Name}";
    }
}

public class Graph
{
    public int NextConditionalNodeIndex = int.MaxValue;
    public int PID;
    public int ID;
    public string Name;
    public Dictionary<int, Node> Nodes = new Dictionary<int, Node>();

    public override string ToString()
    {
        return $"PID: {PID}, ID: {ID}, Name: {Name}";
    }

    public bool AddEdge(int source, int target, string reason)
    {
        if (Nodes.TryGetValue(source, out var a) && Nodes.TryGetValue(target, out var b))
        {
            AddReason(a.Targets, b, reason);
            AddReason(b.Sources, a, reason);
        }
        else
        {
            return false;
        }
        return true;
    }

    public void AddConditionalEdge(int reason1, int reason2, int target, string reason)
    {
        Node reason1Node = Nodes[reason1];
        Node reason2Node = Nodes[reason2];
        Node dependee = Nodes[target];

        int conditionalNodeIndex = NextConditionalNodeIndex--;
        Node conditionalNode = new Node(conditionalNodeIndex, string.Format("Conditional({0} - {1})", reason1Node.ToString(), reason2Node.ToString()));
        Nodes.Add(conditionalNodeIndex, conditionalNode);

        AddReason(conditionalNode.Targets, dependee, reason);
        AddReason(dependee.Sources, conditionalNode, reason);

        AddReason(reason1Node.Targets, conditionalNode, "Reason1Conditional - " + reason);
        AddReason(conditionalNode.Sources, reason1Node, "Reason1Conditional - " + reason);

        AddReason(reason2Node.Targets, conditionalNode, "Reason2Conditional - " + reason);
        AddReason(conditionalNode.Sources, reason2Node, "Reason2Conditional - " + reason);
    }

    public void AddNode(int index, string name)
    {
        Node n = new Node(index, name);
        this.Nodes.Add(index, n);
    }

    public void AddReason(Dictionary<Node, List<string>> dict, Node node, string reason)
    {
        if (dict.TryGetValue(node, out List<string> reasons))
        {
            reasons.Add(reason);
        }
        else
        {
            dict.Add(node, new List<string> { reason });
        }
    }
}

public class DGMLGraphProcessing
{
    internal bool ParseXML(string name, Stream fileStream, out Graph g)
    {
        var settings = new XmlReaderSettings();
        settings.IgnoreWhitespace = true;

        g = new Graph();

        using (var reader = XmlReader.Create(fileStream, settings))
        {
            while (reader.Read())
            {
                if (reader.Name == "Property" || reader.Name == "Properties")
                    continue;

                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                switch (reader.Name)
                {
                    case "Node":
                        int id = int.Parse(reader.GetAttribute("Id"));
                        g.AddNode(id, reader.GetAttribute("Label"));
                        break;
                    case "Link":
                        int source = int.Parse(reader.GetAttribute("Source"));
                        int target = int.Parse(reader.GetAttribute("Target"));
                        if (!g.AddEdge(source, target, reader.GetAttribute("Reason")))
                        {
                            return false;
                        }
                        break;
                    case "DirectedGraph":
                        g.Name = name;
                        break;
                }
            }
        }

        fileStream.Close();
        return true;
    }

    public static Node ParseNode(int id, string label)
    {
        const string RegionStartMarker = "Region ";
        if (label.StartsWith(RegionStartMarker))
        {
            return new RegionNode(id, label.Substring(RegionStartMarker.Length));
        }

        const string ReflectableModuleStartMarker = "Reflectable module: ";
        if (label.StartsWith(ReflectableModuleStartMarker))
        {
            return new ModuleMetadataNode(id, label.Substring(ReflectableModuleStartMarker.Length));
        }

        const string ReflectedMethodStartMarker = "Reflectable method: ";
        if (label.StartsWith(ReflectedMethodStartMarker))
        {
            return new ReflectedMethodNode(id, label.Substring(ReflectedMethodStartMarker.Length));
        }

        const string ReflectedFieldStartMarker = "Reflectable field: ";
        if (label.StartsWith(ReflectedFieldStartMarker))
        {
            return new ReflectedFieldNode(id, label.Substring(ReflectedFieldStartMarker.Length));
        }
        
        const string ReflectedTypeStartMarker = "Reflectable type: ";
        if (label.StartsWith(ReflectedTypeStartMarker))
        {
            return new ReflectedTypeNode(id, label.Substring(ReflectedTypeStartMarker.Length));
        }

        const string VTableSliceStartMarker = "__vtable_";
        if (label.StartsWith(VTableSliceStartMarker))
        {
            return new VTableSliceNode(id, label.Substring(VTableSliceStartMarker.Length));
        }

        const string SealedVTableStartMarker = "__SealedVTable_";
        if (label.StartsWith(SealedVTableStartMarker))
        {
            var (mangledMethodTable, _) = ParseMangledName(label.Substring(SealedVTableStartMarker.Length));
            return new SealedVTableNode(id, mangledMethodTable);
        }

        const string GenericDictionaryStartMarker = "__GenericDict_";
        if (label.StartsWith(GenericDictionaryStartMarker))
        {
            return new GenericDictionaryNode(id, label.Substring(GenericDictionaryStartMarker.Length));
        }

        const string DictionaryLayoutStartMarker = "Dictionary layout for ";
        if (label.StartsWith(DictionaryLayoutStartMarker))
        {
            return new DictionaryLayoutNode(id, label.Substring(DictionaryLayoutStartMarker.Length));
        }

        const string InterfaceDispatchMapStartMarker = "__InterfaceDispatchMap_";
        if (label.StartsWith(InterfaceDispatchMapStartMarker))
        {
            return new InterfaceDispatchMapNode(id, label.Substring(InterfaceDispatchMapStartMarker.Length));
        }

        const string FieldMetadataStartMarker = "Field metadata: ";
        if (label.StartsWith(FieldMetadataStartMarker))
        {
            return new FieldMetadataNode(id, label.Substring(FieldMetadataStartMarker.Length));
        }

        const string WritableDataStartMarker = "__writableData";
        if (label.StartsWith(WritableDataStartMarker))
        {
            return new WritableDataNode(id, label.Substring(WritableDataStartMarker.Length));
        }

        const string EETypeOptionalFieldsStartMarker = "__optionalfields_";
        if (label.StartsWith(EETypeOptionalFieldsStartMarker))
        {
            var (mangledMethodTable, isBoxed) = ParseMangledName(label.Substring(EETypeOptionalFieldsStartMarker.Length));
            return new EETypeOptionalFieldsNode(id, mangledMethodTable);
        }

        const string MethodMetadataStartMarker = "Method metadata: ";
        if (label.StartsWith(MethodMetadataStartMarker))
        {
            return new MethodMetadataNode(id, label.Substring(MethodMetadataStartMarker.Length));
        }

        const string GenericCompositionStartMarker = "__GenericInstance";
        if (label.StartsWith(GenericCompositionStartMarker))
        {
            var nomarker = label.Substring(GenericCompositionStartMarker.Length);
            //if (nomarker.StartsWith('_'))
            {
                var parts = nomarker.Substring(1).Split("__Variance__");
                if (parts.Length == 1)
                    return new GenericCompositionNode(id, parts[0], Array.Empty<int>());
                return new GenericCompositionNode(id, parts[0], parts[1].Split('_', StringSplitOptions.RemoveEmptyEntries).Select(variance => int.Parse(variance)).ToArray());
            }
        }

        const string CustomAttributeMetadataStartMarker = "Reflectable custom attribute ";
        if (label.StartsWith(CustomAttributeMetadataStartMarker))
        {
            var parts = label.Substring(CustomAttributeMetadataStartMarker.Length).Split(" in ");
            return new CustomAttributeMetadataNode(id, parts[0], parts[1]);
        }

        if (label.StartsWith("??_7") && label.EndsWith("@@6B@"))
        {
            var (mangledMethodTable, isBoxed) = ParseMangledName(label);
            return new MethodTableNode(id, mangledMethodTable, isBoxed);
        }

        const string ConstructedEETypeEndMarker = " constructed";
        if (label.EndsWith(ConstructedEETypeEndMarker))
        {
            return new ConstructedEETypeNode(id, label.Substring(0, label.Length - ConstructedEETypeEndMarker.Length));
        }

        return new Node(id, label);
    }

    private static (string UnmangledName, bool IsBoxed) ParseMangledName(string label)
    {
        var mangledMethodTable = label.Substring(4, label.Length - 9);
        const string BoxedStartMarker = "Boxed_";
        if (mangledMethodTable.StartsWith(BoxedStartMarker))
            return (mangledMethodTable.Substring(BoxedStartMarker.Length), true);

        return (mangledMethodTable, false);
    }
}

public class RegionNode : Node
{
    public RegionNode(int id, string regionName)
        : base(id, regionName)
    {
    }
}

public class ModuleMetadataNode : Node
{
    public ModuleMetadataNode(int id, string assemblyFullName)
        : base(id, assemblyFullName)
    {
    }
}

public class ConstructedEETypeNode : Node
{
    public ConstructedEETypeNode(int id, string mangledName)
        : base(id, mangledName)
    {
    }
}

public class ReflectedMethodNode : Node
{
    public ReflectedMethodNode(int id, string methodFullName)
        : base(id, methodFullName)
    {
    }
}

public class ReflectedFieldNode : Node
{
    public ReflectedFieldNode(int id, string methodFullName)
        : base(id, methodFullName)
    {
    }
}

public class MethodTableNode : Node
{
    public MethodTableNode(int id, string methodFullName, bool isBoxed)
        : base(id, methodFullName)
    {
        IsBoxed = isBoxed;
    }

    public bool IsBoxed { get; }
}

public class ReflectedTypeNode : Node
{
    public ReflectedTypeNode(int id, string methodFullName)
        : base(id, methodFullName)
    {
    }

    public bool IsBoxed { get; }
}

public class CustomAttributeMetadataNode : Node
{
    public CustomAttributeMetadataNode(int id, string attributeTypeName, string assemblyName)
        : base(id, attributeTypeName)
    {
        AssemblyName = assemblyName;
    }

    public string AssemblyName { get; }
}

public class VTableSliceNode : Node
{
    public VTableSliceNode(int id, string typeName)
        : base(id, typeName)
    {
    }
}

public class InterfaceDispatchMapNode : Node
{
    public InterfaceDispatchMapNode(int id, string typeName)
        : base(id, typeName)
    {
    }
}

public class SealedVTableNode : Node
{
    public SealedVTableNode(int id, string typeName)
        : base(id, typeName)
    {
    }
}

public class GenericDictionaryNode : Node
{
    public GenericDictionaryNode(int id, string typeName)
        : base(id, typeName)
    {
    }
}

public class DictionaryLayoutNode : Node
{
    public DictionaryLayoutNode(int id, string typeName)
        : base(id, typeName)
    {
    }
}

public class FieldMetadataNode : Node
{
    public FieldMetadataNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class GenericCompositionNode : Node
{
    public GenericCompositionNode(int id, string typeName, int[] variance)
        : base(id, typeName)
    {
        Variance = variance;
    }

    public int[] Variance { get; }
}

public class WritableDataNode : Node
{
    public WritableDataNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class EETypeOptionalFieldsNode : Node
{
    public EETypeOptionalFieldsNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class MethodMetadataNode : Node
{
    public MethodMetadataNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}
