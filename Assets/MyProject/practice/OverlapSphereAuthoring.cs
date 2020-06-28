﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Authoring;

namespace Abarabone.Physics.Authoring
{
    using Abarabone.Motion;
    using Abarabone.Character;
    using UnityEngine.TestTools;

    public class OverlapSphereAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public enum UseageType
        {
            ForGround,

        }
        public UseageType Useage;


        public Vector3 Center;
        public float Radius;

        public PhysicsCategoryTags BelongsTo;
        public PhysicsCategoryTags CollidesWith;


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            var ent = getPostureOrBoneEntity_(conversionSystem, entity, this.gameObject);

            switch (this.Useage)
            {
                case UseageType.ForGround:
                    initOverlapSphere_(conversionSystem.DstEntityManager, ent);
                    break;
            }

            return;


            Entity getPostureOrBoneEntity_(GameObjectConversionSystem gcs_, Entity mainEntity_, GameObject mainObject_)
            {
                var em_ = gcs_.DstEntityManager;

                if (em_.HasComponent<Translation>(mainEntity_)) return mainEntity_;

                return conversionSystem.GetEntities(mainObject_)
                    .Do(x => Debug.Log($"{em_.GetName(x)} - {mainObject_.name}"))
                    .First(x => em_.HasComponent<Translation>(x));
            }
            
            void initOverlapSphere_(EntityManager em_, Entity targetEntity_) =>
                em_.AddComponentData( targetEntity_,
                    new GroundHitSphereData
                    {
                        Center = this.Center,
                        Distance = this.Radius,
                        Filter = new CollisionFilter
                        {
                            BelongsTo = this.BelongsTo.Value,
                            CollidesWith = this.CollidesWith.Value,
                        }
                    }
                );
        }

    }
}
