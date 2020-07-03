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


namespace Abarabone.Motion
{

    using Abarabone.Authoring;
    using Abarabone.SystemGroup;
    using Abarabone.Utilities;
    using Abarabone.Geometry;
    using Abarabone.Character;
    using Abarabone.Model;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ))]
    [UpdateAfter(typeof( StreamToBoneSystem ) )]
    public class BoneTransformSystem : SystemBase//JobComponentSystem
    {


        protected override void OnUpdate()
        {

            var boneRelationLinkers = this.GetComponentDataFromEntity<Bone.RelationLinkData>( isReadOnly: true );
            var boneLocals = this.GetComponentDataFromEntity<BoneLocalValueData>( isReadOnly: true );
            var bonePositions = this.GetComponentDataFromEntity<Translation>();
            var boneRotations = this.GetComponentDataFromEntity<Rotation>();
            var boneVelocities = this.GetComponentDataFromEntity<PhysicsVelocity>();
            var boneMasses = this.GetComponentDataFromEntity<PhysicsMass>( isReadOnly: true );

            var deltaTime = this.Time.DeltaTime;

            this.Entities
                .WithName( "BoneTransformSystem" )
                .WithAll<PostureNeedTransformTag>()
                .WithBurst()
                .WithReadOnly( boneRelationLinkers )
                .WithReadOnly( boneLocals )
                .WithReadOnly( boneMasses )
                .WithNativeDisableParallelForRestriction( bonePositions )
                .WithNativeDisableParallelForRestriction( boneRotations )
                .WithNativeDisableParallelForRestriction( boneVelocities )
                .ForEach(
                    ( in PostureLinkData linker ) =>
                    {
                        for(
                            var ent = linker.BoneRelationTop;
                            ent != Entity.Null;
                            ent = boneRelationLinkers[ ent ].NextBoneEntity
                        )
                        {
                            var parent = boneRelationLinkers[ ent ].ParentBoneEntity;

                            var ppos = bonePositions[ parent ].Value;
                            var prot = boneRotations[ parent ].Value;

                            //var lpos = this.BonePositions[ ent ].Value;
                            //var lrot = this.BoneRotations[ ent ].Value;
                            var lpos = boneLocals[ ent ].Position;
                            var lrot = boneLocals[ ent ].Rotation;

                            var pos = math.mul( prot, lpos ) + ppos;
                            var rot = math.mul( prot, lrot );

                            var mass = boneMasses.Exists( ent ) ? boneMasses[ ent ].InverseMass : 0.0f;
                            if( mass != 0.0f && boneVelocities.Exists( ent ) )
                                { }//setVelocity_( ent, pos, rot );
                            else
                                setPosAndRot_( ent, pos, rot );
                        }
                    }
                )
                .ScheduleParallel();

            return;


            void setPosAndRot_( Entity ent, float3 pos, quaternion rot )
            {
                bonePositions[ ent ] = new Translation { Value = pos };
                boneRotations[ ent ] = new Rotation { Value = rot };
            }

            void setVelocity_( Entity ent, float3 pos, quaternion rot )
            {
                var rcdt = math.rcp( deltaTime );

                var v = boneVelocities[ ent ];

                v.Linear = ( pos - bonePositions[ ent ].Value ) * rcdt;

                //var invprev = math.inverse( this.BoneRotations[ ent ].Value );
                //var drot = math.mul( invprev, rot );
                //var angle = math.acos(drot.value.w);
                //var sin = math.sin( angle );
                //var axis = drot.value.As_float3() * math.rcp(sin);

                var invprev = math.inverse( boneRotations[ ent ].Value );
                var drot = math.mul( invprev, rot );
                var axis = drot.value.As_float3();
                var angle = math.lengthsq( drot );

                v.Angular = axis * ( angle * rcdt );

                boneVelocities[ ent ] = v;

                //setPosAndRot( ent, pos, rot );//
            }
        }
        



        protected JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new BoneTransformJob
            {
                BoneRelationLinkers = this.GetComponentDataFromEntity<Bone.RelationLinkData>( isReadOnly: true ),
                BoneLocals = this.GetComponentDataFromEntity<BoneLocalValueData>( isReadOnly: true ),
                BonePositions = this.GetComponentDataFromEntity<Translation>(),
                BoneRotations = this.GetComponentDataFromEntity<Rotation>(),
                BoneVelocities = this.GetComponentDataFromEntity<PhysicsVelocity>(),
                DeltaTime = UnityEngine.Time.deltaTime,//Time.DeltaTime,
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }


        [BurstCompile]
        struct BoneTransformJob : IJobForEach<PostureNeedTransformTag, PostureLinkData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<Bone.RelationLinkData>    BoneRelationLinkers;
            [ReadOnly]
            public ComponentDataFromEntity<BoneLocalValueData>      BoneLocals;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation>     BonePositions;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Rotation>        BoneRotations;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<PhysicsVelocity> BoneVelocities;

            [ReadOnly]
            public float DeltaTime;


            public void Execute(
                [ReadOnly] ref PostureNeedTransformTag tag,
                [ReadOnly] ref PostureLinkData linker
            )
            {
                for(
                    var ent = linker.BoneRelationTop;
                    ent != Entity.Null;
                    ent = this.BoneRelationLinkers[ent].NextBoneEntity
                )
                {
                    var parent = this.BoneRelationLinkers[ ent ].ParentBoneEntity;

                    var ppos = this.BonePositions[ parent ].Value;
                    var prot = this.BoneRotations[ parent ].Value;

                    //var lpos = this.BonePositions[ ent ].Value;
                    //var lrot = this.BoneRotations[ ent ].Value;
                    var lpos = this.BoneLocals[ ent ].Position;
                    var lrot = this.BoneLocals[ ent ].Rotation;

                    var pos = math.mul( prot, lpos ) + ppos;
                    var rot = math.mul( prot, lrot );

                    if( this.BoneVelocities.Exists( ent ) )
                        { }//this.setVelocity( ent, pos, rot );
                    else
                        this.setPosAndRot( ent, pos, rot );
                }
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            void setPosAndRot( Entity ent, float3 pos, quaternion rot )
            {
                this.BonePositions[ ent ] = new Translation { Value = pos };
                this.BoneRotations[ ent ] = new Rotation { Value = rot };
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            void setVelocity( Entity ent, float3 pos, quaternion rot )
            {
                var rcdt = math.rcp( this.DeltaTime );

                var v = this.BoneVelocities[ ent ];

                v.Linear = ( pos - this.BonePositions[ ent ].Value ) * rcdt;

                //var invprev = math.inverse( this.BoneRotations[ ent ].Value );
                //var drot = math.mul( invprev, rot );
                //var angle = math.acos(drot.value.w);
                //var sin = math.sin( angle );
                //var axis = drot.value.As_float3() * math.rcp(sin);

                var invprev = math.inverse( this.BoneRotations[ ent ].Value );
                var drot = math.mul( invprev, rot );
                var axis = drot.value.As_float3();
                var angle = math.lengthsq( drot );

                v.Angular = axis * ( angle * rcdt );

                this.BoneVelocities[ ent ] = v;

                //this.setPosAndRot( ent, pos, rot );//
            }
        }
    }
    
}
