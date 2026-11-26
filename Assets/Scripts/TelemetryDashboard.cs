using System.Text;
using TMPro;
using UnityEngine;

public class TelemetryDashboard : MonoBehaviour
{
    [SerializeField] private TelemetryClient client;
    [SerializeField] private TMP_Text telemetryText;
    [SerializeField] private bool fetchOnEnable = true;

    private ClusterTelemetry last;

    void OnEnable()
    {
        if (client == null) return;
        client.OnTelemetryReceived += ShowTelemetry;
        if (last != null) ShowTelemetry(last);
        if (fetchOnEnable) client.FetchTelemetry();
    }

    void OnDisable()
    {
        if (client != null)
            client.OnTelemetryReceived -= ShowTelemetry;
    }

    public void Refresh()
    {
        if (client != null) client.FetchTelemetry();
    }

    private void ShowTelemetry(ClusterTelemetry data)
    {
        last = data;
        if (telemetryText == null || data == null) return;

        var sb = new StringBuilder();
        sb.AppendLine($"<b>CLUSTER:</b> {data.cluster}");
        sb.AppendLine($"<size=70%><color=#8FA3B8>{data.timestamp}</color></size>");
        foreach (var n in data.nodes)
        {
            string color = n.status == "Ready" ? "#4CFF8A" : "#FF5A5A";
            sb.AppendLine(
                $"<b>{n.name}</b><pos=21%>CPU {n.cpu:0.0}%<pos=45%>MEM {n.mem:0.0}%" +
                $"<pos=69%>PODS {n.pods}<pos=84%><color={color}>{n.status}</color>");
        }
        telemetryText.text = sb.ToString();
    }
}
