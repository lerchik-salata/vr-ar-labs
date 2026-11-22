using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class PokeButton3D : MonoBehaviour
{
    [SerializeField] private Transform cap;
    [SerializeField] private float pressDepth = 0.012f;
    [SerializeField] private Renderer capRenderer;
    [SerializeField] private Color idle = new Color(0.2f, 0.9f, 1f);
    [SerializeField] private Color pressed = new Color(1f, 1f, 1f);
    [SerializeField] private float cooldown = 0.8f;
    public UnityEvent onPressed = new UnityEvent();

    XRSimpleInteractable _interactable;
    Vector3 _capRest;
    MaterialPropertyBlock _block;
    float _lastPress = float.NegativeInfinity;

    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        _block = new MaterialPropertyBlock();
        if (cap != null) _capRest = cap.localPosition;
        Paint(idle);
    }

    void OnEnable()
    {
        _interactable.selectEntered.AddListener(OnSelectEntered);
        _interactable.selectExited.AddListener(OnSelectExited);
    }

    void OnDisable()
    {
        _interactable.selectEntered.RemoveListener(OnSelectEntered);
        _interactable.selectExited.RemoveListener(OnSelectExited);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (_interactable.interactorsSelecting.Count > 1 || Time.unscaledTime - _lastPress < cooldown) return;
        _lastPress = Time.unscaledTime;
        if (cap != null) cap.localPosition = _capRest + Vector3.forward * pressDepth;
        Paint(pressed);
        ProceduralSfx.Play(ProceduralSfx.Kind.Click, transform.position, 0.6f);
        Debug.Log($"[PokeButton] '{name}' натиснуто ({args.interactorObject.GetType().Name})", this);
        onPressed.Invoke();
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        if (cap != null) cap.localPosition = _capRest;
        Paint(idle);
    }

    void Paint(Color c)
    {
        if (capRenderer == null) return;
        capRenderer.GetPropertyBlock(_block);
        _block.SetColor("_BaseColor", c);
        _block.SetColor("_EmissionColor", c * 1.5f);
        capRenderer.SetPropertyBlock(_block);
    }
}
