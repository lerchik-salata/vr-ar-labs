using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ClusterAssemblyManager : MonoBehaviour
{
    [SerializeField] private List<AssemblyFeedback> rackSlots = new List<AssemblyFeedback>();

    [Header("Status display")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Renderer statusLamp;
    [SerializeField] private Color pendingColor = new Color(1f, 0.55f, 0.1f);
    [SerializeField] private Color readyColor = new Color(0.1f, 1f, 0.35f);
    [SerializeField, Min(0f)] private float lampEmission = 3f;

    [Header("Cluster ready")]
    [SerializeField] private GameObject readyBeacon;
    [SerializeField] private Light readyLight;
    public UnityEvent onClusterReady = new UnityEvent();
    public UnityEvent onClusterDegraded = new UnityEvent();

    MaterialPropertyBlock _block;
    bool _ready;

    public int OnlineCount { get; private set; }
    public int Total => rackSlots.Count;

    void OnEnable()
    {
        foreach (var s in rackSlots)
            if (s != null) s.StateChanged += HandleSlotChanged;
        Refresh(log: false);
    }

    void OnDisable()
    {
        foreach (var s in rackSlots)
            if (s != null) s.StateChanged -= HandleSlotChanged;
    }

    void Update()
    {
        if (statusLamp == null || _ready) return;
        SetLamp(pendingColor, lampEmission * (0.5f + 0.5f * Mathf.Sin(Time.time * 3f)));
    }

    void HandleSlotChanged(AssemblyFeedback slot, bool installed) => Refresh(log: true);

    void Refresh(bool log)
    {
        int online = 0;
        foreach (var s in rackSlots)
            if (s != null && s.IsInstalled) online++;
        OnlineCount = online;

        bool ready = Total > 0 && online == Total;
        if (statusText != null)
            statusText.text = ready ? "CLUSTER READY" : $"NODES ONLINE  {online}/{Total}";
        if (hintText != null)
            hintText.text = ready
                ? "kubectl get nodes: all Ready"
                : "Match node-N to its slot: pinch a module and mount it";

        if (readyBeacon != null) readyBeacon.SetActive(ready);
        if (readyLight != null) readyLight.enabled = ready;
        if (ready) SetLamp(readyColor, lampEmission);

        if (log && Application.isPlaying)
            Debug.Log($"[Cluster] Вузлів у кластері: {online}/{Total}", this);

        if (ready && !_ready)
        {
            if (Application.isPlaying)
            {
                Debug.Log("[Cluster] Кластер зібрано: усі вузли підключено (Ready)", this);
                ProceduralSfx.Play(ProceduralSfx.Kind.Ready, transform.position + Vector3.up * 1.4f, 0.8f);
            }
            onClusterReady.Invoke();
        }
        else if (!ready && _ready)
        {
            if (Application.isPlaying) Debug.Log("[Cluster] Кластер деградував: вузол від'єднано", this);
            onClusterDegraded.Invoke();
        }
        _ready = ready;
    }

    void SetLamp(Color c, float intensity)
    {
        if (statusLamp == null) return;
        _block ??= new MaterialPropertyBlock();
        statusLamp.GetPropertyBlock(_block);
        _block.SetColor("_BaseColor", c);
        _block.SetColor("_EmissionColor", c * intensity);
        statusLamp.SetPropertyBlock(_block);
    }
}
