using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ILevelContextPresenter
{
    List<DiningTable> DiningTables { get; }
    ServingCounter[] ServingCounters { get; }
    Counter[] KitchenCounters { get; }
    Vector3 SpawnPosition { get; }
    Transform[] StaffIdlePoints { get; }
    Vector3 WaitingPosition { get; }
    Quaternion WaitingRotation { get; }
    Transform LevelTransform { get; }

    UniTask LoadLevelContext(int levelNumber);
    UniTask AddTableFromEffect(TableAdditionEffect effect);
}
