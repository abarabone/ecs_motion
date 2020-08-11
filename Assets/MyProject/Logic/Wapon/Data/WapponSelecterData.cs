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
using System;
using System.Runtime.InteropServices;

namespace Abarabone.Arms
{

    static public partial class WaponMessage
    {

        public struct CreateMsgData : IComponentData
        {
            public Entity WaponSelectorEntity;
            public Entity WaponPrefab;
        }

    }



    static public partial class WaponHolder
    {

        /// <summary>
        /// ステータスなどを取得し、
        /// </summary>

        //[StructLayout(LayoutKind.Explicit)]
        //public unsafe struct WaponFunctionUnitLinkData : IComponentData
        //{
        //    [FieldOffset(0)]
        //    public int WaponCarryLength;

        //    [FieldOffset(64*0+4)]
        //    public Entity TopUnitEntity0;
        //    [FieldOffset(64*1+4)]
        //    public Entity TopUnitEntity1;
        //    [FieldOffset(64*2+4)]
        //    public Entity TopUnitEntity2;
        //    [FieldOffset(64*3+4)]
        //    public Entity TopUnitEntity3;

        //    [FieldOffset(4)]
        //    fixed long units[4];

        //    public Entity TopUnitEntity(int id)
        //    {
        //        fixed(long* p = this.units)
        //        {
        //            return *(Entity*)(p + id);
        //        }
        //    }
        //}


    }


    static public partial class WaponSelector
    {

        public struct LinkData : IComponentData
        {
            public Entity OwnerMainEntity;
            public Entity muzzleBodyEntity;// ユニット初期化時にコピーするためにおいておく
        }

        public struct ToggleModeData : IComponentData
        {
            public int CurrentWaponCarryId;
            public int WaponCarryLength;
        }


        //public struct ToggleIdPowerOf2Mask : IComponentData
        //{
        //    public int CarryIdMask;
        //}

        //public struct WaponPrefabsData : IComponentData
        //{
        //    public Entity WaponEntity0;
        //    public Entity WaponEntity1;
        //    public Entity WaponEntity2;
        //    public Entity WaponEntity3;
        //}


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
