using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfigForm : MonoBehaviour
{
    [SerializeField] private Toggle autoscaleToggle;
    [SerializeField] private Slider replicasSlider;
    [SerializeField] private TMP_Text replicasLabel;
    [SerializeField] private TMP_InputField namespaceInput;
    [SerializeField] private TMP_Text statusText;

    [Header("Defaults")]
    [SerializeField] private bool defaultAutoscale = false;
    [SerializeField] private float defaultReplicas = 3f;
    [SerializeField] private string defaultNamespace = "default";

    void Awake()
    {
        if (replicasSlider != null)
            replicasSlider.onValueChanged.AddListener(UpdateReplicasLabel);
    }

    void Start()
    {
        Reset();
    }

    public void Save()
    {
        if (statusText == null) return;

        string autoscale = autoscaleToggle != null && autoscaleToggle.isOn ? "on" : "off";
        int replicas = replicasSlider != null ? Mathf.RoundToInt(replicasSlider.value) : 0;
        string ns = namespaceInput != null && !string.IsNullOrWhiteSpace(namespaceInput.text)
            ? namespaceInput.text.Trim()
            : defaultNamespace;

        statusText.text = $"Saved: namespace={ns}, replicas={replicas}, autoscaling={autoscale}";
    }

    public void Reset()
    {
        if (autoscaleToggle != null)
            autoscaleToggle.isOn = defaultAutoscale;
        if (replicasSlider != null)
            replicasSlider.value = defaultReplicas;
        if (namespaceInput != null)
            namespaceInput.text = defaultNamespace;
        if (statusText != null)
            statusText.text = "Defaults restored";

        UpdateReplicasLabel(defaultReplicas);
    }

    private void UpdateReplicasLabel(float value)
    {
        if (replicasLabel != null)
            replicasLabel.text = $"Replicas: {Mathf.RoundToInt(value)}";
    }
}
