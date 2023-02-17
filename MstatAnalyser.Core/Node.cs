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
