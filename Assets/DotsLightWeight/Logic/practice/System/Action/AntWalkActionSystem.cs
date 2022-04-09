using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace DotsLite.Character.Action
{
    using DotsLite.Dependency;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;
    using DotsLite.Targeting;
    using Motion = DotsLite.CharacterMotion.Motion;


    /// <summary>
    /// 歩き時のアクションステート
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(AntMoveDirectionSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public partial class AntrWalkActionSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }

        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            var motionInfos = this.GetComponentDataFromEntity<Motion.InfoData>(isReadOnly: true);
            var motionCursors = this.GetComponentDataFromEntity<Motion.CursorData>();
            //var motionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>();

            var targetposs = this.GetComponentDataFromEntity<TargetSensorResponse.PositionData>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);


            var post = cmdScope.CommandBuffer;
            this.Entities
                .WithName("Reset")
                .WithAll<AntAction.WalkState, CharacterAction.DamageState>()
                .ForEach((Entity entity) =>
                {
                    post.RemoveComponent<AntAction.WalkState>(entity);
                    post.AddComponent<AntAction.WalkState>(entity);
                })
                .Schedule();

            this.Entities
                .WithName("Action")
                .WithBurst()
                .WithAll<AntTag>()
                .WithNone<CharacterAction.DamageState>()
                .WithReadOnly(motionInfos)
                .WithReadOnly(targetposs)
                .WithReadOnly(poss)
                .WithNativeDisableParallelForRestriction(motionCursors)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref AntAction.WalkState state,
                        in ActionState.MotionLinkDate mlink,
                        in ActionState.PostureLinkData plink,
                        in TargetSensorHolderLink.HolderLinkData holderLink
                    )
                =>
                    {

                        var motion = new MotionOperator
                            (cmd, motionInfos, motionCursors, mlink.MotionEntity, entityInQueryIndex);

                        motion.Start(Motion_ant.walking, isLooping: true, delayTime: 0.1f);


                        var targetpos = targetposs[holderLink.HolderEntity].Position;
                        var originpos = poss[plink.PostureEntity].Value;

                        if (math.distancesq(targetpos, originpos) <= 10.0f * 10.0f)
                        {
                            cmd.RemoveComponent<AntAction.WalkState>(entityInQueryIndex, entity);
                            cmd.AddComponent<AntAction.AttackState>(entityInQueryIndex, entity);
                        }
                    }
                )
                .ScheduleParallel();
        }

    }
}

namespace DotsLite.Character.Action
{
    using Unity.Entities.CodeGeneratedJobForEach;


    public interface IEntityLink
    {
        Entity Entity { get; }
    }
    public struct Components<T0>
        where T0 : struct, IComponentData
    {
        public static ComponentDataFromEntity<T0> Get(SystemBase sys)
        {
            var c0 = sys.GetComponentDataFromEntity<T0>();
            return c0;
        }
    }
    public static class ComponentExtension
    {
        public static ForEachLambdaJobDescription aaa<T0>(this ForEachLambdaJobDescription desc, ComponentDataFromEntity<T0> a)
            where T0 : struct, IComponentData
        {
            return desc.WithReadOnly(a);
        }

        public static ComponentDataFromEntity<T0> Get<T0>(this SystemBase sys)
            where T0 : struct, IComponentData
        {
            var c0 = sys.GetComponentDataFromEntity<T0>();
            return c0;
        }

        public static ComponentDataFromEntity<T0> GetReadOnly<T0>(this SystemBase sys)
            where T0 : struct, IComponentData
        {
            var c0 = sys.GetComponentDataFromEntity<T0>(isReadOnly: true);
            return c0;
        }
    }


}
