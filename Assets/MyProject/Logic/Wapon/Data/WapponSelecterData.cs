﻿using System.Collections;
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

        public struct CharacterMainLink : IComponentData
        {
            public Entity CharacterMainEntity;
        }


        public struct ToggleModeData : IComponentData
        {
            public int CurrentWaponId;
            public int WaponLength;
        }

        //public struct ToggleModeTag : IComponentData
        //{ }


        //public interface IWaponEntityHolder
        //{
        //    Entity GetWaponEntity { get; }
        //}
        //public struct WaponPrefab0 : IComponentData, IWaponEntityHolder
        //{
        //    public Entity WaponPrefab;
        //    public Entity GetWaponEntity { get => this.WaponPrefab; }
        //}
        //public struct WaponPrefab1 : IComponentData, IWaponEntityHolder
        //{
        //    public Entity WaponPrefab;
        //    public Entity GetWaponEntity { get => this.WaponPrefab; }
        //}
        //public struct WaponPrefab2 : IComponentData, IWaponEntityHolder
        //{
        //    public Entity WaponPrefab;
        //    public Entity GetWaponEntity { get => this.WaponPrefab; }
        //}
        //public struct WaponPrefab3 : IComponentData, IWaponEntityHolder
        //{
        //    public Entity WaponPrefab;
        //    public Entity GetWaponEntity { get => this.WaponPrefab; }
        //}

    }

}
