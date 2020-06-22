using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace Abarabone.Model.Authoring
{

    //[DisableAutoCreation]
    public class NoNeedLinkedEntityGroupCleanSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            using( var q = em.CreateEntityQuery(
                typeof( ModelPrefabNoNeedLinkedEntityGroupTag ),
                typeof( LinkedEntityGroup ),
                typeof( Prefab ) ) )
            using( var ents = q.ToEntityArray( Allocator.TempJob ) )
            {
                foreach( var ent in ents )
                {
                    em.RemoveComponent<LinkedEntityGroup>( ent );
                    em.RemoveComponent<ModelPrefabNoNeedLinkedEntityGroupTag>( ent );
                }
            }

            this.Enabled = false;
            base.OnStartRunning();
        }

        protected override void OnUpdate()
        { }
    }


    ///// <summary>
    ///// 単体のエンティティなのに LinkedEntityGroup がついてしまうのを避けたい
    ///// 今のところあとから消すくらししか思いつかない
    ///// </summary>
    ////[DisableAutoCreation]
    //public class NoNeedLinkedEntityGroupCleanSystem : SystemBase
    //{
    //    protected override void OnStartRunning()
    //    {
    //        this.Entities
    //            .WithoutBurst()
    //            .WithAll<ModelPrefabNoNeedLinkedEntityGroupTag, LinkedEntityGroup>()
    //            .ForEach(
    //                ( Entity ent ) =>
    //                {
    //                    this.EntityManager.RemoveComponent<LinkedEntityGroup>( ent );
    //                    this.EntityManager.RemoveComponent<ModelPrefabNoNeedLinkedEntityGroupTag>( ent );
    //                }
    //            )
    //            .();

    //        this.Enabled = false;
    //    }

    //    protected override void OnUpdate()
    //    { }
    //}

    // これだと、.ForEach() でマネージャーが使えない、めんどくさい


    ///// <summary>
    ///// 単体のエンティティなのに LinkedEntityGroup がついてしまうのを避けたい
    ///// 今のところあとから消すくらししか思いつかない
    ///// </summary>
    ////[DisableAutoCreation]
    //[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    //public class NoNeedLinkedEntityGroupCleanSystem : GameObjectConversionSystem
    //{
    //    protected override void OnUpdate()
    //    { }

    //    protected override void OnStopRunning()
    //    {
    //        this.Entities
    //            .WithAll( typeof( ModelPrefabNoNeedLinkedEntityGroupTag ) )
    //            .ForEach(
    //                (Entity ent) =>
    //                {
    //                    this.EntityManager.RemoveComponent<LinkedEntityGroup>( ent );
    //                    this.EntityManager.RemoveComponent<ModelPrefabNoNeedLinkedEntityGroupTag>( ent );
    //                }
    //            );

    //        base.OnStopRunning();
    //    }
    //}

    // でもだめだった、ここでやっても消えてくれない
    // よく考えたら、コンバージョンワールドのマネージャーだからだろう…
}