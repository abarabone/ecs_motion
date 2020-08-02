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

namespace Abarabone.Model.Authoring
{
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
    public class WaponAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public GameObject BeamPrefab;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.BeamPrefab);
        }

        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            //var top = this.GetComponentInParent<CharacterModelAuthoring>();// プレハブだとできない（ children はできるのに）
            var top = this.gameObject.Ancestors().First(go => go.GetComponent<CharacterModelAuthoring>());
            var main = top.transform.GetChild(0).gameObject;

            initWapon_(conversionSystem, main, this.gameObject, this.BeamPrefab);

            return;


            void initWapon_(GameObjectConversionSystem gcs_, GameObject main_, GameObject wapon_, GameObject beam_)
            {
                var em = gcs_.DstEntityManager;


                var mainEntity = gcs_.GetPrimaryEntity(main_);

                var beamPrefab = gcs_.GetPrimaryEntity(beam_);


                var ent = conversionSystem.GetPrimaryEntity(wapon_);

                var types = new ComponentTypes
                (
                    typeof(Wapon.BeamEmitterData)
                );
                em.AddComponents(ent, types);

                em.SetComponentData(ent,
                    new Wapon.BeamEmitterData
                    {
                        MainEntity = mainEntity,
                        BeamPrefab = beamPrefab,
                        MuzzlePositionLocal = new float3(0,0,0.1f),//new float3(0, 0, 1.0f),
                    }
                );
            }
        }

    }
}
