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


using Abss.Arthuring;
using Abss.SystemGroup;
using Abss.Utilities;
using Abss.Geometry;
using Abss.Character;

namespace Abss.Motion
{

    //[DisableAutoCreation]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ))]
    [UpdateAfter(typeof( StreamToBoneSystem ) )]
    public class BoneTransformSystem : JobComponentSystem
    {

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new BoneTransformJob
            {
                BoneRelationLinkers = this.GetComponentDataFromEntity<BoneRelationLinkData>( isReadOnly: true ),
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
            public ComponentDataFromEntity<BoneRelationLinkData>    BoneRelationLinkers;
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

                    //if( this.BoneVelocities.Exists( ent ) )
                    //    this.setVelocity( ent, pos, rot );
                    //else
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
