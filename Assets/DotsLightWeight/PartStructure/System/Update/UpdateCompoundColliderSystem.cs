using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace DotsLite.Draw
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.Structure;
    using DotsLite.Dependency;


    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class UpdateCompoundColliderSystem : DependencyAccessableSystemBase
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

        }

        BarrierDependency.Sender barcopydep;
        //BarrierDependency.Sender barfreedep;

        BitGridMessageAllocSystem messageSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.barcopydep = BarrierDependency.Sender.Create<BitGridCopyToGpuSystem>(this);
            //this.barfreedep = BarrierDependency.Sender.Create<BitGridMessageFreeSystem>(this);

            this.messageSystem = this.World.GetOrCreateSystem<BitGridMessageAllocSystem>();
        }

        protected override void OnUpdate()
        {
            using var barcopyScope = this.barcopydep.WithDependencyScope();
            //using var barfreeScope = this.barfreedep.WithDependencyScope();

            this.Dependency = new JobExecution
            {
                bitgrids = this.GetComponentDataFromEntity<BitGrid.BitLinesData>(isReadOnly: true),
                dirties = this.GetComponentDataFromEntity<BitGrid.UpdateDirtyRangeData>(),
                parents = this.GetComponentDataFromEntity<BitGrid.ParentAreaData>(isReadOnly: true),
                origins = this.GetComponentDataFromEntity<BitGrid.WorldOriginData>(isReadOnly: true),
                areas = this.GetComponentDataFromEntity<BitGridArea.GridLinkData>(isReadOnly: true),
                dims = this.GetComponentDataFromEntity<BitGridArea.UnitDimensionData>(isReadOnly: true),
            }
            .ScheduleParallelKey(this.messageSystem.Reciever, 1, this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : HitMessage<UpdateMessage>.IApplyJobExecutionForKey
        {

            [BurstCompile]
            public unsafe void Execute(
                int index, Entity targetEntity,
                NativeMultiHashMap<Entity, UpdateMessage>.Enumerator msgs)
            {

            }
        }
    }
}
