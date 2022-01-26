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
    [UpdateBefore(typeof(DestroyBlankEntityConversion))]
    public class AddTransformConversion : GameObjectConversionSystem
    {
        public struct Translation : IComponentData
        {
            public float3 Value;
        }

        public struct Rotation : IComponentData
        {
            public quaternion Value;
        }

        public struct Scale : IComponentData
        {
            public float Value;
        }

        public struct NonUniformScale : IComponentData
        {
            public float3 Value;
        }

        public struct CompositeScale : IComponentData
        {
            public float4x4 Value;
        }

        public struct LocalToWorld : IComponentData
        {
            public float4x4 Value;
        }

        static void addComponent<TSrc, TDst>(EntityManager em, Func<TSrc, TDst> srcToDst)
            where TSrc : struct, IComponentData
            where TDst : struct, IComponentData
        {
            var desc = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(TSrc) },
                Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled,
            };
            using var q = em.CreateEntityQuery(desc);
            using var ents = q.ToEntityArray(Allocator.Temp);

            em.AddComponent<TDst>(q);
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

            addComponent(em, (Translation src) => new Unity.Transforms.Translation
            {
                Value = float3.zero//src.Value
            });
            addComponent(em, (Rotation src) => new Unity.Transforms.Rotation
            {
                Value = quaternion.identity//src.Value
            });
            addComponent(em, (Scale src) => new Unity.Transforms.Scale
            {
                Value = src.Value
            });
            addComponent(em, (NonUniformScale src) => new Unity.Transforms.NonUniformScale
            {
                Value = src.Value
            });
            addComponent(em, (CompositeScale src) => new Unity.Transforms.CompositeScale
            {
                Value = src.Value
            });
            addComponent(em, (LocalToWorld src) => new Unity.Transforms.LocalToWorld
            {
                Value = src.Value
            });
        }
    }

}
