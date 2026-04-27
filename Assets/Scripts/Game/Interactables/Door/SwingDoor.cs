using DG.Tweening;
using UnityEngine;

public class SwingDoor : MonoBehaviour
{
    [SerializeField] Transform _leftDoor;
    [SerializeField] Transform _rightDoor;
    [SerializeField] float _openAngle = 90f;
    [SerializeField] float _duration = 0.3f;

    bool _isOpen;

    void OnTriggerEnter(Collider other)
    {
        if (!_isOpen && other.CompareTag("Player"))
        {
            var dot = Vector3.Dot(transform.forward, other.GetComponentInParent<NavMeshMover>().Velocity);
            Open(dot > 0);
            other.GetComponentInParent<NavMeshMover>()?.SetSlowSpeed();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_isOpen && other.CompareTag("Player"))
        {
            Close();
            other.GetComponentInParent<NavMeshMover>()?.SetNormalSpeed();
        }
    }

    void Open(bool fromFront)
    {
        _isOpen = true;
        var angle = fromFront ? _openAngle : -_openAngle;
        _leftDoor.DOLocalRotate(new Vector3(0, -angle, 0), _duration);
        _rightDoor.DOLocalRotate(new Vector3(0, angle, 0), _duration);
    }

    void Close()
    {
        _isOpen = false;
        _leftDoor.DOLocalRotate(Vector3.zero, _duration);
        _rightDoor.DOLocalRotate(Vector3.zero, _duration);
    }
}
