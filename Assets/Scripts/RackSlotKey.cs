using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class RackSlotKey : MonoBehaviour, IXRSelectFilter
{
    [SerializeField, Min(1)] private int acceptedNodeId = 1;
    [SerializeField] private AssemblyFeedback feedback;
    [SerializeField] private bool playSounds = true;
    [SerializeField] private float buzzCooldown = 0.6f;

    XRSocketInteractor _socket;
    bool _defaultHoverMeshes;
    float _lastBuzz = float.NegativeInfinity;
    readonly HashSet<IXRHoverInteractable> _wrongHovers = new HashSet<IXRHoverInteractable>();

    public int AcceptedNodeId => acceptedNodeId;
    public bool canProcess => isActiveAndEnabled;

    void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();
        if (feedback == null) feedback = GetComponent<AssemblyFeedback>();
        _defaultHoverMeshes = _socket.showInteractableHoverMeshes;
    }

    void OnEnable()
    {
        _socket.selectFilters.Add(this);
        _socket.hoverEntered.AddListener(OnHoverEntered);
        _socket.hoverExited.AddListener(OnHoverExited);
    }

    void OnDisable()
    {
        _socket.selectFilters.Remove(this);
        _socket.hoverEntered.RemoveListener(OnHoverEntered);
        _socket.hoverExited.RemoveListener(OnHoverExited);
        _wrongHovers.Clear();
        RefreshRejected();
    }

    public bool Accepts(Transform candidate)
    {
        if (candidate == null) return false;
        var id = candidate.GetComponentInParent<NodeIdentity>();
        return id != null && id.NodeId == acceptedNodeId;
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable) =>
        Accepts(interactable.transform);

    void OnHoverEntered(HoverEnterEventArgs args)
    {
        var t = args.interactableObject.transform;
        if (Accepts(t) || !_wrongHovers.Add(args.interactableObject)) return;
        RefreshRejected();

        if (playSounds && Time.time - _lastBuzz > buzzCooldown)
        {
            _lastBuzz = Time.time;
            ProceduralSfx.Play(ProceduralSfx.Kind.Error, transform.position);
        }
        var id = t.GetComponentInParent<NodeIdentity>();
        Debug.Log($"[RackSlot] {(id != null ? id.DisplayName : t.name)} не підходить до {name} — цей слот для node-{acceptedNodeId}", this);
    }

    void OnHoverExited(HoverExitEventArgs args)
    {
        if (_wrongHovers.Remove(args.interactableObject)) RefreshRejected();
    }

    void RefreshRejected()
    {
        bool rejected = _wrongHovers.Count > 0;
        if (_socket != null) _socket.showInteractableHoverMeshes = !rejected && _defaultHoverMeshes;
        if (feedback != null) feedback.ShowRejected(rejected);
    }
}
