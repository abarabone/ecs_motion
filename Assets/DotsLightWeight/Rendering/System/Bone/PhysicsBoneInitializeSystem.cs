using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;


namespace Abarabone.Physics
{
    
    using Abarabone.SystemGroup;
    using Abarabone.Utilities;
    using Abarabone.Geometry;
    using Abarabone.Character;
    using Abarabone.CharacterMotion;//
    using Abarabone.Model;



    //[DisableAutoCreation]
    //[UpdateInGroup( typeof( BonePhysicsSystemGroup ) )]
    [UpdateBefore(typeof(MotionStreamProgressAndInterporationSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup))]
    public class PhysicsBoneInitializeSystem : SystemBase
    {

        EntityCommandBufferSystem cmdSystem;


        protected override void OnCreate()
        {
            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }


        protected override void OnUpdate()
        {
            var commands = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();
            var translations = this.GetComponentDataFromEntity<Translation>();// isReadOnly: true );
            var rotations = this.GetComponentDataFromEntity<Rotation>();// isReadOnly: true );


            this.Entities
                .WithNativeDisableParallelForRestriction(rotations)
                .WithNativeDisableParallelForRestriction(translations)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        //ref Translation pos,
                        //ref Rotation rot,
                        ref PhysicsVelocity v,
                        in Bone.InitializeData init
                    )
                =>
                    {
                        var basepos = translations[init.PostureEntity];
                        var baserot = rotations[init.PostureEntity];

                        var pos = translations[entity];//
                        var rot = rotations[entity];//

                        pos.Value = basepos.Value + math.mul(baserot.Value, pos.Value);
                        rot.Value = math.mul(baserot.Value, rot.Value);
                        v = new PhysicsVelocity { Linear = float3.zero, Angular = float3.zero };

                        translations[entity] = pos;//
                        rotations[entity] = rot;//

                        commands.RemoveComponent<Bone.InitializeData>(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
        }

    }
}
