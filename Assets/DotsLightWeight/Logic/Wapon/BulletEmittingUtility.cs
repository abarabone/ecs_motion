using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
////using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

using Random = Unity.Mathematics.Random;

namespace DotsLite.Arms
{

    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Draw;
    using DotsLite.Particle;
    using DotsLite.CharacterMotion;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.Collision;
    using DotsLite.SystemGroup;
    using DotsLite.Structure;
    using DotsLite.Dependency;
    using DotsLite.Targeting;


    public enum BulletType
    {
        None,
        Bullet,
        Sphere,
        Explosion,
        Beam,
    }


    static public class BulletEmittingUtility
    {

        /// <summary>
        /// 種類列挙型からコンポーネントの型を取得する
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentType ToComponentType(this BulletType type) =>
            type switch
            {
                BulletType.Bullet => typeof(Bullet.RayTag),
                BulletType.Sphere => typeof(Bullet.SphereTag),
                BulletType.Beam => typeof(Bullet.BeamTag),
                _ => default,
            };





        // 前回の発射が直前のフレームなら連続した発射間隔、はなれたフレームなら今フレームをベースにした発射間隔になる
        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalcBaseTime(float currentTime, float nextEmitableTime, float dt)
        {
            var frameBaseTime = currentTime - dt;
            var isEmitPrevFrame = nextEmitableTime > frameBaseTime;
            var baseTime = math.select(frameBaseTime, nextEmitableTime, isEmitPrevFrame);
            return baseTime;
        }




        public struct BulletEmittingParams
        {
            float4 position;
            float4 acceleration;
            float4 speed;
            float lifetime;
        }


        public static BulletEmittingParams CalcEmittingParams(
            FunctionUnit.BulletEmittingData
            Translation pos, Rotation rot,
            float3 gravity, float gravityFactor)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 CalcAcc(float3 gravity, float gravityFactor)
        {
            var g = new DirectionAndLength { Value = gravity.As_float4(gravityFactor) };
            var acc = g.Ray;
            return acc;
        }



        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 CalcMuzzlePosition(
            quaternion rot, float3 pos, float3 muzzlePositionLocal)
        {

            var muzpos = pos + math.mul(rot, muzzlePositionLocal);

            return muzpos;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static float3 calcBulletPosition_(
        //    quaternion rot, float3 pos, in FunctionUnit.BulletEmittingData emitter)
        //{

        //    var muzpos = pos + math.mul(rot, emitter.MuzzlePositionLocal);

        //    return muzpos;
        //}



        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 CalcBulletDirection(quaternion dirrot, ref Random rnd, float accuracyRad)
        {

            var yrad = rnd.NextFloat(accuracyRad);
            var zrad = rnd.NextFloat(2.0f * math.PI);
            var bulletDir = math.mul(dirrot, math.forward(quaternion.EulerYZX(0.0f, yrad, zrad)));

            return bulletDir;
        }



        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EmitEffect(
            EntityCommandBuffer.ParallelWriter cmd, int eqi, Entity effectPrefab,
            float3 pos, ref Random rnd)
        {
            //if (effectPrefab == Entity.Null) return;

            var ent = cmd.Instantiate(eqi, effectPrefab);

            cmd.SetComponent(eqi, ent, new Translation
            {
                Value = pos,
            });
            cmd.SetComponent(eqi, ent, new BillBoad.RotationData
            {
                Direction = rnd.NextFloat2Direction() * rnd.NextFloat(0.8f, 1.2f),
            });
        }


        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EmitBullet(
            EntityCommandBuffer.ParallelWriter cmd, int eqi,
            Entity bulletPrefab, Entity stateEntity,
            float3 bulletPosition, float range, float3 speed, float3 acc, Corps targetCorps)
        {

            var newBullet = cmd.Instantiate(eqi, bulletPrefab);

            //cmd.SetComponent(eqi, newBullet,
            //    new Particle.TranslationTailData
            //    {
            //        PositionAndSize = bulletPosition.As_float4(),
            //    }
            //);
            cmd.SetComponent(eqi, newBullet,
                new Translation
                {
                    Value = bulletPosition,
                }
            );
            cmd.SetComponent(eqi, newBullet,
                new Bullet.VelocityData
                {
                    Velocity = speed.As_float4(),
                }
            );
            cmd.SetComponent(eqi, newBullet,
                new Bullet.AccelerationData
                {
                    Acceleration = acc.As_float4(),
                }
            );
            cmd.SetComponent(eqi, newBullet,
                new Bullet.DistanceData
                {
                    RestRangeDistance = range,
                }
            );
            cmd.SetComponent(eqi, newBullet,
                new Bullet.LinkData
                {
                    OwnerStateEntity = stateEntity,
                }
            );
            cmd.SetComponent(eqi, newBullet,
                new CorpsGroup.TargetWithArmsData
                {
                    TargetCorps = targetCorps,
                }
            );

        }


    }

}

