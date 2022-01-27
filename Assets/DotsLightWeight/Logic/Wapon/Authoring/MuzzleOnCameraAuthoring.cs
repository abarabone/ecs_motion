using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

namespace DotsLite.Arms.Authoring
{
    using DotsLite.Utilities;
    using DotsLite.EntityTrimmer.Authoring;

    public class MuzzleOnCameraAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float3 MuzzleLocalPosition;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            var muzzle = conversionSystem.CreateAdditionalEntity(this);

            var types = new ComponentTypes(
                typeof(Emitter.MuzzleTransformData),
                typeof(Marker.Translation),
                typeof(Marker.Rotation)
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
