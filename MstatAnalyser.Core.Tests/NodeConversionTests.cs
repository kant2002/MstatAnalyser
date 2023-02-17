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
    public void GenericTypeNode()
    {
        var node = new Node(332, "Microsoft_AspNetCore_Mvc_Core_Microsoft_Extensions_Internal_CopyOnWriteDictionaryHolder_2");
        NodeConverter converter = CreateTestConverter();
        var regionNode = (TypeNode)converter.Convert(node);

        Assert.AreEqual("Microsoft.Extensions.Internal.CopyOnWriteDictionaryHolder`2", regionNode.TypeReference.FullName);
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
        var converter = new NodeConverter(new[]
        {
            moduleType,
            copyOnWriteDictionaryHolderType,
            pageLayoutType,
        });
        return converter;
    }
}
