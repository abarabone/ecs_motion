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

    static public partial class Wapon
    {

        public struct InitializeData : IComponentData
        {
            public int Id;
            public UnitPrefabsData prefabs;
        }

        public struct UnitPrefabsData : IComponentData
        {
            public Entity UnitPrefab0;
            public Entity UnitPrefab1;
            public Entity UnitPrefab2;
            public Entity UnitPrefab3;
        }


        //public struct Unit1HolderData : IComponentData
        //{
        //    public Entity UnitEntity0;
        //}

        //public struct Unit2HolderData : IComponentData
        //{
        //    public Entity UnitEntity0;
        //    public Entity UnitEntity1;
        //}

    }

}
