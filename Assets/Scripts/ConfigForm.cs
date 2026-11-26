using System;
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

    public event Action<ClusterConfig> Saved;

    private bool hasExternalValues;

    void Awake()
    {
        if (replicasSlider != null)
            replicasSlider.onValueChanged.AddListener(UpdateReplicasLabel);
    }

    void Start()
    {
        // Keep a config applied before the panel was first opened
        if (!hasExternalValues) Reset();
    }

    public void Save()
    {
        var config = new ClusterConfig
        {
            ns = namespaceInput != null && !string.IsNullOrWhiteSpace(namespaceInput.text)
                ? namespaceInput.text.Trim()
                : defaultNamespace,
            replicas = replicasSlider != null ? Mathf.RoundToInt(replicasSlider.value) : 0,
            autoscale = autoscaleToggle != null && autoscaleToggle.isOn,
        };

        ShowStatus($"Saved: namespace={config.ns}, replicas={config.replicas}, autoscaling={(config.autoscale ? "on" : "off")}");
        Saved?.Invoke(config);
    }

    public void Reset()
    {
        if (autoscaleToggle != null)
            autoscaleToggle.isOn = defaultAutoscale;
        if (replicasSlider != null)
            replicasSlider.value = defaultReplicas;
        if (namespaceInput != null)
            namespaceInput.text = defaultNamespace;
        ShowStatus("Defaults restored");

        UpdateReplicasLabel(defaultReplicas);
    }

    public void SetValues(ClusterConfig config)
    {
        hasExternalValues = true;
        if (autoscaleToggle != null) autoscaleToggle.SetIsOnWithoutNotify(config.autoscale);
        if (replicasSlider != null) replicasSlider.SetValueWithoutNotify(config.replicas);
        if (namespaceInput != null) namespaceInput.SetTextWithoutNotify(config.ns);
        UpdateReplicasLabel(config.replicas);
    }

    public void ShowStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    private void UpdateReplicasLabel(float value)
    {
        if (replicasLabel != null)
            replicasLabel.text = $"Replicas: {Mathf.RoundToInt(value)}";
    }
}
