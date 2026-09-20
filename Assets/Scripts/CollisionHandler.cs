using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with object: " + collision.gameObject.name);
        if (rend != null)
            rend.material.color = Color.red;
    }

    void OnCollisionExit(Collision collision)
    {
        if (rend != null)
            rend.material.color = originalColor;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object entered the trigger zone: " + other.gameObject.name);
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Object left the trigger zone: " + other.gameObject.name);
    }
}