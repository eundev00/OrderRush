using UnityEngine;

public readonly struct MoveEvent
{
    public Vector3 Destination { get; }

    public MoveEvent(Vector3 destination)
    {
        Destination = destination;
    }
}
