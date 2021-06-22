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
using System.Runtime.CompilerServices;

namespace DotsLite.Model
{

    
    using DotsLite.SystemGroup;
    using DotsLite.Utilities;
    using DotsLite.Geometry;
    using DotsLite.Character;
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using Unity.Physics;

    static public class BoneUtility
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void BoneTransform
            (in this Bone.LocalValueData local, 
                in Translation parentpos, in Rotation parentrot,
                ref Translation pos, ref Rotation rot
            )
        {
            transform_(in parentpos, in parentrot, in local, out var newpos, out var newrot);

            pos = new Translation { Value = newpos };
            rot = new Rotation { Value = newrot };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public PhysicsVelocity BoneTransform
            (in this Bone.LocalValueData local, 
                in Translation parentpos, in Rotation parentrot,
                in Translation pos, in Rotation rot,
                in PhysicsMass mass,
                float deltaTime
            )
        {
            transform_(in parentpos, in parentrot, in local, out var newpos, out var newrot);

            var toRt = new RigidTransform(newrot, newpos);
            var frompos = pos;
            var fromrot = rot;
            return PhysicsVelocity.CalculateVelocityToTarget(in mass, in frompos, in fromrot, in toRt, deltaTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void transform_
            (in Translation parentpos, in Rotation parentrot, in Bone.LocalValueData local, out float3 newpos, out quaternion newrot )
        {
            var ppos = parentpos.Value;
            var prot = parentrot.Value;

            var lpos = local.Position;
            var lrot = local.Rotation;

            newpos = math.mul(prot, lpos) + ppos;
            newrot = math.mul(prot, lrot);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static public void BoneTransform
        //    (
        //        Entity entity, Entity parentEntity,
        //        //in ComponentDataFromEntity<Bone.RelationLinkData> links,
        //        in ComponentDataFromEntity<Bone.LocalValueData> locals,
        //        ref ComponentDataFromEntity<Translation> poss,
        //        ref ComponentDataFromEntity<Rotation> rots,
        //        ref ComponentDataFromEntity<PhysicsVelocity> velocities,
        //        in ComponentDataFromEntity<PhysicsMass> masses,
        //        float deltaTime
        //    )
        //{
        //    var parentpos = poss[parentEntity];
        //    var parentrot = rots[parentEntity];
        //    var local = locals[entity];
        //    transform_(in parentpos, in parentrot, in local, out var newpos, out var newrot);

        //    var im = masses.HasComponent(entity) ? masses[entity].InverseMass : 0.0f;
        //    if (im != 0.0f && velocities.HasComponent(entity))
        //        //if (velocities.HasComponent(entity))
        //    {
        //        var mass = masses[entity];
        //        var toRt = new RigidTransform(newrot, newpos);
        //        var frompos = poss[entity];
        //        var fromrot = rots[entity];
        //        velocities[entity] =
        //            PhysicsVelocity.CalculateVelocityToTarget(in mass, in frompos, in fromrot, in toRt, deltaTime);
        //    }
        //    else
        //    {
        //        poss[entity] = new Translation { Value = newpos };
        //        rots[entity] = new Rotation { Value = newrot };
        //    }
        //}



        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //void setPosAndRot_(Entity ent, float3 pos, quaternion rot)
        //{
        //    bonePositions[ent] = new Translation { Value = pos };
        //    boneRotations[ent] = new Rotation { Value = rot };
        //}

        //void setVelocity_(Entity ent, float3 pos, quaternion rot)
        //{
        //    var rcdt = math.rcp(deltaTime);

        //    var v = boneVelocities[ent];

        //    v.Linear = (pos - bonePositions[ent].Value) * rcdt;

        //    //var invprev = math.inverse( this.BoneRotations[ ent ].Value );
        //    //var drot = math.mul( invprev, rot );
        //    //var angle = math.acos(drot.value.w);
        //    //var sin = math.sin( angle );
        //    //var axis = drot.value.As_float3() * math.rcp(sin);

        //    var invprev = math.inverse(boneRotations[ent].Value);
        //    var drot = math.mul(invprev, rot);
        //    var axis = drot.value.As_float3();
        //    var angle = math.lengthsq(drot);

        //    v.Angular = axis * (angle * rcdt);

        //    boneVelocities[ent] = v;

        //    //setPosAndRot( ent, pos, rot );//
        //}
    }
}