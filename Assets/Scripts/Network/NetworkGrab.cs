using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Grabbing a part requests ownership so its movement is streamed to the other clients.
[RequireComponent(typeof(PhotonView), typeof(XRGrabInteractable))]
public class NetworkGrab : MonoBehaviourPun, IPunObservable, IXRSelectFilter
{
    XRGrabInteractable _grab;
    Rigidbody _rb;
    bool _originalKinematic;
    bool _hadAuthority = true;

    NodeIdentity _node;
    AssemblyFeedback _slot;
    bool _slotSearched;
    bool _remoteInstalled;

    // Offline or owner: this client simulates the part
    public bool HasAuthority => !PhotonNetwork.InRoom || photonView.IsMine;
    public bool canProcess => isActiveAndEnabled;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
        _node = GetComponent<NodeIdentity>();
        if (_rb != null) _originalKinematic = _rb.isKinematic;
    }

    void OnEnable()
    {
        _grab.selectEntered.AddListener(OnSelectEntered);
        _grab.selectFilters.Add(this);
    }

    void OnDisable()
    {
        _grab.selectEntered.RemoveListener(OnSelectEntered);
        _grab.selectFilters.Remove(this);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRSocketInteractor) return; // sockets never take ownership
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            photonView.RequestOwnership();
            Debug.Log($"[Photon] Ownership requested for {name} by {PhotonNetwork.NickName}");
        }
    }

    // Only the owner's sockets may hold the part, otherwise two clients fight over it
    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable) =>
        HasAuthority || !(interactor is XRSocketInteractor);

    void LateUpdate()
    {
        bool authority = HasAuthority;
        if (authority != _hadAuthority)
        {
            _hadAuthority = authority;
            if (authority) SetRemoteInstalled(false);
            else ReleaseLocalSelections();
        }

        if (_rb == null || _grab.isSelected) return;
        bool kinematic = authority ? _originalKinematic : true;
        if (_rb.isKinematic != kinematic) _rb.isKinematic = kinematic;
    }

    void ReleaseLocalSelections()
    {
        var manager = _grab.interactionManager;
        if (manager == null) return;
        for (int i = _grab.interactorsSelecting.Count - 1; i >= 0; i--)
            manager.SelectExit(_grab.interactorsSelecting[i], (IXRSelectInteractable)_grab);
    }

    bool InLocalRackSlot()
    {
        foreach (var interactor in _grab.interactorsSelecting)
            if (interactor is XRSocketInteractor socket && socket.GetComponent<RackSlotKey>() != null)
                return true;
        return false;
    }

    AssemblyFeedback Slot()
    {
        if (_slotSearched || _node == null) return _slot;
        _slotSearched = true;
        foreach (var key in FindObjectsByType<RackSlotKey>(FindObjectsInactive.Include))
            if (key.AcceptedNodeId == _node.NodeId) { _slot = key.GetComponent<AssemblyFeedback>(); break; }
        return _slot;
    }

    void SetRemoteInstalled(bool installed)
    {
        if (_remoteInstalled == installed) return;
        _remoteInstalled = installed;
        var slot = Slot();
        if (slot != null) slot.SetRemoteInstalled(installed);
    }

    // Rack slot state travels with the transform stream
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(InLocalRackSlot());
        else
        {
            bool installed = (bool)stream.ReceiveNext();
            if (!HasAuthority) SetRemoteInstalled(installed);
        }
    }
}
