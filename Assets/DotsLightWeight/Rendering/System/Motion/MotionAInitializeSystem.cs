using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using DotsLite.SystemGroup;

namespace DotsLite.CharacterMotion
{
    using DotsLite.Dependency;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup))]
    [UpdateBefore(typeof(MotionStreamProgressAndInterporationSystem))]
    public class MotionInitializeSystem : DependencyAccessableSystemBase
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
            var linkers = this.GetComponentDataFromEntity<Stream.RelationData>(isReadOnly: true);
            var shifters = this.GetComponentDataFromEntity<Stream.KeyShiftData>();
            var timers = this.GetComponentDataFromEntity<Stream.CursorData>();
            var caches = this.GetComponentDataFromEntity<Stream.NearKeysCacheData>();

            this.Entities
                .WithBurst()
                .WithNone<Motion.CursorData>()
                .WithReadOnly(linkers)
                .WithNativeDisableParallelForRestriction(linkers)
                .WithNativeDisableParallelForRestriction(shifters)
                .WithNativeDisableParallelForRestriction(timers)
                .WithNativeDisableParallelForRestriction(caches)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref Motion.InfoData info,
                        in Motion.InitializeData init,
                        in Motion.StreamLinkData linker,
                        in Motion.ClipData data
                    )
                =>
                    {
                        ref var clip = ref data.MotionClipData.Value;
                        ref var motion = ref clip.Motions[init.MotionIndex];

                        info.MotionIndex = init.MotionIndex;
                        if (init.IsContinuous)
                        {
                            initSection(ref motion, linker.PositionStreamTop, KeyStreamSection.positions, init);
                            initSection(ref motion, linker.RotationStreamTop, KeyStreamSection.rotations, init);
                        }

                        cmd.RemoveComponent<Motion.InitializeData>(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel();

            return;


            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe void initSection
                (ref MotionBlobUnit motionClip, Entity entTop, KeyStreamSection streamSection, Motion.InitializeData init)
            {
                ref var streams = ref motionClip.Sections[(int)streamSection].Streams;

                for (var ent = entTop; ent != Entity.Null; ent = linkers[ent].NextStreamEntity)
                {
                    var i = linkers[ent].BoneId;

                    var shifter = shifters[ent];
                    shifter.Keys = (KeyBlobUnit*)streams[i].Keys.GetUnsafePtr();
                    shifter.KeyLength = streams[i].Keys.Length;

                    var timer = timers[ent];
                    timer.Cursor.InitializeCursor(ref motionClip, init.DelayTime, init.TimeScale);

                    var cache = caches[ent];
                    if (init.IsContinuous)
                    {
                        cache.InitializeKeysContinuous(ref shifter, init.DelayTime);
                    }
                    else
                    {
                        cache.InitializeKeys(ref shifter);
                    }

                    caches[ent] = cache;
                    timers[ent] = timer;
                    shifters[ent] = shifter;
                }
            }
        }




        ////[BurstCompile]
        //[ExcludeComponent(typeof(Motion.CursorData))]
        //struct MotionInitializeJob : IJobForEachWithEntity
        //    <Motion.InitializeData, Motion.StreamLinkData, Motion.ClipData, Motion.InfoData>
        //{

        //    public EntityCommandBuffer.ParallelWriter Commands;

        //    [NativeDisableParallelForRestriction][ReadOnly]
        //    public ComponentDataFromEntity<Stream.RelationData>      Linkers;
        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<Stream.KeyShiftData>      Shifters;
        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<Stream.NearKeysCacheData> Caches;
        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<Stream.CursorData>  Timers;


        //    public void Execute(
        //        Entity entity, int index,
        //        [ReadOnly] ref Motion.InitializeData init,
        //        [ReadOnly] ref Motion.StreamLinkData linker,
        //        [ReadOnly] ref Motion.ClipData data,
        //        ref Motion.InfoData info
        //    )
        //    {
        //        ref var clip = ref data.MotionClipData.Value;
        //        ref var motion = ref clip.Motions[ init.MotionIndex ];

        //        info.MotionIndex = init.MotionIndex;
        //        if( init.IsContinuous )
        //        initSection( ref motion, linker.PositionStreamTop, KeyStreamSection.positions, ref init );
        //        initSection( ref motion, linker.RotationStreamTop, KeyStreamSection.rotations, ref init );

        //        this.Commands.RemoveComponent<Motion.InitializeData>( index, entity );
        //    }

        //    unsafe void initSection
        //        ( ref MotionBlobUnit motionClip, Entity entTop, KeyStreamSection streamSection, ref Motion.InitializeData init )
        //    {
        //        ref var streams = ref motionClip.Sections[(int)streamSection].Streams;

        //        for( var ent = entTop; ent != Entity.Null; ent = this.Linkers[ent].NextStreamEntity )
        //        {
        //            var i = Linkers[ ent ].BoneId;
                    
        //            var shifter = this.Shifters[ ent ];
        //            shifter.Keys = (KeyBlobUnit*)streams[ i ].Keys.GetUnsafePtr();
        //            shifter.KeyLength = streams[ i ].Keys.Length;

        //            var timer = this.Timers[ ent ];
        //            timer.Cursor.InitializeCursor( ref motionClip, init.DelayTime, init.TimeScale );
                    
        //            var cache = this.Caches[ ent ];
        //            if( init.IsContinuous )
        //                cache.InitializeKeysContinuous( ref shifter, init.DelayTime );
        //            else
        //                cache.InitializeKeys( ref shifter );

        //            this.Caches[ ent ] = cache;
        //            this.Timers[ ent ] = timer;
        //            this.Shifters[ ent ] = shifter;
        //        }
        //    }
        //}
    }

}


