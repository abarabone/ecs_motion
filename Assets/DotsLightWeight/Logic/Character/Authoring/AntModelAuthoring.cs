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
using Unity.Physics;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Character;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;
    using DotsLite.Targeting;
    using DotsLite.Collision;
    using DotsLite.Arms.Authoring;

    /// <summary>
    /// 
    /// </summary>
    public class AntModelAuthoring : CharacterModelAuthoring, IConvertGameObjectToEntity
    {


        public float MoveSpeedPerSec;

        public float TurnDegPerSec;

        public float st;
        public float ed;

        public Corps CorpsBelongsTo;
        public Corps CorpsTargets;

        public float ArmorDurability;


        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }

            base.Convert(entity, dstManager, conversionSystem);


            var state = this.GetComponentInChildren<ActionStateAuthoring>();
            initState_(conversionSystem, state);

            var posture = this.GetComponentInChildren<PostureAuthoring>();
            initPosture_(conversionSystem, posture, state);//, this.CorpsBelongsTo);


            //var ent = conversionSystem.GetOrCreateEntity(state);
            //conversionSystem.AddHitTargetsAllRigidBody(this, ent, HitType.charactor);

            var funits = this.GetComponentsInChildren<IFunctionUnitAuthoring>();
            conversionSystem.AddTargetCorpsToFunctionUnit(funits, this.CorpsTargets);

            conversionSystem.AddCorpsToAllRigidBody(this, this.CorpsBelongsTo);
            return;


            //void initPosture_( GameObjectConversionSystem gcs_, GameObject main_ )
            //{
            //    var em = gcs_.DstEntityManager;

            //    var mainEntity = gcs_.GetPrimaryEntity(main_);

            //    var types = new ComponentTypes(new ComponentType []
            //    {
            //        //typeof(PlayerTag),
            //        typeof(AntTag),

            //        typeof(WallingTag),
            //        typeof(WallHangingData),
            //        typeof(WallHitResultData),
            //        typeof(PhysicsGravityFactor),// デフォルトでは付かないっぽい
                    
            //        //typeof(MoveHandlingData),
            //        typeof(Move.SpeedParamaterData),
            //        typeof(Move.TurnParamaterData),

            //        typeof(AntAction.WalkState)
            //    });
            //    em.AddComponents(mainEntity, types);

            //    em.SetComponentData(mainEntity, new PhysicsGravityFactor
            //    {
            //        Value = 1.0f,
            //    });
            //    em.SetComponentData(mainEntity, new Move.SpeedParamaterData
            //    {
            //        SpeedPerSec = this.MoveSpeedPerSec,
            //    });
            //    em.SetComponentData(mainEntity, new Move.TurnParamaterData
            //    {
            //        TurnRadPerSec = math.radians(this.TurnDegPerSec),
            //    });
            //}


            void initPosture_
                (GameObjectConversionSystem gcs, PostureAuthoring posture, ActionStateAuthoring state)//, Corps corps)
            {
                var em = gcs.DstEntityManager;

                var ent = gcs.GetPrimaryEntity(posture);

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(AntTag),

                    typeof(WallingTag),
                    typeof(WallHangingData),
                    typeof(WallHitResultData),
                    typeof(PhysicsGravityFactor),// デフォルトでは付かないっぽい
                    
                    typeof(Move.TurnParamaterData),
                    typeof(Move.SpeedParamaterData),
                    typeof(Move.EasingSpeedData),

                    typeof(Control.MoveData),
                    typeof(Control.WorkData),
                    typeof(Control.StateLinkData),
                });
                em.AddComponents(ent, types);

                em.SetComponentData(ent, new Control.StateLinkData
                {
                    StateEntity = gcs.GetOrCreateEntity(state) 
                });

                em.SetComponentData(ent, new PhysicsGravityFactor
                {
                    Value = 1.0f,
                });

                em.SetComponentData(ent, new Move.TurnParamaterData
                {
                    TurnRadPerSec = math.radians(this.TurnDegPerSec),
                });
                em.SetComponentData(ent, new Move.SpeedParamaterData
                {
                    SpeedPerSecMax = this.MoveSpeedPerSec,
                    SpeedPerSec = 0.0f,//this.MoveSpeedPerSec,//
                });
                em.SetComponentData(ent, new Move.EasingSpeedData
                {
                    TargetSpeedPerSec = this.MoveSpeedPerSec,
                    Rate = 0.5f,
                });

            }

            void initState_
                (GameObjectConversionSystem gcs, ActionStateAuthoring state)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(AntTag),

                    typeof(AntAction.WalkState),
                    typeof(AntAction.AttackTimeRange),

                    typeof(Control.ActionData),

                    typeof(Armor.SimpleDamageData),

                    typeof(ColliderBank.DeadData),// ColliderSwitchingAuthoring にて付与する
                });
                var ent = gcs.GetOrCreateEntity(state, types);

                em.SetComponentData(ent, new AntAction.AttackTimeRange
                {
                    st = st,
                    ed = ed,
                });
                em.SetComponentData(ent, new Armor.SimpleDamageData
                {
                    Durability = this.ArmorDurability,
                });

                //em.SetComponentData(ent, new CorpsGroup.Data
                //{
                //    BelongTo = corps,
                //});
            }

        }

    }
}
