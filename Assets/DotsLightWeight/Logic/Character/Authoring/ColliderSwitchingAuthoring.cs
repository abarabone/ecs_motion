using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

namespace DotsLite.Character.Authoring
{
    using DotsLite.Model.Authoring;
    using DotsLite.Character;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;
    using DotsLite.Targeting;
    using DotsLite.Collision;
    using DotsLite.Arms.Authoring;

    using Collide = Unity.Physics.Collider;

    public struct ColliderSwitchTargetData : IComponentData
    {
        public ColliderSwitchingAuthoring.Mode Mode;
        public Entity Entity;
    }

    /// <summary>
    /// 
    /// </summary>
    public class ColliderSwitchingAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public Mode mode;

        public enum Mode
        {
            none,
            deading,
        }


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //var em = dstManager;
            //var gcs = conversionSystem;

            //var top = this.FindParent<ModelGroupAuthoring.ModelAuthoringBase>();
            //var state = top.GetComponentInChildren<ActionStateAuthoring>();
            //var posture = top.GetComponentInChildren<PostureAuthoring>();

            //var binderEntity = gcs.GetPrimaryEntity(top);
            //var stateEntity = gcs.GetOrCreateEntity(state);
            //var postureEntity = gcs.GetPrimaryEntity(posture);


            //em.AddComponentData(entity, new ColliderSwitchTargetData
            //{
            //    Mode = this.mode,
            //    Entity = stateEntity,
            //});
        }
    }


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateBefore(typeof(NoNeedLinkedEntityGroupCleanUpSystem))]
    public class MakeColliderBankSystem : GameObjectConversionSystem
    {
        //protected override void OnUpdate()
        //{
        //    var em = this.DstEntityManager;

        //    var desc0 = new EntityQueryDesc
        //    {
        //        All = new ComponentType[]
        //        {
        //            typeof(ColliderSwitchTargetData),
        //        }
        //    };
        //    using var q = em.CreateEntityQuery(desc0);

        //    using var ents = q.ToEntityArray(Allocator.Temp);
        //    foreach (var ent in ents)
        //    {
        //        var cst = em.GetComponentData<ColliderSwitchTargetData>(ent);
        //        var collider = em.GetComponentData<PhysicsCollider>(ent);

        //        em.DestroyEntity(ent);

        //        switch (cst.Mode)
        //        {
        //            case ColliderSwitchingAuthoring.Mode.deading:

        //                em.AddComponentData(cst.Entity, new ColliderBank.DeadData
        //                {
        //                    Collider = collider.Value,
        //                });

        //                break;
        //        }
        //    }
        //}
        protected override void OnUpdate()
        {
            this.Entities
                .ForEach((ColliderSwitchingAuthoring sw) =>
                {

                    var em = this.DstEntityManager;
                    var gcs = this;

                    var top = sw.FindParent<ModelGroupAuthoring.ModelAuthoringBase>();
                    var state = top.GetComponentInChildren<ActionStateAuthoring>();
                    var posture = top.GetComponentInChildren<PostureAuthoring>();

                    var binderEntity = gcs.GetPrimaryEntity(top);
                    var stateEntity = gcs.GetOrCreateEntity(state);
                    var postureEntity = gcs.GetPrimaryEntity(posture);

                    var ent = gcs.GetPrimaryEntity(sw);

                    switch (sw.mode)
                    {
                        case ColliderSwitchingAuthoring.Mode.deading:

                            em.AddComponentData(stateEntity, new ColliderBank.DeadData
                            {
                                Collider = em.GetComponentData<PhysicsCollider>(ent).Value,
                            });

                            break;
                    }

                    em.DestroyEntity(ent);
                });
        }
    }
}
