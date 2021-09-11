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
using System.Runtime.CompilerServices;
using UnityEngine.XR;
using Unity.Physics.Systems;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Physics;

namespace DotsLite.Structure
{
    using DotsLite.Dependency;
    //using DotsLite.Model;
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
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    //[UpdateAfter(typeof())]
    public class StructureEnvelopeWakeupTriggerSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;
        //PhysicsHitDependency.Sender phydep;


        private BuildPhysicsWorld buildPhysicsWorld;
        private StepPhysicsWorld stepPhysicsWorld;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);

            //this.phydep = PhysicsHitDependency.Sender.Create(this);

            buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
            stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        }


        protected unsafe override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();
            //using var phyScope = this.phydep.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();
            //var cw = phyScope.PhysicsWorld.CollisionWorld;


            var dt = this.Time.DeltaTime;
            var dtrate = dt * TimeEx.PrevDeltaTimeRcp;


            this.Dependency = new HitEventJob
            {
                structureMain = this.GetComponentDataFromEntity<Structure.Main.MainTag>(isReadOnly: true),
                velocities = this.GetComponentDataFromEntity<PhysicsVelocity>(isReadOnly: true),

                binderLinks = this.GetComponentDataFromEntity<Structure.Main.BinderLinkData>(isReadOnly: true),
                parts = this.GetComponentDataFromEntity<Structure.Part.PartData>(isReadOnly: true),
                linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true),

                cmd = cmd,
            }
            .Schedule(this.stepPhysicsWorld.Simulation, ref this.buildPhysicsWorld.PhysicsWorld, this.Dependency);

        }


        [BurstCompile]
        struct HitEventJob : ICollisionEventsJob
        {
            [ReadOnly] public ComponentDataFromEntity<Structure.Main.MainTag> structureMain;
            [ReadOnly] public ComponentDataFromEntity<PhysicsVelocity> velocities;

            [ReadOnly] public ComponentDataFromEntity<Structure.Main.BinderLinkData> binderLinks;
            [ReadOnly] public ComponentDataFromEntity<Structure.Part.PartData> parts;
            [ReadOnly] public BufferFromEntity<LinkedEntityGroup> linkedGroups;

            public EntityCommandBuffer.ParallelWriter cmd;


            public void Execute(CollisionEvent ev)
            {
                var entA = ev.EntityA;
                var entB = ev.EntityB;

                var isStA = this.structureMain.HasComponent(entA);
                var isStB = this.structureMain.HasComponent(entB);

                // Ç«ÇøÇÁÇ‡ structure main Ç≈Ç»ÇØÇÍÇŒÇ»ÇÁÇ»Ç¢
                if (!(isStA & isStB)) return;
                //Debug.Log($"collision {entA} {entB}");

                var isRbA = this.velocities.HasComponent(entA);
                var isRbB = this.velocities.HasComponent(entB);

                // ï–ï˚ÇæÇØÇ™çÑëÃÇ≈Ç»ÇØÇÍÇŒÇ»ÇÁÇ»Ç¢
                if (isRbA & isRbB) return;


                // çÑëÃÇ≈Ç»Ç¢ï˚ÇçÑëÃÇ…Ç∑ÇÈ
                if (isRbA)
                {
                    var binder = this.binderLinks[entA];
                    this.cmd.ChangeComponentsToWakeUp(entA, ev.BodyIndexA, binder, this.parts, this.linkedGroups);
                    return;
                }
                if (isRbB)
                {
                    var binder = this.binderLinks[entB];
                    this.cmd.ChangeComponentsToWakeUp(entB, ev.BodyIndexB, binder, this.parts, this.linkedGroups);
                    return;
                }
            }

        }
    }


}