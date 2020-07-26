using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;


namespace Abarabone.Arms
{

    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Draw;
    using Abarabone.Particle;
    using Abarabone.CharacterMotion;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.Physics;
    using Abarabone.SystemGroup;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(ObjectInitializeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    public class BeamSystem : SystemBase
    {

        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        EntityCommandBufferSystem cmdSystem;


        protected override void OnCreate()
        {
            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }


        protected override void OnUpdate()
        {
            var cmd = this.cmdSystem.CreateCommandBuffer().ToConcurrent();


            //var handles = this.GetComponentDataFromEntity<MoveHandlingData>(isReadOnly: true);

            var deltaTime = this.Time.DeltaTime;


            this.Entities
                //.WithoutBurst()
                .WithBurst()
                .WithAll<Bullet.BeamTag>()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref Bullet.BulletData beam,
                        ref Particle.AdditionalData additional
                    ) =>
                    {

                        beam.LifeTime -= deltaTime;

                        var dc = math.max(beam.LifeTime, 0.0f) * beam.InvTotalTime;
                        additional.Color = additional.Color.ApplyAlpha(dc);//(additional.Color.to_float4() * dc).ToColor32();

                    if (beam.LifeTime <= 0.0f)
                        {
                            cmd.DestroyEntity(entityInQueryIndex, entity);
                        }
                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
        }

    }


    static public class ColorExtension
    {
        static public int4 to_int4(this Color32 color) => new int4(color.r, color.g, color.b, color.a);
        static public Color32 ToColor32(this int4 color) => new Color32((byte)color.x, (byte)color.y, (byte)color.z, (byte)color.w);

        static public float4 to_float4(this Color32 color) => new float4(color.r, color.g, color.b, color.a);
        static public Color32 ToColor32(this float4 color) => new Color32((byte)color.x, (byte)color.y, (byte)color.z, (byte)color.w);

        static public Color32 ApplyAlpha(this Color32 color, float newAlpha)
        {
            color.a = (byte)(newAlpha * 255);
            return color;
        }
    }

}

