using Mono.Cecil;
using System.Diagnostics;
using System.Xml;

namespace MstatAnalyser.Core;

public class DgmlGraphProcessing
{
    public bool ParseXml(string name, TypeReference[] types, Stream fileStream, out Graph g)
    {
        var converter = new NodeConverter(types);
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
                        var node = DgmlGraphProcessing.ParseNode(id, reader.GetAttribute("Label"));
                        var convertedNode = converter.Convert(node);
                        g.AddNode(id, convertedNode);
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
