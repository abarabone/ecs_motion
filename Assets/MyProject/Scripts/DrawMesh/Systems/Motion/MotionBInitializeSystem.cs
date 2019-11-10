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
                ref var clip = ref data.ClipData.Value;
                ref var motion = ref clip.Motions[ init.MotionIndex ];

                info.MotionIndex = init.MotionIndex;
                initSection( ref motion, linker.PositionStreamTop, KeyStreamSection.positions, ref init );
                initSection( ref motion, linker.RotationStreamTop, KeyStreamSection.rotations, ref init );

                cursor.Timer.TimeLength = motion.TimeLength;
                cursor.Timer.TimeProgress = -init.DelayTime;
                cursor.Timer.TimeScale = 1.0f;

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
                    var prevKeyPtr = shifter.Keys;// 仮
                    shifter.Keys = (KeyBlobUnit*)streams[ i ].Keys.GetUnsafePtr();
                    shifter.KeyLength = streams[ i ].Keys.Length;
                    
                    if( prevKeyPtr != null )// 仮
                    {
                        var cache_ = this.Caches[ ent ];
                        InitializeKeysContinuous_( ref cache_, ref shifter, init.DelayTime );
                        this.Caches[ ent ] = cache_;
                        this.Shifters[ ent ] = shifter;
                        continue;
                    }

                    var cache = this.Caches[ ent ];
                    InitializeKeys_( ref cache, ref shifter );

                    this.Caches[ ent ] = cache;
                    this.Shifters[ ent ] = shifter;
                }



                /// <summary>
                /// キーバッファをストリーム先頭に初期化する。
                /// </summary>
                unsafe void InitializeKeys_(
                    ref StreamNearKeysCacheData nearKeys,
                    ref StreamKeyShiftData shift,
                    float timeOffset = 0.0f
                )
                {
                    var index0 = 0;
                    var index1 = math.min( 1, shift.KeyLength - 1 );
                    var index2 = math.min( 2, shift.KeyLength - 1 );

                    nearKeys.Time_From = shift.Keys[ index0 ].Time.x;
                    nearKeys.Time_To = shift.Keys[ index1 ].Time.x;
                    nearKeys.Time_Next = shift.Keys[ index2 ].Time.x;

                    nearKeys.Value_Prev = shift.Keys[ index0 ].Value;
                    nearKeys.Value_From = shift.Keys[ index0 ].Value;
                    nearKeys.Value_To = shift.Keys[ index1 ].Value;
                    nearKeys.Value_Next = shift.Keys[ index2 ].Value;

                    shift.KeyIndex_Next = index2;
                }

                unsafe void InitializeKeysContinuous_(
                    ref StreamNearKeysCacheData nearKeys,
                    ref StreamKeyShiftData shift,
                    float delayTimer = 0.0f// 再検討の余地あり（変な挙動あり）
                )
                {
                    var index0 = 0;
                    var index1 = math.min( 1, shift.KeyLength - 1 );

                    nearKeys.Time_From = -delayTimer;
                    nearKeys.Time_To = shift.Keys[ index0 ].Time.x;
                    nearKeys.Time_Next = shift.Keys[ index1 ].Time.x;

                    nearKeys.Value_To = shift.Keys[ index0 ].Value;
                    nearKeys.Value_Next = shift.Keys[ index1 ].Value;

                    shift.KeyIndex_Next = index1;
                }

            }

        }

    }

}

