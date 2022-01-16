using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Physics;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using UnityEngine;

using Colider = Unity.Physics.Collider;

namespace DotsLite.Structure
{
    using DotsLite.Dependency;



    public class CompoundColliderInitializeSystem : SystemBase
    {

        

        protected override void OnUpdate()
        {
            var colliders = this.GetComponentDataFromEntity<PhysicsCollider>(isReadOnly: true);


            this.Entities
                .WithAll<Main.ColliderInitializeData>()
                .ForEach((
                    ref DynamicBuffer<Main.PartDestructionResourceData> ress,
                    in DynamicBuffer<Main.ColliderInitializeData> inits) =>
                {
                    for (var i = 0; i < inits.Length; i++)
                    {
                        var res = ress[i];
                        var init = inits[i];
                        ress[i] = new Main.PartDestructionResourceData
                        {
                            ColliderInstance = new CompoundCollider.ColliderBlobInstance
                            {
                                Collider = colliders[init.ChildPartEntity].Value,
                                CompoundFromChild = init.RigidTransform,
                            },
                            PartId = res.PartId,
                            DebrisPrefab = res.DebrisPrefab,
                        };
                    }
                })
                .ScheduleParallel();
        }

    }

}
