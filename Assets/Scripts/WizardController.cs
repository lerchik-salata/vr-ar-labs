using TMPro;
using UnityEngine;

public class WizardController : MonoBehaviour
{
    [SerializeField] private GameObject[] screens;
    [SerializeField] private TMP_Text resultText;

    private int step;
    private int score;

    public int Score => score;
    public int QuestionCount => screens != null ? Mathf.Max(0, screens.Length - 1) : 0;

    void Start()
    {
        Restart();
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
        if (resultText == null) return;

        string verdict = score == QuestionCount ? "Cluster is production-ready!"
            : score > 0 ? "Almost there, check the docs and try again."
            : "Pods are crash-looping. Try again!";
        resultText.text = $"Score: {score} / {QuestionCount}\n{verdict}";
    }
}
