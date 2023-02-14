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
        Assert.IsFalse(regionNode.IsBoxed);
    }

    [TestMethod]
    public void ParseWindowsMethodTableNodeForValueType()
    {
        var node = DGMLGraphProcessing.ParseNode(332, "??_7Boxed_Microsoft_AspNetCore_Mvc_Core_Microsoft_Extensions_Internal_CopyOnWriteDictionaryHolder_2@@6B@");
        Assert.IsNotNull(node);
        var regionNode = (MethodTableNode)node;

        Assert.AreEqual("Microsoft_AspNetCore_Mvc_Core_Microsoft_Extensions_Internal_CopyOnWriteDictionaryHolder_2", regionNode.Name);
        Assert.IsTrue(regionNode.IsBoxed);
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
}