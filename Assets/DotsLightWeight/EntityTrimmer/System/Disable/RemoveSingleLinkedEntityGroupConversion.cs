using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace DotsLite.EntityTrimmer.Authoring
{

    /// <summary>
    /// 単体のエンティティなのに LinkedEntityGroup がついてしまうのを避けたい
    /// 今のところあとから消すくらししか思いつかない
    /// prefab 対応
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateAfter(typeof(TrimLinkedEntityConversion))]
    public class RemoveSingleLinkedEntityGroupConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
        //}
        //protected override void OnDestroy()
        //{
            var em = this.DstEntityManager;

            {
                var desc = new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        //typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                        typeof(LinkedEntityGroup)
                    }
                };
                using var q = em.CreateEntityQuery(desc);

                using var ents = q.ToEntityArray(Allocator.Temp);
                foreach (var ent in ents)
                {
                    if (em.GetComponentCount(ent) != 1) continue;

                    //Debug.Log(em.GetName_(ent));
                    em.RemoveComponent<LinkedEntityGroup>(ent);
                    //em.RemoveComponent<ModelPrefabNoNeedLinkedEntityGroupTag>(ent);
                }
            }

            {
                var desc = new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        //typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                        typeof(LinkedEntityGroup),
                        typeof(Prefab)
                    }
                };
                using var q = em.CreateEntityQuery(desc);

                using var ents = q.ToEntityArray(Allocator.Temp);
                foreach (var ent in ents)
                {
                    if (em.GetComponentCount(ent) != 2) continue;

                    //Debug.Log(em.GetName_(ent));
                    em.RemoveComponent<LinkedEntityGroup>(ent);
                    //em.RemoveComponent<ModelPrefabNoNeedLinkedEntityGroupTag>(ent);
                }
            }
        }

    }


    ///// <summary>
    ///// 単体のエンティティなのに LinkedEntityGroup がついてしまうのを避けたい
    ///// 今のところあとから消すくらししか思いつかない
    ///// </summary>
    ////[DisableAutoCreation]
    //[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    //public class NoNeedLinkedEntityGroupCleanUpConversion : GameObjectConversionSystem
    //{
    //    protected override void OnUpdate()
    //    //{ }

    //    //protected override void OnStopRunning()
    //    {
    //        this.Entities
    //            .WithAll<ModelPrefabNoNeedLinkedEntityGroupTag, LinkedEntityGroup, Prefab>()
    //            .ForEach(
    //                (Entity ent) =>
    //                {
    //                    Debug.Log(this.DstEntityManager.GetName_(ent));
    //                    this.DstEntityManager.RemoveComponent<LinkedEntityGroup>(ent);
    //                    this.DstEntityManager.RemoveComponent<ModelPrefabNoNeedLinkedEntityGroupTag>(ent);
    //                }
    //            );

    //        //base.OnStopRunning();
    //    }
    //}

    //// でもだめだった、ここでやっても消えてくれない
    //// よく考えたら、コンバージョンワールドのマネージャーだからだろう…
}