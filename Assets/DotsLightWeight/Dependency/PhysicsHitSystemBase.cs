
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
//using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.CompilerServices;
using UnityEngine.XR;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Abarabone.Dependency
{


    public struct PhysicsHitDependency
    {

        BuildPhysicsWorld buildPhysicsWorld;
        DependentableSystemBase dependentSystem;


        public PhysicsHitDependency(DependentableSystemBase system)
        {
            this.buildPhysicsWorld = system.World.GetExistingSystem<BuildPhysicsWorld>();
            this.dependentSystem = system;
        }


        public PhysicsWorld PhysicsWorld => this.buildPhysicsWorld.PhysicsWorld;


        public DisposableDependency AsDependencyDisposable()
        {
            dependentSystem.AddInputDependency(this.buildPhysicsWorld.GetOutputDependency());

            return new DisposableDependency { parent = this };
        }


        public struct DisposableDependency : IDisposable
        {
            public PhysicsHitDependency parent;

            public void Dispose()
            {
                var phys = this.parent.buildPhysicsWorld;
                var dep = this.parent.dependentSystem;
                phys.AddInputDependencyToComplete(dep.GetOutputDependency());
            }
        }
    }


    public abstract class PhysicsHitSystemBase : SystemBase
    {

        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            base.OnCreate();

            this.buildPhysicsWorldSystem = this.World.GetExistingSystem<BuildPhysicsWorld>();
        }
        

        protected override void OnUpdate()
        {
            this.Dependency = JobHandle.CombineDependencies
                (this.Dependency, this.buildPhysicsWorldSystem.GetOutputDependency());


            this.OnUpdateWith(this.buildPhysicsWorldSystem);


            this.buildPhysicsWorldSystem.AddInputDependencyToComplete(this.Dependency);
        }

        protected abstract void OnUpdateWith(BuildPhysicsWorld physicsBuilder);
    }
}
