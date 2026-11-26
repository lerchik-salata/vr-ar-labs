using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TelemetryClient : MonoBehaviour
{
    public enum NetStatus { Idle, Loading, Success, Error }

    [SerializeField] private string baseUrl = "http://localhost:8080";
    [SerializeField] private int timeoutSeconds = 5;

    public event Action<NetStatus, string> OnStatusChanged;
    public event Action<ClusterTelemetry> OnTelemetryReceived;
    public event Action<AuditResponse> OnAuditResponse;

    public NetStatus Status { get; private set; } = NetStatus.Idle;
    public string StatusMessage { get; private set; } = "Not connected";

    private void SetStatus(NetStatus s, string msg)
    {
        Status = s;
        StatusMessage = msg;
        OnStatusChanged?.Invoke(s, msg);
        if (s == NetStatus.Error) Debug.LogWarning($"[Net] {s}: {msg}");
        else Debug.Log($"[Net] {s}: {msg}");
    }

    public void FetchTelemetry() => StartCoroutine(FetchTelemetryRoutine());

    private IEnumerator FetchTelemetryRoutine()
    {
        SetStatus(NetStatus.Loading, "Requesting telemetry...");
        using (UnityWebRequest req = UnityWebRequest.Get(baseUrl + "/api/nodes"))
        {
            req.timeout = timeoutSeconds;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                SetStatus(NetStatus.Error, "Connection error: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;
            Debug.Log("[Net] GET /api/nodes -> " + json);

            ClusterTelemetry data = null;
            try { data = JsonUtility.FromJson<ClusterTelemetry>(json); }
            catch (Exception e) { Debug.LogException(e); }

            if (data == null || data.nodes == null)
            {
                SetStatus(NetStatus.Error, "Invalid telemetry JSON");
                yield break;
            }

            SetStatus(NetStatus.Success, $"Telemetry received ({data.nodes.Length} nodes)");
            OnTelemetryReceived?.Invoke(data);
        }
    }

    public void SendAudit(AuditReport report) => StartCoroutine(SendAuditRoutine(report));

    private IEnumerator SendAuditRoutine(AuditReport report)
    {
        SetStatus(NetStatus.Loading, "Sending report...");
        string json = JsonUtility.ToJson(report);
        Debug.Log("[Net] POST /api/audit <- " + json);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(baseUrl + "/api/audit", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                SetStatus(NetStatus.Error, "Send error: " + req.error);
                yield break;
            }

            string text = req.downloadHandler.text;
            Debug.Log("[Net] POST /api/audit -> " + text);

            AuditResponse response = null;
            try { response = JsonUtility.FromJson<AuditResponse>(text); }
            catch (Exception e) { Debug.LogException(e); }

            SetStatus(NetStatus.Success, response != null && !string.IsNullOrEmpty(response.auditId)
                ? $"Report sent ({response.auditId})"
                : "Report sent");
            OnAuditResponse?.Invoke(response);
        }
    }
}
