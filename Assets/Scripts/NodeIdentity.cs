using UnityEngine;

public class NodeIdentity : MonoBehaviour
{
    [SerializeField, Min(1)] private int nodeId = 1;
    [SerializeField] private Color accent = new Color(0.2f, 0.9f, 1f);

    public int NodeId => nodeId;
    public Color Accent => accent;
    public string DisplayName => $"node-{nodeId}";
}
