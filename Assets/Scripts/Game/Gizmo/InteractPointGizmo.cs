using UnityEngine;

/// <summary>
/// InteractPoint를 Gizmo로 시각화하는 컴포넌트
/// GameObject에 추가하면 지정된 Transform 위치에 Gizmo 표시
/// </summary>
public class InteractPointGizmo : MonoBehaviour
{
    [Header("Gizmo Settings")]
    [SerializeField] Color _normalColor = Color.magenta;
    [SerializeField] Color _selectedColor = Color.yellow;
    [SerializeField] float _radius = 0.3f;
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = _normalColor;

    //     if (_drawWireOnNormal)
    //         Gizmos.DrawWireSphere(transform.position, _radius);
    //     else
    //         Gizmos.DrawSphere(transform.position, _radius);
    // }

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = _normalColor;
    //     UnityEditor.Handles.color = _normalColor;
    //     UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _radius);
    // }

    void OnDrawGizmos()
    {
        // 반투명 채움
        Gizmos.color = new Color(_normalColor.r, _normalColor.g, _normalColor.b, 0.3f);
        Gizmos.DrawSphere(transform.position, _radius);

        // 외곽선
        Gizmos.color = _normalColor;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = _selectedColor;
        Gizmos.DrawSphere(transform.position, _radius * 1.2f);
    }
}
