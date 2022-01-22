//using System.Collections.Generic;
//using Unity.Entities;
//using Unity.Collections;
//using UnityEngine;
//using Unity.Physics.Authoring;
//using Unity.Transforms;
//using Unity.Entities.Conversion;
//using Unity.Entities.Hybrid;
//using System.Linq;

//namespace DotsLite.Model.Authoring
//{

//    /// <summary>
//    /// 単体のエンティティなのに LinkedEntityGroup がついてしまうのを避けたい
//    /// 今のところあとから消すくらししか思いつかない
//    /// </summary>
//    //[DisableAutoCreation]
//    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
//    public class NoNeedLinkedEntityGroupCleanUpSystem : GameObjectConversionSystem
//    {
//        protected override void OnUpdate()
//        { }
//        protected override void OnDestroy()
//        {
//            var em = this.DstEntityManager;

//            var desc0 = new EntityQueryDesc
//            {
//                All = new ComponentType[]
//                {
//                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
//                    typeof(LinkedEntityGroup),
//                    typeof(Prefab)
//                }
//            };
//            var desc1 = new EntityQueryDesc
//            {
//                All = new ComponentType[]
//                {
//                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
//                    typeof(LinkedEntityGroup)
//                }
//            };
//            using var q = em.CreateEntityQuery(desc0, desc1);

//            using var ents = q.ToEntityArray(Allocator.Temp);
//            foreach (var ent in ents)
//            {
//                //Debug.Log(em.GetName_(ent));
//                em.RemoveComponent<LinkedEntityGroup>(ent);
//                em.RemoveComponent<ModelPrefabNoNeedLinkedEntityGroupTag>(ent);
//            }
//        }

//    }


//    ///// <summary>
//    ///// 単体のエンティティなのに LinkedEntityGroup がついてしまうのを避けたい
//    ///// 今のところあとから消すくらししか思いつかない
//    ///// </summary>
//    ////[DisableAutoCreation]
//    //[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
//    //public class NoNeedLinkedEntityGroupCleanUpConversion : GameObjectConversionSystem
//    //{
//    //    protected override void OnUpdate()
//    //    //{ }

//    //    //protected override void OnStopRunning()
//    //    {
//    //        this.Entities
//    //            .WithAll<ModelPrefabNoNeedLinkedEntityGroupTag, LinkedEntityGroup, Prefab>()
//    //            .ForEach(
//    //                (Entity ent) =>
//    //                {
//    //                    Debug.Log(this.DstEntityManager.GetName_(ent));
//    //                    this.DstEntityManager.RemoveComponent<LinkedEntityGroup>(ent);
//    //                    this.DstEntityManager.RemoveComponent<ModelPrefabNoNeedLinkedEntityGroupTag>(ent);
//    //                }
//    //            );

//    //        //base.OnStopRunning();
//    //    }
//    //}

//    //// でもだめだった、ここでやっても消えてくれない
//    //// よく考えたら、コンバージョンワールドのマネージャーだからだろう…
//}