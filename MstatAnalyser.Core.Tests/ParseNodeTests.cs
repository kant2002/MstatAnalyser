namespace MstatAnalyser.Core.Tests;

[TestClass]
public class ParseNodeTests
{
    [TestMethod]
    public void ParseRegionNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Region __GCStaticRegionStart");
        Assert.IsNotNull(node);
        var regionNode = (RegionNode)node;

        Assert.AreEqual("__GCStaticRegionStart", regionNode.Name);
    }
    [TestMethod]
    public void ParseReflectableModuleNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Reflectable module: System.Private.CoreLib, Version=7.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e");
        Assert.IsNotNull(node);
        var regionNode = (ModuleMetadataNode)node;

        Assert.AreEqual("System.Private.CoreLib, Version=7.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", regionNode.Name);
    }
    [TestMethod]
    public void ParseConstructedEETypeNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "??_7S_P_CoreLib_System_Action_2<Object__S_P_CoreLib_System_Nullable_1<Int32>>@@6B@ constructed");
        Assert.IsNotNull(node);
        var regionNode = (ConstructedEETypeNode)node;

        Assert.AreEqual("??_7S_P_CoreLib_System_Action_2<Object__S_P_CoreLib_System_Nullable_1<Int32>>@@6B@", regionNode.Name);
    }

    [TestMethod]
    public void ParseReflectedMethodNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Reflectable method: [S.P.CoreLib]System.Delegate..ctor(object,string)");
        Assert.IsNotNull(node);
        var regionNode = (ReflectedMethodNode)node;

        Assert.AreEqual("[S.P.CoreLib]System.Delegate..ctor(object,string)", regionNode.Name);
    }

    [TestMethod]
    public void ParseReflectedFieldNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Reflectable field: [Microsoft.AspNetCore.Mvc.Core]<>f__AnonymousType0`3<System.__Canon,System.__Canon,System.__Canon>.<actionContext>i__Field");
        Assert.IsNotNull(node);
        var regionNode = (ReflectedFieldNode)node;

        Assert.AreEqual("[Microsoft.AspNetCore.Mvc.Core]<>f__AnonymousType0`3<System.__Canon,System.__Canon,System.__Canon>.<actionContext>i__Field", regionNode.Name);
    }

    [TestMethod]
    public void ParseWindowsMethodTableNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "??_7Microsoft_AspNetCore_Mvc_Core__Module_@@6B@");
        Assert.IsNotNull(node);
        var regionNode = (MethodTableNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Mvc_Core__Module_", regionNode.Name);
        Assert.IsFalse(regionNode.IsBoxedValueType);
    }

    [TestMethod]
    public void ParseWindowsMethodTableNodeForValueType()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "??_7Boxed_Microsoft_AspNetCore_Mvc_Core_Microsoft_Extensions_Internal_CopyOnWriteDictionaryHolder_2@@6B@");
        Assert.IsNotNull(node);
        var regionNode = (MethodTableNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Mvc_Core_Microsoft_Extensions_Internal_CopyOnWriteDictionaryHolder_2", regionNode.Name);
        Assert.IsTrue(regionNode.IsBoxedValueType);
    }

    [TestMethod]
    public void ParseReflectedTypeNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Reflectable type: [Library]Library.Pages.Shared.Pages_Shared__Layout+<>c+<<ExecuteAsync>b__17_2>d");
        Assert.IsNotNull(node);
        var regionNode = (ReflectedTypeNode)node;

        Assert.AreEqual("[Library]Library.Pages.Shared.Pages_Shared__Layout+<>c+<<ExecuteAsync>b__17_2>d", regionNode.Name);
    }

    [TestMethod]
    public void ParseCustomAttributeMetadataNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Reflectable custom attribute System.Reflection.Metadata.CustomAttributeHandle in Library");
        Assert.IsNotNull(node);
        var regionNode = (CustomAttributeMetadataNode)node;

        Assert.AreEqual("System.Reflection.Metadata.CustomAttributeHandle", regionNode.Name);
        Assert.AreEqual("Library", regionNode.AssemblyName);
    }

    [TestMethod]
    public void ParseVTableSliceNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__vtable_S_P_CoreLib_System_Collections_Generic_IEnumerable_1<Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext>");
        Assert.IsNotNull(node);
        var regionNode = (VTableSliceNode)node;

        Assert.AreEqual("S_P_CoreLib_System_Collections_Generic_IEnumerable_1<Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext>", regionNode.Name);
    }

    [TestMethod]
    public void ParseInterfaceDispatchMapNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__InterfaceDispatchMap_S_P_CoreLib_System_Collections_Generic_List_1_Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext_");
        Assert.IsNotNull(node);
        var regionNode = (InterfaceDispatchMapNode)node;

        Assert.AreEqual("S_P_CoreLib_System_Collections_Generic_List_1_Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext_", regionNode.Name);
    }

    [TestMethod]
    public void ParseSealedVTableNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__SealedVTable_??_7Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Hosting_RazorCompiledItemAttribute@@6B@");
        Assert.IsNotNull(node);
        var regionNode = (SealedVTableNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Hosting_RazorCompiledItemAttribute", regionNode.Name);
    }

    [TestMethod]
    public void ParseTypeGenericDictionaryNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__GenericDict_S_P_CoreLib_System_Collections_Generic_List_1<Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext>");
        Assert.IsNotNull(node);
        var regionNode = (GenericDictionaryNode)node;

        Assert.AreEqual("S_P_CoreLib_System_Collections_Generic_List_1<Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext>", regionNode.Name);
    }

    [TestMethod]
    public void ParseDictionaryLayoutNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Dictionary layout for [S.P.CoreLib]System.Collections.Generic.IReadOnlyList`1<System.__Canon>");
        Assert.IsNotNull(node);
        var regionNode = (DictionaryLayoutNode)node;

        Assert.AreEqual("[S.P.CoreLib]System.Collections.Generic.IReadOnlyList`1<System.__Canon>", regionNode.Name);
    }

    [TestMethod]
    public void ParseFieldMetadataNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Field metadata: [Microsoft.AspNetCore.Razor.Runtime]Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager+ExecutionContextPool._nextIndex");
        Assert.IsNotNull(node);
        var regionNode = (FieldMetadataNode)node;

        Assert.AreEqual("[Microsoft.AspNetCore.Razor.Runtime]Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager+ExecutionContextPool._nextIndex", regionNode.Name);
    }

    [TestMethod]
    public void ParseGenericCompositionNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__GenericInstance_Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext__Variance___1");
        Assert.IsNotNull(node);
        var regionNode = (GenericCompositionNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext", regionNode.Name);
        CollectionAssert.AreEqual(new[] { 1 }, regionNode.Variance);
    }

    [TestMethod]
    public void ParseGenericCompositionNodeNoVariance()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__GenericInstance_Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext");
        Assert.IsNotNull(node);
        var regionNode = (GenericCompositionNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext", regionNode.Name);
        CollectionAssert.AreEqual(Array.Empty<int>(), regionNode.Variance);
    }

    [TestMethod]
    public void ParseWritableDataNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__writableDataMicrosoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Hosting_RazorCompiledItemAttribute");
        Assert.IsNotNull(node);
        var regionNode = (WritableDataNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Hosting_RazorCompiledItemAttribute", regionNode.Name);
    }

    [TestMethod]
    public void ParseEETypeOptionalFieldsNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__optionalfields_??_7Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Hosting_RazorCompiledItemAttribute@@6B@");
        Assert.IsNotNull(node);
        var regionNode = (EETypeOptionalFieldsNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Hosting_RazorCompiledItemAttribute", regionNode.Name);
    }

    [TestMethod]
    public void ParseMethodMetadataNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Method metadata: [Microsoft.AspNetCore.Html.Abstractions]Microsoft.CodeAnalysis.EmbeddedAttribute..ctor()");
        Assert.IsNotNull(node);
        var regionNode = (MethodMetadataNode)node;

        Assert.AreEqual("[Microsoft.AspNetCore.Html.Abstractions]Microsoft.CodeAnalysis.EmbeddedAttribute..ctor()", regionNode.Name);
    }

    [TestMethod]
    public void ParseSimpleEmbeddedPointerIndirectionNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Embedded pointer to __InterfaceDispatchMap_S_P_CoreLib_System_Collections_Generic_List_1_Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext_");
        Assert.IsNotNull(node);
        var regionNode = (SimpleEmbeddedPointerIndirectionNode)node;

        Assert.AreEqual("__InterfaceDispatchMap_S_P_CoreLib_System_Collections_Generic_List_1_Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext_", regionNode.Name);
    }

    [TestMethod]
    public void ParseVirtualMethodUseNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "VirtualMethodUse [S.P.CoreLib]System.Func`1<Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContent>.Invoke()");
        Assert.IsNotNull(node);
        var regionNode = (VirtualMethodUseNode)node;

        Assert.AreEqual("[S.P.CoreLib]System.Func`1<Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContent>.Invoke()", regionNode.Name);
    }

    [TestMethod]
    public void ParseGCStaticEETypeNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__GCStaticEEType_01");
        Assert.IsNotNull(node);
        var regionNode = (GCStaticEETypeNode)node;

        Assert.AreEqual("01", regionNode.Name);
    }

    [TestMethod]
    public void ParseTentativeInstanceMethodNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Tentative instance method: Microsoft_AspNetCore_Html_Abstractions_Microsoft_CodeAnalysis_EmbeddedAttribute___ctor");
        Assert.IsNotNull(node);
        var regionNode = (TentativeInstanceMethodNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Html_Abstractions_Microsoft_CodeAnalysis_EmbeddedAttribute___ctor", regionNode.Name);
    }

    [TestMethod]
    public void ParseNativeLayoutTemplateMethodLayoutVertexNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "NativeLayoutTemplateTypeLayoutVertexNode_S_P_CoreLib_System_Threading_Tasks_Task_1<TResult_System___Canon>");
        Assert.IsNotNull(node);
        var regionNode = (NativeLayoutTemplateMethodLayoutVertexNode)node;

        Assert.AreEqual("S_P_CoreLib_System_Threading_Tasks_Task_1<TResult_System___Canon>", regionNode.Name);
    }

    [TestMethod]
    public void ParseNativeLayoutTypeSignatureVertexNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "NativeLayoutTypeSignatureVertexNode: [S.P.CoreLib]System.ICloneable");
        Assert.IsNotNull(node);
        var regionNode = (NativeLayoutTypeSignatureVertexNode)node;

        Assert.AreEqual("[S.P.CoreLib]System.ICloneable", regionNode.Name);
    }

    [TestMethod]
    public void ParseNativeLayoutPlacedSignatureVertexNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "NativeLayoutPlacedSignatureVertexNode");
        Assert.IsNotNull(node);
        var regionNode = (NativeLayoutPlacedSignatureVertexNode)node;

        Assert.AreEqual("NativeLayoutPlacedSignatureVertexNode", regionNode.Name);
    }

    [TestMethod]
    public void ParseNativeLayoutMethodNameAndSignatureVertexNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "NativeLayoutMethodNameAndSignatureVertexNodeMicrosoft_AspNetCore_Razor_Microsoft_AspNetCore_Razor_TagHelpers_TagHelperComponent__get_Order");
        Assert.IsNotNull(node);
        var regionNode = (NativeLayoutMethodNameAndSignatureVertexNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Razor_Microsoft_AspNetCore_Razor_TagHelpers_TagHelperComponent__get_Order", regionNode.Name);
    }

    [TestMethod]
    public void ParseNativeLayoutMethodSignatureVertexNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "NativeLayoutMethodSignatureVertexNode System.Int32,System.Private.CoreLib()");
        Assert.IsNotNull(node);
        var regionNode = (NativeLayoutMethodSignatureVertexNode)node;

        Assert.AreEqual("System.Int32,System.Private.CoreLib()", regionNode.Name);
    }

    [TestMethod]
    public void ParseFrozenObjectNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__FrozenObj_Microsoft_AspNetCore_Razor_Microsoft_AspNetCore_Razor_TagHelpers_TagHelperOutput___c1");
        Assert.IsNotNull(node);
        var regionNode = (FrozenObjectNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Razor_Microsoft_AspNetCore_Razor_TagHelpers_TagHelperOutput___c1", regionNode.Name);
    }

    [TestMethod]
    public void ParseFatFunctionPointerNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__fatpointer_S_P_CoreLib_System_Collections_Generic_List_1<Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext>___cctor");
        Assert.IsNotNull(node);
        var regionNode = (FatFunctionPointerNode)node;

        Assert.AreEqual("S_P_CoreLib_System_Collections_Generic_List_1<Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext>___cctor", regionNode.Name);
        Assert.IsFalse(regionNode.IsUnboxingStub);
    }

    [TestMethod]
    public void ParseFatFunctionPointerNodeUnboxingStub()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__fatunboxpointer_System_Linq_System_Collections_Generic_LargeArrayBuilder_1<String>__AllocateBuffer");
        Assert.IsNotNull(node);
        var regionNode = (FatFunctionPointerNode)node;

        Assert.AreEqual("System_Linq_System_Collections_Generic_LargeArrayBuilder_1<String>__AllocateBuffer", regionNode.Name);
        Assert.IsTrue(regionNode.IsUnboxingStub);
    }

    [TestMethod]
    public void ParseShadowConcreteMethodNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "[S.P.CoreLib]System.Collections.Generic.Dictionary`2+ValueCollection<System.Linq.Expressions.Expression,bool>.System.Collections.IEnumerable.GetEnumerator() backed by S_P_CoreLib_System_Collections_Generic_Dictionary_2_ValueCollection<System___Canon__Bool>__System_Collections_IEnumerable_GetEnumerator");
        Assert.IsNotNull(node);
        var regionNode = (ShadowConcreteMethodNode)node;

        Assert.AreEqual("[S.P.CoreLib]System.Collections.Generic.Dictionary`2+ValueCollection<System.Linq.Expressions.Expression,bool>.System.Collections.IEnumerable.GetEnumerator()", regionNode.Name);
        Assert.AreEqual("S_P_CoreLib_System_Collections_Generic_Dictionary_2_ValueCollection<System___Canon__Bool>__System_Collections_IEnumerable_GetEnumerator", regionNode.CanonicalMethodName);
    }

    [TestMethod]
    public void ParseInterfaceDispatchCellNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__InterfaceDispatchCell_S_P_CoreLib_System_Collections_Generic_ICollection_1<Microsoft_EntityFrameworkCore_Microsoft_EntityFrameworkCore_Metadata_Conventions_Internal_ConventionDispatcher_ConventionNode>__get_Count___GenericDict_S_P_CoreLib_System_Collections_ObjectModel_ReadOnlyCollection_1<Microsoft_EntityFrameworkCore_Microsoft_EntityFrameworkCore_Metadata_Conventions_Internal_ConventionDispatcher_ConventionNode>");
        Assert.IsNotNull(node);
        var regionNode = (InterfaceDispatchCellNode)node;

        //Assert.AreEqual("S_P_CoreLib_System_Collections_Generic_ICollection_1<Microsoft_EntityFrameworkCore_Microsoft_EntityFrameworkCore_Metadata_Conventions_Internal_ConventionDispatcher_ConventionNode>__get_Count___GenericDict_S_P_CoreLib_System_Collections_ObjectModel_ReadOnlyCollection_1<Microsoft_EntityFrameworkCore_Microsoft_EntityFrameworkCore_Metadata_Conventions_Internal_ConventionDispatcher_ConventionNode>", regionNode.Name);
        Assert.AreEqual("S_P_CoreLib_System_Collections_Generic_ICollection_1<Microsoft_EntityFrameworkCore_Microsoft_EntityFrameworkCore_Metadata_Conventions_Internal_ConventionDispatcher_ConventionNode>__get_Count", regionNode.Name);
        Assert.AreEqual("S_P_CoreLib_System_Collections_ObjectModel_ReadOnlyCollection_1<Microsoft_EntityFrameworkCore_Microsoft_EntityFrameworkCore_Metadata_Conventions_Internal_ConventionDispatcher_ConventionNode>", regionNode.CallSiteIdentifier);
    }

    [TestMethod]
    public void ParseRuntimeMethodHandleNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__RuntimeMethodHandle_System_Linq_System_Linq_Enumerable_Iterator_1<S_P_CoreLib_System_Type>__Select<S_P_CoreLib_System_Reflection_TypeInfo>");
        Assert.IsNotNull(node);
        var regionNode = (RuntimeMethodHandleNode)node;

        Assert.AreEqual("System_Linq_System_Linq_Enumerable_Iterator_1<S_P_CoreLib_System_Type>__Select<S_P_CoreLib_System_Reflection_TypeInfo>", regionNode.Name);
    }

    [TestMethod]
    public void ParseTypeGVMEntriesNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__TypeGVMEntriesNode_Microsoft_AspNetCore_Mvc_RazorPages_Microsoft_AspNetCore_Mvc_RazorPages_PageBase");
        Assert.IsNotNull(node);
        var regionNode = (TypeGVMEntriesNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Mvc_RazorPages_Microsoft_AspNetCore_Mvc_RazorPages_PageBase", regionNode.Name);
    }

    [TestMethod]
    [Ignore("this is method, but cannot guess what's node it is")]
    public void ParseUnboxingThunkNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "unbox_Microsoft_AspNetCore_Mvc_ViewFeatures_Microsoft_AspNetCore_Components_ComponentParameter__set_Assembly");
        Assert.IsNotNull(node);
        var regionNode = (RuntimeMethodHandleNode)node;

        Assert.AreEqual("System_Linq_System_Linq_Enumerable_Iterator_1<S_P_CoreLib_System_Type>__Select<S_P_CoreLib_System_Reflection_TypeInfo>", regionNode.Name);
    }

    [TestMethod]
    [Ignore("this is mangled type and method method separated by '_', but cannot guess what's node it is")]
    public void ParseTypeAndMethodManglingNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "<Boxed>S_P_CoreLib_System_Runtime_CompilerServices_ValueTaskAwaiter_1<System___Canon>__<unbox>S_P_CoreLib_System_Runtime_CompilerServices_ValueTaskAwaiter_1__UnsafeOnCompleted");
        Assert.IsNotNull(node);
        var regionNode = (RuntimeMethodHandleNode)node;

        Assert.AreEqual("System_Linq_System_Linq_Enumerable_Iterator_1<S_P_CoreLib_System_Type>__Select<S_P_CoreLib_System_Reflection_TypeInfo>", regionNode.Name);
    }

    [TestMethod]
    public void ParseGCStaticsNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "?__GCSTATICS@Microsoft_AspNetCore_Mvc_RazorPages_Microsoft_AspNetCore_Mvc_RazorPages_Infrastructure_DefaultPageHandlerMethodSelector___c@@");
        Assert.IsNotNull(node);
        var regionNode = (GCStaticsNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Mvc_RazorPages_Microsoft_AspNetCore_Mvc_RazorPages_Infrastructure_DefaultPageHandlerMethodSelector___c", regionNode.Name);
    }

    [TestMethod]
    public void ParseGCStaticsPreInitDataNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "?__GCSTATICS@Microsoft_AspNetCore_Mvc_RazorPages_Microsoft_AspNetCore_Mvc_RazorPages_Infrastructure_DefaultPageHandlerMethodSelector___c@@__PreInitData");
        Assert.IsNotNull(node);
        var regionNode = (GCStaticsPreInitDataNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Mvc_RazorPages_Microsoft_AspNetCore_Mvc_RazorPages_Infrastructure_DefaultPageHandlerMethodSelector___c", regionNode.Name);
    }

    [TestMethod]
    public void ParseNonGCStaticsNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "?__NONGCSTATICS@Microsoft_AspNetCore_Mvc_RazorPages_Microsoft_AspNetCore_Mvc_ApplicationModels_PageRouteModelFactory@@");
        Assert.IsNotNull(node);
        var regionNode = (NonGCStaticsNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Mvc_RazorPages_Microsoft_AspNetCore_Mvc_ApplicationModels_PageRouteModelFactory", regionNode.Name);
    }

    [TestMethod]
    public void ParseNativeLayoutTemplateMethodSignatureVertexNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "NativeLayoutTemplateMethodSignatureVertexNode_Microsoft_AspNetCore_Mvc_RazorPages_Microsoft_AspNetCore_Mvc_RazorPages_PageModel__TryUpdateModelAsync_5<System___Canon>");
        Assert.IsNotNull(node);
        var regionNode = (NativeLayoutTemplateMethodSignatureVertexNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Mvc_RazorPages_Microsoft_AspNetCore_Mvc_RazorPages_PageModel__TryUpdateModelAsync_5<System___Canon>", regionNode.Name);
    }

    [TestMethod]
    public void ParseGVMDependenciesNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__GVMDependenciesNode_System_Linq_System_Linq_Enumerable_SelectListPartitionIterator_2<System___Canon__System___Canon>__Select<System___Canon>");
        Assert.IsNotNull(node);
        var regionNode = (GVMDependenciesNode)node;

        Assert.AreEqual("System_Linq_System_Linq_Enumerable_SelectListPartitionIterator_2<System___Canon__System___Canon>__Select<System___Canon>", regionNode.Name);
    }

    [TestMethod]
    public void ParseVariantInterfaceMethodUseNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "VariantInterfaceMethodUse [S.P.CoreLib]System.Collections.Generic.IComparer`1.Compare(!0,!0)");
        Assert.IsNotNull(node);
        var regionNode = (VariantInterfaceMethodUseNode)node;

        Assert.AreEqual("[S.P.CoreLib]System.Collections.Generic.IComparer`1.Compare(!0,!0)", regionNode.Name);
    }

    [TestMethod]
    public void ParseDataflowAnalyzedMethodNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "Dataflow analysis for Microsoft_AspNetCore_Mvc_ViewFeatures_Microsoft_AspNetCore_Mvc_ViewFeatures_HtmlHelper__ObjectToDictionary");
        Assert.IsNotNull(node);
        var regionNode = (DataflowAnalyzedMethodNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Mvc_ViewFeatures_Microsoft_AspNetCore_Mvc_ViewFeatures_HtmlHelper__ObjectToDictionary", regionNode.Name);
    }

    [TestMethod]
    public void ParseReadyToRunGenericHelperNode()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "__GenericLookupFromDict_Microsoft_EntityFrameworkCore_Microsoft_EntityFrameworkCore_EF__CompileAsyncQuery_40<System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon>_TypeHandle_S_P_CoreLib_System_Func_12<TContext_System___Canon__TParam1_System___Canon__TParam2_System___Canon__TParam3_System___Canon__TParam4_System___Canon__TParam5_System___Canon__TParam6_System___Canon__TParam7_System___Canon__TParam8_System___Canon__TParam9_System___Canon__TParam10_System___Canon__S_P_CoreLib_System_Collections_Generic_IAsyncEnumerable_1<TResult_System___Canon>>");
        Assert.IsNotNull(node);
        var regionNode = (ReadyToRunGenericHelperNode)node;

        Assert.AreEqual("Microsoft_EntityFrameworkCore_Microsoft_EntityFrameworkCore_EF__CompileAsyncQuery_40<System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon__System___Canon>_TypeHandle_S_P_CoreLib_System_Func_12<TContext_System___Canon__TParam1_System___Canon__TParam2_System___Canon__TParam3_System___Canon__TParam4_System___Canon__TParam5_System___Canon__TParam6_System___Canon__TParam7_System___Canon__TParam8_System___Canon__TParam9_System___Canon__TParam10_System___Canon__S_P_CoreLib_System_Collections_Generic_IAsyncEnumerable_1<TResult_System___Canon>>", regionNode.Name);
    }
}