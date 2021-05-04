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


namespace DotsLite.Collision
{
    
    using DotsLite.SystemGroup;
    using DotsLite.Utilities;
    using DotsLite.Geometry;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;//
    using DotsLite.Model;
    using DotsLite.Dependency;


    //[DisableAutoCreation]
    //[UpdateInGroup( typeof( BonePhysicsSystemGroup ) )]
    [UpdateBefore(typeof(MotionStreamProgressAndInterporationSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup))]
    public class PhysicsBoneInitializeSystem : DependencyAccessableSystemBase
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

                        cmd.RemoveComponent<Bone.InitializeData>(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel();
        }

    }
}
