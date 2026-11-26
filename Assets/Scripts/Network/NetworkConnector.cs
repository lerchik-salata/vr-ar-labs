using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// Photon: connect -> lobby -> shared room.
public class NetworkConnector : MonoBehaviourPunCallbacks
{
    [SerializeField] private string roomName = "cloud-mesh-ops";
    [SerializeField] private byte maxPlayers = 8;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.NickName = "operator-" + Random.Range(100, 999);
        Debug.Log("[Photon] Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[Photon] Connected to Master. Joining lobby...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[Photon] In lobby. Joining room...");
        var options = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[Photon] Joined '{PhotonNetwork.CurrentRoom.Name}' as {PhotonNetwork.NickName} " +
                  $"({PhotonNetwork.CurrentRoom.PlayerCount} operators online)");
        if (AvatarSpawner.Instance != null) AvatarSpawner.Instance.SpawnAvatar();
        else Debug.LogError("[Photon] AvatarSpawner not found in scene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
        => Debug.LogWarning($"[Photon] Join room failed ({returnCode}): {message}");

    public override void OnPlayerEnteredRoom(Player p)
        => Debug.Log($"[Photon] Operator joined: {p.NickName} (actor {p.ActorNumber})");

    public override void OnPlayerLeftRoom(Player p)
        => Debug.Log($"[Photon] Operator left: {p.NickName} (actor {p.ActorNumber})");

    public override void OnDisconnected(DisconnectCause cause)
        => Debug.LogWarning($"[Photon] Disconnected: {cause}");
}
