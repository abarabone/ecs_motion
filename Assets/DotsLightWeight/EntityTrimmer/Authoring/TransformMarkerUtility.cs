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

    public static class Marker
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


        public static void CopyTransformToMarker(this EntityManager em, Entity ent, Transform tf)
        {
            SetTransformMarkerValue_<Marker.Translation, Translation>(() => new Translation
            {
                Value = tf.position
            });

            SetTransformMarkerValue_<Marker.Rotation, Rotation>(() => new Rotation
            {
                Value = tf.rotation
            });

            SetTransformMarkerValue_<Marker.Scale, Scale>(() => new Scale
            {
                Value = tf.localScale.magnitude
            });

            SetTransformMarkerValue_<Marker.NonUniformScale, NonUniformScale>(() => new NonUniformScale
            {
                Value = tf.localScale
            });

            //add_<Marker.CompositeScale, CompositeScale>(() => new CompositeScale
            //{
            //    Value = tf.localScale
            //});

            void SetTransformMarkerValue_<TSrc, TDst>(Func<TDst> f)
                where TSrc : struct, IComponentData
                where TDst : struct, IComponentData
            {
                if (!em.HasComponent<TSrc>(ent)) return;

                em.SetComponentData(ent, f());
            }
        }

    }

}
