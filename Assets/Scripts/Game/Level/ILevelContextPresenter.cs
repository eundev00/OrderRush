using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ILevelContextPresenter
{
    IReadOnlyList<DiningTable> DiningTables { get; }
    Vector3 SpawnPosition { get; }
    Vector3 WaitingPosition { get; }
    Quaternion WaitingRotation { get; }
    Transform LevelTransform { get; }

    UniTask LoadLevelContext(int levelNumber);
}
