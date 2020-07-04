using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abarabone.SystemGroup;

namespace Abarabone.CharacterMotion
{
    
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ))]
    [UpdateBefore(typeof(MotionStreamProgressAndInterporationSystem))]
    public class MotionInitializeSystem : JobComponentSystem
    {

        EntityCommandBufferSystem ecb;


        protected override void OnCreate()
        {
            this.ecb = this.World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }
        


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var commandBuffer = this.ecb.CreateCommandBuffer();


            inputDeps = new MotionInitializeJob
            {
                Commands = commandBuffer.ToConcurrent(),
                Linkers  = this.GetComponentDataFromEntity<Stream.RelationData>(),
                Shifters = this.GetComponentDataFromEntity<Stream.KeyShiftData>(),
                Timers   = this.GetComponentDataFromEntity<Stream.CursorData>(),
                Caches   = this.GetComponentDataFromEntity<Stream.NearKeysCacheData>(),

            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );


            return inputDeps;
        }



        //[BurstCompile]
        [ExcludeComponent(typeof(Motion.CursorData))]
        struct MotionInitializeJob : IJobForEachWithEntity
            <Motion.InitializeData, Motion.StreamLinkData, Motion.ClipData, Motion.InfoData>
        {

            public EntityCommandBuffer.Concurrent Commands;

            [NativeDisableParallelForRestriction][ReadOnly]
            public ComponentDataFromEntity<Stream.RelationData>      Linkers;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Stream.KeyShiftData>      Shifters;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Stream.NearKeysCacheData> Caches;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Stream.CursorData>  Timers;


            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref Motion.InitializeData init,
                [ReadOnly] ref Motion.StreamLinkData linker,
                [ReadOnly] ref Motion.ClipData data,
                ref Motion.InfoData info
            )
            {
                ref var clip = ref data.MotionClipData.Value;
                ref var motion = ref clip.Motions[ init.MotionIndex ];

                info.MotionIndex = init.MotionIndex;
                if( init.IsContinuous )
                initSection( ref motion, linker.PositionStreamTop, KeyStreamSection.positions, ref init );
                initSection( ref motion, linker.RotationStreamTop, KeyStreamSection.rotations, ref init );

                this.Commands.RemoveComponent<Motion.InitializeData>( index, entity );
            }

            unsafe void initSection
                ( ref MotionBlobUnit motionClip, Entity entTop, KeyStreamSection streamSection, ref Motion.InitializeData init )
            {
                ref var streams = ref motionClip.Sections[(int)streamSection].Streams;

                for( var ent = entTop; ent != Entity.Null; ent = this.Linkers[ent].NextStreamEntity )
                {
                    var i = Linkers[ ent ].BoneId;
                    
                    var shifter = this.Shifters[ ent ];
                    shifter.Keys = (KeyBlobUnit*)streams[ i ].Keys.GetUnsafePtr();
                    shifter.KeyLength = streams[ i ].Keys.Length;

                    var timer = this.Timers[ ent ];
                    timer.Cursor.InitializeCursor( ref motionClip, init.DelayTime, init.TimeScale );
                    
                    var cache = this.Caches[ ent ];
                    if( init.IsContinuous )
                        cache.InitializeKeysContinuous( ref shifter, init.DelayTime );
                    else
                        cache.InitializeKeys( ref shifter );

                    this.Caches[ ent ] = cache;
                    this.Timers[ ent ] = timer;
                    this.Shifters[ ent ] = shifter;
                }
            }
        }
    }

}


