using Photon.Pun;
using UnityEngine;

public class AvatarSpawner : MonoBehaviour
{
    public static AvatarSpawner Instance { get; private set; }

    [SerializeField] private string avatarPrefabName = "NetworkAvatar"; // Assets/Resources/
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float operatorSpacing = 1.2f;

    void Awake() => Instance = this;

    public void SpawnAvatar()
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        // Each operator stands at their own spot: 0, +1, -1, +2, ... * spacing
        int i = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % 8;
        int step = (i + 1) / 2 * (i % 2 == 1 ? 1 : -1);
        pos += rot * Vector3.right * (step * operatorSpacing);

        var rig = XRRigRefs.Instance;
        if (rig != null) rig.transform.SetPositionAndRotation(pos, rot);

        var avatar = PhotonNetwork.Instantiate(avatarPrefabName, pos, rot);
        Debug.Log($"[Photon] Avatar spawned: {avatar.name} at {pos}");
    }
}
