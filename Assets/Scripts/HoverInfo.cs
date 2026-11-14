using UnityEngine;

public class HoverInfo : MonoBehaviour
{
    [SerializeField] private GameObject card;
    [SerializeField] private bool hideOnStart = true;

    void Start()
    {
        if (card != null && hideOnStart)
            card.SetActive(false);
    }

    public void ShowCard()
    {
        if (card != null)
            card.SetActive(true);
    }

    public void HideCard()
    {
        if (card != null)
            card.SetActive(false);
    }
}
