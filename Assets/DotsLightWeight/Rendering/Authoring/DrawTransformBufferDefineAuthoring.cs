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

        public bool UseTempJobNativeBuffer;// コンピュートバッファーは TransformBufferMaxVectorLength 固定なのでいまいち感ある
        public bool UseDrawInstanceSort;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initEntity_(entity, dstManager, this.UseTempJobNativeBuffer, this.UseDrawInstanceSort);
            
            initNativeBufferComponent_(entity, dstManager, this.TransformBufferMaxVectorLength, this.UseTempJobNativeBuffer);
            
            initComputeBufferComponent_(entity, dstManager, this.TransformBufferMaxVectorLength);

            initSortingBufferComponent_(entity, dstManager, this.TransformBufferMaxVectorLength, this.UseTempJobNativeBuffer, this.UseDrawInstanceSort);

            return;


            static void initEntity_(Entity ent, EntityManager em, bool useTempBuffer, bool useSort)
            {
                var types = new List<ComponentType>() {
                    typeof(DrawSystem.ComputeTransformBufferData),
                    typeof(DrawSystem.NativeTransformBufferData)
                };
                if (useTempBuffer) types.Add(typeof(DrawSystem.TransformBufferUseTempJobTag));
                if (useSort) types.Add(typeof(DrawSystem.SortingNativeTransformBufferData));

                em.AddComponents(ent, new ComponentTypes(types.ToArray()));
                em.SetName_(ent, "draw system");

            }

            static void initNativeBufferComponent_(
                Entity ent, EntityManager em, int vectorLength, bool useTempBuffer)
            {
                if (!useTempBuffer) return;

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

            static void initSortingBufferComponent_(
                Entity ent, EntityManager em, int vectorLength, bool useTempBuffer, bool useSort)
            {
                if (!useSort) return;

                em.SetComponentData(ent,
                    new DrawSystem.SortingNativeTransformBufferData
                    {
                        Transforms = useTempBuffer
                            ? new SimpleNativeBuffer<float4>(vectorLength, Allocator.Persistent)
                            : new SimpleNativeBuffer<float4>()
                            ,
                    }
                );
            }

        }
    }
}