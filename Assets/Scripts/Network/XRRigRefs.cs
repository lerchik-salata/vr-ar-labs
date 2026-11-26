using UnityEngine;

// Single access point to the local player's XR rig.
public class XRRigRefs : MonoBehaviour
{
    public static XRRigRefs Instance { get; private set; }

    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    void Awake()
    {
        Instance = this;
        if (head == null && Camera.main != null) head = Camera.main.transform;
    }
}
