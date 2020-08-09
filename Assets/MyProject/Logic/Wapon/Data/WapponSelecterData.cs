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

        public struct ToggleModeTag : IComponentData
        { }


        public interface IWaponEntityHolder
        {
            Entity GetWaponEntity { get; }
        }
        public struct WaponEntity0 : IComponentData, IWaponEntityHolder
        {
            public Entity WaponEntity;
            public Entity GetWaponEntity { get => this.WaponEntity; }
        }
        public struct WaponEntity1 : IComponentData, IWaponEntityHolder
        {
            public Entity WaponEntity;
            public Entity GetWaponEntity { get => this.WaponEntity; }
        }
        public struct WaponEntity2 : IComponentData, IWaponEntityHolder
        {
            public Entity WaponEntity;
            public Entity GetWaponEntity { get => this.WaponEntity; }
        }
        public struct WaponEntity3 : IComponentData, IWaponEntityHolder
        {
            public Entity WaponEntity;
            public Entity GetWaponEntity { get => this.WaponEntity; }
        }

    }

}
