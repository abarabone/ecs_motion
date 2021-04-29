using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using System.Runtime.InteropServices;

namespace DotsLite.Draw.Authoring
{
    public class DrawTransformBufferDefineAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public int TransformBufferMaxVectorLength;

        public bool UseTempJobNativeBuffer;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            switch (this.UseTempJobNativeBuffer)
            {
                case true:
                    initEntityTempJob_(entity, dstManager);
                    initComputeBufferComponent_(entity, dstManager, this.TransformBufferMaxVectorLength);
                    removeComponents_(entity, dstManager);
                    break;

                case false:
                    initEntityStatic_(entity, dstManager);
                    initNativeBufferComponent_(entity, dstManager, this.TransformBufferMaxVectorLength);
                    initComputeBufferComponent_(entity, dstManager, this.TransformBufferMaxVectorLength);
                    removeComponents_(entity, dstManager);
                    break;
            }
            return;


            static void initEntityTempJob_(Entity ent, EntityManager em)
            {
                var types = new ComponentTypes(
                    typeof(DrawSystem.TransformBufferUseTempJobTag),
                    typeof(DrawSystem.ComputeTransformBufferData),
                    typeof(DrawSystem.NativeTransformBufferData)
                );
                em.AddComponents(ent, types);
                em.SetName_(ent, "draw system");

            }
            static void initEntityStatic_(Entity ent, EntityManager em)
            {
                var types = new ComponentTypes(
                    typeof(DrawSystem.ComputeTransformBufferData),
                    typeof(DrawSystem.NativeTransformBufferData)
                );
                em.AddComponents(ent, types);
                em.SetName_(ent, "draw system");
            }

            static void initNativeBufferComponent_(Entity ent, EntityManager em, int vectorLength)
            {
                em.SetComponentData(ent,
                    new DrawSystem.NativeTransformBufferData
                    {
                        Transforms = new SimpleNativeBuffer<float4>(vectorLength, Allocator.Persistent),
                    }
                );
            }

            static void initComputeBufferComponent_(Entity ent, EntityManager em, int vectorLength)
            {
                var stride = Marshal.SizeOf(typeof(float4));

                em.SetComponentData(ent,
                    new DrawSystem.ComputeTransformBufferData
                    {
                        Transforms = new ComputeBuffer
                            (vectorLength, stride, ComputeBufferType.Default, ComputeBufferMode.Immutable),
                    }
                );
            }

            static void removeComponents_(Entity ent, EntityManager em)
            {
                em.RemoveComponent<Translation>(ent);
                em.RemoveComponent<Rotation>(ent);
                em.RemoveComponent<LocalToWorld>(ent);
            }
        }
    }
}