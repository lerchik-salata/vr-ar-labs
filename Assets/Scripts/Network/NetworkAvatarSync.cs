using Photon.Pun;
using TMPro;
using UnityEngine;

// Local avatar copies the real XR rig and streams it; remote avatars smoothly follow received poses.
public class NetworkAvatarSync : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField, Range(0.05f, 1f)] private float smoothing = 0.25f;

    private Vector3 hP, lP, rP;
    private Quaternion hR = Quaternion.identity, lR = Quaternion.identity, rR = Quaternion.identity;
    private bool hasData;

    void Start()
    {
        var owner = photonView.Owner;
        name = $"NetworkAvatar ({(owner != null ? owner.NickName : "?")})";
        if (nameLabel != null && owner != null) nameLabel.text = owner.NickName;

        if (photonView.IsMine)
        {
            // Never render your own avatar from the inside
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        }
        else
        {
            Tint(owner != null ? owner.ActorNumber : 0);
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            var rig = XRRigRefs.Instance;
            if (rig == null) return;
            Map(head, rig.head);
            Map(leftHand, rig.leftHand);
            Map(rightHand, rig.rightHand);
        }
        else if (hasData)
        {
            Follow(head, hP, hR);
            Follow(leftHand, lP, lR);
            Follow(rightHand, rP, rR);
            FaceCamera();
        }
    }

    static void Map(Transform target, Transform source)
    {
        if (target != null && source != null) target.SetPositionAndRotation(source.position, source.rotation);
    }

    void Follow(Transform t, Vector3 p, Quaternion q)
    {
        if (t == null) return;
        t.position = Vector3.Lerp(t.position, p, smoothing);
        t.rotation = Quaternion.Slerp(t.rotation, q, smoothing);
    }

    void FaceCamera()
    {
        var cam = Camera.main;
        if (nameLabel == null || cam == null) return;
        nameLabel.transform.rotation = Quaternion.LookRotation(nameLabel.transform.position - cam.transform.position);
    }

    void Tint(int actor)
    {
        var color = Color.HSVToRGB(actor * 0.27f % 1f, 0.65f, 1f);
        var block = new MaterialPropertyBlock();
        foreach (var r in GetComponentsInChildren<MeshRenderer>(true))
        {
            r.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            r.SetPropertyBlock(block);
        }
        if (nameLabel != null) nameLabel.color = color;
    }

    // Owner writes, everyone else reads
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(head.position);      stream.SendNext(head.rotation);
            stream.SendNext(leftHand.position);  stream.SendNext(leftHand.rotation);
            stream.SendNext(rightHand.position); stream.SendNext(rightHand.rotation);
        }
        else
        {
            hP = (Vector3)stream.ReceiveNext(); hR = (Quaternion)stream.ReceiveNext();
            lP = (Vector3)stream.ReceiveNext(); lR = (Quaternion)stream.ReceiveNext();
            rP = (Vector3)stream.ReceiveNext(); rR = (Quaternion)stream.ReceiveNext();

            if (!hasData)
            {
                hasData = true;
                head.SetPositionAndRotation(hP, hR);
                leftHand.SetPositionAndRotation(lP, lR);
                rightHand.SetPositionAndRotation(rP, rR);
            }
        }
    }
}
