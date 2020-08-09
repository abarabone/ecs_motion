using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Unity.Physics;

namespace Abarabone.Arms
{

    static public partial class WaponSelector
    {

        public struct InitializeData : IComponentData
        {
            public Entity CharacterMainEntity;
            public Entity MuzzleEntity;
            public Entity 
            public Entity WaponEntity0;
            public Entity WaponEntity1;
            public Entity WaponEntity2;
        }


        public struct Toggle2Data : IComponentData
        {
            public Entity WaponEntity0;
            public Entity WaponEntity1;
        }

        public struct Toggle3Data : IComponentData
        {
            public Entity WaponEntity0;
            public Entity WaponEntity1;
            public Entity WaponEntity2;
        }

    }

}
