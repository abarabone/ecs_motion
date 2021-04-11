//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Burst;
//using Unity.Transforms;
//using System.Runtime.CompilerServices;

//namespace Abarabone.Structure//Character.Action
//{
//    using Abarabone.Dependency;
//    using Abarabone.Structure;
//    using HitMessageUnit = StructureHitMessage;



//    //[UpdateInGroup(typeof(InitializationSystemGroup))]
//    //public class BulletHitApplyToCharacterAllocationSystem : SystemBase
//    //{
//    //    BulletHitApplyToCharacterSystem hitSystem;

//    //    protected override void OnCreate() =>
//    //        this.hitSystem = this.World.GetExistingSystem<BulletHitApplyToCharacterSystem>();

//    //    protected override void OnUpdate()
//    //    {
//    //        this.hitSystem.Clear();
//    //    }
//    //}


//    [DisableAutoCreation]
//    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
//    public class StructureHitMessageApplySystem__
//        : CommandSystemBase<BeginInitializationEntityCommandBufferSystem>
//    {



//        NativeList<Entity> keyEntities;
//        NativeMultiHashMap<Entity, HitMessageUnit> messageHolder;
//        //ParallelWriter writer;

//        public struct ParallelWriter
//        {
//            NativeList<Entity>.ParallelWriter nl;
//            NativeMultiHashMap<Entity, HitMessageUnit>.ParallelWriter hm;
//            public ParallelWriter(ref NativeList<Entity> nl, ref NativeMultiHashMap<Entity, HitMessageUnit> hm)
//            {
//                this.nl = nl.AsParallelWriter();
//                this.hm = hm.AsParallelWriter();
//            }
//            public void Add(Entity entity, HitMessageUnit hitMessage)
//            {
//                this.nl.AddNoResize(entity);
//                this.hm.Add(entity, hitMessage);
//            }
//        }

//        public ParallelWriter GetParallelWriter() => new ParallelWriter(ref this.keyEntities, ref this.messageHolder);//this.writer;
        
//        public void Clear()
//        {
//            this.keyEntities.Clear();
//            this.messageHolder.Clear();
//        }



//        NativeList<JobHandle> dependencyJobHandles = new NativeList<JobHandle>(32, Allocator.Persistent);

//        public void AddDependencyBeforeHitApply(JobHandle jobHandle) => this.dependencyJobHandles.Add(jobHandle);

//        void waitAllDependencyJobs()
//        {
//            JobHandle.CombineDependencies(this.dependencyJobHandles).Complete();

//            this.dependencyJobHandles.Clear();
//        }




//        protected override void OnCreate()
//        {
//            base.OnCreate();

//            var capacity = 10000;
//            this.keyEntities = new NativeList<Entity>(capacity, Allocator.Persistent);
//            this.messageHolder = new NativeMultiHashMap<Entity, HitMessageUnit>(capacity, Allocator.Persistent);
//        }

//        protected override void OnDestroy()
//        {
//            base.OnDestroy();

//            this.keyEntities.Dispose();
//            this.messageHolder.Dispose();
//        }

//        protected override void OnUpdateWith(EntityCommandBuffer commandBuffer)
//        {
//            this.waitAllDependencyJobs();

//            var cmd = commandBuffer.AsParallelWriter();

//            var destructions = this.GetComponentDataFromEntity<Structure.PartDestructionData>();
//            var prefabs = this.GetComponentDataFromEntity<StructurePart.DebrisPrefabData>(isReadOnly: true);
//            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
//            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

//            this.Dependency = new HitMessageApplyJob
//            {
//                MessageHolder = this.messageHolder,
//                KeyEntities = this.keyEntities.AsDeferredJobArray(),

//                Cmd = cmd,
//                Destructions = destructions,
//                Prefabs = prefabs,
//                Rotations = rots,
//                Positions = poss,
//            }
//            .Schedule(this.keyEntities, 8, this.Dependency);


//            var ke = this.keyEntities;
//            var mh = this.messageHolder;
//            this.Job
//                .WithName("hitMessageClear")
//                .WithCode(
//                    () =>
//                    {
//                        ke.Clear();
//                        mh.Clear();
//                    }
//                )
//                .Schedule();
//        }

//        [BurstCompile]
//        struct HitMessageApplyJob : IJobParallelForDefer
//        {
//            [ReadOnly]
//            public NativeMultiHashMap<Entity, HitMessageUnit> MessageHolder;

//            [ReadOnly]
//            public NativeArray<Entity> KeyEntities;

//            public void Execute(int index)
//            {
//                var key = this.KeyEntities[index];
//                var msgs = this.MessageHolder.GetValuesForKey(key);

//                foreach (var msg in msgs)
//                {
//                    ExecuteNext(index, key, msg);
//                }
//            }


//            public EntityCommandBuffer.ParallelWriter Cmd;

//            [NativeDisableParallelForRestriction]
//            public ComponentDataFromEntity<Structure.PartDestructionData> Destructions;

//            [ReadOnly]
//            public ComponentDataFromEntity<StructurePart.DebrisPrefabData> Prefabs;
//            [ReadOnly]
//            public ComponentDataFromEntity<Rotation> Rotations;
//            [ReadOnly]
//            public ComponentDataFromEntity<Translation> Positions;


//            [BurstCompile]
//            public void ExecuteNext(int uniqueIndex, Entity key, StructureHitMessage value)
//            {

//                var destruction = this.Destructions[key];

//                // 複数の子パーツから１つの親構造物のフラグを立てることがあるので、並列化の際に注意が必要
//                // 今回は、同じ key は同じスレッドで処理されるようなので成立する。
//                destruction.SetDestroyed(value.PartId);

//                this.Destructions[key] = destruction;


//                var part = value.PartEntity;
//                var prefab = this.Prefabs[part].DebrisPrefab;
//                var rot = this.Rotations[part];
//                var pos = this.Positions[part];
//                createDebris_(this.Cmd, uniqueIndex, prefab, rot, pos);

//                destroyPart_(this.Cmd, uniqueIndex, part);

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
