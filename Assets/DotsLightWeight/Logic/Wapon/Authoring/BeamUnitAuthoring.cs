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

namespace DotsLite.Arms.Authoring
{
    using DotsLite.Model.Authoring;
    using DotsLite.Character;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;
    using DotsLite.Model;

    public class BeamUnitAuthoring : MonoBehaviour,
        IFunctionUnitAuthoring, IConvertGameObjectToEntity, IDeclareReferencedPrefabs, IMuzzleLocalPostion
    {

        public int Id;

        public BeamBulletAuthoring BeamPrefab;
        public bool UseEffect => false;

        public GameObject MuzzleObject;
        public float3 MuzzleLocalPosition;
        public float3 Local => this.MuzzleLocalPosition;


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
                    typeof(Emitter.BulletEmittingData),
                    typeof(Bullet.MoveSpecData), // 通常なら弾丸に持たせるところ、瞬時に着弾するため unit に持たせる。
                    typeof(Emitter.TriggerData),
                    //typeof(FunctionUnit.OwnerLinkData),
                    ////typeof(FunctionUnit.ActivateData),
                    typeof(Targeting.CorpsGroup.TargetWithArmsData)
                });
                em.AddComponents(ent, types);

                em.SetComponentData(ent,
                    new Emitter.BulletEmittingData
                    {
                        //BulletPrefab = beamPrefab,
                        //MuzzlePositionLocal = this.MuzzleLocalPosition,
                        RangeDistanceFactor = 1.0f,
                    }
                );
                em.SetComponentData(ent,
                    new Bullet.MoveSpecData
                    {
                        RangeDistanceFactor = beamBullet.RangeDistance,
                    }
                );
            }
        }

    }
}
