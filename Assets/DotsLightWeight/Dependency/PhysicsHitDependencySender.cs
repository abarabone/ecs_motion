
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
    public static partial class PhysicsHitDependency
    {

        public struct Sender
        {

            BuildPhysicsWorld buildPhysicsWorld;
            DependencyAccessableSystemBase dependentSystem;


            public static Sender Create(DependencyAccessableSystemBase system) =>
                new Sender
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
                public Sender parent;

                public void Dispose()
                {
                    var phys = this.parent.buildPhysicsWorld;
                    var dep = this.parent.dependentSystem;
                    phys.AddInputDependencyToComplete(dep.GetOutputDependency());
                }
            }
        }

    }
}
