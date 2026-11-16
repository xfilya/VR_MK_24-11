using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(MeshCollider), typeof(XRGrabInteractable))]
public class PistolSlide : MonoBehaviour
{
    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private Camera _interactionCamera;
    [SerializeField] private Pistol _pistol;

    private XRGrabInteractable _grab;
    private Transform _grabAttach;
    private Rigidbody _rigidbody;
    private Vector3 _attachPositionAtGrab;
    private float _progressAtGrab;
    private float _progress;
    private bool _mouseDragging;
    private Vector2 _mouseStartPosition;
    private Vector2 _mouseDragAxis;
    private float _mouseDragLength;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (!_rigidbody)
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (!meshCollider)
            meshCollider = gameObject.AddComponent<MeshCollider>();
        if (!meshCollider.sharedMesh && TryGetComponent(out MeshFilter meshFilter))
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        meshCollider.convex = true;

        _grab = GetComponent<XRGrabInteractable>();
        if (!_grab)
            _grab = gameObject.AddComponent<XRGrabInteractable>();

        if (startPos)
            transform.position = startPos.position;
        else
            Debug.LogError("PistolSlide: назначь Start Pos в инспекторе.", this);

        if (!endPos)
            Debug.LogError("PistolSlide: назначь End Pos в инспекторе.", this);

        _grab.trackPosition = false;
        _grab.trackRotation = false;
        _grab.selectEntered.AddListener(BeginDrag);
        _grab.selectExited.AddListener(EndDrag);
    }

    private void OnDestroy()
    {
        if (!_grab) return;
        _grab.selectEntered.RemoveListener(BeginDrag);
        _grab.selectExited.RemoveListener(EndDrag);
    }

    private void Update()
    {
        if (_mouseDragging)
        {
            UpdateMouseDrag();
            return;
        }

        if (_grab && _grab.isSelected && _grabAttach)
        {
            GetEndpoints(out Vector3 start, out Vector3 end);
            Vector3 path = end - start;
            float lengthSquared = path.sqrMagnitude;
            if (lengthSquared <= Mathf.Epsilon) return;

            Vector3 movement = _grabAttach.position - _attachPositionAtGrab;
            _progress = Mathf.Clamp01(_progressAtGrab + Vector3.Dot(movement, path) / lengthSquared);
            transform.position = Vector3.Lerp(start, end, _progress);
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryBeginMouseDrag();
    }

    private void UpdateMouseDrag()
    {
        if (Mouse.current == null)
        {
            FinishDrag(true);
            return;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            float pixelDelta = Vector2.Dot(Mouse.current.position.ReadValue() - _mouseStartPosition, _mouseDragAxis);
            _progress = Mathf.Clamp01(_progressAtGrab + pixelDelta / _mouseDragLength);
            SetPositionAtProgress(_progress);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            FinishDrag(false);
    }

    private void TryBeginMouseDrag()
    {
        if (_grab && _grab.isSelected) return;

        Camera viewCamera = _interactionCamera ? _interactionCamera : Camera.main;
        if (!viewCamera)
            viewCamera = FindAnyObjectByType<Camera>();
        if (!viewCamera) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = viewCamera.ScreenPointToRay(mousePosition);
        bool hitSlide = false;
        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            if (hit.collider.GetComponentInParent<PistolSlide>() != this)
                continue;
            hitSlide = true;
            break;
        }
        if (!hitSlide)
            return;

        _mouseDragging = true;
        _mouseStartPosition = mousePosition;
        GetEndpoints(out Vector3 start, out Vector3 end);
        Vector3 path = end - start;
        _progressAtGrab = path.sqrMagnitude > Mathf.Epsilon
            ? Mathf.Clamp01(Vector3.Dot(transform.position - start, path) / path.sqrMagnitude)
            : 0f;
        _progress = _progressAtGrab;

        Vector2 screenPath = (Vector2)viewCamera.WorldToScreenPoint(end) -
            (Vector2)viewCamera.WorldToScreenPoint(start);
        _mouseDragLength = screenPath.magnitude;
        _mouseDragAxis = _mouseDragLength > 1f ? screenPath / _mouseDragLength : Vector2.up;
        if (_mouseDragLength <= 1f)
            _mouseDragLength = 150f;
    }

    private void BeginDrag(SelectEnterEventArgs args)
    {
        _grabAttach = args.interactorObject.GetAttachTransform(args.interactableObject);
        _attachPositionAtGrab = _grabAttach.position;
        GetEndpoints(out Vector3 start, out Vector3 end);
        Vector3 path = end - start;
        _progressAtGrab = path.sqrMagnitude > Mathf.Epsilon
            ? Mathf.Clamp01(Vector3.Dot(transform.position - start, path) / path.sqrMagnitude)
            : 0f;
        _progress = _progressAtGrab;
    }

    private void EndDrag(SelectExitEventArgs args)
    {
        FinishDrag(args.isCanceled);
        _grabAttach = null;
    }

    private void FinishDrag(bool canceled)
    {
        if (!canceled && _progress >= 0.98f)
        {
            SetPositionAtProgress(1f);
            Slide();
        }

        _progress = 0f;
        SetPositionAtProgress(0f);
        _mouseDragging = false;
    }

    private void SetPositionAtProgress(float progress)
    {
        GetEndpoints(out Vector3 start, out Vector3 end);
        transform.position = Vector3.Lerp(start, end, progress);
    }

    private void GetEndpoints(out Vector3 start, out Vector3 end)
    {
        start = startPos ? startPos.position : transform.position;
        end = endPos ? endPos.position : start;
    }

    public void Slide()
    {
        if (_pistol.IsBulletInChamber)
        {
            
        }
        else{}
    }
}
