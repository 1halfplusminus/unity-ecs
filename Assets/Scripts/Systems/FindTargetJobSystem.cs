using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//[DisableAutoCreation]
public class FindTargetJobSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBuffer;
    protected override void OnCreate()
    {
        endSimulationEntityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    private struct EntityWithPosition
    {
        public Entity entity;
        public float3 position;
    }
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    [BurstCompile]
    [RequireComponentTag(typeof(UnitData))]
    [ExcludeComponent(typeof(HasTarget))]
    struct FindTargetJobSystemJob : IJobForEachWithEntity<Translation>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;


        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation unitTranslation)
        {
            /* Debug.Log("FindTargetJobSystemJob :" + entity.ToString()); */
            Entity closestTargetEntity = Entity.Null;
            float3 unitPosition = unitTranslation.Value;
            float3 closestTargetPosition = new float3(0);
            for (int i = 0; i < targetArray.Length; i++)
            {
                var targetEntity = targetArray[i].entity;
                var targetTranslation = targetArray[i].position;
                if (closestTargetEntity == Entity.Null)
                {
                    closestTargetEntity = targetEntity;
                    closestTargetPosition = targetTranslation;
                }
                else
                {
                    if (math.distance(unitPosition, targetTranslation) <= math.distance(unitPosition, closestTargetPosition))
                    {
                        closestTargetEntity = targetEntity;
                        closestTargetPosition = targetTranslation;
                    }
                }
            }
            if (closestTargetEntity != Entity.Null)
            {
                entityCommandBuffer.AddComponent(index, entity, new HasTarget() { target = closestTargetEntity });
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        EntityQuery targetQuery = GetEntityQuery(typeof(TargetData), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> targetEntityArray = targetQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> targetTranslation = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<EntityWithPosition> targetArray = new NativeArray<EntityWithPosition>(targetEntityArray.Length, Allocator.TempJob);

        for (int i = 0; i < targetEntityArray.Length; i++)
        {
            targetArray[i] = new EntityWithPosition { entity = targetEntityArray[i], position = targetTranslation[i].Value };
        }

        targetEntityArray.Dispose();
        targetTranslation.Dispose();

        var job = new FindTargetJobSystemJob()
        {
            targetArray = targetArray,
            entityCommandBuffer = endSimulationEntityCommandBuffer.CreateCommandBuffer().ToConcurrent()
        };

        var JobHandle = job.Schedule(this, inputDependencies);
        endSimulationEntityCommandBuffer.AddJobHandleForProducer(JobHandle);
        return JobHandle;
    }
}