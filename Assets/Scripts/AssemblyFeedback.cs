using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AssemblyFeedback : MonoBehaviour
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [Header("Rack slot visual")]
    [SerializeField] private Renderer slotRenderer;
    [SerializeField] private Color idleColor = new Color(0.18f, 0.22f, 0.30f);
    [SerializeField] private Color installedColor = new Color(0.10f, 0.85f, 0.30f);
    [SerializeField, Min(0f)] private float installedEmission = 1.5f;
    [SerializeField] private Color rejectedColor = new Color(1f, 0.12f, 0.08f);

    [Header("Node online indicator (\"checkmark\")")]
    [SerializeField] private GameObject installedIndicator;

    [Header("Sound")]
    [SerializeField] private bool playSounds = true;

    MaterialPropertyBlock _block;
    bool _rejected;

    public bool IsInstalled { get; private set; }

    public event System.Action<AssemblyFeedback, bool> StateChanged;

    void Awake()
    {
        _block = new MaterialPropertyBlock();
        Apply(false);
    }

    public void OnPartInstalled(SelectEnterEventArgs args)
    {
        _rejected = false;
        Apply(true);
        var nodeName = args != null && args.interactableObject != null
            ? args.interactableObject.transform.name
            : "?";
        if (playSounds) ProceduralSfx.Play(ProceduralSfx.Kind.Success, transform.position);
        Debug.Log($"[AssemblyFeedback] Node connected to cluster ({nodeName} → {name})", this);
    }

    public void OnPartRemoved(SelectExitEventArgs args)
    {
        Apply(false);
        if (args != null && args.isCanceled) return;
        if (playSounds) ProceduralSfx.Play(ProceduralSfx.Kind.Unplug, transform.position);
        Debug.Log($"[AssemblyFeedback] Node disconnected from cluster ({name})", this);
    }

    public void ShowRejected(bool rejected)
    {
        _rejected = rejected && !IsInstalled;
        UpdateVisual();
    }

    void Apply(bool installed)
    {
        bool changed = IsInstalled != installed;
        IsInstalled = installed;
        if (changed) StateChanged?.Invoke(this, installed);
        UpdateVisual();
        if (installedIndicator != null)
            installedIndicator.SetActive(installed);
    }

    void UpdateVisual()
    {
        if (slotRenderer == null) return;
        _block ??= new MaterialPropertyBlock();
        slotRenderer.GetPropertyBlock(_block);
        Color c, e;
        if (_rejected) { c = rejectedColor; e = rejectedColor * 2f; }
        else if (IsInstalled) { c = installedColor; e = installedColor * installedEmission; }
        else { c = idleColor; e = Color.black; }
        _block.SetColor(BaseColorId, c);
        _block.SetColor(ColorId, c);
        _block.SetColor(EmissionColorId, e);
        slotRenderer.SetPropertyBlock(_block);
    }
}
