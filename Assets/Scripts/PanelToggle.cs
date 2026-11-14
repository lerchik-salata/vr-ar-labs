using UnityEngine;
using UnityEngine.InputSystem;

public class PanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private Key key = Key.M;
    [SerializeField] private InputActionReference toggleAction;
    [SerializeField] private string[] controllerBindings =
    {
        "<XRController>{LeftHand}/{MenuButton}",
        "<XRController>{RightHand}/{MenuButton}",
    };
    [SerializeField] private bool startVisible = false;
    [SerializeField] private float cooldown = 0.25f;

    [SerializeField] private bool showInFrontOfUser = true;
    [SerializeField] private Transform head;
    [SerializeField] private float distance = 1f;
    [SerializeField] private float heightOffset = -0.25f;

    private InputAction fallbackAction;
    private InputAction activeAction;
    private float lastToggleTime = float.NegativeInfinity;

    public bool IsVisible
    {
        get
        {
            if (panels == null) return false;
            foreach (var panel in panels)
                if (panel != null && panel.activeSelf)
                    return true;
            return false;
        }
    }

    void Start()
    {
        SetVisible(startVisible);
    }

    void OnEnable()
    {
        activeAction = ResolveAction();
        if (activeAction != null)
        {
            activeAction.performed += OnTogglePerformed;
            activeAction.Enable();
        }
    }

    void OnDisable()
    {
        if (activeAction != null)
            activeAction.performed -= OnTogglePerformed;
        fallbackAction?.Disable();
        activeAction = null;
    }

    void OnDestroy()
    {
        fallbackAction?.Dispose();
        fallbackAction = null;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard[key].wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        if (Time.unscaledTime - lastToggleTime < cooldown) return;
        lastToggleTime = Time.unscaledTime;
        SetVisible(!IsVisible);
    }

    public void Show() => SetVisible(true);

    public void Hide() => SetVisible(false);

    public void SetVisible(bool visible)
    {
        if (panels == null) return;
        if (visible && showInFrontOfUser)
            PlaceInFrontOfUser();
        foreach (var panel in panels)
            if (panel != null)
                panel.SetActive(visible);
    }

    public void PlaceInFrontOfUser()
    {
        var viewer = head != null ? head : Camera.main != null ? Camera.main.transform : null;
        if (viewer == null || panels == null) return;

        var forward = Vector3.ProjectOnPlane(viewer.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(viewer.up, Vector3.up);
        forward.Normalize();

        var position = viewer.position + forward * distance + Vector3.up * heightOffset;
        var rotation = Quaternion.LookRotation(forward, Vector3.up);
        foreach (var panel in panels)
            if (panel != null)
                panel.transform.SetPositionAndRotation(position, rotation);
    }

    private InputAction ResolveAction()
    {
        if (toggleAction != null && toggleAction.action != null)
            return toggleAction.action;

        if (fallbackAction == null && controllerBindings != null && controllerBindings.Length > 0)
        {
            fallbackAction = new InputAction("TogglePanels", InputActionType.Button);
            foreach (var binding in controllerBindings)
                if (!string.IsNullOrEmpty(binding))
                    fallbackAction.AddBinding(binding);
        }
        return fallbackAction;
    }

    private void OnTogglePerformed(InputAction.CallbackContext context) => Toggle();
}
