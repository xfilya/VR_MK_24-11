using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(MeshCollider), typeof(XRGrabInteractable))]
public class PistolSlide : MonoBehaviour
{
    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private Vector3 _slideDirection = Vector3.back;
    [SerializeField, Min(0.001f)] private float _slideDistance = 0.04f;

    private XRGrabInteractable _grab;
    private Transform _grabAttach;
    private Rigidbody _rigidbody;
    private Vector3 _fallbackStart;
    private Vector3 _fallbackEnd;
    private Vector3 _attachPositionAtGrab;
    private float _progressAtGrab;
    private float _progress;

    private void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        GetComponent<MeshCollider>().convex = true;

        _fallbackStart = transform.localPosition;
        _fallbackEnd = _fallbackStart + _slideDirection.normalized * _slideDistance;
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
        if (!_grab || !_grab.isSelected || !_grabAttach) return;

        GetEndpoints(out Vector3 start, out Vector3 end);
        Vector3 path = end - start;
        float lengthSquared = path.sqrMagnitude;
        if (lengthSquared <= Mathf.Epsilon) return;

        Vector3 movement = _grabAttach.position - _attachPositionAtGrab;
        _progress = Mathf.Clamp01(_progressAtGrab + Vector3.Dot(movement, path) / lengthSquared);
        transform.position = Vector3.Lerp(start, end, _progress);
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
        if (args.isCanceled)
        {
            _progress = 0f;
            SetPositionAtProgress(0f);
        }
        else if (_progress >= 0.98f)
        {
            SetPositionAtProgress(1f);
            Slide();
        }
        else
        {
            _progress = 0f;
            SetPositionAtProgress(0f);
        }

        _grabAttach = null;
    }

    private void SetPositionAtProgress(float progress)
    {
        GetEndpoints(out Vector3 start, out Vector3 end);
        transform.position = Vector3.Lerp(start, end, progress);
    }

    private void GetEndpoints(out Vector3 start, out Vector3 end)
    {
        start = startPos ? startPos.position : transform.parent
            ? transform.parent.TransformPoint(_fallbackStart)
            : _fallbackStart;
        end = endPos ? endPos.position : transform.parent
            ? transform.parent.TransformPoint(_fallbackEnd)
            : _fallbackEnd;
    }

    public void Slide()
    {
        
    }
}
