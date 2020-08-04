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
    public class BeamUnitAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
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

            //var top = this.GetComponentInParent<CharacterModelAuthoring>();// プレハブだとできない（ children はできるのに）
            var top = this.gameObject.Ancestors().First(go => go.GetComponent<CharacterModelAuthoring>());
            var main = top.transform.GetChild(0).gameObject;

            initWapon_(conversionSystem, main, this.gameObject, this.MuzzleObject, this.BeamPrefab.gameObject);

            return;


            void initWapon_
                (
                    GameObjectConversionSystem gcs_,
                    GameObject main_, GameObject wapon_, GameObject muzzle_, GameObject beam_
                )
            {
                var em = gcs_.DstEntityManager;

                var beamBullet = beam_.GetComponent<BeamBulletAuthoring>();

                var mainEntity = gcs_.GetPrimaryEntity(main_);
                var beamPrefab = gcs_.GetPrimaryEntity(beam_);
                var muzzleEntity = gcs_.GetPrimaryEntity(muzzle_);


                var ent = conversionSystem.GetPrimaryEntity(wapon_);

                var types = new ComponentTypes
                (
                    typeof(Wapon.BulletEmittingData),
                    typeof(Bullet.Data),
                    typeof(Bullet.DistanceData)
                );
                em.AddComponents(ent, types);

                em.SetComponentData(ent,
                    new Wapon.BulletEmittingData
                    {
                        MainEntity = mainEntity,
                        BulletPrefab = beamPrefab,
                        MuzzleBodyEntity = muzzleEntity,
                        MuzzlePositionLocal = this.MuzzleLocalPosition,
                        RangeDistanceFactor = 1.0f,
                    }
                );
                em.SetComponentData(ent,
                    new Bullet.Data
                    {
                        MainEntity = mainEntity,
                        RangeDistanceFactor = beamBullet.RangeDistance,
                    }
                );
            }
        }

    }
}
