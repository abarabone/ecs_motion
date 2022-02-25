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
    /// Marker を頼りに Transform 系のコンポーネントをつけていく
    /// ついでに Marker を消す
    /// </summary>
    [DisableAutoCreation]
    //[UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateAfter(typeof(RemoveTransformAllConversion))]
    public class AddTransformByMarkerConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities
                .ForEach((Entity e, Transform tf) =>
                {
                    //var e = this.GetPrimaryEntity(tf);

                    addAndRemove_<Marker.Translation, Translation>(() => new Translation
                    {
                        Value = tf.position
                    });

                    addAndRemove_<Marker.Rotation, Rotation>(() => new Rotation
                    {
                        Value = tf.rotation
                    });

                    addAndRemove_<Marker.Scale, Scale>(() => new Scale
                    {
                        Value = tf.localScale.magnitude
                    });

                    addAndRemove_<Marker.NonUniformScale, NonUniformScale>(() => new NonUniformScale
                    {
                        Value = tf.localScale
                    });

                    //add_<Marker.CompositeScale, CompositeScale>(() => new CompositeScale
                    //{
                    //    Value = tf.localScale
                    //});


                    void addAndRemove_<TSrc, TDst>(Func<TDst> f)
                        where TSrc : struct, IComponentData
                        where TDst : struct, IComponentData
                    {
                        if (!em.HasComponent<TSrc>(e)) return;

                        em.AddComponentData(e, f());
                        em.RemoveComponent<TSrc>(e);
                    }
                });
        }
    }

}
