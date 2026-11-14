using UnityEngine;
using UnityEngine.InputSystem;

public class PanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Key key = Key.M;
    [SerializeField] private InputActionReference toggleAction;
    [SerializeField] private bool startVisible = true;

    void Start()
    {
        if (panel != null)
            panel.SetActive(startVisible);
    }

    void OnEnable()
    {
        if (toggleAction != null && toggleAction.action != null)
        {
            toggleAction.action.performed += OnTogglePerformed;
            toggleAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleAction != null && toggleAction.action != null)
            toggleAction.action.performed -= OnTogglePerformed;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard[key].wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        if (panel != null)
            panel.SetActive(!panel.activeSelf);
    }

    public void Show()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void OnTogglePerformed(InputAction.CallbackContext context) => Toggle();
}
