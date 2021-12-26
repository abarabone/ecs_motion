//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Transforms;
//using Unity.Mathematics;
//////using Microsoft.CSharp.RuntimeBinder;
//using Unity.Entities.UniversalDelegates;
//using System.Runtime.InteropServices.WindowsRuntime;
//using UnityEngine.XR;
//using Unity.Physics.Systems;
//using Unity.Collections.LowLevel.Unsafe;

//namespace DotsLite.Arms
//{
//    using DotsLite.Dependency;
//    using DotsLite.Model;
//    using DotsLite.Model.Authoring;
//    using DotsLite.Arms;
//    using DotsLite.Character;
//    using DotsLite.Particle;
//    using DotsLite.SystemGroup;
//    using DotsLite.Geometry;
//    using Unity.Physics;
//    using DotsLite.Structure;
//    using DotsLite.Character.Action;
//    using DotsLite.Collision;
//    using DotsLite.Targeting;
//    using DotsLite.Misc;
//    using DotsLite.HeightGrid;
//    using DotsLite.Utilities;

//    [DisableAutoCreation]
//    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.Hit))]
//    //[UpdateAfter(typeof(BulletMoveSystem))]
//    //[UpdateBefore(typeof(StructureHitMessageApplySystem))]
//    public class BulletRayGridHitSystem : DependencyAccessableSystemBase
//    {

//        CommandBufferDependency.Sender cmddep;

//        PhysicsHitDependency.Sender phydep;

//        //HitMessage<Structure.HitMessage>.Sender stSender;
//        //HitMessage<Character.HitMessage>.Sender chSender;


//        protected override void OnCreate()
//        {
//            base.OnCreate();

//            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);

//            this.phydep = PhysicsHitDependency.Sender.Create(this);

//            //this.stSender = HitMessage<Structure.HitMessage>.Sender.Create<StructureHitMessageApplySystem>(this);
//            //this.chSender = HitMessage<Character.HitMessage>.Sender.Create<CharacterHitMessageApplySystem>(this);
//        }


//        protected unsafe override void OnUpdate()
//        {
//            using var cmdScope = this.cmddep.WithDependencyScope();
//            using var phyScope = this.phydep.WithDependencyScope();


//            var cmd = cmdScope.CommandBuffer.AsParallelWriter();
//            var cw = phyScope.PhysicsWorld.CollisionWorld;


//            //var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);


//            var dt = this.Time.DeltaTime;
//            var dtrate = dt * TimeEx.PrevDeltaTimeRcp;


//            var grid = this.GetSingleton<GridMaster.Data>();
//            var emit = this.GetSingleton<GridMaster.Emitting>();
//            var currs = grid.Currs;
//            var xspan = grid.Info.UnitLengthInGrid.x * grid.Info.NumGrids.x;
//            var ginfo = grid.Info;
//            var p = (float*)currs.GetUnsafePtr();

//            this.Entities
//                .WithBurst()
//                .WithAll<GridMaster.Data>()
//                .WithAll<Bullet.RayTag>()
//                .WithNone<Particle.LifeTimeInitializeTag>()
//                //.WithReadOnly(targets)
//                .WithNativeDisableUnsafePtrRestriction(p)
//                .WithNativeDisableContainerSafetyRestriction(currs)
//                .ForEach(
//                    (
//                        Entity entity, int entityInQueryIndex,
//                        in Translation pos,
//                        in Psyllium.TranslationTailData tail,
//                        in Particle.VelocityFactorData vfact,
//                        in Bullet.LinkData link,
//                        in Bullet.HitResponceTypesData hres,
//                        in CorpsGroup.TargetWithArmsData corps
//                    ) =>
//                    {
//                        var eqi = entityInQueryIndex;
                        

//                        // 暫定
//                        //var h = ginfo.CalcVerticalHeight(p, pos.Value.xz);
//                        //var wxz = pos.Value.xz - ginfo.LeftTopLocation.xz;
//                        //var i = wxz * ginfo.UnitScaleRcp;
//                        //var index2 = (int2)i;
//                        //if (!(math.any(index2 < int2.zero) || math.any(index2 >= ginfo.TotalLength)))
//                        //{
//                        //    var serialIndex = index2.x + index2.y * ginfo.TotalLength.x;
//                        //    var a = -10.0f;
//                        //    currs[serialIndex] -= a * dt * dt * 0.5f;
//                        //}
//                        var res = ginfo.RaycastHit2(p, tail.Position, pos.Value);
//                        if (!res.isHit) return;

//                        var ent = cmd.Instantiate(eqi, emit.SplashPrefab);
//                        cmd.SetComponent(eqi, ent, new Translation { Value = res.p });

//                        Debug.DrawRay(res.p, Vector3.up, Color.green, 0.5f);

//                        var wp = res.p - ginfo.LeftTopLocation;
//                        var i = wp.xz * ginfo.UnitScaleRcp;
//                        var index2 = (int2)i;
//                        var serialIndex = index2.x + index2.y * ginfo.TotalLength.x;
//                        var a = -1.0f;
//                        var d = a * dt * dt * 0.5f;
//                        var wxz = (float2)index2 * ginfo.UnitScale;
//                        currs[serialIndex + 0] -= d * math.length((wxz + new float2(0,0)).x_y(currs[serialIndex + 0]) - wp);
//                        currs[serialIndex + 1] -= d * math.length((wxz + new float2(1,0)).x_y(currs[serialIndex + 1]) - wp);
//                        currs[serialIndex + ginfo.TotalLength.x + 0] -= d * math.length((wxz + new float2(0, 1)).x_y(currs[serialIndex + ginfo.TotalLength.x + 0]) - wp);
//                        currs[serialIndex + ginfo.TotalLength.x + 1] -= d * math.length((wxz + new float2(1, 1)).x_y(currs[serialIndex + ginfo.TotalLength.x + 1]) - wp);
//                    }
//                )
//                .ScheduleParallel();
//        }

//    }

//}