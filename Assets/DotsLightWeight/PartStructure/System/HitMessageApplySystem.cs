//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Burst;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Physics;
//using Unity.Physics.Systems;
//using UnityEngine.InputSystem;
//using Unity.Collections.LowLevel.Unsafe;
//using System;
//using Unity.Jobs.LowLevel.Unsafe;

//using Collider = Unity.Physics.Collider;
//using SphereCollider = Unity.Physics.SphereCollider;

//namespace Abarabone.Structure
//{
//    using Abarabone.Misc;
//    using Abarabone.Utilities;
//    using Abarabone.SystemGroup;
//    using Abarabone.Character;
//    using System.Security.Cryptography;
//    using UnityEngine.Video;
//    using System.Runtime.CompilerServices;

//    public struct StructureHitMessage
//    {
//        public int PartId;
//        public Entity PartEntity;
//        public float3 Position;
//        public float3 Normale;
//    }


//    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
//    public class HitMessageApplySystem : SystemBase
//    {

//        StructureHitMessageHolderAllocationSystem messageSystem;

//        EntityCommandBufferSystem cmdSystem;


//        protected override void OnCreate()
//        {
//            base.OnCreate();

//            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

//            this.messageSystem = this.World.GetExistingSystem<StructureHitMessageHolderAllocationSystem>();
//        }

//        protected override void OnUpdate()
//        {
//            var cmd = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();

//            //var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);

//            var destructions = this.GetComponentDataFromEntity<Structure.PartDestructionData>();
//            var prefabs = this.GetComponentDataFromEntity<StructurePart.DebrisPrefabData>(isReadOnly: true);
//            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
//            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            

//            var msgs = this.messageSystem.MsgHolder;
            
//            this.Dependency = new StructureHitApplyJob
//            {
//                Cmd = cmd,
//                Destructions = destructions,
//                Prefabs = prefabs,
//                Rotations = rots,
//                Positions = poss,
//            }
//            .Schedule(msgs, 0, this.Dependency);

//            // Make sure that the ECB system knows about our job
//            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
//        }


//        [BurstCompile]
//        struct StructureHitApplyJob : IJobNativeMultiHashMapVisitKeyMutableValue<Entity, StructureHitMessage>
//        {

//            public EntityCommandBuffer.ParallelWriter Cmd;


//            [NativeDisableParallelForRestriction]
//            public ComponentDataFromEntity<Structure.PartDestructionData> Destructions;

//            [ReadOnly]
//            public ComponentDataFromEntity<StructurePart.DebrisPrefabData> Prefabs;
//            [ReadOnly]
//            public ComponentDataFromEntity<Rotation> Rotations;
//            [ReadOnly]
//            public ComponentDataFromEntity<Translation> Positions;


//            //[BurstCompile]
//            public void ExecuteNext(int uniqueIndex, Entity key, ref StructureHitMessage value)
//            {

//                var destruction = this.Destructions[key];

//                // 複数の子パーツから１つの親構造物のフラグを立てることがあるので、並列化の際に注意が必要
//                destruction.SetDestroyed(value.PartId);

//                this.Destructions[key] = destruction;


//                var prefab = this.Prefabs[value.PartEntity].DebrisPrefab;
//                var rot = this.Rotations[value.PartEntity];
//                var pos = this.Positions[value.PartEntity];
//                createDebris_(this.Cmd, uniqueIndex, prefab, rot, pos);

//                destroyPart_(this.Cmd, uniqueIndex, value.PartEntity);

//            }

//        }
        


//        //[BurstCompile]
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static void createDebris_
//            (
//                EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity debrisPrefab_,
//                Rotation rot_, Translation pos_
//            )
//        {

//            var ent = cmd_.Instantiate(uniqueIndex_, debrisPrefab_);
//            cmd_.SetComponent(uniqueIndex_, ent, rot_);
//            cmd_.SetComponent(uniqueIndex_, ent, pos_);

//        }

//        //[BurstCompile]
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static void destroyPart_
//            (EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity part_)
//        {
//            cmd_.DestroyEntity(uniqueIndex_, part_);
//        }
//    }




    

//}
