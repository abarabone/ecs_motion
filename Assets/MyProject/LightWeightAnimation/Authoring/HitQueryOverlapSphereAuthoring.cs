using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Mathematics;

namespace Abarabone.Physics.Authoring
{
    using Abarabone.CharacterMotion;
    using Abarabone.Character;
    using Abarabone.Utilities;

    public class HitQueryOverlapSphereAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        /// <summary>
        /// エンティティはコンポーネントを重複して持てないので、使い方によって型を変えるための指定
        /// </summary>
        public enum UseageType
        {
            SphereForGround,
            RayForGround,
        }
        public UseageType Useage;


        public Vector3 Center;
        public float Radius;

        public PhysicsCategoryTags BelongsTo;
        public PhysicsCategoryTags CollidesWith;



        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            var em = dstManager;
            var ent = getPostureOrBoneEntity_(conversionSystem, entity, this.gameObject);

            addHitQueryComponent[(int)Useage](this, em, ent);

            return;


            Entity getPostureOrBoneEntity_(GameObjectConversionSystem gcs_, Entity mainEntity_, GameObject mainObject_)
            {
                var em_ = gcs_.DstEntityManager;

                if (em_.HasComponent<Translation>(mainEntity_)) return mainEntity_;

                return conversionSystem.GetEntities(mainObject_)
                    .Do(x => Debug.Log($"{em_.GetName_(x)} - {mainObject_.name}"))
                    .First(x => em_.HasComponent<Translation>(x));
            }
        }


        public Action<HitQueryOverlapSphereAuthoring, EntityManager, Entity>[] addHitQueryComponent =
            new Action<HitQueryOverlapSphereAuthoring, EntityManager, Entity>[]
        {
            (this_, em_, targetEntity_) => em_.AddComponentData(targetEntity_,
                    new GroundHitSphereData
                    {
                        Center = this_.Center,
                        Distance = this_.Radius,
                        Filter = new CollisionFilter
                        {
                            BelongsTo = this_.BelongsTo.Value,
                            CollidesWith = this_.CollidesWith.Value,
                        }
                    }
                ),

            (this_, em_, targetEntity_) => em_.AddComponentData(targetEntity_,
                    new GroundHitRayData
                    {
                        Start = this_.Center,
                        Ray = new DirectionAndLength { value = new float4(math.up() * -1, this_.Radius) },
                        Filter = new CollisionFilter
                        {
                            BelongsTo = this_.BelongsTo.Value,
                            CollidesWith = this_.CollidesWith.Value,
                        }
                    }
                ),
         };

    }
}
