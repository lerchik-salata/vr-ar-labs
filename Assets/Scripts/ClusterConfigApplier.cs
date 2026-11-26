using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Applies the Control Panel form (namespace, replicas, autoscaling) to the node modules on the rack.
public class ClusterConfigApplier : MonoBehaviour
{
    [SerializeField] private ConfigForm form;
    [SerializeField] private ClusterConfigSync sync;

    readonly Dictionary<int, AssemblyFeedback> _slots = new Dictionary<int, AssemblyFeedback>();
    NodePods[] _nodes;
    ClusterConfig _config;

    void Awake()
    {
        _nodes = GetComponentsInChildren<NodePods>(true).OrderBy(n => n.NodeId).ToArray();
        foreach (var key in GetComponentsInChildren<RackSlotKey>(true))
        {
            var feedback = key.GetComponent<AssemblyFeedback>();
            if (feedback != null) _slots[key.AcceptedNodeId] = feedback;
        }
    }

    void OnEnable()
    {
        if (form != null) form.Saved += OnFormSaved;
        foreach (var s in _slots.Values) s.StateChanged += OnSlotChanged;
        Refresh();
    }

    void OnDisable()
    {
        if (form != null) form.Saved -= OnFormSaved;
        foreach (var s in _slots.Values) s.StateChanged -= OnSlotChanged;
    }

    void OnSlotChanged(AssemblyFeedback slot, bool installed) => Refresh();

    void OnFormSaved(ClusterConfig config)
    {
        config.placement = Schedule(config.replicas);
        if (sync != null) sync.Publish(config);
        else Apply(config);
    }

    // Entry point for both local saves and configs received from the room
    public void Apply(ClusterConfig config)
    {
        _config = config.Clone();
        if (form != null) form.SetValues(_config);
        Debug.Log($"[ClusterConfig] Applied ns={_config.ns}, replicas={_config.replicas}, " +
                  $"autoscaling={(_config.autoscale ? "on" : "off")}, placement=[{string.Join(",", _config.placement)}]", this);
        Refresh();
    }

    bool IsOnline(NodePods node) => _slots.TryGetValue(node.NodeId, out var s) && s.IsInstalled;

    // Round-robin over online nodes, limited by each node's pod capacity
    int[] Schedule(int replicas)
    {
        var counts = new int[_nodes.Length];
        var online = Enumerable.Range(0, _nodes.Length).Where(i => IsOnline(_nodes[i])).ToList();
        int left = replicas;
        while (left > 0 && online.Count > 0)
        {
            bool placed = false;
            foreach (int i in online)
            {
                if (left == 0) break;
                if (counts[i] >= _nodes[i].Capacity) continue;
                counts[i]++; left--; placed = true;
            }
            if (!placed) break;
        }
        return counts;
    }

    void Refresh()
    {
        if (_nodes == null) return;
        if (_config == null)
        {
            foreach (var n in _nodes) n.Show(0, null);
            return;
        }

        // Autoscaling reschedules on every node change; otherwise pods stay bound to their node
        int[] counts = _config.autoscale ? Schedule(_config.replicas) : _config.placement;
        int running = 0, onlineNodes = 0;
        for (int i = 0; i < _nodes.Length; i++)
        {
            bool online = IsOnline(_nodes[i]);
            int n = online && i < counts.Length ? counts[i] : 0;
            if (online) onlineNodes++;
            running += n;
            _nodes[i].Show(n, _config.ns);
        }

        int pending = Mathf.Max(0, _config.replicas - running);
        string status = $"Applied: ns={_config.ns}, {running}/{_config.replicas} pods running on " +
                        $"{onlineNodes}/{_nodes.Length} nodes, autoscaling={(_config.autoscale ? "on" : "off")}";
        if (pending > 0) status += $"\n<color=#FFB020>{pending} pod(s) Pending</color>";
        if (form != null) form.ShowStatus(status);
    }
}
