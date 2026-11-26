using TMPro;
using UnityEngine;

public class WizardController : MonoBehaviour
{
    [SerializeField] private GameObject[] screens;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TelemetryClient telemetryClient;
    [SerializeField] private string userId = "IA-32-salata";

    private int step;
    private int score;
    private float startTime;
    private bool reportSent;

    public int Score => score;
    public int QuestionCount => screens != null ? Mathf.Max(0, screens.Length - 1) : 0;

    void Start()
    {
        Restart();
    }

    void OnEnable()
    {
        if (telemetryClient != null) telemetryClient.OnAuditResponse += OnAuditResponse;
    }

    void OnDisable()
    {
        if (telemetryClient != null) telemetryClient.OnAuditResponse -= OnAuditResponse;
    }

    public void Answer(bool correct)
    {
        if (step >= QuestionCount) return;

        if (correct)
            score++;
        ShowStep(step + 1);
    }

    public void Restart()
    {
        score = 0;
        startTime = Time.time;
        reportSent = false;
        ShowStep(0);
    }

    private void ShowStep(int index)
    {
        if (screens == null || screens.Length == 0) return;

        step = Mathf.Clamp(index, 0, screens.Length - 1);
        for (int i = 0; i < screens.Length; i++)
            if (screens[i] != null)
                screens[i].SetActive(i == step);

        if (step == screens.Length - 1)
            ShowResult();
    }

    private void ShowResult()
    {
        SendReport(score, QuestionCount - score);

        if (resultText == null) return;

        string verdict = score == QuestionCount ? "Cluster is production-ready!"
            : score > 0 ? "Almost there, check the docs and try again."
            : "Pods are crash-looping. Try again!";
        resultText.text = $"Score: {score} / {QuestionCount}\n{verdict}";
    }

    private void SendReport(int score, int errors)
    {
        if (telemetryClient == null || reportSent) return;
        reportSent = true;

        var report = new AuditReport
        {
            userId = userId,
            timeSpentSeconds = Time.time - startTime,
            errors = errors,
            score = score,
            timestamp = System.DateTime.UtcNow.ToString("o"),
        };
        telemetryClient.SendAudit(report);
    }

    private void OnAuditResponse(AuditResponse response)
    {
        if (resultText == null || response == null || string.IsNullOrEmpty(response.auditId)) return;
        resultText.text += $"\n<size=70%>Audit: {response.auditId}</size>";
    }
}
