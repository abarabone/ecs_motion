using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;


namespace Abarabone.Model
{

    using Abarabone.Authoring;
    using Abarabone.SystemGroup;
    using Abarabone.Utilities;
    using Abarabone.Geometry;
    using Abarabone.Character;
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Model.Authoring;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup ))]
    [UpdateAfter(typeof( StreamToBoneSystem ) )]
    public class BoneTransformSystem : SystemBase//JobComponentSystem
    {


        protected override void OnUpdate()
        {

            var boneLinkers = this.GetComponentDataFromEntity<Bone.RelationLinkData>( isReadOnly: true );
            var locals = this.GetComponentDataFromEntity<Bone.LocalValueData>( isReadOnly: true );
            var poss = this.GetComponentDataFromEntity<Translation>();
            var rots = this.GetComponentDataFromEntity<Rotation>();
            var velocities = this.GetComponentDataFromEntity<PhysicsVelocity>();
            var masses = this.GetComponentDataFromEntity<PhysicsMass>( isReadOnly: true );

            var deltaTime = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .WithReadOnly( boneLinkers )
                .WithReadOnly( locals )
                .WithReadOnly( masses )
                .WithNativeDisableParallelForRestriction( poss )
                .WithNativeDisableParallelForRestriction( rots )
                .WithNativeDisableParallelForRestriction( velocities )
                .ForEach(
                        (
                            in DrawInstance.BoneLinkData linker,
                            in DrawInstance.TargetWorkData target
                        ) =>
                    {

                        if (target.DrawInstanceId == -1) return;

                        for(
                            var entity = linker.BoneRelationTop;
                            entity != Entity.Null;
                            entity = boneLinkers[ entity ].NextBoneEntity
                        )
                        {
                            var parentEntity = boneLinkers[entity].ParentBoneEntity;

                            var parentpos = poss[parentEntity];
                            var parentrot = rots[parentEntity];
                            var local = locals[entity];

                            BoneUtility.transform_(in parentpos, in parentrot, in local, out var newpos, out var newrot);


                            var im = masses.HasComponent(entity) ? masses[entity].InverseMass : 0.0f;
                            if (im != 0.0f && velocities.HasComponent(entity))
                            //if (velocities.HasComponent(entity))
                            {
                                var mass = masses[entity];
                                var toRt = new RigidTransform(newrot, newpos);
                                var frompos = poss[entity];
                                var fromrot = rots[entity];
                                velocities[entity] =
                                    PhysicsVelocity.CalculateVelocityToTarget
                                    (in mass, in frompos, in fromrot, in toRt, deltaTime);
                            }
                            else
                            {
                                poss[entity] = new Translation { Value = newpos };
                                rots[entity] = new Rotation { Value = newrot };
                            }

                            //var parent = boneRelationLinkers[ ent ].ParentBoneEntity;

                            //var ppos = bonePositions[ parent ].Value;
                            //var prot = boneRotations[ parent ].Value;

                            ////var lpos = this.BonePositions[ ent ].Value;
                            ////var lrot = this.BoneRotations[ ent ].Value;
                            //var lpos = boneLocals[ ent ].Position;
                            //var lrot = boneLocals[ ent ].Rotation;

                            //var pos = math.mul( prot, lpos ) + ppos;
                            //var rot = math.mul( prot, lrot );

                            //var mass = boneMasses.HasComponent( ent ) ? boneMasses[ ent ].InverseMass : 0.0f;
                            //if( mass != 0.0f && boneVelocities.HasComponent( ent ) )
                            //    { }//setVelocity_( ent, pos, rot );
                            //else
                            //    setPosAndRot_( ent, pos, rot );
                        }
                    }
                )
                .ScheduleParallel();

            //return;


            //void setPosAndRot_( Entity ent, float3 pos, quaternion rot )
            //{
            //    bonePositions[ ent ] = new Translation { Value = pos };
            //    boneRotations[ ent ] = new Rotation { Value = rot };
            //}

            //void setVelocity_( Entity ent, float3 pos, quaternion rot )
            //{
            //    var rcdt = math.rcp( deltaTime );

            //    var v = boneVelocities[ ent ];

            //    v.Linear = ( pos - bonePositions[ ent ].Value ) * rcdt;

            //    //var invprev = math.inverse( this.BoneRotations[ ent ].Value );
            //    //var drot = math.mul( invprev, rot );
            //    //var angle = math.acos(drot.value.w);
            //    //var sin = math.sin( angle );
            //    //var axis = drot.value.As_float3() * math.rcp(sin);

            //    var invprev = math.inverse( boneRotations[ ent ].Value );
            //    var drot = math.mul( invprev, rot );
            //    var axis = drot.value.As_float3();
            //    var angle = math.lengthsq( drot );

            //    v.Angular = axis * ( angle * rcdt );

            //    boneVelocities[ ent ] = v;

            //    //setPosAndRot( ent, pos, rot );//
            //}
        }
        



        //protected JobHandle OnUpdate( JobHandle inputDeps )
        //{

        //    inputDeps = new BoneTransformJob
        //    {
        //        BoneRelationLinkers = this.GetComponentDataFromEntity<Bone.RelationLinkData>( isReadOnly: true ),
        //        BoneLocals = this.GetComponentDataFromEntity<Bone.LocalValueData>( isReadOnly: true ),
        //        BonePositions = this.GetComponentDataFromEntity<Translation>(),
        //        BoneRotations = this.GetComponentDataFromEntity<Rotation>(),
        //        BoneVelocities = this.GetComponentDataFromEntity<PhysicsVelocity>(),
        //        DeltaTime = UnityEngine.Time.deltaTime,//Time.DeltaTime,
        //    }
        //    .Schedule( this, inputDeps );


        //    return inputDeps;
        //}


        //[BurstCompile]
        //struct BoneTransformJob : IJobForEach<Posture.NeedTransformTag, Posture.LinkData>
        //{

        //    [ReadOnly]
        //    public ComponentDataFromEntity<Bone.RelationLinkData>    BoneRelationLinkers;
        //    [ReadOnly]
        //    public ComponentDataFromEntity<Bone.LocalValueData>      BoneLocals;

        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<Translation>     BonePositions;
        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<Rotation>        BoneRotations;
        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<PhysicsVelocity> BoneVelocities;

        //    [ReadOnly]
        //    public float DeltaTime;


        //    public void Execute(
        //        [ReadOnly] ref Posture.NeedTransformTag tag,
        //        [ReadOnly] ref Posture.LinkData linker
        //    )
        //    {
        //        for(
        //            var ent = linker.BoneRelationTop;
        //            ent != Entity.Null;
        //            ent = this.BoneRelationLinkers[ent].NextBoneEntity
        //        )
        //        {
        //            var parent = this.BoneRelationLinkers[ ent ].ParentBoneEntity;

        //            var ppos = this.BonePositions[ parent ].Value;
        //            var prot = this.BoneRotations[ parent ].Value;

        //            //var lpos = this.BonePositions[ ent ].Value;
        //            //var lrot = this.BoneRotations[ ent ].Value;
        //            var lpos = this.BoneLocals[ ent ].Position;
        //            var lrot = this.BoneLocals[ ent ].Rotation;

        //            var pos = math.mul( prot, lpos ) + ppos;
        //            var rot = math.mul( prot, lrot );

        //            if( this.BoneVelocities.HasComponent( ent ) )
        //                { }//this.setVelocity( ent, pos, rot );
        //            else
        //                this.setPosAndRot( ent, pos, rot );
        //        }
        //    }

        //    [MethodImpl( MethodImplOptions.AggressiveInlining )]
        //    void setPosAndRot( Entity ent, float3 pos, quaternion rot )
        //    {
        //        this.BonePositions[ ent ] = new Translation { Value = pos };
        //        this.BoneRotations[ ent ] = new Rotation { Value = rot };
        //    }

        //    [MethodImpl( MethodImplOptions.AggressiveInlining )]
        //    void setVelocity( Entity ent, float3 pos, quaternion rot )
        //    {
        //        var rcdt = math.rcp( this.DeltaTime );

        //        var v = this.BoneVelocities[ ent ];

        //        v.Linear = ( pos - this.BonePositions[ ent ].Value ) * rcdt;

        //        //var invprev = math.inverse( this.BoneRotations[ ent ].Value );
        //        //var drot = math.mul( invprev, rot );
        //        //var angle = math.acos(drot.value.w);
        //        //var sin = math.sin( angle );
        //        //var axis = drot.value.As_float3() * math.rcp(sin);

        //        var invprev = math.inverse( this.BoneRotations[ ent ].Value );
        //        var drot = math.mul( invprev, rot );
        //        var axis = drot.value.As_float3();
        //        var angle = math.lengthsq( drot );

        //        v.Angular = axis * ( angle * rcdt );

        //        this.BoneVelocities[ ent ] = v;

        //        //this.setPosAndRot( ent, pos, rot );//
        //    }
        //}
    }
    
}
