using System;

[Serializable]
public class ClusterConfig
{
    public string ns = "default";
    public int replicas = 3;
    public bool autoscale;
    public int[] placement = new int[0]; // pods per node, ordered by nodeId

    public ClusterConfig Clone() => new ClusterConfig
    {
        ns = ns,
        replicas = replicas,
        autoscale = autoscale,
        placement = (int[])placement.Clone(),
    };
}
