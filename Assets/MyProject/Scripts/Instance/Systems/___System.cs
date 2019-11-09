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
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;

namespace Abss.Character
{

    [DisableAutoCreation]
    //[UpdateInGroup( typeof( ObjectLogicSystemGroup ) )]
    [UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class ___System : JobComponentSystem
    {
        

        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }
        


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            return inputDeps;
        }
        
    }
}
