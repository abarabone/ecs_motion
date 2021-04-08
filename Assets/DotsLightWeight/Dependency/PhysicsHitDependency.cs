
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
        DependencyAccessableSystemBase dependentSystem;


        public static PhysicsHitDependency Create(DependencyAccessableSystemBase system) =>
            new PhysicsHitDependency
            {
                buildPhysicsWorld = system.World.GetExistingSystem<BuildPhysicsWorld>(),
                dependentSystem = system,
            };


        public PhysicsWorld PhysicsWorld => this.buildPhysicsWorld.PhysicsWorld;


        public DisposableDependency WithDependencyScope()
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

}
