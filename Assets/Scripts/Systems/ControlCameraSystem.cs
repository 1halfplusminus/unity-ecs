using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;
using static Unity.Mathematics.math;


public class ControlCameraSystem : JobComponentSystem
{
    float zoomAmount = 1f;
    float maxToClamp = 10f;
    float rotSpeed = 1f;
    [BurstCompile]

    struct ControlCameraSystemJob : IJobForEach<CameraData>
    {
        public float axis;
        public float zoomAmount;
        public float maxToClamp;
        public float rotSpeed;

        public void Execute(ref CameraData cameraData)
        {
            zoomAmount = math.clamp(zoomAmount, -maxToClamp, maxToClamp);
            var translate = math.min(math.abs(axis), maxToClamp - math.abs(zoomAmount));
            var size = (translate * rotSpeed) * math.sign(axis);
            cameraData.size += size;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        var job = new ControlCameraSystemJob()
        {
            axis = Input.GetAxis("Mouse ScrollWheel"),
            maxToClamp = maxToClamp,
            zoomAmount = zoomAmount,
            rotSpeed = rotSpeed
        };

        return job.Schedule(this, inputDependencies);
    }
}