using System;
using System.Reflection.Emit;

namespace MstatAnalyser.Core;

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
