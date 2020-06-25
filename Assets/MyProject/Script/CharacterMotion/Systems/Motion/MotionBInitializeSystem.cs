﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abarabone.SystemGroup;

namespace Abarabone.Motion
{
    
    [UpdateBefore( typeof( MotionProgressSystem ) )]// MotionB
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ))]
    public class MotionBInitializeSystem : SystemBase//JobComponentSystem
    {

        EntityCommandBufferSystem ecb;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.ecb = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commands = this.ecb.CreateCommandBuffer().ToConcurrent();

            var linkers = this.GetComponentDataFromEntity<StreamRelationData>( isReadOnly: true );
            var shifters = this.GetComponentDataFromEntity<StreamKeyShiftData>();
            var caches = this.GetComponentDataFromEntity<StreamNearKeysCacheData>();
            
            this.Entities
                .WithName( "MotionBInitializeSystem" )
                .WithBurst()
                .WithReadOnly(linkers)
                .WithNativeDisableParallelForRestriction(linkers)
                .WithNativeDisableParallelForRestriction(shifters)
                .WithNativeDisableParallelForRestriction(caches)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref MotionInfoData info,
                        ref MotionCursorData cursor,
                        in MotionInitializeData init,
                        in MotionClipData data,
                        in MotionStreamLinkData linker
                    ) =>
                    {
                        ref var blob = ref data.ClipData.Value;
                        ref var motionClip = ref blob.Motions[ init.MotionIndex ];

                        info.MotionIndex = init.MotionIndex;
                        initSection( ref motionClip, linker.PositionStreamTop, KeyStreamSection.positions, in init );
                        initSection( ref motionClip, linker.RotationStreamTop, KeyStreamSection.rotations, in init );

                        cursor.InitializeCursor( ref motionClip, init.DelayTime );

                        makeEnableSection( entityInQueryIndex, linker.PositionStreamTop );
                        makeEnableSection( entityInQueryIndex, linker.RotationStreamTop );

                        commands.RemoveComponent<MotionInitializeData>( entityInQueryIndex, entity );
                    }
                )
                .ScheduleParallel();

            return;


            unsafe void initSection
                ( ref MotionBlobUnit motion, Entity entTop, KeyStreamSection streamSection, in MotionInitializeData init )
            {
                ref var streams = ref motion.Sections[ (int)streamSection ].Streams;

                for( var ent = entTop; ent != Entity.Null; ent = linkers[ ent ].NextStreamEntity )
                {
                    var i = linkers[ ent ].BoneId;

                    var shifter = shifters[ ent ];
                    shifter.Keys = (KeyBlobUnit*)streams[ i ].Keys.GetUnsafePtr();
                    shifter.KeyLength = streams[ i ].Keys.Length;

                    var cache = caches[ ent ];
                    if( init.IsContinuous )
                        cache.InitializeKeysContinuous( ref shifter, init.DelayTime );
                    else
                        cache.InitializeKeys( ref shifter );

                    caches[ ent ] = cache;
                    shifters[ ent ] = shifter;
                }
            }

            void makeEnableSection( int index, Entity entTop )
            {
                for( var ent = entTop; ent != Entity.Null; ent = linkers[ ent ].NextStreamEntity )
                {
                    commands.RemoveComponent<Disabled>( index, ent );
                }
            }
        }






        //protected JobHandle OnUpdate( JobHandle inputDeps )
        //{
        //    var commandBuffer = this.ecb.CreateCommandBuffer();


        //    inputDeps = new MotionInitializeJob
        //    {
        //        Commands = commandBuffer.ToConcurrent(),
        //        Linkers  = this.GetComponentDataFromEntity<StreamRelationData>(),
        //        Shifters = this.GetComponentDataFromEntity<StreamKeyShiftData>(),
        //        Caches   = this.GetComponentDataFromEntity<StreamNearKeysCacheData>(),

        //    }
        //    .Schedule( this, inputDeps );
        //    this.ecb.AddJobHandleForProducer( inputDeps );


        //    return inputDeps;
        //}



        ////[BurstCompile]
        //struct MotionInitializeJob : IJobForEachWithEntity
        //    <MotionInitializeData, MotionStreamLinkData, MotionClipData, MotionInfoData, MotionCursorData>
        //{
            
        //    public EntityCommandBuffer.Concurrent Commands;

        //    [NativeDisableParallelForRestriction][ReadOnly]
        //    public ComponentDataFromEntity<StreamRelationData>      Linkers;
        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<StreamKeyShiftData>      Shifters;
        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<StreamNearKeysCacheData> Caches;
            

        //    public void Execute(
        //        Entity entity, int index,
        //        [ReadOnly] ref MotionInitializeData init,
        //        [ReadOnly] ref MotionStreamLinkData linker,
        //        [ReadOnly] ref MotionClipData data,
        //        ref MotionInfoData info,
        //        ref MotionCursorData cursor
        //    )
        //    {
        //        ref var blob = ref data.ClipData.Value;
        //        ref var motionClip = ref blob.Motions[ init.MotionIndex ];

        //        info.MotionIndex = init.MotionIndex;
        //        initSection( ref motionClip, linker.PositionStreamTop, KeyStreamSection.positions, ref init );
        //        initSection( ref motionClip, linker.RotationStreamTop, KeyStreamSection.rotations, ref init );

        //        cursor.InitializeCursor( ref motionClip, init.DelayTime );
                
        //        makeEnableSection( index, linker.PositionStreamTop );
        //        makeEnableSection( index, linker.RotationStreamTop );

        //        this.Commands.RemoveComponent<MotionInitializeData>( index, entity );
        //    }

        //    unsafe void initSection
        //        ( ref MotionBlobUnit motion, Entity entTop, KeyStreamSection streamSection, ref MotionInitializeData init )
        //    {
        //        ref var streams = ref motion.Sections[(int)streamSection].Streams;

        //        for( var ent = entTop; ent != Entity.Null; ent = this.Linkers[ent].NextStreamEntity )
        //        {
        //            var i = Linkers[ ent ].BoneId;
                    
        //            var shifter = this.Shifters[ ent ];
        //            shifter.Keys = (KeyBlobUnit*)streams[ i ].Keys.GetUnsafePtr();
        //            shifter.KeyLength = streams[ i ].Keys.Length;
                    
        //            var cache = this.Caches[ ent ];
        //            if( init.IsContinuous )
        //                cache.InitializeKeysContinuous( ref shifter, init.DelayTime );
        //            else
        //                cache.InitializeKeys( ref shifter );

        //            this.Caches[ ent ] = cache;
        //            this.Shifters[ ent ] = shifter;
        //        }
        //    }

            
        //    void makeEnableSection( int index, Entity entTop )
        //    {
        //        for( var ent = entTop; ent != Entity.Null; ent = this.Linkers[ ent ].NextStreamEntity )
        //        {
        //            this.Commands.RemoveComponent<Disabled>( index, ent );
        //        }
        //    }
        //}

    }

}

