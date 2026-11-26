using TMPro;
using UnityEngine;

// Pod indicators on a grabbable node module.
[RequireComponent(typeof(NodeIdentity))]
public class NodePods : MonoBehaviour
{
    [SerializeField] private Renderer[] pods;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Color runningColor = new Color(0.1f, 1f, 0.45f);
    [SerializeField] private Color freeColor = new Color(0.12f, 0.16f, 0.2f);
    [SerializeField] private float emission = 2.5f;

    MaterialPropertyBlock _block;
    NodeIdentity _id;

    public int Capacity => pods != null ? pods.Length : 0;
    public int NodeId => (_id != null ? _id : _id = GetComponent<NodeIdentity>()).NodeId;

    void Awake()
    {
        _id = GetComponent<NodeIdentity>();
        Show(0, null);
    }

    public void Show(int running, string ns)
    {
        if (label != null)
            label.text = string.IsNullOrEmpty(ns)
                ? $"node-{NodeId}"
                : $"node-{NodeId}\n<size=60%>ns: {ns}  pods: {running}</size>";

        if (pods == null) return;
        _block ??= new MaterialPropertyBlock();
        for (int i = 0; i < pods.Length; i++)
        {
            if (pods[i] == null) continue;
            bool on = i < running;
            pods[i].GetPropertyBlock(_block);
            _block.SetColor("_BaseColor", on ? runningColor : freeColor);
            _block.SetColor("_EmissionColor", on ? runningColor * emission : Color.black);
            pods[i].SetPropertyBlock(_block);
        }
    }
}
