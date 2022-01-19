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



    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class CompoundColliderInitializeSystem : GameObjectConversionSystem
    {



        protected override void OnUpdate()
        {
            var colliders = this.GetComponentDataFromEntity<PhysicsCollider>(isReadOnly: true);
            var resbufs = this.GetBufferFromEntity<Structure.Bone.PartDestructionResourceData>();
            var initbufs = this.GetBufferFromEntity<Structure.Bone.ColliderInitializeData>(isReadOnly: true);


            this.Entities
                .ForEach((Entity ent, Structure.Bone.PartInfoData info) =>
                {
                    var inits = initbufs[ent];
                    var ress = resbufs[ent];
                    for (var i = 0; i < inits.Length; i++)
                    {
                        var init = inits[i];
                        var res = ress[i];
                        ress[i] = new Structure.Bone.PartDestructionResourceData
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
                });
        }

    }

}
