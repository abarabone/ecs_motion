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
    [UpdateAfter(typeof(RemoveTransformAllConversion))]
    public class CopyTransformToMarkerConversion1 : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            var pos_markers = this.GetComponentDataFromEntity<Marker.Translation>(isReadOnly: true);
            var rot_markers = this.GetComponentDataFromEntity<Marker.Rotation>(isReadOnly: true);
            var scale_markers = this.GetComponentDataFromEntity<Marker.Scale>(isReadOnly: true);
            var scale3_markers = this.GetComponentDataFromEntity<Marker.NonUniformScale>(isReadOnly: true);
            //var scale4x4_markers = this.GetComponentDataFromEntity<Marker.CompositeScale>(isReadOnly: true);

            this.Entities
                .ForEach((Transform tf) =>
                {
                    var ent = this.GetPrimaryEntity(tf);

                    addComponentData_(pos_markers, () => new Translation
                    {
                        Value = tf.position
                    });
                    addComponentData_(rot_markers, () => new Rotation
                    {
                        Value = tf.rotation
                    });
                    addComponentData_(scale_markers, () => new Scale
                    {
                        Value = tf.lossyScale.magnitude
                    });
                    addComponentData_(scale3_markers, () => new NonUniformScale
                    {
                        Value = tf.lossyScale
                    });
                    //addComponentData_(scale4x4_markers, () => new CompositeScale
                    //{
                    //    Value = tf.position
                    //});

                    void addComponentData_<TSrc, TDst>(ComponentDataFromEntity<TSrc> markers, Func<TDst> f)
                        where TSrc : struct, IComponentData
                        where TDst : struct, IComponentData
                    {
                        if (!markers.HasComponent(ent)) return;

                        em.AddComponentData(ent, f());
                    }
                });
        }
    }

}
