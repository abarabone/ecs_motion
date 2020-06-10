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

using Abarabone.Misc;
using Abarabone.Utilities;
using Abarabone.SystemGroup;

namespace Abarabone.Character
{

    [DisableAutoCreation]
    //[UpdateInGroup( typeof( SystemGroup.Presentation.Logic.ObjectLogicSystemGroup ) )]
    [UpdateInGroup( typeof( SystemGroup.Simulation.Move.ObjectMoveSystemGroup ) )]
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
