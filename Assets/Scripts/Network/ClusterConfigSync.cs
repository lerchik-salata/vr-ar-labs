using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Shares the cluster config through Photon room properties, so late joiners get it too.
public class ClusterConfigSync : MonoBehaviourPunCallbacks
{
    const string KeyNs = "cfg.ns";
    const string KeyReplicas = "cfg.replicas";
    const string KeyAutoscale = "cfg.autoscale";
    const string KeyPlacement = "cfg.placement";

    [SerializeField] private ClusterConfigApplier applier;

    public void Publish(ClusterConfig config)
    {
        if (!PhotonNetwork.InRoom)
        {
            applier.Apply(config);
            return;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { KeyNs, config.ns },
            { KeyReplicas, config.replicas },
            { KeyAutoscale, config.autoscale },
            { KeyPlacement, config.placement },
        });
        Debug.Log($"[Photon] Cluster config published by {PhotonNetwork.NickName}");
    }

    public override void OnJoinedRoom() => TryApply(PhotonNetwork.CurrentRoom.CustomProperties);

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (changed.ContainsKey(KeyNs)) TryApply(PhotonNetwork.CurrentRoom.CustomProperties);
    }

    void TryApply(Hashtable props)
    {
        if (applier == null || props == null || !props.ContainsKey(KeyNs)) return;
        applier.Apply(new ClusterConfig
        {
            ns = (string)props[KeyNs],
            replicas = (int)props[KeyReplicas],
            autoscale = (bool)props[KeyAutoscale],
            placement = props[KeyPlacement] as int[] ?? new int[0],
        });
    }
}
