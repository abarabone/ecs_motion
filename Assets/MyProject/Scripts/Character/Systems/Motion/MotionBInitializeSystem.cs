using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abss.SystemGroup;

namespace Abss.Motion
{
    
    [UpdateBefore( typeof( MotionProgressSystem ) )]// MotionB
    [UpdateInGroup(typeof(MotionSystemGroup))]
    public class MotionBInitializeSystem : JobComponentSystem
    {

        EntityCommandBufferSystem ecb;


        protected override void OnCreate()
        {
            this.ecb = World.Active.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }
        


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var commandBuffer = this.ecb.CreateCommandBuffer();


            inputDeps = new MotionInitializeJob
            {
                Commands = commandBuffer.ToConcurrent(),
                Linkers  = this.GetComponentDataFromEntity<StreamRelationData>(),
                Shifters = this.GetComponentDataFromEntity<StreamKeyShiftData>(),
                Caches   = this.GetComponentDataFromEntity<StreamNearKeysCacheData>(),

            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );


            return inputDeps;
        }



        //[BurstCompile]
        struct MotionInitializeJob : IJobForEachWithEntity
            <MotionInitializeData, MotionStreamLinkData, MotionClipData, MotionInfoData, MotionCursorData>
        {

            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;

            [NativeDisableParallelForRestriction][ReadOnly]
            public ComponentDataFromEntity<StreamRelationData>      Linkers;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<StreamKeyShiftData>      Shifters;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<StreamNearKeysCacheData> Caches;
            

            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref MotionInitializeData init,
                [ReadOnly] ref MotionStreamLinkData linker,
                [ReadOnly] ref MotionClipData data,
                ref MotionInfoData info,
                ref MotionCursorData cursor
            )
            {
                ref var blob = ref data.ClipData.Value;
                ref var motionClip = ref blob.Motions[ init.MotionIndex ];

                info.MotionIndex = init.MotionIndex;
                initSection( ref motionClip, linker.PositionStreamTop, KeyStreamSection.positions, ref init );
                initSection( ref motionClip, linker.RotationStreamTop, KeyStreamSection.rotations, ref init );

                cursor.InitializeCursor( ref motionClip, init.DelayTime );

                //if( )

                this.Commands.RemoveComponent<MotionInitializeData>( index, entity );
            }

            unsafe void initSection
                ( ref MotionBlobUnit motion, Entity entTop, KeyStreamSection streamSection, ref MotionInitializeData init )
            {
                ref var streams = ref motion.Sections[(int)streamSection].Streams;

                for( var ent = entTop; ent != Entity.Null; ent = this.Linkers[ent].NextStreamEntity )
                {
                    var i = Linkers[ ent ].BoneId;
                    
                    var shifter = this.Shifters[ ent ];
                    shifter.Keys = (KeyBlobUnit*)streams[ i ].Keys.GetUnsafePtr();
                    shifter.KeyLength = streams[ i ].Keys.Length;
                    
                    var cache = this.Caches[ ent ];
                    if( init.IsContinuous )
                        cache.InitializeKeysContinuous( ref shifter, init.DelayTime );
                    else
                        cache.InitializeKeys( ref shifter );

                    this.Caches[ ent ] = cache;
                    this.Shifters[ ent ] = shifter;
                }
            }


            void makeEnableStreams( )
            {

            }
        }

    }

}

