using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class QuadrantSystem : JobComponentSystem
{
    private struct QuandrantEntity
    {
        Entity entity;
        int index;
    }
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;
    protected override void OnCreate()
    {
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    struct QuadrantSystemJob : IJobForEachWithEntity<Translation>
    {
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public NativeMultiHashMap<int, Entity>.ParallelWriter quandrantMultiHashMap;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            int hashMapKey = GetPositionHasMapKey(translation.Value);
            quandrantMultiHashMap.Add(hashMapKey, entity);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        EntityQuery query = GetEntityQuery(typeof(Translation));
        NativeMultiHashMap<int, Entity> quandrantMultiHashMap = new NativeMultiHashMap<int, Entity>(query.CalculateEntityCountWithoutFiltering(), Allocator.TempJob);
        var job = new QuadrantSystemJob() { entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent(), quandrantMultiHashMap = quandrantMultiHashMap.AsParallelWriter() };
        var schedule = job.Schedule(this, inputDependencies);
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(schedule);
        schedule.Complete();
        var currentMousePosition = Camera.main.ScreenToWorldPoint(
                 new Vector3(Input.mousePosition.x,
                 Input.mousePosition.y,
                 Camera.main.nearClipPlane));
        DebugDrawQuadrant(currentMousePosition);
        Debug.Log(quandrantMultiHashMap.CountValuesForKey(GetPositionHasMapKey(currentMousePosition)));
        return schedule;
    }

    const int quadrantYMultiplier = 1000;
    const int quadrantCellSize = 5;
    private static void DebugDrawQuadrant(float3 position)
    {
        Vector3 lowerLeft = new Vector3(math.floor(position.x / quadrantCellSize) * quadrantCellSize, (position.y / quadrantCellSize) * quadrantCellSize, (position.z / quadrantCellSize) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, +0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+1, +0) * quadrantCellSize, lowerLeft + new Vector3(+1, +1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(+0, +1) * quadrantCellSize, lowerLeft + new Vector3(+1, +1) * quadrantCellSize);

        Debug.Log(GetPositionHasMapKey(position) + " " + position);
    }
    private static int GetPositionHasMapKey(float3 position)
    {
        return (int)(math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.y / quadrantCellSize)));
    }

}