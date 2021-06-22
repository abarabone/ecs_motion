using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

namespace DotsLite.Arms.Authoring
{
    using DotsLite.Utilities;

    public class MuzzleOnCameraAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float3 MuzzleLocalPosition;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var muzzle = conversionSystem.CreateAdditionalEntity(this);

            var types = new ComponentTypes(
                typeof(Emitter.MuzzleTransformData),
                typeof(Translation),
                typeof(Rotation)
            );
            dstManager.AddComponents(muzzle, types);

            dstManager.SetComponentData(muzzle,
                new Emitter.MuzzleTransformData
                {
                    MuzzlePositionLocal = this.MuzzleLocalPosition.As_float4(),
                    ParentEntity = entity,
                }
            );
        }
    }
}
