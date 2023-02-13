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
        var regionNode = (ReflectableModuleNode)node;

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
}