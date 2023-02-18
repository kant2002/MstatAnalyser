using Mono.Cecil;

namespace MstatAnalyser.Core.Tests;

[TestClass]
public class NodeConversionTests
{
    [TestMethod]
    public void TypeNodeVariant1()
    {
        var node = new Node(332, "Microsoft_AspNetCore_Mvc_Core__Module_");
        NodeConverter converter = CreateTestConverter();
        var regionNode = (TypeNode)converter.Convert(node);

        Assert.AreEqual("<Module>", regionNode.TypeReference.FullName);
    }
    [TestMethod]
    public void TypeNodeVariant2()
    {
        var node = new Node(332, "System.Reflection.Metadata.CustomAttributeHandle");
        NodeConverter converter = CreateTestConverter();
        var regionNode = (TypeNode)converter.Convert(node);

        Assert.AreEqual("System.Reflection.Metadata.CustomAttributeHandle", regionNode.TypeReference.FullName);
    }
    [TestMethod]
    public void GenericTypeNode()
    {
        var node = new Node(332, "Microsoft_AspNetCore_Mvc_Core_Microsoft_Extensions_Internal_CopyOnWriteDictionaryHolder_2");
        NodeConverter converter = CreateTestConverter();
        var regionNode = (TypeNode)converter.Convert(node);

        Assert.AreEqual("Microsoft.Extensions.Internal.CopyOnWriteDictionaryHolder`2", regionNode.TypeReference.FullName);
    }
    [TestMethod]
    public void GenericTypeNode2()
    {
        var node = new Node(332, "S_P_CoreLib_System_Collections_Generic_IEnumerable_1<Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext>");
        NodeConverter converter = CreateTestConverter();
        var regionNode = (TypeNode)converter.Convert(node);

        Assert.AreEqual("System.Collections.Generic.IEnumerable`1<Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext>", regionNode.TypeReference.FullName);
    }

    [TestMethod]
    public void GenericTypeNode3()
    {
        var node = new Node(332, "S_P_CoreLib_System_Collections_Generic_List_1_Microsoft_AspNetCore_Razor_Runtime_Microsoft_AspNetCore_Razor_Runtime_TagHelpers_TagHelperExecutionContext_");
        NodeConverter converter = CreateTestConverter();
        var regionNode = (TypeNode)converter.Convert(node);

        Assert.AreEqual("System.Collections.Generic.List`1<Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext>", regionNode.TypeReference.FullName);
    }

    [TestMethod]
    public void GenericTypeNode4()
    {
        var node = new Node(332, "[S.P.CoreLib]System.Collections.Generic.IReadOnlyList`1<System.__Canon>");
        NodeConverter converter = CreateTestConverter();
        var regionNode = (TypeNode)converter.Convert(node);

        Assert.AreEqual("System.Collections.Generic.IReadOnlyList`1<System.__Canon>", regionNode.TypeReference.FullName);
    }

    [TestMethod]
    public void NestedTypeNode()
    {
        var node = new Node(332, "[Library]Library.Pages.Shared.Pages_Shared__Layout+<>c+<<ExecuteAsync>b__17_2>d");
        NodeConverter converter = CreateTestConverter();
        var regionNode = (TypeNode)converter.Convert(node);

        Assert.AreEqual("Library.Pages.Shared.Pages_Shared__Layout/<>c/<<ExecuteAsync>b__17_2>d", regionNode.TypeReference.FullName);
    }

    private static NodeConverter CreateTestConverter()
    {
        var microsoftAspnetCoreAssembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("Microsoft.AspNetCore.Mvc.Core", new()), "Microsoft.AspNetCore.Mvc.Core", ModuleKind.Dll);
        var moduleType = new TypeDefinition("", "<Module>", TypeAttributes.Class);
        microsoftAspnetCoreAssembly.Modules.First().Types.Add(moduleType);
        var copyOnWriteDictionaryHolderType = new TypeDefinition("Microsoft.Extensions.Internal", "CopyOnWriteDictionaryHolder`2", TypeAttributes.Class);
        copyOnWriteDictionaryHolderType.GenericParameters.Add(new GenericParameter("T1", copyOnWriteDictionaryHolderType));
        copyOnWriteDictionaryHolderType.GenericParameters.Add(new GenericParameter("T2", copyOnWriteDictionaryHolderType));
        microsoftAspnetCoreAssembly.Modules.First().Types.Add(copyOnWriteDictionaryHolderType);
        
        var libraryAssembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("Library", new()), "Library", ModuleKind.Dll);
        var pageLayoutType = new TypeDefinition("Library.Pages.Shared", "Pages_Shared__Layout", TypeAttributes.Class);
        var pageLayoutCType = new TypeDefinition("", "<>c", TypeAttributes.Class);
        pageLayoutType.NestedTypes.Add(pageLayoutCType);
        var pageLayoutCExecuteAsyncType = new TypeDefinition("", "<<ExecuteAsync>b__17_2>d", TypeAttributes.Class);
        pageLayoutCType.NestedTypes.Add(pageLayoutCExecuteAsyncType);
        libraryAssembly.Modules.First().Types.Add(pageLayoutType);

        var systemReflectionMetadataAssembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("System.Reflection.Metadata", new()), "System.Reflection.Metadata", ModuleKind.Dll);
        var customAttributeHandleType = new TypeDefinition("System.Reflection.Metadata", "CustomAttributeHandle", TypeAttributes.Class);
        systemReflectionMetadataAssembly.Modules.First().Types.Add(customAttributeHandleType);

        var systemPrivateCorelibAssembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("System.Private.CoreLib", new()), "System.Private.CoreLib", ModuleKind.Dll);
        var ienumerableType = new TypeDefinition("System.Collections.Generic", "IEnumerable`1", TypeAttributes.Interface);
        ienumerableType.GenericParameters.Add(new GenericParameter("T1", ienumerableType));
        systemPrivateCorelibAssembly.Modules.First().Types.Add(ienumerableType);
        var listType = new TypeDefinition("System.Collections.Generic", "List`1", TypeAttributes.Class);
        listType.GenericParameters.Add(new GenericParameter("T1", listType));
        systemPrivateCorelibAssembly.Modules.First().Types.Add(listType);
        var ireadonnlyListType = new TypeDefinition("System.Collections.Generic", "IReadOnlyList`1", TypeAttributes.Interface);
        ireadonnlyListType.GenericParameters.Add(new GenericParameter("T1", ireadonnlyListType));
        systemPrivateCorelibAssembly.Modules.First().Types.Add(ireadonnlyListType);
        var canonType = new TypeDefinition("System", "__Canon", TypeAttributes.Class);
        systemPrivateCorelibAssembly.Modules.First().Types.Add(canonType);

        var microsoftAspNetCoreRazorRuntimeAssembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("Microsoft.AspNetCore.Razor.Runtime", new()), "Microsoft.AspNetCore.Razor.Runtime", ModuleKind.Dll);
        var tagHelperExecutionContextType = new TypeDefinition("Microsoft.AspNetCore.Razor.Runtime.TagHelpers", "TagHelperExecutionContext", TypeAttributes.Class);
        microsoftAspNetCoreRazorRuntimeAssembly.Modules.First().Types.Add(tagHelperExecutionContextType);

        var converter = new NodeConverter(new[]
        {
            moduleType,
            copyOnWriteDictionaryHolderType,
            pageLayoutType,
            customAttributeHandleType,
            ienumerableType,
            listType,
            ireadonnlyListType,
            canonType,
            tagHelperExecutionContextType,
        });
        return converter;
    }
}
