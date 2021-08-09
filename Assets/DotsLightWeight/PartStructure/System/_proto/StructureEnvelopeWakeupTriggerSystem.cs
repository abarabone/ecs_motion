using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
////using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.XR;
using Unity.Physics.Systems;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Physics;

namespace DotsLite.Arms
{
    using DotsLite.Dependency;
    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Particle;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using DotsLite.Structure;
    using DotsLite.Character.Action;
    using DotsLite.Collision;
    using DotsLite.Targeting;
    using DotsLite.Misc;
    using DotsLite.HeightGrid;
    using DotsLite.Utilities;

    //[DisableAutoCreation]
    //[UpdateAfter(typeof(EndFramePhysicsSystem))]
    public class StructureEnvelopeWakeupTriggerSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;

        //PhysicsHitDependency.Sender phydep;

        //HitMessage<Structure.PartHitMessage>.Sender stSender;
        //HitMessage<Character.HitMessage>.Sender chSender;

        private BuildPhysicsWorld buildPhysicsWorld;
        private StepPhysicsWorld stepPhysicsWorld;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);

            //this.phydep = PhysicsHitDependency.Sender.Create(this);

            //this.stSender = HitMessage<Structure.PartHitMessage>.Sender.Create<StructurePartHitMessageApplySystem>(this);
            //this.chSender = HitMessage<Character.HitMessage>.Sender.Create<CharacterHitMessageApplySystem>(this);

            buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
            stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        }


        protected unsafe override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();
            //using var phyScope = this.phydep.WithDependencyScope();
            //using var pthitScope = this.stSender.WithDependencyScope();
            //using var chhitScope = this.chSender.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();
            //var cw = phyScope.PhysicsWorld.CollisionWorld;
            //var pthit = pthitScope.MessagerAsParallelWriter;
            //var chhit = chhitScope.MessagerAsParallelWriter;


            var dt = this.Time.DeltaTime;
            var dtrate = dt * TimeEx.PrevDeltaTimeRcp;


            this.Dependency = new HitEventJob
            {

            }
            .Schedule(this.stepPhysicsWorld.Simulation, ref this.buildPhysicsWorld.PhysicsWorld, this.Dependency);

        }


        [BurstCompile]
        struct HitEventJob : ITriggerEventsJob
        {
            //[ReadOnly] public ComponentDataFromEntity<Ball> balls;
            //[ReadOnly] public ComponentDataFromEntity<Lifetime> lifetimes;
            public EntityCommandBuffer cmd;

            public void Execute(TriggerEvent triggerEvent)
            {
                var entA = triggerEvent.EntityA;
                var entB = triggerEvent.EntityB;

                Debug.Log($"collision {entA} {entB}");
            }
        }
    }


}