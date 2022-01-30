using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Physics.Authoring;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;
using Unity.Mathematics;
using Unity.Collections;
using System;

namespace DotsLite.EntityTrimmer.Authoring
{
    using Utilities;

    /// <summary>
    /// TransformConversion によって付与される、トランスフォーム系のコンポーネントデータを削除する。
    /// ExcludeTransformConversion とか はよ
    /// </summary>
    /// transform conversion は [UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateBefore(typeof(RemoveTransformAllConversion))]
    public class CopyTransformToMarkerConversion : GameObjectConversionSystem
    {
        static void CopyComponent<TSrc, TDst>(EntityManager em, Func<TSrc, TDst> srcToDst)
            where TSrc : struct, IComponentData
            where TDst : struct, IComponentData
        {
            var desc = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(TSrc), typeof(TDst) },
                Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled,
            };
            using var q = em.CreateEntityQuery(desc);
            using var ents = q.ToEntityArray(Allocator.Temp);

            foreach (var ent in ents)
            {
                var src = em.GetComponentData<TSrc>(ent);
                em.SetComponentData(ent, srcToDst(src));
            }
            em.RemoveComponent<TSrc>(q);
        }

        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            CopyComponent(em, (Translation src) => new Marker.Translation
            {
                Value = src.Value
            });
            CopyComponent(em, (Rotation src) => new Marker.Rotation
            {
                Value = src.Value
            });
            CopyComponent(em, (Scale src) => new Marker.Scale
            {
                Value = src.Value
            });
            CopyComponent(em, (NonUniformScale src) => new Marker.NonUniformScale
            {
                Value = src.Value
            });
            CopyComponent(em, (CompositeScale src) => new Marker.CompositeScale
            {
                Value = src.Value
            });
            CopyComponent(em, (LocalToWorld src) => new Marker.LocalToWorld
            {
                Value = src.Value
            });
        }
    }

}
