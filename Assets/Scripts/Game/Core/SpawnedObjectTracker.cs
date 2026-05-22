using UnityEngine;

public class SpawnedObjectTracker : MonoBehaviour
{
    private SpawnFactory _factory;

    public void Initialize(SpawnFactory factory)
    {
        _factory = factory;
    }

    void OnDestroy()
    {
        if (_factory != null)
        {
            _factory.OnObjectDestroyed(gameObject);
        }
    }
}
