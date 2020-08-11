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
    public class ShotUnitAuthoring : WaponAuthoring.FunctionUnitAuthoring, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public ShotBulletAuthoring BulletPrefab;

        public GameObject MuzzleObject;
        public float3 MuzzleLocalPosition;

        public int NumEmitMultiple;
        public float EmittingInterval;
        public float DirectionAccuracyDegree;
        public float RangeDistanceFactor;// 弾丸のと掛け合わせて射程とする


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.BulletPrefab.gameObject);
        }

        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initEmitter_(conversionSystem, this.gameObject, this.BulletPrefab.gameObject);

            return;


            void initEmitter_
                (
                    GameObjectConversionSystem gcs_,
                    GameObject emitter_, GameObject bullet_
                )
            {
                var em = gcs_.DstEntityManager;


                var beamPrefab = gcs_.GetPrimaryEntity(bullet_);


                var ent = conversionSystem.GetPrimaryEntity(emitter_);

                var types = new ComponentTypes
                (new ComponentType[] {
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(FunctionUnit.BulletEmittingData),
                    typeof(FunctionUnit.EmittingStateData),
                    typeof(FunctionUnit.SightModeData),
                    typeof(FunctionUnit.TriggerData),
                    typeof(FunctionUnit.OwnerLinkData)
                    //typeof(FunctionUnit.UnitChainLinkData)
                });
                em.AddComponents(ent, types);

                em.SetComponentData(ent,
                    new FunctionUnit.BulletEmittingData
                    {
                        BulletPrefab = beamPrefab,
                        //MainEntity = mainEntity,
                        //MuzzleBodyEntity = muzzleEntity,
                        MuzzlePositionLocal = this.MuzzleLocalPosition,
                        EmittingInterval = this.EmittingInterval,
                        NumEmitMultiple = this.NumEmitMultiple,
                        AccuracyRad = math.radians(this.DirectionAccuracyDegree),
                        RangeDistanceFactor = this.RangeDistanceFactor,
                    }
                );
                em.SetComponentData(ent,
                    new FunctionUnit.EmittingStateData
                    {
                        NextEmitableTime = this.EmittingInterval,
                    }
                );
            }
        }

    }
}
