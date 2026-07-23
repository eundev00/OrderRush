using UnityEngine;

[ExecuteAlways]
public class Billboard : MonoBehaviour
{
    void Update()
    {
        if (Camera.main == null) return;
        transform.rotation = Camera.main.transform.rotation;
    }
}
