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

    /// <summary>
    /// プライマリエンティティは LinkedEntityGroup のみとする。
    /// その１つ下に、ＦＢＸのキャラクターを置く。それがメインエンティティとなる。
    /// </summary>
    public class BeamUnitAuthoring : WaponAuthoring.FunctionUnitAuthoring, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public BeamBulletAuthoring BeamPrefab;

        public GameObject MuzzleObject;
        public float3 MuzzleLocalPosition;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.BeamPrefab.gameObject);
        }

        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initEmitter_(conversionSystem, this.gameObject, this.BeamPrefab.gameObject);

            return;


            void initEmitter_
                (
                    GameObjectConversionSystem gcs_,
                    GameObject emitter_, GameObject beam_
                )
            {
                var em = gcs_.DstEntityManager;

                var beamBullet = beam_.GetComponent<BeamBulletAuthoring>();
                var beamPrefab = gcs_.GetPrimaryEntity(beam_);


                var ent = conversionSystem.GetPrimaryEntity(emitter_);

                var types = new ComponentTypes
                (
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(FunctionUnit.BulletEmittingData),
                    typeof(Bullet.SpecData), // 通常なら弾丸に持たせるところ、瞬時に着弾するため unit に持たせる。
                    //typeof(Bullet.DistanceData), // 同上
                    typeof(FunctionUnit.UnitChainLinkData)
                );
                em.AddComponents(ent, types);

                em.SetComponentData(ent,
                    new FunctionUnit.BulletEmittingData
                    {
                        BulletPrefab = beamPrefab,
                        //MainEntity = mainEntity,
                        //MuzzleBodyEntity = muzzleEntity,
                        MuzzlePositionLocal = this.MuzzleLocalPosition,
                        RangeDistanceFactor = 1.0f,
                    }
                );
                em.SetComponentData(ent,
                    new Bullet.SpecData
                    {
                        //MainEntity = mainEntity,
                        RangeDistanceFactor = beamBullet.RangeDistance,
                    }
                );
            }
        }

    }
}
