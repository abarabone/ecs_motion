using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics.Authoring;

namespace Abarabone.Arms.Authoring
{
    using Abarabone.Model.Authoring;
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;
    using Abarabone.Model;

    public class BeamUnitAuthoring : MonoBehaviour, IFunctionUnitAuthoring, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public int Id;

        public BeamBulletAuthoring BeamPrefab;

        public GameObject MuzzleObject;
        public float3 MuzzleLocalPosition;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.BeamPrefab.gameObject);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initEmitter_(conversionSystem, entity);

            return;

            
            void initEmitter_(GameObjectConversionSystem gcs_, Entity emitter_)
            {

                var em = gcs_.DstEntityManager;

                var beamPrefab = gcs_.GetPrimaryEntity(this.BeamPrefab.gameObject);
                var beamBullet = this.BeamPrefab;

                var ent = emitter_;


                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(FunctionUnit.BulletEmittingData),
                    typeof(Bullet.SpecData), // 通常なら弾丸に持たせるところ、瞬時に着弾するため unit に持たせる。
                    typeof(FunctionUnit.TriggerData),
                    //typeof(FunctionUnit.OwnerLinkData),
                    typeof(FunctionUnit.ActivateData)
                });
                em.AddComponents(ent, types);

                em.SetComponentData(ent,
                    new FunctionUnit.BulletEmittingData
                    {
                        BulletPrefab = beamPrefab,
                        MuzzlePositionLocal = this.MuzzleLocalPosition,
                        RangeDistanceFactor = 1.0f,
                    }
                );
                em.SetComponentData(ent,
                    new Bullet.SpecData
                    {
                        RangeDistanceFactor = beamBullet.RangeDistance,
                    }
                );
            }
        }

    }
}
