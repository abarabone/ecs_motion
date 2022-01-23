using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Physics;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace DotsLite.Model.Authoring
{

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class DynamicToStaticPhysicsBodyConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities.ForEach
            (
                (Entity e, DynamicToStaticPhysicsBody c) =>
                {
                    foreach (var tf in c.GetComponentsInChildren<Transform>())
                    {
                        //Debug.Log(tf.name);
                        var ent = this.GetPrimaryEntity(tf);

                        em.RemoveComponent<PhysicsVelocity>(ent);

                    }
                }
            );

        }
    }

}
