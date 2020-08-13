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

        public struct WaponTag : IComponentData
        { }


        //public struct UnitTopLinkData : IComponentData
        //{
        //    public Entity TopUnitEntity;
        //}


        //public struct InitializeData : IComponentData
        //{
        //    public int CarryId;
        //    //public FunctionUnitPrefabsData Prefabs;

        //    public Entity CharacterMainEntity;
        //    public Entity MuzzleBodyEntity;
        //}

        //public struct FunctionUnitPrefabsData : IComponentData
        //{
        //    public Entity FunctionUnitPrefab0;
        //    public Entity FunctionUnitPrefab1;
        //    public Entity FunctionUnitPrefab2;
        //    public Entity FunctionUnitPrefab3;
        //}


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
