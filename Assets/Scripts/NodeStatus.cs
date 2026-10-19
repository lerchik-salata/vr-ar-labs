using UnityEngine;

public class NodeStatus : MonoBehaviour
{
    [SerializeField] private string nodeName = "worker-1";
    [SerializeField] private Renderer[] podLeds;

    [SerializeField] private Color healthyColor = new Color(0f, 1f, 0.53f);
    [SerializeField] private Color trafficColor = new Color(1f, 0.69f, 0.13f);
    [SerializeField] private float emission = 3f;
    [SerializeField] private float flashTime = 1.5f;
    [SerializeField] private float pulseSpeed = 2f;

    private int packets;
    private float flashUntil;
    private MaterialPropertyBlock block;

    void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (podLeds == null) return;

        bool busy = Time.time < flashUntil;
        Color color = busy ? trafficColor : healthyColor;

        for (int i = 0; i < podLeds.Length; i++)
        {
            if (podLeds[i] == null) continue;
            float pulse = busy
                ? Mathf.PingPong(Time.time * 8f + i * 0.25f, 1f)
                : 0.6f + 0.4f * Mathf.Sin(Time.time * pulseSpeed + i);

            podLeds[i].GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_EmissionColor", color * emission * pulse);
            podLeds[i].SetPropertyBlock(block);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == null) return;

        packets++;
        flashUntil = Time.time + flashTime;
        Debug.Log($"[{nodeName}] packet received from {collision.gameObject.name}, total: {packets}");
    }
}
