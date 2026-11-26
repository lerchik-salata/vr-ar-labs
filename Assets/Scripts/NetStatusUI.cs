using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetStatusUI : MonoBehaviour
{
    [SerializeField] private TelemetryClient client;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image statusLamp;

    void OnEnable()
    {
        if (client == null) return;
        client.OnStatusChanged += UpdateStatus;
        UpdateStatus(client.Status, client.StatusMessage);
    }

    void OnDisable()
    {
        if (client != null) client.OnStatusChanged -= UpdateStatus;
    }

    private void UpdateStatus(TelemetryClient.NetStatus s, string msg)
    {
        if (statusText != null) statusText.text = msg;
        if (statusLamp != null)
            statusLamp.color = s switch
            {
                TelemetryClient.NetStatus.Loading => Color.yellow,
                TelemetryClient.NetStatus.Success => Color.green,
                TelemetryClient.NetStatus.Error   => Color.red,
                _ => Color.gray,
            };
    }
}
