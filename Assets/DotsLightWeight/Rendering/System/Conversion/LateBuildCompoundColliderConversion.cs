using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using Unity.Collections.LowLevel.Unsafe;
using System;
using Unity.Jobs.LowLevel.Unsafe;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Misc;


    /// <summary>
    /// 
    /// </summary>
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class LateBuildCompoundColliderConversion : GameObjectConversionSystem
    {



        public class TargetData : IComponentData
        {
            public GameObject Dst;
            public IEnumerable<GameObject> Srcs;
        }



        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;
            Debug.Log("aaaaaaa");
            this.Entities.ForEach
            (
                (Entity e, TargetData c) =>
                {
                    Debug.Log(c.Dst.name);
                    var qPartCollider =
                        from src in c.Srcs
                        let tf = src.transform
                        let ptent = this.GetPrimaryEntity(src)
                        let col = em.GetComponentData<PhysicsCollider>(ptent)
                        select new CompoundCollider.ColliderBlobInstance
                        {
                            Collider = col.Value,
                            //Collider = BlobAssetReference<Collider>.Null,
                            CompoundFromChild = new RigidTransform
                            {
                                pos = tf.localPosition,
                                rot = tf.localRotation,
                            },
                        };
                    using var arr = qPartCollider.ToNativeArray(Allocator.Temp);
                    var collider = CompoundCollider.Create(arr);

                    var ent = this.GetPrimaryEntity(c.Dst);
                    em.AddComponentData(ent, new PhysicsCollider
                    {
                        Value = collider,
                    });
                }
            );
        }
    }
}
