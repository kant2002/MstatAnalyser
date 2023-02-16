using System;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Xml;

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
        if (Nodes.TryGetValue(source, out var sourceNode) && Nodes.TryGetValue(target, out var targetNode))
        {
            AddReason(sourceNode.Targets, targetNode, reason);
            //AddReason(targetNode.Sources, sourceNode, reason);
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

    public void AddNode(int index, Node node)
    {
        this.Nodes.Add(index, node);
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
                        var node = DGMLGraphProcessing.ParseNode(id, reader.GetAttribute("Label"));
                        g.AddNode(id, node);
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
            var (mangledMethodTable, _, unboxingThunk) = ParseMangledName(label.Substring(SealedVTableStartMarker.Length));
            Debug.Assert(unboxingThunk == false);
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
            var (mangledMethodTable, isBoxed, unboxingThunk) = ParseMangledName(label.Substring(EETypeOptionalFieldsStartMarker.Length));
            Debug.Assert(unboxingThunk == false);
            return new EETypeOptionalFieldsNode(id, mangledMethodTable);
        }

        const string MethodMetadataStartMarker = "Method metadata: ";
        if (label.StartsWith(MethodMetadataStartMarker))
        {
            return new MethodMetadataNode(id, label.Substring(MethodMetadataStartMarker.Length));
        }

        const string SimpleEmbeddedPointerIndirectionStartMarker = "Embedded pointer to ";
        if (label.StartsWith(SimpleEmbeddedPointerIndirectionStartMarker))
        {
            return new SimpleEmbeddedPointerIndirectionNode(id, label.Substring(SimpleEmbeddedPointerIndirectionStartMarker.Length));
        }

        const string VirtualMethodUseStartMarker = "VirtualMethodUse ";
        if (label.StartsWith(VirtualMethodUseStartMarker))
        {
            return new VirtualMethodUseNode(id, label.Substring(VirtualMethodUseStartMarker.Length));
        }

        const string GCStaticEETypeStartMarker = "__GCStaticEEType_";
        if (label.StartsWith(GCStaticEETypeStartMarker))
        {
            return new GCStaticEETypeNode(id, label.Substring(GCStaticEETypeStartMarker.Length));
        }

        const string TentativeInstanceMethodStartMarker = "Tentative instance method: ";
        if (label.StartsWith(TentativeInstanceMethodStartMarker))
        {
            return new TentativeInstanceMethodNode(id, label.Substring(TentativeInstanceMethodStartMarker.Length));
        }

        const string NativeLayoutTemplateMethodLayoutVertexStartMarker = "NativeLayoutTemplateTypeLayoutVertexNode_";
        if (label.StartsWith(NativeLayoutTemplateMethodLayoutVertexStartMarker))
        {
            return new NativeLayoutTemplateMethodLayoutVertexNode(id, label.Substring(NativeLayoutTemplateMethodLayoutVertexStartMarker.Length));
        }

        const string NativeLayoutTypeSignatureVertexStartMarker = "NativeLayoutTypeSignatureVertexNode: ";
        if (label.StartsWith(NativeLayoutTypeSignatureVertexStartMarker))
        {
            return new NativeLayoutTypeSignatureVertexNode(id, label.Substring(NativeLayoutTypeSignatureVertexStartMarker.Length));
        }

        const string NativeLayoutMethodNameAndSignatureVertexStartMarker = "NativeLayoutMethodNameAndSignatureVertexNode";
        if (label.StartsWith(NativeLayoutMethodNameAndSignatureVertexStartMarker))
        {
            return new NativeLayoutMethodNameAndSignatureVertexNode(id, label.Substring(NativeLayoutMethodNameAndSignatureVertexStartMarker.Length));
        }

        const string NativeLayoutMethodSignatureVertexStartMarker = "NativeLayoutMethodSignatureVertexNode ";
        if (label.StartsWith(NativeLayoutMethodSignatureVertexStartMarker))
        {
            return new NativeLayoutMethodSignatureVertexNode(id, label.Substring(NativeLayoutMethodSignatureVertexStartMarker.Length));
        }

        const string FrozenObjectStartMarker = "__FrozenObj_";
        if (label.StartsWith(FrozenObjectStartMarker))
        {
            return new FrozenObjectNode(id, label.Substring(FrozenObjectStartMarker.Length));
        }

        const string FatFunctionPointerStartMarker = "__fatpointer_";
        if (label.StartsWith(FatFunctionPointerStartMarker))
        {
            return new FatFunctionPointerNode(id, label.Substring(FatFunctionPointerStartMarker.Length), false);
        }

        const string FatFunctionPointerUnboxingStubStartMarker = "__fatunboxpointer_";
        if (label.StartsWith(FatFunctionPointerUnboxingStubStartMarker))
        {
            return new FatFunctionPointerNode(id, label.Substring(FatFunctionPointerUnboxingStubStartMarker.Length), true);
        }

        const string RuntimeMethodHandleStartMarker = "__RuntimeMethodHandle_";
        if (label.StartsWith(RuntimeMethodHandleStartMarker))
        {
            return new RuntimeMethodHandleNode(id, label.Substring(RuntimeMethodHandleStartMarker.Length));
        }

        const string TypeGVMEntriesStartMarker = "__TypeGVMEntriesNode_";
        if (label.StartsWith(TypeGVMEntriesStartMarker))
        {
            return new TypeGVMEntriesNode(id, label[TypeGVMEntriesStartMarker.Length..]);
        }

        const string GCStaticsStartMarker = "?__GCSTATICS@";
        if (label.StartsWith(GCStaticsStartMarker))
        {
            var leftover = label.Substring(GCStaticsStartMarker.Length);
            const string PreinitEndMarker = "@@__PreInitData";
            if (leftover.EndsWith(PreinitEndMarker))
                return new GCStaticsPreInitDataNode(id, leftover[..^PreinitEndMarker.Length]);
            return new GCStaticsNode(id, leftover.TrimEnd('@'));
        }

        const string NonGCStaticsStartMarker = "?__NONGCSTATICS@";
        if (label.StartsWith(NonGCStaticsStartMarker))
        {
            return new NonGCStaticsNode(id, label.Substring(NonGCStaticsStartMarker.Length).TrimEnd('@'));
        }

        const string NativeLayoutTemplateMethodSignatureVertexStartMarker = "NativeLayoutTemplateMethodSignatureVertexNode_";
        if (label.StartsWith(NativeLayoutTemplateMethodSignatureVertexStartMarker))
        {
            return new NativeLayoutTemplateMethodSignatureVertexNode(id, label.Substring(NativeLayoutTemplateMethodSignatureVertexStartMarker.Length));
        }

        const string NativeLayoutMethodLdTokenVertexStartMarker = "NativeLayoutMethodLdTokenVertexNode_";
        if (label.StartsWith(NativeLayoutMethodLdTokenVertexStartMarker))
        {
            return new NativeLayoutMethodLdTokenVertexNode(id, label.Substring(NativeLayoutMethodLdTokenVertexStartMarker.Length));
        }

        const string NativeLayoutFieldLdTokenVertexStartMarker = "NativeLayoutFieldLdTokenVertexNode_";
        if (label.StartsWith(NativeLayoutFieldLdTokenVertexStartMarker))
        {
            return new NativeLayoutFieldLdTokenVertexNode(id, label.Substring(NativeLayoutFieldLdTokenVertexStartMarker.Length));
        }

        const string NativeLayoutExternalReferenceVertexStartMarker = "NativeLayoutISymbolNodeReferenceVertexNode ";
        if (label.StartsWith(NativeLayoutExternalReferenceVertexStartMarker))
        {
            return new NativeLayoutExternalReferenceVertexNode(id, label.Substring(NativeLayoutExternalReferenceVertexStartMarker.Length));
        }

        if (label == "NativeLayoutPlacedVertexSequenceVertexNode")
        {
            return new NativeLayoutPlacedVertexSequenceOfUIntVertexNode(id);
        }

        const string NativeLayoutDictionarySignatureStartMarker = "Dictionary layout signature for ";
        if (label.StartsWith(NativeLayoutDictionarySignatureStartMarker))
        {
            return new NativeLayoutDictionarySignatureNode(id, label.Substring(NativeLayoutDictionarySignatureStartMarker.Length));
        }

        const string NativeLayoutTypeHandleGenericDictionarySlotStartMarker = "NativeLayoutTypeHandleGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutTypeHandleGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutTypeHandleGenericDictionarySlotNode(id, label.Substring(NativeLayoutTypeHandleGenericDictionarySlotStartMarker.Length));
        }

        const string NativeLayoutUnwrapNullableGenericDictionarySlotStartMarker = "NativeLayoutUnwrapNullableGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutUnwrapNullableGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutUnwrapNullableGenericDictionarySlotNode(id, label.Substring(NativeLayoutUnwrapNullableGenericDictionarySlotStartMarker.Length));
        }

        const string NativeLayoutAllocateObjectGenericDictionarySlotStartMarker = "NativeLayoutAllocateObjectGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutAllocateObjectGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutAllocateObjectGenericDictionarySlotNode(id, label.Substring(NativeLayoutAllocateObjectGenericDictionarySlotStartMarker.Length));
        }

        const string NativeLayoutThreadStaticBaseIndexDictionarySlotStartMarker = "NativeLayoutThreadStaticBaseIndexDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutThreadStaticBaseIndexDictionarySlotStartMarker))
        {
            return new NativeLayoutThreadStaticBaseIndexDictionarySlotNode(id, label.Substring(NativeLayoutThreadStaticBaseIndexDictionarySlotStartMarker.Length));
        }

        const string NativeLayoutDefaultConstructorGenericDictionarySlotStartMarker = "NativeLayoutDefaultConstructorGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutDefaultConstructorGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutDefaultConstructorGenericDictionarySlotNode(id, label.Substring(NativeLayoutDefaultConstructorGenericDictionarySlotStartMarker.Length));
        }

        const string NativeLayoutGcStaticsGenericDictionarySlotStartMarker = "NativeLayoutGcStaticsGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutGcStaticsGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutGcStaticsGenericDictionarySlotNode(id, label.Substring(NativeLayoutGcStaticsGenericDictionarySlotStartMarker.Length));
        }

        const string NativeLayoutNonGcStaticsGenericDictionarySlotStartMarker = "NativeLayoutNonGcStaticsGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutNonGcStaticsGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutNonGcStaticsGenericDictionarySlotNode(id, label[NativeLayoutNonGcStaticsGenericDictionarySlotStartMarker.Length..]);
        }

        const string NativeLayoutInterfaceDispatchGenericDictionarySlotStartMarker = "NativeLayoutInterfaceDispatchGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutInterfaceDispatchGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutInterfaceDispatchGenericDictionarySlotNode(id, label[NativeLayoutInterfaceDispatchGenericDictionarySlotStartMarker.Length..]);
        }

        const string NativeLayoutMethodDictionaryGenericDictionarySlotStartMarker = "NativeLayoutMethodDictionaryGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutMethodDictionaryGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutMethodDictionaryGenericDictionarySlotNode(id, label[NativeLayoutMethodDictionaryGenericDictionarySlotStartMarker.Length..]);
        }

        const string WrappedMethodDictionaryVertexStartMarker = "WrappedMethodEntryVertexNodeForDictionarySlot_";
        if (label.StartsWith(WrappedMethodDictionaryVertexStartMarker))
        {
            return new WrappedMethodDictionaryVertexNode(id, label[WrappedMethodDictionaryVertexStartMarker.Length..]);
        }

        const string NativeLayoutFieldLdTokenGenericDictionarySlotStartMarker = "NativeLayoutFieldLdTokenGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutFieldLdTokenGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutFieldLdTokenGenericDictionarySlotNode(id, label[NativeLayoutFieldLdTokenGenericDictionarySlotStartMarker.Length..]);
        }

        const string NativeLayoutMethodLdTokenGenericDictionarySlotStartMarker = "NativeLayoutMethodLdTokenGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutMethodLdTokenGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutMethodLdTokenGenericDictionarySlotNode(id, label[NativeLayoutMethodLdTokenGenericDictionarySlotStartMarker.Length..]);
        }

        const string NativeLayoutConstrainedMethodDictionarySlotStartMarker = "NativeLayoutConstrainedMethodDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutConstrainedMethodDictionarySlotStartMarker))
        {
            Debug.Assert(false);
            return new NativeLayoutConstrainedMethodDictionarySlotNode(id, label[NativeLayoutConstrainedMethodDictionarySlotStartMarker.Length..]);
        }

        const string NativeLayoutMethodEntrypointGenericDictionarySlotStartMarker = "NativeLayoutMethodEntrypointGenericDictionarySlotNode_";
        if (label.StartsWith(NativeLayoutMethodEntrypointGenericDictionarySlotStartMarker))
        {
            return new NativeLayoutMethodEntrypointGenericDictionarySlotNode(id, label[NativeLayoutMethodEntrypointGenericDictionarySlotStartMarker.Length..]);
        }

        const string WrappedMethodEntryVertexStartMarker = "WrappedMethodEntryVertexNodeForDictionarySlot_";
        if (label.StartsWith(WrappedMethodEntryVertexStartMarker))
        {
            Debug.Assert(false);
            return new WrappedMethodEntryVertexNode(id, label[WrappedMethodEntryVertexStartMarker.Length..]);
        }

        if (label == "NativeLayoutNotSupportedDictionarySlotNode")
        {
            return new NativeLayoutNotSupportedDictionarySlotNode(id);
        }

        const string GVMDependenciesStartMarker = "__GVMDependenciesNode_";
        if (label.StartsWith(GVMDependenciesStartMarker))
        {
            return new GVMDependenciesNode(id, label[GVMDependenciesStartMarker.Length..]);
        }

        const string VariantInterfaceMethodUseStartMarker = "VariantInterfaceMethodUse ";
        if (label.StartsWith(VariantInterfaceMethodUseStartMarker))
        {
            return new VariantInterfaceMethodUseNode(id, label[VariantInterfaceMethodUseStartMarker.Length..]);
        }

        const string DataflowAnalyzedMethodStartMarker = "Dataflow analysis for ";
        if (label.StartsWith(DataflowAnalyzedMethodStartMarker))
        {
            return new DataflowAnalyzedMethodNode(id, label[DataflowAnalyzedMethodStartMarker.Length..]);
        }

        const string ReadyToRunGenericHelperNodeStartMarker = "__GenericLookupFromDict_";
        if (label.StartsWith(ReadyToRunGenericHelperNodeStartMarker))
        {
            return new ReadyToRunGenericHelperNode(id, label[ReadyToRunGenericHelperNodeStartMarker.Length..]);
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

        const string ShadowConcreteMethodMarker = " backed by ";
        if (label.Contains(ShadowConcreteMethodMarker))
        {
            var parts = label.Split(ShadowConcreteMethodMarker);
            return new ShadowConcreteMethodNode(id, parts[0], parts[1]);
        }

        const string CustomAttributeMetadataStartMarker = "Reflectable custom attribute ";
        if (label.StartsWith(CustomAttributeMetadataStartMarker))
        {
            var parts = label.Substring(CustomAttributeMetadataStartMarker.Length).Split(" in ");
            return new CustomAttributeMetadataNode(id, parts[0], parts[1]);
        }

        const string CInterfaceDispatchCellStartMarker = "__InterfaceDispatchCell_";
        if (label.StartsWith(CInterfaceDispatchCellStartMarker))
        {
            var parts = label.Substring(CInterfaceDispatchCellStartMarker.Length).Split("___GenericDict_");
            Debug.Assert(parts.Length <= 2);
            return new InterfaceDispatchCellNode(id, parts[0], parts.ElementAtOrDefault(1));
        }

        if (label.StartsWith("??_7") && label.EndsWith("@@6B@"))
        {
            var (mangledMethodTable, isBoxed, unboxingThunk) = ParseMangledName(label);
            Debug.Assert(unboxingThunk == false);
            return new MethodTableNode(id, mangledMethodTable, isBoxed);
        }

        const string ConstructedEETypeEndMarker = " constructed";
        if (label.EndsWith(ConstructedEETypeEndMarker))
        {
            return new ConstructedEETypeNode(id, label.Substring(0, label.Length - ConstructedEETypeEndMarker.Length));
        }

        if (label == "NativeLayoutPlacedSignatureVertexNode")
        {
            return new NativeLayoutPlacedSignatureVertexNode(id);
        }

        return new Node(id, label);
    }

    private static (string UnmangledName, bool IsBoxedValueType, bool UnboxingThunk) ParseMangledName(string label)
    {
        var mangledMethodTable = label.Substring(4, label.Length - 9);
        const string BoxedStartMarker = "Boxed_";
        if (mangledMethodTable.StartsWith(BoxedStartMarker))
            return (mangledMethodTable.Substring(BoxedStartMarker.Length), true, false);

        const string UnboxingStartMarker = "unbox_";
        if (mangledMethodTable.StartsWith(UnboxingStartMarker))
            return (mangledMethodTable.Substring(UnboxingStartMarker.Length), false, true);

        return (mangledMethodTable, false, false);
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
        IsBoxedValueType = isBoxed;
    }

    public bool IsBoxedValueType { get; }
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

public class SimpleEmbeddedPointerIndirectionNode : Node
{
    public SimpleEmbeddedPointerIndirectionNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class VirtualMethodUseNode : Node
{
    public VirtualMethodUseNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class GCStaticEETypeNode : Node
{
    public GCStaticEETypeNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class TentativeInstanceMethodNode : Node
{
    public TentativeInstanceMethodNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutTemplateMethodLayoutVertexNode : Node
{
    public NativeLayoutTemplateMethodLayoutVertexNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutTypeSignatureVertexNode : Node
{
    public NativeLayoutTypeSignatureVertexNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutPlacedSignatureVertexNode : Node
{
    public NativeLayoutPlacedSignatureVertexNode(int id)
        : base(id, "NativeLayoutPlacedSignatureVertexNode")
    {
    }
}

public class NativeLayoutMethodNameAndSignatureVertexNode : Node
{
    public NativeLayoutMethodNameAndSignatureVertexNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutMethodSignatureVertexNode : Node
{
    public NativeLayoutMethodSignatureVertexNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class FrozenObjectNode : Node
{
    public FrozenObjectNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class FatFunctionPointerNode : Node
{
    public FatFunctionPointerNode(int id, string fieldName, bool isUnboxingStub)
        : base(id, fieldName)
    {
        IsUnboxingStub = isUnboxingStub;
    }

    public bool IsUnboxingStub { get; }
}

public class ShadowConcreteMethodNode : Node
{
    public ShadowConcreteMethodNode(int id, string fieldName, string canonicalMethodName)
        : base(id, fieldName)
    {
        CanonicalMethodName = canonicalMethodName;
    }

    public string CanonicalMethodName { get; }
}

public class InterfaceDispatchCellNode : Node
{
    public InterfaceDispatchCellNode(int id, string fieldName, string? callSiteIdentifier)
        : base(id, fieldName)
    {
        CallSiteIdentifier = callSiteIdentifier;
    }

    public string? CallSiteIdentifier { get; }
}

public class RuntimeMethodHandleNode : Node
{
    public RuntimeMethodHandleNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class TypeGVMEntriesNode : Node
{
    public TypeGVMEntriesNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class GCStaticsNode : Node
{
    public GCStaticsNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class GCStaticsPreInitDataNode : Node
{
    public GCStaticsPreInitDataNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NonGCStaticsNode : Node
{
    public NonGCStaticsNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutTemplateMethodSignatureVertexNode : Node
{
    public NativeLayoutTemplateMethodSignatureVertexNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutMethodLdTokenVertexNode : Node
{
    public NativeLayoutMethodLdTokenVertexNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutFieldLdTokenVertexNode : Node
{
    public NativeLayoutFieldLdTokenVertexNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutExternalReferenceVertexNode : Node
{
    public NativeLayoutExternalReferenceVertexNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutPlacedVertexSequenceOfUIntVertexNode : Node
{
    public NativeLayoutPlacedVertexSequenceOfUIntVertexNode(int id)
        : base(id, "NativeLayoutPlacedVertexSequenceOfUIntVertexNode")
    {
    }
}

public class NativeLayoutDictionarySignatureNode : Node
{
    public NativeLayoutDictionarySignatureNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutTypeHandleGenericDictionarySlotNode : Node
{
    public NativeLayoutTypeHandleGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutUnwrapNullableGenericDictionarySlotNode : Node
{
    public NativeLayoutUnwrapNullableGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutAllocateObjectGenericDictionarySlotNode : Node
{
    public NativeLayoutAllocateObjectGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutThreadStaticBaseIndexDictionarySlotNode : Node
{
    public NativeLayoutThreadStaticBaseIndexDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutDefaultConstructorGenericDictionarySlotNode : Node
{
    public NativeLayoutDefaultConstructorGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutGcStaticsGenericDictionarySlotNode : Node
{
    public NativeLayoutGcStaticsGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutNonGcStaticsGenericDictionarySlotNode : Node
{
    public NativeLayoutNonGcStaticsGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutInterfaceDispatchGenericDictionarySlotNode : Node
{
    public NativeLayoutInterfaceDispatchGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutMethodDictionaryGenericDictionarySlotNode : Node
{
    public NativeLayoutMethodDictionaryGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class WrappedMethodDictionaryVertexNode : Node
{
    public WrappedMethodDictionaryVertexNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutFieldLdTokenGenericDictionarySlotNode : Node
{
    public NativeLayoutFieldLdTokenGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutMethodLdTokenGenericDictionarySlotNode : Node
{
    public NativeLayoutMethodLdTokenGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutConstrainedMethodDictionarySlotNode : Node
{
    public NativeLayoutConstrainedMethodDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutMethodEntrypointGenericDictionarySlotNode : Node
{
    public NativeLayoutMethodEntrypointGenericDictionarySlotNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class WrappedMethodEntryVertexNode : Node
{
    public WrappedMethodEntryVertexNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class NativeLayoutNotSupportedDictionarySlotNode : Node
{
    public NativeLayoutNotSupportedDictionarySlotNode(int id)
        : base(id, "NativeLayoutNotSupportedDictionarySlotNode")
    {
    }
}

public class GVMDependenciesNode : Node
{
    public GVMDependenciesNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class VariantInterfaceMethodUseNode : Node
{
    public VariantInterfaceMethodUseNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class DataflowAnalyzedMethodNode : Node
{
    public DataflowAnalyzedMethodNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}

public class ReadyToRunGenericHelperNode : Node
{
    public ReadyToRunGenericHelperNode(int id, string fieldName)
        : base(id, fieldName)
    {
    }
}
