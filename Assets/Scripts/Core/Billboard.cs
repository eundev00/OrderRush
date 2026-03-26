using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        if (Camera.main == null) return;

        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180f, 0); // 뒤집힘 방지
    }
}
