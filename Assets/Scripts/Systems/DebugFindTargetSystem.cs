using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class DebugFindTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var componentDataFromEntity = GetComponentDataFromEntity<Translation>(true);
        Entities.WithAll<HasTarget>().ForEach((Entity entity, ref Translation translation, ref HasTarget hasTarget) =>
        {
            Debug.Log("here");
            if (componentDataFromEntity.Exists(hasTarget.target))
            {
                Debug.Log("here");
                Translation targetPosition = componentDataFromEntity[hasTarget.target];
                Debug.DrawLine(targetPosition.Value, translation.Value);
            }
        });
    }
}