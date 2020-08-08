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

namespace Abarabone.Arms.Authoring
{
    using Abarabone.Model.Authoring;
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;
    using Unity.Physics.Authoring;

    /// <summary>
    /// プライマリエンティティは LinkedEntityGroup のみとする。
    /// その１つ下に、ＦＢＸのキャラクターを置く。それがメインエンティティとなる。
    /// </summary>
    public class ShotUnitAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
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

            //var top = this.GetComponentInParent<CharacterModelAuthoring>();// プレハブだとできない（ children はできるのに）
            var top = this.gameObject.Ancestors().First(go => go.GetComponent<CharacterModelAuthoring>());
            var main = top.transform.GetChild(0).gameObject;

            initWapon_(conversionSystem, main, this.gameObject, this.MuzzleObject, this.BulletPrefab.gameObject);

            return;


            void initWapon_
                (
                    GameObjectConversionSystem gcs_,
                    GameObject main_, GameObject wapon_, GameObject muzzle_, GameObject beam_
                )
            {
                var em = gcs_.DstEntityManager;


                var mainEntity = gcs_.GetPrimaryEntity(main_);
                var beamPrefab = gcs_.GetPrimaryEntity(beam_);
                var muzzleEntity = gcs_.GetPrimaryEntity(muzzle_);


                var ent = conversionSystem.GetPrimaryEntity(wapon_);

                var types = new ComponentTypes
                (
                    typeof(Wapon.BulletEmittingData),
                    typeof(Wapon.EmittingStateData),
                    typeof(Wapon.SightModeData)
                );
                em.AddComponents(ent, types);

                em.SetComponentData(ent,
                    new Wapon.BulletEmittingData
                    {
                        BulletPrefab = beamPrefab,
                        MainEntity = mainEntity,
                        MuzzleBodyEntity = muzzleEntity,
                        MuzzlePositionLocal = this.MuzzleLocalPosition,
                        EmittingInterval = this.EmittingInterval,
                        NumEmitMultiple = this.NumEmitMultiple,
                        AccuracyRad = math.radians(this.DirectionAccuracyDegree),
                        RangeDistanceFactor = this.RangeDistanceFactor,
                    }
                );
                em.SetComponentData(ent,
                    new Wapon.EmittingStateData
                    {
                        NextEmitableTime = this.EmittingInterval,
                    }
                );
            }
        }

    }
}
